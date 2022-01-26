// Sck.cs
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
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Text;
using System.Collections;
using System.IO;

namespace FileScope.Gnutella
{
	/// <summary>
	/// Condition of a Sck.
	/// </summary>
	public enum Condition
	{
		Closed,		//disconnected socket
		Connecting,	//connecting
		Connected,	//connected superficially: only at hardware level
		hndshk,		//first handshake sent or received
		hndshk2,	//response handshake sent or received
		hndshk3		//final response handshake sent or received; this is the "real" connected state
	}

	/// <summary>
	/// Connection class used for the gnutella network.
	/// Each instance of this class contains an instance of System.Net.Sockets.Socket.
	/// This is a blocking socket model as we implement SACHRIFC hop flow scheme.
	/// Connecting is the only asynchronous thing.
	/// Connections to WebCache servers are also managed here.
	/// Each instance of this class is reusable after a disconnect.
	/// </summary>
	public class Sck
	{
		//a maximum of 810 gnutella sockets
		public static Sck[] scks = new Sck[20];//20 for now

		public Socket sock1;						//socket instance
		HttpWebRequest httpRequest;					//for connecting to web cache servers
		public int sockNum;							//socket identifier
		byte[] receiveBuff = new byte[32768];		//main receive buffer
		MemoryStream sendMS = new MemoryStream();	//main send buffer
		public ArrayList buf = new ArrayList();		//receive buffer used for dealing with handshakes
		public ArrayList pckBuf = new ArrayList();	//arraylist of actual Message objs ready for processing
		public ArrayList oBuf1 = new ArrayList();	//high priority send buffer (hop flow)
		public ArrayList oBuf2 = new ArrayList();	//medium priority send buffer (hop flow)
		public ArrayList oBuf3 = new ArrayList();	//low priority send buffer (hop flow)
		public string address;
		public int port;
		public QueryRouteTable recQRT;			//QRT received from incoming leaf
		public volatile Condition state;		//state of the connection
		public volatile int protocolVer;		//the gnutella version of remotehost
		public volatile bool incoming;			//is it an incoming connection
		public volatile bool ultrapeer;			//is this socket connected to an ultrapeer node?
		public volatile string vendor;			//vendor name
		public volatile bool newHost;			//supports ultrapeers
		public volatile bool webCache;			//are we connecting to a web cache server?
		public volatile int bytesOut=0;			//outgoing bytes sent in last second
		public volatile int bytesIn=0;			//incoming bytes received in last second

		//outgoing connection timers
		public GoodTimer connectYet = new GoodTimer();			//6 seconds to try and connect to node
		public GoodTimer responseYet = new GoodTimer();			//4 seconds for a Handshake response

		//incoming connection timers
		public GoodTimer inResponseYet1 = new GoodTimer();		//7 seconds for Handshake initiation
		public GoodTimer inResponseYet2 = new GoodTimer();		//7 seconds for ending hanshake

		//connection status timer
		public GoodTimer checkConnection = new GoodTimer();		//checks to see if connection exists
		public GoodTimer bandwidth = new GoodTimer();			//store bandwidth

		//threads
		public System.Threading.Thread sendThread;		//thread that sends data
		public System.Threading.Thread receiveThread;	//thread that receives data
		public System.Threading.Thread webCacheThread;	//thread connecting to web cache

