// DownloadManager.cs
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
using System.Collections;
using System.IO;
using System.Timers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileScope
{
	/// <summary>
	/// Used in the DownloadManager's endpoints list.
	/// endOffset is the byte offset where a downloader is up to.
	/// The keys in the endpoints list are starting offsets.
	/// sockNum is the downloader index associated with those offset ranges.
	/// A sockNum of -1 means the download for the offset range is finished.
	/// </summary>
	[Serializable]
	public class EndPoint
	{
		public uint endOffset;
		public int sockNum;
	}

	/// <summary>
	/// 100 instances of this class are allowed.
	/// Each instance of this class manages ONE (1) file download.
	/// Each instance will swarm download via several Downloader objects.
	/// </summary>
	[Serializable]
	public class DownloadManager : IDeserializationCallback
	{
		//instances of DMers
		public static DownloadManager[] dms = new DownloadManager[100];
		//boundary size for swarm downloading is 1000 bytes
		public static int boundary = 1000;
		//file used for serializing the downloads
		public static string serializationPath = Utils.GetCurrentPath(Path.Combine("temp", "DLinfo"));
		//delegate for NullifyDMerAndDLer
		public delegate void nullifyDMerAndDLer(int dmNum);

		/// <summary>
		/// Setup a new DownloadManager instance for this new download.
		/// </summary>
		public static void NewDownload(QueryHitTable qht)
		{
			//find ourselves a DownloadManager not in use
			int index = GetDM();
			if(index == -1)
				return;

			Stats.Updated.downloadsNow++;
			//update gui
			GUIBridge.NewDownload(qht, index);
			dms[index].qhos = qht.queryHitObjects;
			dms[index].sha1 = qht.sha1;
			dms[index].md4sum = qht.md4sum;
			dms[index].fsfName = dms[index].dlID + Path.GetExtension(((QueryHitObject)dms[index].qhos[0]).fileName);
			//start download
			dms[index].active = true;
			dms[index].Begin();
		}

		public static bool shutflag = false;

		/// <summary>
		/// Shutdown each download down.
		/// </summary>
		public static void StopAllDownloads()
		{
			foreach(DownloadManager dm in dms)
				if(dm != null)
					if(dm.active)
					{
						shutflag = true;
						dm.ShutDownload();
					}

			/*
			 * if we never shutdown any downloaders, there's nothing to resume when we re-open FileScope
			 * in case there is a file where it shouldn't be, we'll remove it
			 * we check the path where the normal serialized downloader objects data would be
			 */
			if(!shutflag && File.Exists(serializationPath))
			{
				try
				{
					File.Delete(serializationPath);
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("StopAllDownloads delete serializationPath");
				}
			}

			//if any download was shutdown when active, we have to serialize everything
			if(shutflag)
			{
				try
				{
					FileStream fsSer = File.Create(serializationPath);
					BinaryFormatter bfer = new BinaryFormatter();
					bfer.Serialize(fsSer, dms);
					fsSer.Close();
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("StopAllDownloads: " + e.Message);
					System.Diagnostics.Debug.WriteLine(e.StackTrace);
				}
			}
		}

		/// <summary>
		/// After opening the program, we will try to recover all broken downloads.
		/// </summary>
		public static void RecoverAllDownloads()
		{
			try
			{
				if(File.Exists(serializationPath))
				{
					FileStream fsSer = new FileStream(serializationPath, FileMode.Open, FileAccess.Read);
					BinaryFormatter bfer = new BinaryFormatter();
					dms = (DownloadManager[])bfer.Deserialize(fsSer);
					fsSer.Close();
					File.Delete(serializationPath);
				}
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("RecoverAllDownloads: " + e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
		}

		/// <summary>
		/// Pushed download.
		/// </summary>
		public static void Incoming(System.Net.Sockets.Socket sockPushDL, ref string elMsg)
		{
			foreach(DownloadManager dm in dms)
				if(dm != null)
					if(dm.active)
						foreach(Downloader dlller in dm.downloaders)
						{
							if(elMsg.Substring(0, 4).ToLower() == "send")
							{
								//find match for opennap
								if(elMsg.ToLower().IndexOf(dlller.qho.ip.ToLower()) != -1)
									if(elMsg.ToLower().IndexOf(dlller.qho.fileName.ToLower()) != -1)
										if(elMsg.IndexOf(dlller.qho.fileSize.ToString()) != -1)
										{
											dlller.ResetIncoming(sockPushDL);
											return;
										}
							}
							else if(elMsg.Substring(0, 4).ToLower() == "push")
							{
Utils.Diag("ok g2 push check stuff:");
Utils.Diag(Utils.HexGuid(dlller.qho.servIdent));
Utils.Diag(elMsg);
								//find match for gnutella 2
								if(elMsg.ToLower().IndexOf(Utils.HexGuid(dlller.qho.servIdent).ToLower()) != -1)
								{
Utils.Diag("g2 push succeeded");
									if(dlller.qho.servIdent != null)
										dlller.ResetIncoming(sockPushDL);
									return;
								}
							}
							else
							{
								//find match for gnutella
								if(elMsg.ToLower().IndexOf(dlller.qho.guid.ToLower()) != -1)
								{
									dlller.ResetIncoming(sockPushDL);
									return;
								}
							}
						}
			System.Diagnostics.Debug.WriteLine("DownloadManager: no pushed servent found: " + elMsg);
			try
			{
				if(sockPushDL != null)
				{
					if(sockPushDL.Connected)
						sockPushDL.Shutdown(System.Net.Sockets.SocketShutdown.Both);
					sockPushDL.Close();
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("DMer Incoming");
			}
		}

		/// <summary>
		/// Get a free DownloadManager.
		/// </summary>
		public static int GetDM()
		{
			try
			{
				for(int x = 0; x < dms.Length; x++)
					if(dms[x] == null)
					{
						dms[x] = new DownloadManager(x);
						return x;
					}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("GetDM error");
			}
			return -1;
		}

		/// <summary>
		/// Set the DownloadManager to null.
		/// Set its Downloaders to null.
		/// Indirectly take out references and stop timers.
		/// </summary>
		public static void NullifyDMerAndDLer(int dmNumber)
		{
			if(dms[dmNumber] != null)
			{
				dms[dmNumber].NullifyMeAndDLers();
				dms[dmNumber] = null;
				GC.Collect();
			}
		}

		//similar query hits from nodes that have this file (used only temporarily)
		public ArrayList qhos;
		//downloader objects for all these nodes
		public ArrayList downloaders = new ArrayList();
		//whether this instance is active or not
		public bool active = false;
		//file we're writing to
		[NonSerialized]
		public FileStream fs = null;
		public string fsfName = "";
		//store the index of this instance
		public int dmNum;
		//unique id for this download
		public string dlID;
		public string sha1;		//urn:sha1:**** format
		public string md4sum;	//base32 format
		/*
		 * this SortedList keeps track of all byte offsets for current Downloader objects
		 * it'll allow us to see when the download is finished
		 * it'll allow us to give a waiting downloader a good offset to start downloading at
		 */
		public SortedList endpoints = new SortedList();
		//timer object allows us to update the gui at a good interval with good stats; also handles re-searching
		[NonSerialized]
		public GoodTimer tmrUpdate = new GoodTimer();
		//hold the number of bytes received since the last timer event
		public int bytesRec = 0;

		//countdown to re-search; a max of 100 mins (6000 sec)
		int countdown = 6000;
		const int countdownOrig = 6000;
		//actual re-search intervals per network:
		const int countdownG1 = 1800;			//gnutella 30 mins
		const int countdownG2 = 25;				//gnutella2 25 secs
		const int countdownOpenNap = 900;		//opennap 15 mins
		const int countdownEDonkey = 900;		//eDonkey 15 mins

		public DownloadManager(int dmNum)
		{
			this.dmNum = dmNum;
			dlID = Base32.Encode(GUID.newGUID(), 0, 16);
		}

		public virtual void OnDeserialization(object dmer)
		{
			if(!active)
				return;
			//create our temp directory if it doesn't exist
			Directory.CreateDirectory(Utils.GetCurrentPath("temp"));
			this.fs = new FileStream(Utils.GetCurrentPath(Path.Combine("temp", this.fsfName)), FileMode.OpenOrCreate, FileAccess.Write);
			//the NonSerialized stuff
			tmrUpdate = new GoodTimer();
			tmrUpdate.Interval = 1000;
			tmrUpdate.AddEvent(new ElapsedEventHandler(tmrUpdate_Tick));
			Stats.Updated.downloadsNow++;
			tmrUpdate.Start();
			respawnDLers = true;
		}

		bool respawnDLers = false;

		/// <summary>
		/// Begin downloading.
		/// </summary>
		public void Begin()
		{
			//create our temp directory if it doesn't exist
			Directory.CreateDirectory(Utils.GetCurrentPath("temp"));
			this.fs = new FileStream(Utils.GetCurrentPath(Path.Combine("temp", this.fsfName)), FileMode.OpenOrCreate, FileAccess.Write);
			//start update timer
			tmrUpdate.Interval = 1000;
			tmrUpdate.AddEvent(new ElapsedEventHandler(tmrUpdate_Tick));
			tmrUpdate.Start();
			//instantiate each downloader
			for(int x = 0; x < qhos.Count; x++)
			{
				//each downloader will have it's own copy of a QueryHitObject
				QueryHitObject qho = (QueryHitObject)qhos[x];
				//filename can't be too big because the whole path can't be greater than 260
				if(qho.fileName.Length > 150)
				{
					string ext = Path.GetExtension(qho.fileName);
					qho.fileName = qho.fileName.Substring(0, 140);
					if(ext.Length > 0)
						qho.fileName += ext;
				}
				//take out some potentially illegal characters
				qho.fileName = qho.fileName.Replace(@"\", "-");
				qho.fileName = qho.fileName.Replace(@"/", "-");
				Downloader elDLer = new Downloader(qho, x, dmNum, qhos.Count);
				downloaders.Add(elDLer);
			}
			//requery immediately on eDonkey
			if(((Downloader)downloaders[0]).qho.networkType == NetworkType.EDonkey)
				countdown -= (countdownEDonkey - 3);
			this.qhos = null;
		}

		/// <summary>
		/// A downloader has disconnected.
		/// </summary>
		public void DLerDone(int dlNum)
		{
			lock(endpoints)
			{
				if(!active)
					return;
				Downloader elDLer = (Downloader)downloaders[dlNum];
				if(elDLer.curOffset < boundary && (elDLer.curOffset + elDLer.startOffset) < elDLer.qho.fileSize)
				{
					//remove entry from endpoints
					if(elDLer.startOffset == 0)
					{
						//we can't remove this item
						EndPoint ep = new EndPoint();
						ep.sockNum = -1;
						ep.endOffset = 0;
						endpoints[(uint)0] = ep;
					}
					else
						endpoints.Remove(elDLer.startOffset);
				}
				else
				{
					//so we know this chunk is done
					((EndPoint)endpoints[elDLer.startOffset]).sockNum = -1;
				}
			}
		}

		/// <summary>
		/// This function is called when a Downloader instance connects to a host.
		/// It will find an offset to start downloading from.
		/// </summary>
		public void AvailableDownloader(int dlNum)
		{
			lock(endpoints)
			{
				if(shutflag || !active)
					return;
				try
				{
					Downloader elDLer = (Downloader)downloaders[dlNum];
					if(elDLer.state != DLState.Connected)
						return;
					Downloader elDLer0 = (Downloader)downloaders[0];
					//check if we're just starting this download
					if(endpoints.Count < 2)
					{
						endpoints.Clear();

						//create lower bound entry
						EndPoint e1 = new EndPoint();
						e1.endOffset = 0;
						e1.sockNum = dlNum;
						endpoints.Add(e1.endOffset, e1);

						//create upper bound entry
						EndPoint e2 = new EndPoint();
						e2.endOffset = elDLer0.qho.fileSize;
						e2.sockNum = -1;
						endpoints.Add(e2.endOffset, e2);

						//offsets
						uint startOffset = 0;
						uint endOffset = 0;

						//start transfer
						elDLer.StartTransfer(startOffset, endOffset);
					}
					else
					{
						//non-swarm networks
						if(elDLer.qho.networkType == NetworkType.OpenNap)
						{
							EndPoint e = (EndPoint)endpoints.GetByIndex(0);
							if(e.sockNum == -1)
								elDLer.StartTransfer(e.endOffset, elDLer0.qho.fileSize);
							else
								elDLer.Disconnect("opennap limits to one host");
							return;
						}

						int lowerIndex = 0;
						uint highestDiff = 0;
						//find the biggest difference between two download offsets
						for(int x = 1; x < endpoints.Count; x++)
						{
							EndPoint e = (EndPoint)endpoints.GetByIndex(x-1);
							uint lower = e.endOffset;
							uint higher = (uint)endpoints.GetKey(x);
							//be careful, we're dealing with uint here
							if(higher - lower > highestDiff && higher > lower)
							{
								highestDiff = higher - lower;
								lowerIndex = x-1;
							}
						}
						if(highestDiff == 0)
							return;

						//ok we know there is a difference between two offset points since we got this far
						EndPoint ep = (EndPoint)endpoints.GetByIndex(lowerIndex);
						uint lowOffset = ep.endOffset;
						uint highOffset = (uint)endpoints.GetKey(lowerIndex+1);
						uint finalOffset;

						//oh yeah
						finalOffset = lowOffset + (uint)(Math.Floor((double)((((double)highOffset - (double)lowOffset) / 2) / (double)boundary)) * (double)boundary);

						//check this first; if the difference is too small, we start from the lowOffset
						if(ep.sockNum == -1 && (highOffset - lowOffset) < (300 * boundary))
							finalOffset = lowOffset;
						if((highOffset - lowOffset) < (100 * boundary))
							finalOffset = lowOffset;

						//if the first chunk isn't downloading, we start it... for previewing sake
						EndPoint e44 = (EndPoint)endpoints.GetByIndex(0);
						//has to be inactive
						if(e44.sockNum == -1)
							//there has to be a space before the next offset
							if(e44.endOffset < (uint)endpoints.GetKey(1))
							{
								finalOffset = e44.endOffset;
								highOffset = (uint)endpoints.GetKey(1);
							}

						//make sure no current transfer is working on this offset
						if(endpoints.ContainsKey(finalOffset))
						{
							EndPoint e9 = (EndPoint)endpoints[finalOffset];
							//if there is a transfer
							if(e9.sockNum != -1)
							{
								if(((Downloader)this.downloaders[e9.sockNum]).state == DLState.Downloading)
								{
									elDLer.lastMessage = "Server Busy";
									elDLer.Disconnect("offset already owned");
									return;
								}
								System.Diagnostics.Debug.WriteLine("A host's sockNum should have been -1");
							}
							//so we'll just start downloading from the same spot, from a different host
							EndPoint e5 = new EndPoint();
							e5.endOffset = finalOffset;
							e5.sockNum = dlNum;
							endpoints[finalOffset] = e5;
							elDLer.StartTransfer(finalOffset, highOffset);
						}
						else
						{
							//add this to our endpoints list as it's not already on there
							EndPoint e4 = new EndPoint();
							e4.endOffset = finalOffset;
							e4.sockNum = dlNum;
							endpoints.Add(finalOffset, e4);
							elDLer.StartTransfer(finalOffset, highOffset);
						}
					}
				}
				catch(Exception exc)
				{
					System.Diagnostics.Debug.WriteLine("AvailableDownloader: " + exc.Message);
					System.Diagnostics.Debug.WriteLine(exc.Source);
					System.Diagnostics.Debug.WriteLine(exc.StackTrace);
				}
			}
		}

		/// <summary>
		/// Write the data we received into a file.
		/// </summary>
		public void DLFlushData(uint offset, byte[] data, int start, int len, int dlNum, ref uint curOffset)
		{
			lock(endpoints)
			{
				if(!this.active || shutflag)
					return;

				((Downloader)downloaders[dlNum]).dlflushed = true;
				if(len > 0)
				{
					fs.Seek(offset, SeekOrigin.Begin);
					fs.Write(data, start, len);
					curOffset += (uint)len;
				}
				UpdateOffsetsAndCheck(dlNum);
			}
		}

		/// <summary>
		/// We're going to update the values in 'endpoints' SortedList.
		/// Then check to see if the download is done.
		/// </summary>
		public void UpdateOffsetsAndCheck(int dlNum)
		{
			lock(endpoints)
			{
				if(!this.active || shutflag)
					return;
				try
				{
					Downloader elDLer = (Downloader)downloaders[dlNum];
					int elIndex = endpoints.IndexOfKey(elDLer.startOffset);
					//update the offset value
					EndPoint e = (EndPoint)endpoints.GetByIndex(elIndex);

					//the last chunk is the only one allowed less than the boundary
					if((elDLer.curOffset + elDLer.startOffset) < elDLer.qho.fileSize)
						e.endOffset = (uint)(Math.Floor((double)((double)(elDLer.curOffset+elDLer.startOffset) / (double)boundary)) * (double)boundary);
					else
						e.endOffset = elDLer.curOffset+elDLer.startOffset;

					//check to see if the chunk download is done; we know it is when it overlaps into the next chunk
					if(e.endOffset >= (uint)endpoints.GetKey(elIndex+1))
					{
						//since this download just finished; we'll check if all are done
						uint highestDiff = 0;
						//find the biggest difference between two download offsets
						for(int x = 1; x < endpoints.Count; x++)
						{
							EndPoint ep = (EndPoint)endpoints.GetByIndex(x-1);
							uint lower = ep.endOffset;
							uint higher = (uint)endpoints.GetKey(x);
							//careful with the uint values
							if(higher - lower > highestDiff && higher > lower)
								highestDiff = higher - lower;
						}
						//moment of truth
						if(highestDiff == 0)
							DownloadDone();
						else
						{
							elDLer.quickReconnect = true;
							elDLer.Disconnect("chunk done");
						}
					}
				}
				catch(Exception exc)
				{
					System.Diagnostics.Debug.WriteLine("UpdateOffsetsAndCheck: " + exc.Message);
					System.Diagnostics.Debug.WriteLine(exc.Source);
					System.Diagnostics.Debug.WriteLine(exc.StackTrace);
				}
				return;
			}
		}

		/// <summary>
		/// Simple way to find out how many active connections we already have.
		/// </summary>
		public int GetActiveCount()
		{
			int activeCount = 0;
			foreach(Downloader dler in downloaders)
				if(dler.state == DLState.Downloading)
					activeCount++;
			return activeCount;
		}

		//keep count of how many current DownloadDone()s or CancelDownload()s are running
		public static int countFinishes = 0;

		/// <summary>
		/// Run this function whenever a download is finished.
		/// </summary>
		void DownloadDone()
		{
			lock(endpoints)
			{
				if(!active)
					return;

				countFinishes++;
				active = false;
				try
				{
					if(!Directory.Exists(Stats.settings.dlDirectory))
					{
						Directory.CreateDirectory(Utils.GetCurrentPath("downloads"));
						Stats.settings.dlDirectory = Utils.GetCurrentPath("downloads");
					}

					//make sure all files are closed and no downloads are active
					for(int x = 0; x < downloaders.Count; x++)
					{
						Downloader elDLerX = (Downloader)downloaders[x];
						elDLerX.Disconnect("cleaning up");
						elDLerX.state = DLState.CouldNotConnect;
					}

					this.fs.Close();
					//make the final file
					string path = Utils.GetCurrentPath(Path.Combine("temp", this.fsfName));
					string finalPath = GetFile("");
					File.Move(path, finalPath);
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("DownloadDone: " + e.Message + "\n" + e.StackTrace);
				}
				//update download count
				Stats.Updated.downloadsNow--;
				//update gui
				GUIBridge.RemoveDownload(dmNum);
				//clean up
				countFinishes--;
				StartApp.main.BeginInvoke(new nullifyDMerAndDLer(DownloadManager.NullifyDMerAndDLer), new object[] {dmNum});
			}
		}

		/// <summary>
		/// Runs whenever a download is cancelled.
		/// </summary>
		public void CancelDownload(bool deleteAfter)
		{
			lock(endpoints)
			{
				if(!active)
					return;
				Downloader elDLer = (Downloader)downloaders[0];
				active = false;
				countFinishes++;
				try
				{
					if(!Directory.Exists(Stats.settings.dlDirectory))
					{
						Directory.CreateDirectory(Utils.GetCurrentPath("downloads"));
						Stats.settings.dlDirectory = Utils.GetCurrentPath("downloads");
					}
					Directory.CreateDirectory(Path.Combine(Stats.settings.dlDirectory, "incomplete"));

					//make sure all files are closed and no downloads are active
					for(int x = 0; x < downloaders.Count; x++)
					{
						Downloader elDLerX = (Downloader)downloaders[x];
						elDLerX.Disconnect("cleaning up");
						elDLerX.state = DLState.CouldNotConnect;
					}

					this.fs.Close();
					//make the final file
					string path = Utils.GetCurrentPath(Path.Combine("temp", this.fsfName));
					string finalPath = GetFile("incomplete");

					if(File.Exists(path))
					{
						if(deleteAfter)
							File.Delete(path);
						else
						{
							//move what we can
							uint finalOffset = 0;
							if(endpoints.Count > 1)
								for(int x = 1; x < endpoints.Count; x++)
								{
									EndPoint ep = (EndPoint)endpoints.GetByIndex(x-1);
									uint lower = ep.endOffset;
									uint higher = (uint)endpoints.GetKey(x);
									if(higher > lower)
									{
										finalOffset = lower;
										break;
									}
								}
							if(finalOffset != 0)
							{
								FileStream fsOut = new FileStream(finalPath, FileMode.OpenOrCreate, FileAccess.Write);
								FileStream fsIn = new FileStream(path, FileMode.Open, FileAccess.Read);
								uint bytes_to_read = finalOffset;
								byte[] buf = new byte[32768];
								while(bytes_to_read > 0)
								{
									if(bytes_to_read > buf.Length)
									{
										fsIn.Read(buf, 0, buf.Length);
										fsOut.Write(buf, 0, buf.Length);
										bytes_to_read -= (uint)buf.Length;
									}
									else   
									{
										fsIn.Read(buf, 0, (int)bytes_to_read);
										fsOut.Write(buf, 0, (int)bytes_to_read);
										bytes_to_read = 0;
									}
								}
								fsOut.Close();
								fsIn.Close();
							}
							File.Delete(path);
						}
					}
				}
				catch(Exception exce)
				{
					System.Diagnostics.Debug.WriteLine("CancelDownload: " + exce.Message + "\n" + exce.StackTrace);
				}
				//update download count
				Stats.Updated.downloadsNow--;
				//clean up
				elDLer = null;
				countFinishes--;
				StartApp.main.BeginInvoke(new nullifyDMerAndDLer(DownloadManager.NullifyDMerAndDLer), new object[] {dmNum});
			}
		}

		/// <summary>
		/// We're closing the program... so we'll just disconnect each download.
		/// Each downloadmanager and downloader object will be serialized.
		/// They will all deserialize and resume when we re-open the program.
		/// </summary>
		public void ShutDownload()
		{
			foreach(Downloader dlllller in this.downloaders)
				dlllller.Disconnect("ShutDownload");
			this.fs.Close();
		}

		/// <summary>
		/// Loop through all downloader objects and reconnect on those not downloading.
		/// </summary>
		public void RetryAll()
		{
			//we set a countdown till each reconnect
			foreach(Downloader dde in downloaders)
				dde.count = GUID.rand.Next(1, 4);
		}

		public string GetFile(string overdir)
		{
			string eldir;
			if(Directory.Exists(Stats.settings.dlDirectory))
				eldir = Stats.settings.dlDirectory;
			else
				eldir = Utils.GetCurrentPath("downloads");
			if(overdir != "")
				eldir = Path.Combine(eldir, overdir);

			Downloader elDLer = (Downloader)downloaders[0];
			//we basically loop until we find something
			int count = 0;
			while(true)
			{
				string fileName = "";
				if(count != 0)
					fileName += "(" + count.ToString() + ") ";
				fileName += elDLer.qho.fileName;
				string path = Path.Combine(eldir, fileName);
				if(File.Exists(path))
				{
					count++;
					continue;
				}
				else
					return path;
			}
		}

		void tmrUpdate_Tick(object sender, ElapsedEventArgs e)
		{
			Downloader elDLer = (Downloader)downloaders[0];

			//deal with a possible respawn after deserialization
			if(respawnDLers)
			{
				try
				{
					respawnDLers = false;
					//start downloads
					int reconint;
					if(downloaders.Count < 2)
						reconint = 600;
					else if(downloaders.Count > 800)
						reconint = 600000;
					else if(downloaders.Count > 400)
						reconint = 220000;
					else if(downloaders.Count > 200)
						reconint = 80000;
					else if(downloaders.Count > 80)
						reconint = 35000;
					else if(downloaders.Count > 40)
						reconint = 15000;
					else
						reconint = 4000;
					QueryHitTable tempqht = new QueryHitTable();
					while(true)
					{
						System.Threading.Thread.Sleep(1000);
						if(Stats.Updated.opened)
							break;
					}
					foreach(Downloader dler in downloaders)
					{
						dler.reConnect.Interval = GUID.rand.Next(500, reconint);
						tempqht.queryHitObjects.Add(dler.qho);
					}
					//update gui
					GUIBridge.NewDownload(tempqht, this.dmNum);
				}
				catch(Exception emsg)
				{
					System.Diagnostics.Debug.WriteLine("respawn dl error: " + emsg.Message);
					System.Diagnostics.Debug.WriteLine(emsg.StackTrace);
				}
			}

			//deal with the re-searching business first
			countdown--;
			int timePassed = countdownOrig - countdown;
			int timetogo = 0;
			if(elDLer.qho.networkType == NetworkType.OpenNap)
			{
				if(timePassed >= countdownOpenNap)
				{
					countdown = countdownOrig;
					if(elDLer.state == DLState.CouldNotConnect || elDLer.state == DLState.Waiting)
						ReQuery.OpenNapRequery(elDLer.qho.fileName);
				}
				else
					timetogo = countdownOpenNap - timePassed;
			}
			else if(elDLer.qho.networkType == NetworkType.EDonkey)
			{
				//requery technique 1
				if(timePassed >= countdownEDonkey)
				{
					countdown = countdownOrig;
					if(this.active)
					{
						ReQuery.EDonkeyRequery(elDLer.qho.md4sum);
						System.Threading.Thread.Sleep(300);
						ReQuery.EDonkeyRequeryGlobalUdp(elDLer.qho.md4sum);
					}
				}
				else
					timetogo = countdownEDonkey - timePassed;

				//requery technique 2
				int freq = 2;
				if(Stats.settings.connectionType == EnumConnectionType.dialUp)
					freq = 4;
				int rndres = GUID.rand.Next(0, freq);
				if(rndres == 1 && this.active && downloaders.Count < 200)
					ReQuery.EDonkeyRequeryGlobalUdp(elDLer.qho.md4sum);
			}
			else if(elDLer.qho.networkType == NetworkType.Gnutella1)
			{
				if(timePassed >= countdownG1)
				{
					countdown = countdownOrig;
					if(this.active)
						ReQuery.GnutellaRequery(elDLer.qho.fileName, this.sha1);
				}
				else
					timetogo = countdownG1 - timePassed;
			}
			else if(elDLer.qho.networkType == NetworkType.Gnutella2)
			{
				if(timePassed >= countdownG2)
				{
					countdown = countdownOrig;
					if(this.active)
						ReQuery.Gnutella2Requery(elDLer.qho);
				}
				else
					timetogo = countdownG2 - timePassed;
			}
			string re_search_status;
			if(timetogo <= 0)
			{
				if(elDLer.qho.networkType == NetworkType.Gnutella2)
					re_search_status = "sent to a hub";
				else
					re_search_status = "sent";
			}
			else
				re_search_status = "in " + Utils.TimeFormatFromSeconds(timetogo);

			//now handle the updating
			double curPerc;
			uint allDiffs = 0;
			lock(endpoints)
			{
				if(endpoints.Count > 1)
				{
					//accumulated diffs
					for(int x = 1; x < endpoints.Count; x++)
					{
						EndPoint ep = (EndPoint)endpoints.GetByIndex(x-1);
						uint lower = ep.endOffset;
						uint higher = (uint)endpoints.GetKey(x);
						if(higher > lower)
							allDiffs += (higher - lower);
					}
					uint fileSize = elDLer.qho.fileSize;
					uint fileDone = fileSize - allDiffs;
					curPerc = Math.Round(((double)fileDone / (double)fileSize * 100), 2);
				}
				else if(elDLer.qho.networkType == NetworkType.OpenNap)
				{
					curPerc = Math.Round(((double)elDLer.curOffset / (double)elDLer.qho.fileSize * 100), 2);
				}
				else
					curPerc = 0;
			}

			//start with speed
			double kbRec = (double)bytesRec / (double)1024;
			double spEEd = Math.Round(kbRec, 2);
			string speed = spEEd.ToString() + " KB/s";
			bytesRec = 0;

			//percent
			string percent = curPerc.ToString() + "%";

			//status
			int dlCount = 0;
			string status = "";
			foreach(Downloader dr in downloaders)
			{
				if(dr.state == DLState.Downloading)
					dlCount++;
			}
			if(dlCount > 0)
				status = "Downloading from " + dlCount.ToString() + " hosts";
			else
				status = "Connecting(" + downloaders.Count.ToString() + ") Re-Search " + re_search_status;

			//marshal the stuff to the UI thread
			GUIBridge.UpdateDownload(ref status, ref percent, ref speed, dmNum);
		}

		public void NullifyMeAndDLers()
		{
			try
			{
				tmrUpdate.Stop();
				tmrUpdate.RemoveEvent(new ElapsedEventHandler(tmrUpdate_Tick));
				tmrUpdate.Close();
				tmrUpdate = null;
				this.fs = null;
				this.endpoints.Clear();
				for(int dlrNum = downloaders.Count-1; dlrNum >= 0; dlrNum--)
				{
					Downloader dlr = (Downloader)downloaders[dlrNum];
					if(dlr != null)
					{
						dlr.NullifyMe();
						downloaders[dlrNum] = null;
						dlr = null;
					}
				}
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("NullifyMeAndDLers: " + e.Message);
			}
		}
	}
}
