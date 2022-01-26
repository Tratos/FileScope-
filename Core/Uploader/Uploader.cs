// Uploader.cs
// Copyright (C) 2002 Matt Zyzik (www.FileScope.com)
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Net.Sockets;
using System.Net;
using System.Timers;
using System.Text;
using System.IO;

namespace FileScope
{
	/// <summary>
	/// Core class used for uploading files.
	/// Not meant to be reusable.
	/// </summary>
	public class Uploader
	{
		//still being used?
		public volatile bool active = false;
		//connected and uploading?
		public volatile bool ready = false;
		//socket object
		public Socket sock1;
		//10 seconds to wait for request
		GoodTimer wait = new GoodTimer();
		//1 second timer to check upload bandwidth
		GoodTimer upBand = new GoodTimer();
		//6 seconds to connect
		GoodTimer connectYet = new GoodTimer(6000);
		//bytes sent since last upBand timer event
		private volatile int upBytes = 0;
		//thread
		public System.Threading.Thread sendThread;
		//filestream
		FileStream fs;
		//binary reader
		BinaryReader br;
		//identifier
		int upNum;
		//how much is left?
		public volatile uint bytes_to_send;
		//store ip
		public string ipStored = "";

		//for edonkey
		public EDonkey.Sck edsck = null;
		public byte[] md4sum = null;

		//values determined from the request
		bool openNapUpload;
		public string elSha1 = "";
		public int fileIndex = -1;
		public string fileName = "";
		public string userAgent = "";
		public string http11 = "";
		public uint start;
		public uint stop;
		public uint totalSize = 0;
		string request = "";
		public string chatIP = "";

		public Uploader()
		{
			wait.AddEvent(new ElapsedEventHandler(wait_Tick));
			upBand.AddEvent(new ElapsedEventHandler(upBand_Tick));
			connectYet.AddEvent(new ElapsedEventHandler(connectYet_Tick));
		}

		/// <summary>
		/// Take care of that incoming connection.
		/// </summary>
		public void Incoming(int upNum, Socket tmpSock, ref string elMsg)
		{
			try
			{
				this.upNum = upNum;
				this.request = elMsg;

				//stats
				Stats.Updated.uploadsNow++;
				active = true;

				//update gui
				GUIBridge.NewUpload(upNum);

				sendThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncThread));

				wait.Interval = 10000;
				wait.Start();
				upBand.Interval = 1000;
				upBand.Start();

				//accept connection
				sock1 = tmpSock;