		public Sck(int gnutellaSckIndex)
		{
			//take care of the timers with event handlers
			connectYet.Interval = 6000;
			connectYet.AddEvent(new ElapsedEventHandler(connectYet_Tick));
			responseYet.Interval = 4000;
			responseYet.AddEvent(new ElapsedEventHandler(responseYet_Tick));
			inResponseYet1.Interval = 7000;
			inResponseYet1.AddEvent(new ElapsedEventHandler(inResponseYet1_Tick));
			inResponseYet2.Interval = 7000;
			inResponseYet2.AddEvent(new ElapsedEventHandler(inResponseYet2_Tick));
			checkConnection.Interval = 20000;
			checkConnection.AddEvent(new ElapsedEventHandler(checkConnection_Tick));
			bandwidth.Interval = 1000;
			bandwidth.AddEvent(new ElapsedEventHandler(bandwidth_Tick));
			checkConnection.Start();
			bandwidth.Start();

			state = Condition.Closed;
			sockNum = gnutellaSckIndex;
			sendThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncSend));
			sendThread.IsBackground = true;
			sendThread.Start();
			receiveThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncRec));
			receiveThread.IsBackground = true;
			receiveThread.Start();
		}

		/// <summary>
		/// Regular outgoing connection happens here.
		/// </summary>
		public void Reset(string addr)
		{
			try
			{
				sock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				state = Condition.Connecting;
				GUIBridge.GNewConnection(addr, "Out", this.sockNum);
				bandwidth.Start();
				ultrapeer = false;
				incoming = false;
				newHost = false;
				webCache = false;
				//we do not know the version yet; we guess 6
				protocolVer = 6;
				vendor = "Unknown";
				//parse *.*.*.*:* into IP address and port
				Utils.AddrParse(addr, out address, out port, 6346);
				IPHostEntry IPHost = Dns.GetHostByName(address);
				IPEndPoint endPoint = new IPEndPoint(IPHost.AddressList[0], port);
				sock1.BeginConnect(endPoint, new AsyncCallback(OnConnect), sock1);
				connectYet.Start();
			}
			catch(Exception e)
			{
				Disconnect("Reset: " + e.Message);
			}
		}

		/// <summary>
		/// Take care of incoming connection.
		/// </summary>
		public void ResetIncoming(Socket tmpSock, byte[] bytesMsg)
		{
			try
			{
				state = Condition.Connecting;
				sock1 = tmpSock;
				GUIBridge.GNewConnection(RemoteIP(), "In", this.sockNum);
				bandwidth.Start();
				ultrapeer = false;
				incoming = true;
				newHost = false;
				webCache = false;
				recQRT = null;
				//we do not know the version yet; we guess 6
				protocolVer = 6;
				vendor = "Unknown";
				state = Condition.Connected;
				inResponseYet1.Start();
				//add to the buf for later processing
				lock(buf)
					buf.AddRange(bytesMsg);
				if(!StartStop.enabled)
				{
					Disconnect("g1 turned off");
					return;
				}
				lock(Stats.gnutellaQueue)
				{
					//queue this socket for processing if it isn't already queued
					if(!Stats.gnutellaQueue.Contains(this.sockNum))
						Stats.gnutellaQueue.Enqueue(this.sockNum);
				}
			}
			catch
			{
				Disconnect("ResetIncoming");
			}
		}

		/// <summary>
		/// Outgoing connection to a web cache server occurs here.
		/// </summary>
		public void ResetWebCache(string server)
		{
			try
			{
				state = Condition.Connecting;
				ultrapeer = false;
				incoming = false;
				newHost = false;
				webCache = true;
				protocolVer = 6;
				vendor = "WebCache";
				address = server;
				//notify UI thread of this
				GUIBridge.GNewConnection(server, "Out", this.sockNum);
				responseYet.Start();
				webCacheThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncWebCache));
				webCacheThread.Start();
			}
			catch
			{
				Disconnect("ResetWebCache error");
			}
		}

		void FuncWebCache()
		{
			try
			{
				//setup HttpWebRequest object; connect to server; initiate request
				httpRequest = (HttpWebRequest)WebRequest.Create(address + "?hostfile=1");
				httpRequest.UserAgent = "FileScope";
				//begin asynchronous get request
				httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), httpRequest);
			}
			catch
			{
				Disconnect("FuncWebCache");
			}
		}

		void OnGetResponse(IAsyncResult ar)
		{
			try
			{
				HttpWebRequest tmpReq = (HttpWebRequest)ar.AsyncState;
				//get the response from the server
				HttpWebResponse resp = (HttpWebResponse)tmpReq.EndGetResponse(ar);
				//create stream to read from
				StreamReader stream = new StreamReader(resp.GetResponseStream());

				//read all of the hosts
				string host = stream.ReadLine();
				int overflowCheck = 0;
				while(host != null)
				{
					//used for security purposes
					overflowCheck++;
					if(overflowCheck > 50)
						break;

					if(host.Length > 6 && host.Length < 22 && char.IsDigit(host, 0))
						lock(Stats.gnutellaHosts)
							Stats.gnutellaHosts.Add(host);
					host = stream.ReadLine();
				}
				stream.Close();
				Disconnect("webcache done");
			}
			catch(Exception e)
			{
				if(e.Message.IndexOf("404") != -1 || e.Message.IndexOf("403") != -1 || e.Message.ToLower().IndexOf("error") != -1 || e.Message.ToLower().IndexOf("closed") != -1)
				{
					lock(Stats.gnutellaWebCache)
					{
						//remove it
						for(int posgwc = 0; posgwc < Stats.gnutellaWebCache.Count; posgwc++)
							if((string)Stats.gnutellaWebCache[posgwc] == this.address)
							{
								System.Diagnostics.Debug.WriteLine("gwc removed");
								Stats.gnutellaWebCache.RemoveAt(posgwc);
								return;
							}
					}
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("g1 wc get response error: " + e.Message);
					System.Diagnostics.Debug.WriteLine(e.StackTrace);
				}
				Disconnect("OnGetResponse");
			}
		}

		void OnConnect(IAsyncResult ar)
		{
			//make sure we're not already connected
			if(state == Condition.hndshk3)
				return;
			if(state != Condition.Connecting)
				return;

			try
			{
				Socket tmpSock = (Socket)ar.AsyncState;
				tmpSock.EndConnect(ar);

				//connected
				connectYet.Stop();
				state = Condition.Connected;
				Handshake tempGuy = new Handshake(this.sockNum);
				tempGuy.Start();
			}
			catch
			{
				Disconnect("OnConnect routine");
			}
		}

		public void Disconnect(string why)
		{
			//if(state == Condition.hndshk3)
			//	System.Diagnostics.Debug.WriteLine("--Disconnected--" + sockNum.ToString() + "--" + why);
			if(state == Condition.Closed)
				return;
			try
			{if(webCacheThread != null) if(webCacheThread.IsAlive) webCacheThread.Abort();}
			catch(System.Threading.ThreadAbortException tae)
			{tae=tae;}
			catch(Exception e)
			{System.Diagnostics.Debug.WriteLine("webCacheThread: " + e.Message + " " + e.ToString());}
			try
			{if(httpRequest != null) httpRequest.Abort();}
			catch(System.Threading.ThreadAbortException tae)
			{tae=tae;}
			catch(Exception e)
			{System.Diagnostics.Debug.WriteLine("httpRequest: " + e.Message);}
			try
			{
				this.state = Condition.Closed;
				if(!Stats.Updated.closing)
					GUIBridge.GJustDisconnected(this.sockNum);
				connectYet.Stop();
				responseYet.Stop();
				inResponseYet1.Stop();
				inResponseYet2.Stop();
				bandwidth.Stop();
				bytesOut = 0;
				bytesIn = 0;
				lock(oBuf1)
					oBuf1.Clear();
				lock(oBuf2)
					oBuf2.Clear();
				lock(oBuf3)
					oBuf3.Clear();
				lock(buf)
					buf.Clear();
				lock(pckBuf)
					pckBuf.Clear();
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("g1 disconnect problem: " + e.Message);
			}
			try
			{
				if(sock1 != null)
				{
					if(sock1.Connected)
						sock1.Shutdown(SocketShutdown.Both);
					sock1.Close();
					sock1 = null;
				}
			}
			catch(System.Threading.ThreadAbortException tae)
			{tae=tae;}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Gnutella Sck sock1 shutdown: " + e.Message);
			}
			if(Stats.Updated.closing)
			{
				try
				{if(sendThread != null) if(sendThread.IsAlive) this.sendThread.Abort();}
				catch(System.Threading.ThreadAbortException tae)
				{tae=tae;}
				catch(Exception e)
				{System.Diagnostics.Debug.WriteLine("sendThread: " + e.Message);}
				try
				{if(receiveThread != null) if(receiveThread.IsAlive) this.receiveThread.Abort();}
				catch(System.Threading.ThreadAbortException tae)
				{tae=tae;}
				catch(Exception e)
				{System.Diagnostics.Debug.WriteLine("receiveThread: " + e.Message);}
			}
			this.httpRequest = null;
			this.recQRT = null;
		}

		/// <summary>
		/// Send something to this gnutella connection.
		/// </summary>
		public void SendPacket(Message packet)
		{
			if(this.state == Condition.Closed)
				return;

			//if it's created on this node, it's top priority
			if(packet.GetHOPS() == 0 || !packet.packet)
			{
				lock(oBuf1)
					oBuf1.Add(packet);
				return;
			}

			//if it's a push request or query hit or leaf node query, it's medium priority
			if(packet.GetPayloadDescriptor() == 0x81 || packet.GetPayloadDescriptor() == 0x40 || (packet.GetPayloadDescriptor() == 0x80 && packet.GetHOPS() == 1))
			{
				lock(oBuf2)
					oBuf2.Add(packet);
				return;
			}

			//anything else is low priority
			lock(oBuf3)
				oBuf3.Add(packet);
		}

		/// <summary>
		/// Hop flow scheme implemented in this send thread.
		/// </summary>
		void FuncSend()
		{
			while(true)
			{
				if(Stats.Updated.closing)
					return;

				//max bytes to send at once
				int maxbuff = 4096;
				//keep track of the buffer len
				int bufflen = 0;
				//set pos back to beginning of the stream
				sendMS.Position = 0;

				//start with high priority buffer
				lock(oBuf1)
					for(int x = 0; x < oBuf1.Count; x++)
					{
						if(bufflen >= maxbuff)
							break;
						Message pckt = (Message)oBuf1[0];
						oBuf1.RemoveAt(0);
						byte[] tmpBuf = pckt.GetWholePacket();
						bufflen += tmpBuf.Length;
						sendMS.Write(tmpBuf, 0, tmpBuf.Length);
					}

				//medium priority buffer
				lock(oBuf2)
					for(int x = 0; x < oBuf2.Count; x++)
					{
						if(bufflen >= maxbuff)
							break;
						Message pckt = (Message)oBuf2[0];
						oBuf2.RemoveAt(0);
						byte[] tmpBuf = pckt.GetWholePacket();
						bufflen += tmpBuf.Length;
						sendMS.Write(tmpBuf, 0, tmpBuf.Length);
					}

				//low priority buffer
				lock(oBuf3)
					for(int x = 0; x < oBuf3.Count; x++)
					{
						if(bufflen >= maxbuff)
							break;
						Message pckt = (Message)oBuf3[0];
						oBuf3.RemoveAt(0);
						byte[] tmpBuf = pckt.GetWholePacket();
						bufflen += tmpBuf.Length;
						sendMS.Write(tmpBuf, 0, tmpBuf.Length);
					}

				//do we have anything to send
				if(bufflen > 0)
				{
					try
					{
						sock1.Send(sendMS.GetBuffer(), 0, bufflen, SocketFlags.None);
						Stats.Updated.outNetworkBandwidth += bufflen;
						bytesOut += bufflen;
					}
					catch(Exception e)
					{
						Disconnect("g1 send error: " + e.Message);
					}
					//cleanup anything that's not being sent
					lock(oBuf1)
					{
						if(oBuf1.Count > 300)
						{
							oBuf1.RemoveRange(0, 20);
							//System.Diagnostics.Debug.WriteLine("oBuf1 trimming");
						}
					}
					lock(oBuf2)
					{
						if(oBuf2.Count > 300)
						{
							oBuf2.RemoveRange(0, 20);
							//System.Diagnostics.Debug.WriteLine("oBuf2 trimming");
						}
					}
					lock(oBuf3)
					{
						if(oBuf3.Count > 300)
						{
							oBuf3.RemoveRange(0, 20);
							//System.Diagnostics.Debug.WriteLine("oBuf3 trimming");
						}
					}
				}
				else
				{
					//rest a sec, or even less
					System.Threading.Thread.Sleep(20);
				}
			}
		}

		//used in FuncRec
		byte[] midbuf = new byte[7];

		/// <summary>
		/// Blocking receive.
		/// </summary>
		void FuncRec()
		{
			while(true)
			{
				if(Stats.Updated.closing)
					return;
				//make sure we're connected first
				if(state == Condition.Closed || state == Condition.Connecting)
				{
					System.Threading.Thread.Sleep(200);
					continue;
				}

				/*
				 * because of the way things worked out, we process handshakes in a different area than normal packets
				 * so that's why we have two sections here
				 */
				if(state != Condition.hndshk3)
				{
					int bytesRec = 0;
					try
					{
						bytesRec = sock1.Receive(receiveBuff);
						Stats.Updated.inNetworkBandwidth += bytesRec;
						bytesIn += bytesRec;
					}
					catch(Exception e)
					{
						Disconnect("before hndshk3; FuncRec" + e.Message);
						System.Threading.Thread.Sleep(10);
						continue;
					}
					if(bytesRec > 0)
					{
						//create a smaller image of the buffer
						byte[] tempMsg = new byte[bytesRec];
						Array.Copy(receiveBuff, 0, tempMsg, 0, bytesRec);

						/*
						 * instead of processing the data right away, we append tempMsg into buf
						 * we update Stats.gnutellaQueue so that this socket number is added
						 * ProcessThread will read the sockets in queue and process what is in buf
						 */
						lock(buf)
							buf.AddRange(tempMsg);
						//this could have happened during the blocking receive above
						if(state != Condition.hndshk3)
						{
							lock(Stats.gnutellaQueue)
							{
								//queue this socket for processing if it isn't already queued
								if(!Stats.gnutellaQueue.Contains(this.sockNum))
									Stats.gnutellaQueue.Enqueue(this.sockNum);
							}
						}
					}
					else
					{
						//we're probably disconnected right about now
						Disconnect("zero length message");
					}
				}
				else
				//now that the handshake is finished, we carry out !"*efficient*"! data processing
				{
					Message newPacket = new Message(this, this.midbuf);
					//if it's a good packet, we'll take a gander at it
					if(newPacket.headerType == 0)
					{
						lock(this.pckBuf)
							pckBuf.Add(newPacket);
						lock(Stats.gnutellaQueue)
						{
							//queue this socket for processing if it isn't already queued
							if(!Stats.gnutellaQueue.Contains(this.sockNum))
								Stats.gnutellaQueue.Enqueue(this.sockNum);
						}
					}
				}
				System.Threading.Thread.Sleep(10);
			}
		}

		/// <summary>
		/// Send the QueryRouteTable to the ultrapeer in patches.
		/// </summary>
		public void SendQRT(QueryRouteTable qrt)
		{
			while(true)
			{
				byte[] nextPacket = qrt.GetNextPacket();
				if(nextPacket.Length == 0)
					//sequence is over
					return;

				//create the final packet
				Message message = new Message(nextPacket);
				SendPacket(message);
			}
		}

		/// <summary>
		/// Return a Sck that is not in use.
		/// </summary>
		public static int GetSck()
		{
			try
			{
				for(int x = 0; x < scks.Length; x++)
				{
					if(scks[x] == null)
					{
						scks[x] = new Sck(x);
						return x;
					}
					if(scks[x].state == Condition.Closed)
						return x;
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("GetSck error");
			}
			//this should not happen
			return -1;
		}

		/// <summary>
		/// Return the remote IP address.
		/// </summary>
		public string RemoteIP()
		{
			string tempEnd = "";
			try
			{
				tempEnd = sock1.RemoteEndPoint.ToString();
			}
			catch
			{
				//System.Diagnostics.Debug.WriteLine("Sck RemoteIP");
			}
			int xPos = tempEnd.IndexOf(":");
			if(xPos == -1)
				return "";
			else
				return tempEnd.Substring(0, xPos);
		}

		/// <summary>
		/// This function is called when the handshake is complete.
		/// We can now assume we're fully connected to the gnutella node.
		/// </summary>
		public void HandshakeComplete()
		{
			connectYet.Stop();
			responseYet.Stop();
			inResponseYet1.Stop();
			inResponseYet2.Stop();
			//we're connected
			//System.Diagnostics.Debug.WriteLine("Just Connected: " + sockNum.ToString());
			GUIBridge.GJustConnected(sockNum, ultrapeer.ToString(), vendor);
			//send query routing table
			if(this.ultrapeer && !Stats.Updated.Gnutella.ultrapeer)
				QueryRouteTable.SendQRTables(sockNum);
			//send broadcast ping if we need to
			if(Stats.gnutellaHosts.Count < 50)
				if(this.ultrapeer)
					Ping.BroadcastPing(this.sockNum);
		}

		/// <summary>
		/// Accept incoming connection.
		/// </summary>
		public static void Incoming(Socket tmpSock, byte[] bytesMsg)
		{
			scks[GetSck()].ResetIncoming(tmpSock, bytesMsg);
		}

		/// <summary>
		/// Spawn outgoing connection.
		/// </summary>
		public static void Outgoing(string remoteIP)
		{
			if(!StartStop.enabled)
				return;
			if(IPfilter.Private(remoteIP))
				return;
			scks[GetSck()].Reset(remoteIP);
		}

		/// <summary>
		/// Spawn outgoing connection to a web cache server.
		/// </summary>
		public static void OutgoingWebCache(string server)
		{
			scks[GetSck()].ResetWebCache(server);
		}

		//*******************************Timers*******************************//

		//6 seconds to try and connect to node... otherwise forget the connection
		void connectYet_Tick(object sender, ElapsedEventArgs e)
		{
			connectYet.Stop();
			this.Disconnect("connectYet_Tick");
		}

		//4 seconds for a Handshake response... try older version just in case
		void responseYet_Tick(object sender, ElapsedEventArgs e)
		{
			responseYet.Stop();
			this.Disconnect("responseYet_Tick");
		}

		//7 seconds we wait for remotehost to start the Handshake
		void inResponseYet1_Tick(object sender, ElapsedEventArgs e)
		{
			inResponseYet1.Stop();
			this.Disconnect("inResponseYet1_Tick");
		}

		//7 seconds we wait for the final response to that Handshake
		void inResponseYet2_Tick(object sender, ElapsedEventArgs e)
		{
			inResponseYet2.Stop();
			this.Disconnect("inResponseYet2_Tick");
		}

		//timer checks for a disconnect because there's no other good way
		void checkConnection_Tick(object sender, ElapsedEventArgs e)
		{
			//we're not interested if this socket is closed or connecting
			if(state == Condition.Closed)
				return;
			if(state == Condition.Connecting)
				return;

			//send that handshake ping; if we can't, we're disconnected
			Ping.HandshakePing(this.sockNum);
		}

		//timer checks bandwidth
		void bandwidth_Tick(object sender, ElapsedEventArgs e)
		{
			double kbIn = Math.Round(((double)bytesIn / 1024), 2);
			string kbInVal = kbIn.ToString();
			double kbOut = Math.Round(((double)bytesOut / 1024), 2);
			string kbOutVal = kbOut.ToString();
			bytesIn = bytesOut = 0;
			if(kbOutVal == "0")
				kbOutVal = "0.00";
			if(kbInVal == "0")
				kbInVal = "0.00";
			if(kbOutVal.Length == 3)
				kbOutVal += "0";
			if(kbInVal.Length == 3)
				kbInVal += "0";
			GUIBridge.GBandwidth(this.sockNum, kbInVal, kbOutVal);
		}
	}
}