				//start thread / upload
				sendThread.Start();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("uploader Incoming");
			}
		}

		public void IncomingEdSck(int upNum, byte[] md4hash, uint start, uint stop, int edsckNum)
		{
			bool match = false;
			lock(Stats.fileList)
				foreach(FileObject fo in Stats.fileList)
					if(fo.md4 != null)
						if(GUID.Compare(md4hash, 0, fo.md4, 0))
						{
							match = true;
							this.start = start;
							this.stop = stop;
							this.md4sum = md4hash;
							this.totalSize = fo.b;
							this.fileName = fo.location;
							this.fileIndex = fo.fileIndex;
							break;
						}
			if(!match)
				return;

			try
			{
				this.upNum = upNum;
				//only for eDonkey do we stop--
				this.stop--;
				this.edsck = EDonkey.Sck.scks[edsckNum];
				this.edsck.upNum = upNum;

				//stats
				Stats.Updated.uploadsNow++;
				active = true;
				GUIBridge.NewUpload(upNum);
				sendThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncThread));
				wait.Interval = 10000;
				wait.Start();
				upBand.Interval = 1000;
				upBand.Start();

				//start thread / upload
				sendThread.Start();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("uploader IncomingEdSck");
			}
		}

		/// <summary>
		/// Connect to host to push the upload.
		/// </summary>
		public void Outgoing(int upNum, int fileIndex, string ip, int port)
		{
			try
			{
				this.upNum = upNum;
				this.fileIndex = fileIndex;
				this.request = "";

				//stats
				Stats.Updated.uploadsNow++;
				active = true;

				//update gui
				GUIBridge.NewUpload(upNum);

				//start
				sock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPHostEntry IPHost = Dns.GetHostByName(ip);
				IPEndPoint endPoint = new IPEndPoint(IPHost.AddressList[0], port);
				sock1.BeginConnect(endPoint, new AsyncCallback(OnConnect), sock1);
				connectYet.Start();
			}
			catch
			{
				StopEverything("uploader Outgoing");
			}
		}

		void OnConnect(IAsyncResult ar)
		{
			if(ready || !active)
				return;

			try
			{
				Socket tmpSock = (Socket)ar.AsyncState;
				tmpSock.EndConnect(ar);

				//connected
				connectYet.Stop();
				sendThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncThread));

				wait.Interval = 10000;
				wait.Start();
				upBand.Interval = 1000;
				upBand.Start();

				//-5 means G2
				if(this.fileIndex == -5)
				{
					string strMsg = "PUSH guid:";
					strMsg += Utils.HexGuid(Stats.settings.myGUID) + "\r\n\r\n";
					sock1.Send(Encoding.ASCII.GetBytes(strMsg));
				}
				else
				{
					string strMsg = "GIV ";
					strMsg += fileIndex.ToString() + ":";
					strMsg += Utils.HexGuid(Stats.settings.myGUID) + "/";
					FileObject fo = (FileObject)Stats.fileList[fileIndex];
					strMsg += Path.GetFileName(fo.location) + "\n\n";
					sock1.Send(Encoding.ASCII.GetBytes(strMsg));
				}

				//start thread / upload
				sendThread.Start();
			}
			catch
			{
				StopEverything("OnConnect");
			}
		}

		/// <summary>
		/// Runs on another thread.
		/// Used for the blocking upload of a file.
		/// </summary>
		void FuncThread()
		{
			try
			{
				//only for Gnutella
				if((request.ToLower().IndexOf(@"get /") == 0 || request == "") && this.edsck == null)
				{
					//loop for the HTTP request
					while(true)
					{
						if(request.IndexOf("\r\n\r\n") != -1)
							break;
						byte[] receiveBuff = new byte[4096];
						int bytesRec = sock1.Receive(receiveBuff);
						Stats.Updated.inTransferBandwidth += bytesRec;
						if(bytesRec == 0)
							goto over;
						request += Encoding.ASCII.GetString(receiveBuff, 0, bytesRec);
						System.Threading.Thread.Sleep(10);
					}
				}
				if(request.ToLower().IndexOf(@"http/1.1") != -1)
					this.http11 = "/1.1";
				else
					this.http11 = "";
				ready = true;
				string response = "";
				if(this.edsck == null)
				{
					//parse request
					ParseThis(ref request);
					//create a response
					response = CreateResponse();
				}
				//send update to gui
				GUIBridge.UpdateUpload(Path.GetFileName(fileName), totalSize, "Uploading", "0%", start.ToString() + " - " + stop.ToString(), "0 KB/s", upNum);
				this.ipStored = this.RemoteIP();

				//send response
				if(response != "" && this.edsck == null)
				{
					sock1.Send(Encoding.ASCII.GetBytes(response));
					Stats.Updated.outTransferBandwidth += response.Length;
				}

				//send the file
				bytes_to_send = stop - start + 1;
				fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
				fs.Seek(start, SeekOrigin.Begin);
				uint curOffset = start;
				br = new BinaryReader(fs);
				byte[] readBuf = new byte[4096];
				while(bytes_to_send > 0)
				{
					if(bytes_to_send > 4096)
					{
						br.Read(readBuf, 0, 4096);
						if(edsck == null)
						{
							sock1.Send(readBuf);
							Stats.Updated.outTransferBandwidth += 4096;
						}
						else
						{
							byte[] temp = new byte[4096];
							Array.Copy(readBuf, 0, temp, 0, 4096);
							EDonkey.Messages.SendingPart(this.md4sum, curOffset, curOffset+4096, temp, this.edsck.sockNum);
						}
						upBytes += 4096;
						curOffset += 4096;
						bytes_to_send -= 4096;
					}
					else
					{
						br.Read(readBuf, 0, (int)bytes_to_send);
						if(edsck == null)
						{
							sock1.Send(readBuf, 0, (int)bytes_to_send, SocketFlags.None);
							Stats.Updated.outTransferBandwidth += (int)bytes_to_send;
						}
						else
						{
							byte[] temp = new byte[bytes_to_send];
							Array.Copy(readBuf, 0, temp, 0, (int)bytes_to_send);
							EDonkey.Messages.SendingPart(this.md4sum, curOffset, curOffset+bytes_to_send, temp, this.edsck.sockNum);
						}
						upBytes += (int)bytes_to_send;
						curOffset += bytes_to_send;
						bytes_to_send -= bytes_to_send;						
					}
					System.Threading.Thread.Sleep(10);
				}

				//stats for # of files uploaded
				Stats.Updated.uploads++;
			}
			catch(Exception e)
			{
				if(e.ToString().IndexOf("at System.Net.Sockets.Socket") == -1)
					System.Diagnostics.Debug.WriteLine("Upload FuncThread: " + e.ToString());
			}
			over:
				StopEverything("Upload FuncThread");
		}

		void connectYet_Tick(object sender, ElapsedEventArgs e)
		{
			StopEverything("connectYet: " + upNum.ToString());
		}

		void wait_Tick(object sender, ElapsedEventArgs e)
		{
			if(ready == true)
				return;

			StopEverything("wait_Tick");
		}

		void upBand_Tick(object sender, ElapsedEventArgs e)
		{
			//update truespeed
			double kbTrue = ((double)Stats.Updated.inTransferBandwidth / (double)1024) + ((double)Stats.Updated.outTransferBandwidth / (double)1024);
			if((int)kbTrue > Stats.GetSpeed() && (int)kbTrue > 1)
			{
				Stats.Updated.trueSpeed = (int)kbTrue;
				//System.Diagnostics.Debug.WriteLine("trueSpeed updated: " + kbTrue.ToString());
			}

			if(ready != true)
				return;

			//start with speed
			double kbSent = (double)upBytes / (double)1024;
			double spEEd = Math.Round(kbSent, 2);
			string speed = spEEd.ToString() + " KB/s";
			upBytes = 0;

			//percent done
			uint total_to_send = stop - start + 1;
			double perCent = Math.Round((double)(total_to_send-bytes_to_send) / (double)total_to_send * 100, 2);
			string percent = perCent.ToString() + "%";

			//update
			GUIBridge.UpdateUpload("", totalSize, "Uploading (" + ipStored + ")", percent, ((uint)(total_to_send-bytes_to_send+start)).ToString() + "-" + stop.ToString(), speed, upNum);
		}

		/// <summary>
		/// Stop everything.
		/// </summary>
		public void StopEverything(string where)
		{
			if(active)
			{
				active = false;
				System.Diagnostics.Debug.WriteLine("Uploader StopEverything: " + where);
				Stats.Updated.uploadsNow--;
			}

			//update gui
			if(fileName == "al;skdfj;asldfj")
				GUIBridge.RemoveUpload(upNum, 0);
			else if(ready && bytes_to_send == 0)
				GUIBridge.RemoveUpload(upNum, 1);
			else
				GUIBridge.RemoveUpload(upNum, 2);

			//close the connection
			try
			{
				if(edsck != null)
				{
					edsck.UPend();
					edsck = null;
				}
				if(sock1 != null)
				{
					if(sock1.Connected)
						sock1.Shutdown(SocketShutdown.Both);
					sock1.Close();
					sock1 = null;
				}
				this.ipStored = "";
			}
			catch (Exception e83729)
			{
				System.Diagnostics.Debug.WriteLine("Uploader StopEverything sock1 shutdown: " + e83729.ToString());
			}

			try
			{if(br != null) br.Close();}
			catch
			{System.Diagnostics.Debug.WriteLine("Uploader StopEverything 1");}
			try
			{if(fs != null) fs.Close();}
			catch
			{System.Diagnostics.Debug.WriteLine("Uploader StopEverything 2");}

			//stop all timers
			wait.Stop();
			wait.RemoveEvent(new ElapsedEventHandler(wait_Tick));
			wait.Close();
			upBand.Stop();
			upBand.RemoveEvent(new ElapsedEventHandler(upBand_Tick));
			upBand.Close();
			connectYet.Stop();
			connectYet.RemoveEvent(new ElapsedEventHandler(connectYet_Tick));
			connectYet.Close();

			//set this Uploader to null
			StartApp.main.Invoke(new UploadManager.nullifyMe(UploadManager.NullifyMe), new object[]{this.upNum});
		}

		/// <summary>
		/// Return the remote IP address.
		/// </summary>
		public string RemoteIP()
		{
			string tempEnd = "";
			try
			{
				if(this.edsck != null)
					tempEnd = this.edsck.sock1.RemoteEndPoint.ToString();
				else
					tempEnd = sock1.RemoteEndPoint.ToString();
			}
			catch
			{
				//System.Diagnostics.Debug.WriteLine("Uploader RemoteIP");
			}
			int xPos = tempEnd.IndexOf(":");
			if(xPos == -1)
				return "";
			else
				return tempEnd.Substring(0, xPos);
		}

		/// <summary>
		/// Parse an http request.
		/// </summary>
		void ParseThis(ref string elRequesto)
		{
			try
			{
				openNapUpload = (request.ToLower().IndexOf(@"get /") != 0) ? true : false;
				//System.Diagnostics.Debug.WriteLine("openNapUpload: " + openNapUpload.ToString());

				if(openNapUpload)
				{
					//System.Diagnostics.Debug.WriteLine(elRequesto);
					/*
					Request:
					GET<mynick> "<filename>" <offset>
					ex. GETcoolkid509 "c:\pornos\december\b-d\brunettes\tall\julie.mpg" 23

					Response:
					<filesize><file data stream>
					*/
					elRequesto = elRequesto.Substring(3, elRequesto.Length - 3);
					string onapUserName = elRequesto.Substring(0, elRequesto.IndexOf("\"")-1);
					int nextpos = elRequesto.IndexOf("\"")+1;
					string onapFileName = elRequesto.Substring(nextpos, elRequesto.LastIndexOf("\"")-nextpos);
					nextpos = elRequesto.LastIndexOf("\"")+2;
					uint onapStartOffset = Convert.ToUInt32(elRequesto.Substring(nextpos, elRequesto.Length-nextpos));
					
					//copy to our global variables
					fileName = onapFileName;
					userAgent = onapUserName;
					start = onapStartOffset;
					stop = 0;
				}
				else
				{
					//chat support
					int chatIndex = elRequesto.IndexOf("Chat: ") + 6;
					if(chatIndex != 5)
						this.chatIP = elRequesto.Substring(chatIndex, elRequesto.IndexOf("\r", chatIndex) - chatIndex);

					if(elRequesto.ToLower().IndexOf("get /uri-res/n2r?urn:sha1:") == 0)
					{
						//System.Diagnostics.Debug.WriteLine("urn:sha1 upload request");
						elRequesto = elRequesto.Substring(26, elRequesto.Length - 26);
						this.elSha1 = elRequesto.Substring(0, 32);
					}
					else if(elRequesto.ToLower().IndexOf("uri-res/n2r") != -1)
					{
						System.Diagnostics.Debug.WriteLine("unknown uri request: " + elRequesto);
						throw new Exception("unknown uri");
					}
					else
					{
						//file index
						elRequesto = elRequesto.Substring(9, elRequesto.Length - 9);
						fileIndex = Convert.ToInt32(elRequesto.Substring(0, elRequesto.IndexOf("/")));
						//System.Diagnostics.Debug.WriteLine(fileIndex.ToString());

						//file name
						elRequesto = elRequesto.Substring(elRequesto.IndexOf("/")+1, elRequesto.Length - (elRequesto.IndexOf("/")+1));
						int end1 = elRequesto.ToLower().IndexOf(" http/");
						int end2 = elRequesto.ToLower().IndexOf("/");
						fileName = elRequesto.Substring(0, Math.Min(end1, end2));
						//System.Diagnostics.Debug.WriteLine(fileName);
					}

					//vendor
					int vendorIndex = elRequesto.ToLower().IndexOf("user-agent:");
					if(vendorIndex != -1)
					{
						vendorIndex += 11;
						string vendor = elRequesto.Substring(vendorIndex, (elRequesto.IndexOf("\r\n", vendorIndex) - vendorIndex));
						if(vendor[0].ToString() == " ")
							vendor = vendor.Substring(1, vendor.Length-1);
						userAgent = vendor;
					}
					//System.Diagnostics.Debug.WriteLine(userAgent);

					//start-stop
					int startstop = elRequesto.ToLower().IndexOf("bytes=") + 6;
					int minusLoc = elRequesto.IndexOf("-", startstop);
					start = Convert.ToUInt32(elRequesto.Substring(startstop, minusLoc - startstop));
					//System.Diagnostics.Debug.WriteLine(start.ToString());
					string numStop = elRequesto.Substring(minusLoc+1, elRequesto.IndexOf("\r", minusLoc) - (minusLoc+1));
					if(numStop.Length > 0)
						stop = Convert.ToUInt32(numStop);
					else
						//we'll calculate the stop point later
						stop = 0;
					//System.Diagnostics.Debug.WriteLine(stop.ToString());
				}
				return;
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("ParseThis: " + e.Message);
				System.Diagnostics.Debug.WriteLine("what's left of the request:\n" + elRequesto);
			}
		}

		/// <summary>
		/// Create a response for the request.
		/// </summary>
		string CreateResponse()
		{
			//find the requested file
			bool match = false;
			if(fileName != "" || elSha1 != "")
				lock(Stats.fileList)
				{
					foreach(FileObject fo in Stats.fileList)
					{
						if(openNapUpload && fo.location == fileName)
						{
							//we'll need to verify the actual filesizes are the same
							uint actualfsize = 0;
							FileInfo inf = new FileInfo(fo.location);
							try{actualfsize = (uint)inf.Length;}
							catch{System.Diagnostics.Debug.WriteLine("CreateResponse filesize check");}
							if(actualfsize == 0)
							{
								match = false;
								break;
							}
							//update in case it's off
							fo.b = actualfsize;

							this.fileIndex = fo.fileIndex;
							match = true;
							//update to the real stop value
							stop = fo.b-1;
							totalSize = fo.b;
							break;
						}
						else if(elSha1 != "" && elSha1.ToLower() == fo.sha1.ToLower())
						{
							//we'll need to verify the actual filesizes are the same
							uint actualfsize = 0;
							FileInfo inf = new FileInfo(fo.location);
							try{actualfsize = (uint)inf.Length;}
							catch{System.Diagnostics.Debug.WriteLine("CreateResponse filesize check");}
							if(actualfsize == 0)
							{
								match = false;
								break;
							}
							//update in case it's off
							fo.b = actualfsize;

							this.fileIndex = fo.fileIndex;
							this.fileName = fo.location;
							match = true;
							//update to the real stop value
							if(stop == 0)
								stop = fo.b-1;
							totalSize = fo.b;
							break;
						}
						else if(fo.fileIndex == fileIndex)
						{
							if(fo.location.IndexOf(fileName) != -1 || fo.location.IndexOf(System.Web.HttpUtility.UrlDecode(fileName)) != -1)
							{
								//we'll need to verify the actual filesizes are the same
								uint actualfsize = 0;
								FileInfo inf = new FileInfo(fo.location);
								try{actualfsize = (uint)inf.Length;}
								catch{System.Diagnostics.Debug.WriteLine("CreateResponse filesize check");}
								if(actualfsize == 0)
								{
									match = false;
									break;
								}
								//update in case it's off
								fo.b = actualfsize;

								//we now put the whole path into fileName
								fileName = fo.location;
								match = true;
								//update to the real stop value
								if(stop == 0)
									stop = fo.b-1;
								totalSize = fo.b;
							}
							break;
						}
					}
				}

			if(!match)
			{
				fileName = "al;skdfj;asldfj";
				if(openNapUpload)
					return "";
				else
					return "HTTP" + this.http11 + " 404 File Not Found\r\n\r\n";
			}

			if(Stats.Updated.uploadsNow > Stats.settings.maxUploads)
			{
				fileName = "al;skdfj;asldfj";
				if(openNapUpload)
					return "";
				else
					return "HTTP" + this.http11 + " 503 Server Busy\r\n\r\n";
			}

			if(!openNapUpload)
			{
				string resp = "";
				resp += "HTTP" + this.http11 + " 200 OK\r\n";
				resp += "Server: FileScope " + Stats.version + "\r\n";
				resp += "Chat: " + Stats.settings.ipAddress + ":" + Stats.settings.port.ToString() + "\r\n";
				resp += "Content-type: " + FileType.GetContentType(fileName) + "\r\n";
				resp += "Content-length: " + Convert.ToString(stop - start + 1) + "\r\n";
				resp += "Content-range: bytes=" + start.ToString() + "-" + stop.ToString() + "/" + totalSize.ToString() + "\r\n";
				if(elSha1 != "")
				{
					resp += "X-Gnutella-Content-URN: urn:sha1:" + elSha1 + "\r\n\r\n";
					//System.Diagnostics.Debug.WriteLine("Our Response:\r\n" + resp);
				}
				else
				{
					resp += "\r\n";
				}
				return resp;
			}
			else
				return totalSize.ToString();
		}
	}
}
