// Messages.cs
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
using System.IO;
using System.Text;

namespace FileScope.EDonkey
{
	/// <summary>
	/// This class has any reusable static routines for dealing with messages.
	/// </summary>
	public class Routines
	{
		public static void ClientInfo(MemoryStream ms)
		{
			//client hash
			ms.Write(Stats.settings.myGUID, 0, 16);
			//client id
			ms.Write(Endian.GetBytes(Sck.clientID, false), 0, 4);
			//port
			ms.Write(Endian.GetBytes((ushort)Stats.settings.port, false), 0, 2);
			//meta tag list
			ms.Write(Endian.GetBytes((uint)3, false), 0, 4);
			//meta 1 (nickname)
			ms.WriteByte(0x02);
			ms.Write(Endian.GetBytes((ushort)1, false), 0, 2);
			ms.WriteByte(0x01);
			byte[] bytesNick = Encoding.ASCII.GetBytes(Sck.nick);
			ms.Write(Endian.GetBytes((ushort)bytesNick.Length, false), 0, 2);
			ms.Write(bytesNick, 0, bytesNick.Length);
			//meta 2 (version)
			ms.WriteByte(0x03);
			ms.Write(Endian.GetBytes((ushort)1, false), 0, 2);
			ms.WriteByte(0x11);
			ms.Write(Endian.GetBytes((uint)0x3c, false), 0, 4);
			//meta 3 (port)
			ms.WriteByte(0x03);
			ms.Write(Endian.GetBytes((ushort)1, false), 0, 2);
			ms.WriteByte(0x0F);
			ms.Write(Endian.GetBytes((uint)Stats.settings.port, false), 0, 4);
		}

		public static void FileInfoList(MemoryStream ms)
		{
			int fileCount = Stats.fileList.Count;
			ms.Write(Endian.GetBytes(fileCount, false), 0, 4);
			for(int x = 0; x < fileCount; x++)
			{
				FileObject elFile = (FileObject)Stats.fileList[x];
				//file hash
				ms.Write(elFile.md4, 0, 16);
				//client id
				ms.Write(Endian.GetBytes(Sck.clientID, false), 0, 4);
				//port
				ms.Write(Endian.GetBytes((ushort)Stats.settings.port, false), 0, 2);
				//meta stuff
				ms.Write(Endian.GetBytes((uint)4, false), 0, 4);
				string elFname = Path.GetFileName(elFile.location);
				//meta 1
				ms.WriteByte(0x02);
				ms.Write(Endian.GetBytes((ushort)1, false), 0, 2);
				ms.WriteByte(0x01);
				byte[] bytesName = Encoding.ASCII.GetBytes(elFname);
				ms.Write(Endian.GetBytes((ushort)bytesName.Length, false), 0, 2);
				ms.Write(bytesName, 0, bytesName.Length);
				//meta 2
				ms.WriteByte(0x02);
				ms.Write(Endian.GetBytes((ushort)1, false), 0, 2);
				ms.WriteByte(0x03);
				byte[] bytesType = Encoding.ASCII.GetBytes(QHOStuff.GetType(elFname));
				ms.Write(Endian.GetBytes((ushort)bytesType.Length, false), 0, 2);
				ms.Write(bytesType, 0, bytesType.Length);
				//meta 3
				ms.WriteByte(0x02);
				ms.Write(Endian.GetBytes((ushort)1, false), 0, 2);
				ms.WriteByte(0x04);
				byte[] bytesFormat = Encoding.ASCII.GetBytes(QHOStuff.GetFormat(elFname));
				ms.Write(Endian.GetBytes((ushort)bytesFormat.Length, false), 0, 2);
				ms.Write(bytesFormat, 0, bytesFormat.Length);
				//meta 4
				ms.WriteByte(0x03);
				ms.Write(Endian.GetBytes((ushort)1, false), 0, 2);
				ms.WriteByte(0x02);
				ms.Write(Endian.GetBytes(elFile.b, false), 0, 4);
			}
		}
	}

	/// <summary>
	/// This class contains all static functions for handling messages.
	/// </summary>
	public class Messages
	{
		//keep track of the last FileObject we deal with, as most following messages deal with the same one
		public static FileObject lastRequestedFO;

		/// <summary>
		/// Incoming id change message.
		/// </summary>
		public static void IDChange(Packet pckt, int sockNum)
		{
			uint newid = Endian.ToUInt32(pckt.payload, 1, false);
			Sck.clientID = newid;
			GUIBridge.EMessage(sockNum, "ID: " + newid.ToString());
		}

		/// <summary>
		/// Incoming server string-based message.
		/// </summary>
		public static void ServerMessage(Packet pckt, int sockNum)
		{
			ushort len = Endian.ToUInt16(pckt.payload, 1, false);
			GUIBridge.EMessage(sockNum, Encoding.ASCII.GetString(pckt.payload, 3, len));
		}

		/// <summary>
		/// Incoming list of other eDonkey servers.
		/// </summary>
		public static void ServerList(Packet pckt, int sockNum)
		{
			int numServers = pckt.payload[1];
			for(int x = 0; x < numServers; x++)
			{
				int offset = 2 + (x * 6);
				string server = Endian.BigEndianIP(pckt.payload, offset) + ":" + Endian.ToUInt16(pckt.payload, offset+4, false).ToString();
				if(!IPfilter.Private(server))
					GUIBridge.ENewServer(ref server);
			}
		}

		/// <summary>
		/// Incoming info about the server.
		/// </summary>
		public static void ServerInfoData(Packet pckt, int sockNum)
		{
			uint metaCount = Endian.ToUInt32(pckt.payload, 23, false);
			int offset = 27;
			string name = "";
			string desc = "";
			for(int x = 0; x < metaCount; x++)
			{
				if(pckt.payload[offset] == 0x02)
				{
					offset++;
					ushort len = Endian.ToUInt16(pckt.payload, offset, false);
					offset += 2;
					if(len == 1)
					{
						byte specialTag = pckt.payload[offset];
						offset++;
						ushort strLen = Endian.ToUInt16(pckt.payload, offset, false);
						offset += 2;
						switch(specialTag)
						{
							case 0x01:
								name = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
								offset += strLen;
								break;
							case 0x0B:
								desc = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
								offset += strLen;
								break;
							default:
								System.Diagnostics.Debug.WriteLine("ServerInfoData unexpected 3: " + specialTag.ToString());
								return;
						}
					}
					else
					{
						System.Diagnostics.Debug.WriteLine("ServerInfoData unexpected 2");
						return;
					}
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("ServerInfoData unexpected 1 " + pckt.payload[offset].ToString());
					return;
				}
			}
			//update gui
			if(name.Length > 0 && desc.Length > 0)
			{
				string server = Sck.scks[sockNum].address + ":" + Sck.scks[sockNum].port.ToString();
				Stats.EDonkeyServerInfo edsi = (Stats.EDonkeyServerInfo)Stats.edonkeyHosts[server];
				edsi.servName = name;
				edsi.servDesc = desc;
				GUIBridge.EUpdateStats(sockNum);
			}
		}

		/// <summary>
		/// Incoming info on the number of files and users available on the server.
		/// </summary>
		public static void ServerStatus(Packet pckt, int sockNum)
		{
			uint users = Endian.ToUInt32(pckt.payload, 1, false);
			uint files = Endian.ToUInt32(pckt.payload, 5, false);
			//make the update
			string server = Sck.scks[sockNum].address + ":" + Sck.scks[sockNum].port.ToString();
			Stats.EDonkeyServerInfo edsi = (Stats.EDonkeyServerInfo)Stats.edonkeyHosts[server];
			edsi.curUsers = users;
			edsi.curFiles = files;
			//inform gui
			GUIBridge.EUpdateStats(sockNum);
		}

		/// <summary>
		/// Incoming search results for the file we searched for.
		/// </summary>
		public static void SearchResults(Packet pckt, int sockNum)
		{
			int offset = 1;
			uint count = Endian.ToUInt32(pckt.payload, offset, false);
			offset += 4;
			for(uint x = 0; x < count; x++)
			{
				byte[] bytesHash = new byte[16];
				//uint clientID = 0;
				ushort port = 0;
				string name = "";
				uint size = 0;
				uint sources = 0;
				string type = "";
				uint lastseencomplete = 0;
				uint priority = 0;
				uint bitrate = 0;
				uint ulpriority = 0;
				string format = "";
				string codec = "";
				string length = "";
				string artist = "";
				string album = "";
				string title = "";

				Array.Copy(pckt.payload, offset, bytesHash, 0, 16);
				offset += 16;
				//clientID = Endian.ToUInt32(pckt.payload, offset, false);
				int clientIDoffset = offset;
				offset += 4;
				port = Endian.ToUInt16(pckt.payload, offset, false);
				offset += 2;
				uint metaCount = Endian.ToUInt32(pckt.payload, offset, false);
				offset += 4;
				for(uint y = 0; y < metaCount; y++)
				{
					if(pckt.payload[offset] == 0x02)
					{
						offset++;
						ushort len = Endian.ToUInt16(pckt.payload, offset, false);
						offset += 2;
						if(len == 1)
						{
							byte specialTag = pckt.payload[offset];
							offset++;
							ushort strLen = Endian.ToUInt16(pckt.payload, offset, false);
							offset += 2;
							switch(specialTag)
							{
								case 0x01:
									name = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
									offset += strLen;
									//System.Diagnostics.Debug.WriteLine("name: " + name.ToString());
									break;
								case 0x03:
									type = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
									offset += strLen;
									//System.Diagnostics.Debug.WriteLine("type: " + type.ToString());
									break;
								case 0x04:
									format = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
									offset += strLen;
									//System.Diagnostics.Debug.WriteLine("format: " + format.ToString());
									break;
								default:
									System.Diagnostics.Debug.WriteLine("SearchResults unexpected 3: " + specialTag.ToString());
									offset += strLen;
									break;
							}
						}
						else
						{
							string stuff = Encoding.ASCII.GetString(pckt.payload, offset, len);
							offset += len;
							ushort strLen = Endian.ToUInt16(pckt.payload, offset, false);
							offset += 2;
							if(stuff.ToLower().IndexOf("codec") != -1)
							{
								codec = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
								offset += strLen;
								//System.Diagnostics.Debug.WriteLine("codec: " + codec);
							}
							else if(stuff.ToLower().IndexOf("length") != -1)
							{
								length = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
								offset += strLen;
								//System.Diagnostics.Debug.WriteLine("length: " + length);
							}
							else if(stuff.ToLower().IndexOf("artist") != -1)
							{
								artist = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
								offset += strLen;
								//System.Diagnostics.Debug.WriteLine("artist: " + artist);
							}
							else if(stuff.ToLower().IndexOf("album") != -1)
							{
								album = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
								offset += strLen;
								//System.Diagnostics.Debug.WriteLine("album: " + album);
							}
							else if(stuff.ToLower().IndexOf("title") != -1)
							{
								title = Encoding.ASCII.GetString(pckt.payload, offset, strLen);
								offset += strLen;
								//System.Diagnostics.Debug.WriteLine("title: " + title);
							}
							else
							{
								System.Diagnostics.Debug.WriteLine("(harmless) SearchResults unknown metaname1: " + stuff);
								offset += strLen;
							}
						}
					}
					else if(pckt.payload[offset] == 0x03)
					{
						offset++;
						ushort len = Endian.ToUInt16(pckt.payload, offset, false);
						offset += 2;
						if(len == 1)
						{
							byte specialTag = pckt.payload[offset];
							offset++;
							switch(specialTag)
							{
								case 0x02:
									size = Endian.ToUInt32(pckt.payload, offset, false);
									offset += 4;
									//System.Diagnostics.Debug.WriteLine("size: " + size.ToString());
									break;
								case 0x05:
									lastseencomplete = Endian.ToUInt32(pckt.payload, offset, false);
									offset += 4;
									//System.Diagnostics.Debug.WriteLine("lastseencomplete: " + lastseencomplete.ToString());
									break;
								case 0x13:
									priority = Endian.ToUInt32(pckt.payload, offset, false);
									offset += 4;
									//System.Diagnostics.Debug.WriteLine("priority: " + priority.ToString());
									break;
								case 0x15:
									sources = Endian.ToUInt32(pckt.payload, offset, false);
									offset += 4;
									//System.Diagnostics.Debug.WriteLine("sources: " + sources.ToString());
									break;
								case 0x17:
									ulpriority = Endian.ToUInt32(pckt.payload, offset, false);
									offset += 4;
									//System.Diagnostics.Debug.WriteLine("ulpriority: " + ulpriority.ToString());
									break;
								default:
									System.Diagnostics.Debug.WriteLine("(harmless) SearchResults unexpected 5: " + specialTag.ToString());
									offset += 4;
									break;
							}
						}
						else
						{
							string stuff = Encoding.ASCII.GetString(pckt.payload, offset, len);
							offset += len;
							if(stuff.ToLower().IndexOf("bitrate") != -1)
							{
								bitrate = Endian.ToUInt32(pckt.payload, offset, false);
								offset += 4;
								//System.Diagnostics.Debug.WriteLine("bitrate: " + bitrate.ToString());
							}
							else
							{
								System.Diagnostics.Debug.WriteLine("SearchResults unknown metaname2: " + stuff);
								offset += 4;
							}
						}
					}
					else
					{
						System.Diagnostics.Debug.WriteLine("SearchResults unexpected 1: " + pckt.payload[offset].ToString());
						offset++;
						ushort len = Endian.ToUInt16(pckt.payload, offset, false);
						offset += 2;
						System.Diagnostics.Debug.WriteLine("the length: " + len.ToString());
						System.Diagnostics.Debug.WriteLine("what: " + Encoding.ASCII.GetString(pckt.payload, offset, len));
						offset += len;
						return;
					}
				}
				//make qho
				QueryHitObject qho = new QueryHitObject();
				if(bitrate > 0)
				{
					qho.extensions = new string[]{bitrate.ToString() + " kbps  "};
					if(length != "")
						qho.extensions[0] += length.Replace(" ", "");
				}
				qho.fileIndex = 0;
				qho.fileName = name;
				qho.fileSize = size;
				qho.ip = Endian.BigEndianIP(pckt.payload, clientIDoffset);
				qho.md4sum = bytesHash;
				qho.networkType = NetworkType.EDonkey;
				qho.port = port;
				qho.sockWhereFrom = sockNum;
				qho.speed = 20;
				qho.unseenHosts = (int)sources;
				qho.vendor = "eDonkey";

				//find the appropriate ActiveSearch this search response corresponds to
				lock(ActiveSearch.searches)
					foreach(ActiveSearch search in ActiveSearch.searches)
						if((Match(name.ToLower(), search.query.ToLower()) || ActiveSearch.searches.Count == 1) && search.guid != "stopped")
						{
							//same method as in Gnutella
							GUIBridge.AddQueryHit(qho, null, ref search.query);
							break;
						}
			}
		}

		static bool Match(string filename, string query)
		{
			string[] keywords = Keywords.GetKeywords(query);
			foreach(string word in keywords)
				if(filename.IndexOf(word) == -1)
					return false;
			return true;
		}

		/// <summary>
		/// Incoming sources for a file hash.
		/// </summary>
		public static void FoundSources(byte[] msg, int start, int sockNum)
		{
			byte[] md4sum = new byte[16];
			Array.Copy(msg, start, md4sum, 0, 16);
			string hexmd4sum = Utils.HexGuid(md4sum);
			int numServers = msg[start+16];
			for(int x = 0; x < numServers; x++)
			{
				int offset = start + 17 + (x * 6);
				string ip = Endian.BigEndianIP(msg, offset);
				ushort port = Endian.ToUInt16(msg, offset+4, false);
				ReQuery.EDonkeyFoundSource(ref hexmd4sum, ref ip, port, numServers, sockNum);
			}
		}

		/// <summary>
		/// Incoming request for a callback connection.
		/// </summary>
		public static void CallbackRequest(Packet pckt, int sockNum)
		{
			//System.Diagnostics.Debug.WriteLine("eDonkey Callback Requested");
			//process it
			string ip = Endian.BigEndianIP(pckt.payload, 1);
			//System.Diagnostics.Debug.WriteLine("ed2k callback ip: " + ip);
			ushort port = Endian.ToUInt16(pckt.payload, 5, false);
			//first see if it's for a download
			foreach(DownloadManager dMEr in DownloadManager.dms)
				if(dMEr != null)
					if(dMEr.active)
						foreach(Downloader dler in dMEr.downloaders)
						{
							//if(dler.qho.ip == ip)
							//	System.Diagnostics.Debug.WriteLine("ed2k callback download found");
							if(dler.qho.ip == ip && dler.queueState != 0 && dler.qho.networkType == NetworkType.EDonkey)
							{
								lock(dMEr.endpoints)
								{
									//System.Diagnostics.Debug.WriteLine("callbacking to: " + dler.qho.ip);
									dler.lastMessage = "Callbacking";
									dler.state = DLState.Waiting;
									dler.count = 1;
								}
							}
						}
		}

		/// <summary>
		/// Incoming info that our callback failed.
		/// </summary>
		public static void CallbackRequestFailed(Packet pckt, int sockNum)
		{
			//System.Diagnostics.Debug.WriteLine("eDonkey CallbackRequestFailed");
		}

		/// <summary>
		/// Incoming client hello message.
		/// </summary>
		public static void ClientHello(Packet pckt, int sockNum)
		{
			byte[] clientHash = new byte[16];
			Array.Copy(pckt.payload, 2, clientHash, 0, 16);
			//uint clientID = Endian.ToUInt32(pckt.payload, 18, false);
			//ushort port = Endian.ToUInt16(pckt.payload, 22, false);
			uint metaCount = Endian.ToUInt32(pckt.payload, 24, false);
			//if(metaCount > 0)
			//{
			//	System.Diagnostics.Debug.WriteLine("ClientHello metaCount: " + metaCount.ToString());
			//}

			//send our response
			HelloResponse(sockNum, Endian.BigEndianIP(pckt.payload, 18));
		}

		/// <summary>
		/// Incoming client hello response message.
		/// </summary>
		public static void ClientHelloResponse(Packet pckt, int sockNum)
		{
			byte[] clientHash = new byte[16];
			Array.Copy(pckt.payload, 1, clientHash, 0, 16);
			uint clientID = Endian.ToUInt32(pckt.payload, 17, false);
			ushort port = Endian.ToUInt16(pckt.payload, 21, false);
			uint metaCount = Endian.ToUInt32(pckt.payload, 23, false);
			if(metaCount > 0)
			{
				//System.Diagnostics.Debug.WriteLine("ClientHelloResponse metacount: " + metaCount.ToString());
			}
			//let the downloader know that we're good
			int dmNum = Sck.scks[sockNum].dmNum;
			int dlNum = Sck.scks[sockNum].dlNum;
			if(dmNum == -1 && dlNum == -1)
				return;
			if(DownloadManager.dms[dmNum] != null)
				if(DownloadManager.dms[dmNum].downloaders[dlNum] != null)
				{
					if(((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).queueState == 0)
						((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).EDonkeyReady();
				}
		}

		/// <summary>
		/// Incoming file request.
		/// </summary>
		public static void FileRequest(Packet pckt, int sockNum)
		{
			//System.Diagnostics.Debug.WriteLine("in ed2k FileRequest " + Sck.scks[sockNum].RemoteIP());
			string filename = "";
			byte[] hash = new byte[16];
			Array.Copy(pckt.payload, 1, hash, 0, 16);
			foreach(FileObject fo in Stats.fileList)
				if(Utils.SameArray(fo.md4, hash))
				{
					filename = Path.GetFileName(fo.location);
					lastRequestedFO = fo;
					FileRequestAnswer(sockNum, true, hash, ref filename);
					return;
				}
			FileRequestAnswer(sockNum, false, hash, ref filename);
		}

		/// <summary>
		/// Incoming response to our file request.
		/// </summary>
		public static void FileRequestAnswer(Packet pckt, int sockNum, bool good)
		{
			int dmNum = Sck.scks[sockNum].dmNum;
			int dlNum = Sck.scks[sockNum].dlNum;
			if(DownloadManager.dms[dmNum] != null)
				if(DownloadManager.dms[dmNum].downloaders[dlNum] != null)
					((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).EDonkeyStage1(good);
		}

		/// <summary>
		/// Incoming file part completion status request.
		/// </summary>
		public static void FileStatusRequest(Packet pckt, int sockNum)
		{
			//System.Diagnostics.Debug.WriteLine("in ed2k FileStatusRequest");
			byte[] hash = new byte[16];
			Array.Copy(pckt.payload, 1, hash, 0, 16);
			if(lastRequestedFO != null)
				if(Utils.SameArray(hash, lastRequestedFO.md4))
					goto found;
			foreach(FileObject fo in Stats.fileList)
				if(Utils.SameArray(fo.md4, hash))
				{
					lastRequestedFO = fo;
					goto found;
				}
			return;
			found:
				FileStatusResponse(sockNum);
		}

		/// <summary>
		/// Incoming status for file parts.
		/// </summary>
		public static void FileStatusResponse(Packet pckt, int sockNum)
		{
			int offset = 17;
			int count = Endian.ToUInt16(pckt.payload, offset, false);
			offset += 2;
			byte[] vals = new byte[count];
			int actCount = 0;
			while(actCount != count)
			{
				for(int y = 0; y < 8; y++)
				{
					vals[actCount] = (((pckt.payload[offset]>>y) & 0x01) == 0x01) ? (byte)1 : (byte)0;
					actCount++;
					if(actCount == count)
						break;
				}
				offset += 1;
			}
			int dmNum = Sck.scks[sockNum].dmNum;
			int dlNum = Sck.scks[sockNum].dlNum;
			if(DownloadManager.dms[dmNum] != null)
				if(DownloadManager.dms[dmNum].downloaders[dlNum] != null)
					((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).EDonkeyStage2(vals);
		}

		/// <summary>
		/// Incoming hash set request.
		/// </summary>
		public static void HashSetRequest(Packet pckt, int sockNum)
		{
			//System.Diagnostics.Debug.WriteLine("in ed2k HashSetRequest");
			byte[] hash = new byte[16];
			Array.Copy(pckt.payload, 1, hash, 0, 16);
			if(lastRequestedFO != null)
				if(Utils.SameArray(hash, lastRequestedFO.md4))
					goto found;
			foreach(FileObject fo in Stats.fileList)
				if(Utils.SameArray(fo.md4, hash))
				{
					lastRequestedFO = fo;
					goto found;
				}
			return;
			found:
				HashSetResponse(sockNum);
		}

		/// <summary>
		/// Incoming hash set response... contains hash values for the file parts.
		/// </summary>
		public static void HashSetResponse(Packet pckt, int sockNum)
		{
			int dmNum = Sck.scks[sockNum].dmNum;
			int dlNum = Sck.scks[sockNum].dlNum;
			if(DownloadManager.dms[dmNum] != null)
				if(DownloadManager.dms[dmNum].downloaders[dlNum] != null)
					((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).EDonkeyStage3();
		}

		/// <summary>
		/// Incoming message asking for a slot for an upload.
		/// </summary>
		public static void SlotRequest(Packet pckt, int sockNum)
		{
			//System.Diagnostics.Debug.WriteLine("SlotRequest in haus");
			if(Stats.Updated.uploadsNow < Stats.settings.maxUploads)
				SlotGiven(sockNum);
			else
				Sck.scks[sockNum].Disconnect("no slot");
		}

		/// <summary>
		/// Incoming message indicating an open or closed slot.
		/// </summary>
		public static void SlotResponse(Packet pckt, int sockNum)
		{
			//System.Diagnostics.Debug.WriteLine("SLOT GIVEN");
			int dmNum = Sck.scks[sockNum].dmNum;
			int dlNum = Sck.scks[sockNum].dlNum;
			if(dmNum == -1 && dlNum == -1)
				System.Diagnostics.Debug.WriteLine("couldn't find DLer for the SlotResponse");
			else
			{
				if(DownloadManager.dms[dmNum] != null)
					if(DownloadManager.dms[dmNum].downloaders[dlNum] != null)
						((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).EDonkeyStage4();
			}
		}

		/// <summary>
		/// Incoming message informing us that the host doesn't want a queue slot anymore.
		/// </summary>
		public static void SlotRelease(Packet pckt, int sockNum)
		{
			//
		}

		/// <summary>
		/// Incoming request for the file upload.
		/// </summary>
		public static void RequestParts(Packet pckt, int sockNum)
		{
			//System.Diagnostics.Debug.WriteLine("edonkey upload started");
			if(Sck.scks[sockNum].upNum != -1)
				return;

			//0 is type
			//1-16 is hash
			byte[] md4hash = new byte[16];
			Array.Copy(pckt.payload, 1, md4hash, 0, 16);
			//17-20 is start
			uint start = Endian.ToUInt32(pckt.payload, 17, false);
			//21-24 is 2nd start
			//25-28 is 3rd start
			//29-32 is stop
			uint stop = Endian.ToUInt32(pckt.payload, 29, false);
			//33-36 is 2nd stop
			//37-40 is 3rd stop

			UploadManager.IncomingEdSck(md4hash, start, stop, sockNum);
		}

		/// <summary>
		/// Incoming request for the file upload to terminate.
		/// </summary>
		public static void EndOfDownload(Packet pckt, int sockNum)
		{
			Sck.scks[sockNum].Disconnect("eod");
		}

		/// <summary>
		/// Incoming finish to a transfer chunk.
		/// We'll inform the downloader, and if the file is not done, it'll request more parts.
		/// </summary>
		public static void SendPartFinished(int sockNum)
		{
			/*
			int dmNum = Sck.scks[sockNum].dmNum;
			int dlNum = Sck.scks[sockNum].dlNum;
			if(DownloadManager.dms[dmNum] != null)
				if(DownloadManager.dms[dmNum].downloaders[dlNum] != null)
					((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).Disconnect("ed2k part finished");
			*/
		}

		/// <summary>
		/// Incoming request to see what files we're sharing.
		/// </summary>
		public static void ViewFiles(int sockNum)
		{
			SendClientFiles(sockNum);
		}

		/// <summary>
		/// Incoming request about the queue state for an item we're hosting.
		/// </summary>
		public static void QueueStateCheck(System.Net.EndPoint ep, byte[] msg)
		{
			//
		}

		/// <summary>
		/// Incoming response informing us about our queued item.
		/// </summary>
		public static void QueueStateInfo(System.Net.EndPoint ep, byte[] msg)
		{
			//System.Diagnostics.Debug.WriteLine("alddfjowijef: " + ep.ToString());
		}

		/// <summary>
		/// Outgoing login message.
		/// Could be a hello to server, but also to client.
		/// </summary>
		public static void Login(int sockNum)
		{
			MemoryStream ms = new MemoryStream();
			//type
			ms.WriteByte(0x01);
			if(!Sck.scks[sockNum].server)
				ms.WriteByte(0x10);
			//client info
			Routines.ClientInfo(ms);
			//server address
			if(!Sck.scks[sockNum].server)
			{
				if(Sck.scks[sockNum].dlNum == -1 && Sck.scks[sockNum].dmNum == -1)
				{
					ms.Write(Endian.GetBytes((uint)0, false), 0, 4);
					ms.Write(Endian.GetBytes((ushort)0, false), 0, 2);
				}
				else
				{
					int dmNum = Sck.scks[sockNum].dmNum;
					int dlNum = Sck.scks[sockNum].dlNum;
					//we want the uploader to be able to send us a callback through a server
					if(DownloadManager.dms[dmNum] != null)
						if(DownloadManager.dms[dmNum].downloaders[dlNum] != null)
						{
							int servSockNum = ((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).qho.sockWhereFrom;
							if(servSockNum != -1)
								if(Sck.scks[servSockNum] != null)
									if(Sck.scks[servSockNum].state == Condition.Connected)
									{
										byte[] addr = Endian.BigEndianIP(Sck.scks[servSockNum].address);
										if(addr.Length != 4)
											throw new Exception("eDonkey Login addr problem");
										ms.Write(addr, 0, 4);
										ms.Write(Endian.GetBytes((ushort)Sck.scks[servSockNum].port, false), 0, 2);
										goto next;
									}
						}
					ms.Write(Endian.GetBytes((uint)0, false), 0, 4);
					ms.Write(Endian.GetBytes((ushort)0, false), 0, 2);
				}
			}
			next:
				int bufSize = (int)ms.Position;
			ms.Close();
			Packet pckt = new Packet(bufSize);
			pckt.outPayload = ms.GetBuffer();
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing request to get some more servers.
		/// </summary>
		public static void GetServerList(int sockNum)
		{
			Packet pckt = new Packet(1);
			pckt.outPayload = new byte[1];
			pckt.outPayload[0] = 0x14;
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing search to all eDonkey servers for a file.
		/// </summary>
		public static void Search(string file)
		{
			MemoryStream ms = new MemoryStream();
			//type
			ms.WriteByte(0x16);
			//rest
			ms.WriteByte(0x01);
			ms.Write(Endian.GetBytes((ushort)file.Length, false), 0, 2);
			ms.Write(Encoding.ASCII.GetBytes(file), 0, file.Length);

			//broadcast the search
			int bufSize = (int)ms.Position;
			ms.Close();
			Packet pckt = new Packet(bufSize);
			pckt.outPayload = ms.GetBuffer();
			foreach(Sck sck in Sck.scks)
				if(sck != null)
					if(sck.state == Condition.Connected)
						sck.SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing request to all eDonkey servers for a specific file.
		/// </summary>
		public static void FindSources(byte[] md4hash, bool globalUdp)
		{
			if(globalUdp)
			{
				byte[] msg = new byte[18];
				msg[0] = 0xE3;
				msg[1] = 0x9A;
				Array.Copy(md4hash, 0, msg, 2, 16);
				//find a random server
				IPandPort ipap = new IPandPort();
				GUIBridge.EGetRandomServer(ipap);
				Listener.UdpSend(ref ipap.ip, ipap.port+4, msg, 0, msg.Length);
			}
			else
			{
				MemoryStream ms = new MemoryStream();
				//type
				ms.WriteByte(0x19);
				//rest
				ms.Write(md4hash, 0, 16);

				int bufSize = (int)ms.Position;
				ms.Close();
				Packet pckt = new Packet(bufSize);
				pckt.outPayload = ms.GetBuffer();
				foreach(Sck sck in Sck.scks)
					if(sck != null)
						if(sck.server)
							if(sck.state == Condition.Connected)
								sck.SendPacket(pckt);
			}
		}

		/// <summary>
		/// Outgoing request for a callback connection.
		/// </summary>
		public static void SendCallback(ref string hostip, int sockNum)
		{
			byte[] msg = new byte[5];
			msg[0] = 0x1C;
			Array.Copy(Endian.BigEndianIP(hostip), 0, msg, 1, 4);

			Packet pckt = new Packet(msg.Length);
			pckt.outPayload = msg;
			if(sockNum == -1)
				return;
			if(Sck.scks[sockNum].state != Condition.Connected)
				return;
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing response to a client hello.
		/// </summary>
		public static void HelloResponse(int sockNum, string clientip)
		{
			MemoryStream ms = new MemoryStream();
			//type
			ms.WriteByte(0x4C);
			//client info
			Routines.ClientInfo(ms);
			//server address
			ms.Write(Endian.GetBytes((uint)0, false), 0, 4);
			ms.Write(Endian.GetBytes((ushort)0, false), 0, 2);

			int bufSize = (int)ms.Position;
			ms.Close();
			Packet pckt = new Packet(bufSize);
			pckt.outPayload = ms.GetBuffer();

			//also check if we need this host for downloading
			int dmNum = Sck.scks[sockNum].dmNum;
			int dlNum = Sck.scks[sockNum].dlNum;
			if(dmNum == -1 && dlNum == -1)
			{
				//find a corresponding downloader
				foreach(DownloadManager dMEr in DownloadManager.dms)
					if(dMEr != null)
						if(dMEr.active)
							foreach(Downloader dler in dMEr.downloaders)
								if(dler.qho.ip == clientip && dler.qho.networkType == NetworkType.EDonkey)
								{
									if(dler.queueState == 0)
									{
										if(dler.edsck != null)
										{
											System.Diagnostics.Debug.WriteLine("eDonkey HelloResponse incompatibility 1 " + dler.state.ToString());
											return;
										}
										lock(dMEr.endpoints)
										{
											dler.edsck = Sck.scks[sockNum];
											Sck.scks[sockNum].dlNum = dler.dlNum;
											Sck.scks[sockNum].dmNum = dler.dmNumParent;
											dler.state = DLState.Connecting;
										}
										Sck.scks[sockNum].SendPacket(pckt);
										dler.EDonkeyReady();
										return;
									}
									else
									{
										if(dler.edsck != null)
										{
											System.Diagnostics.Debug.WriteLine("eDonkey HelloResponse incompatibility 2 " + dler.state.ToString());
											return;
										}
										lock(dMEr.endpoints)
										{
											dler.edsck = Sck.scks[sockNum];
											Sck.scks[sockNum].dlNum = dler.dlNum;
											Sck.scks[sockNum].dmNum = dler.dmNumParent;
											dler.state = DLState.Connected;
											dler.queueState = 0;
										}
										Sck.scks[sockNum].SendPacket(pckt);
										return;
									}
								}
			}
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing file request.
		/// </summary>
		public static void FileRequest(byte[] fileHash, int sockNum)
		{
			byte[] ba = new byte[17];
			ba[0] = 0x58;
			Array.Copy(fileHash, 0, ba, 1, 16);

			Packet pckt = new Packet(ba.Length);
			pckt.outPayload = ba;
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing file request response.
		/// The found variable indicates whether or not we have the file.
		/// </summary>
		public static void FileRequestAnswer(int sockNum, bool found, byte[] md4, ref string filename)
		{
			byte[] ba;
			if(!found)
			{
				ba = new byte[17];
				ba[0] = 0x48;
				Array.Copy(md4, 0, ba, 1, 16);
			}
			else
			{
				ba = new byte[17+2+filename.Length];
				ba[0] = 0x59;
				Array.Copy(md4, 0, ba, 1, 16);
				ushort flen = (ushort)filename.Length;
				Array.Copy(Endian.GetBytes(flen, false), 0, ba, 17, 2);
				Array.Copy(System.Text.Encoding.ASCII.GetBytes(filename), 0, ba, 19, flen);
			}

			Packet pckt = new Packet(ba.Length);
			pckt.outPayload = ba;
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing file status request.
		/// </summary>
		public static void FileStatusRequest(byte[] fileHash, int sockNum)
		{
			byte[] ba = new byte[17];
			ba[0] = 0x4F;
			Array.Copy(fileHash, 0, ba, 1, 16);

			Packet pckt = new Packet(ba.Length);
			pckt.outPayload = ba;
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing response to a file status packet.
		/// </summary>
		public static void FileStatusResponse(int sockNum)
		{
			//we don't support partial file uploading yet
			MemoryStream ms = new MemoryStream();
			ms.WriteByte(0x50);
			ms.Write(lastRequestedFO.md4, 0, 16);
			ms.Write(Endian.GetBytes((ushort)0, false), 0 , 2);

			int bufSize = (int)ms.Position;
			ms.Close();
			Packet pckt = new Packet(bufSize);
			pckt.outPayload = ms.GetBuffer();
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing request for part hashes.
		/// </summary>
		public static void HashSetRequest(byte[] fileHash, int sockNum)
		{
			byte[] ba = new byte[17];
			ba[0] = 0x51;
			Array.Copy(fileHash, 0, ba, 1, 16);

			Packet pckt = new Packet(ba.Length);
			pckt.outPayload = ba;
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing response to a hash set request packet.
		/// </summary>
		public static void HashSetResponse(int sockNum)
		{
			//we don't support partial file uploading yet
			MemoryStream ms = new MemoryStream();
			ms.WriteByte(0x52);
			ms.Write(lastRequestedFO.md4, 0, 16);
			ms.Write(Endian.GetBytes((ushort)0, false), 0 , 2);

			int bufSize = (int)ms.Position;
			ms.Close();
			Packet pckt = new Packet(bufSize);
			pckt.outPayload = ms.GetBuffer();
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing request for a download slot.
		/// </summary>
		public static void SlotRequest(int sockNum)
		{
			Packet pckt = new Packet(1);
			pckt.outPayload = new byte[1] {0x54};
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing open slot is assigned.
		/// </summary>
		public static void SlotGiven(int sockNum)
		{
			Packet pckt = new Packet(1);
			pckt.outPayload = new byte[1] {0x55};
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing release for a download slot.
		/// </summary>
		public static void SlotRelease(int sockNum)
		{
			Packet pckt = new Packet(1);
			pckt.outPayload = new byte[1] {0x56};
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing request for file parts.
		/// </summary>
		public static void RequestParts(byte[] fileHash, int sockNum, uint start, uint stop)
		{
			//System.Diagnostics.Debug.WriteLine("REQUESTING OUR ed2k file");
			byte[] msg = new byte[41];
			msg[0] = 0x47;
			Array.Copy(fileHash, 0, msg, 1, 16);
			Array.Copy(Endian.GetBytes(start, false), 0, msg, 17, 4);
			Array.Copy(Endian.GetBytes(0, false), 0, msg, 21, 4);
			Array.Copy(Endian.GetBytes(0, false), 0, msg, 25, 4);
			Array.Copy(Endian.GetBytes(stop, false), 0, msg, 29, 4);
			Array.Copy(Endian.GetBytes(0, false), 0, msg, 33, 4);
			Array.Copy(Endian.GetBytes(0, false), 0, msg, 37, 4);

			Packet pckt = new Packet(41);
			pckt.outPayload = msg;
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing file transfer.
		/// </summary>
		public static void SendingPart(byte[] md4hash, uint range1, uint range2, byte[] data, int sockNum)
		{
			if(range2 - range1 != data.Length)
				System.Diagnostics.Debug.WriteLine("ed2k SendingPart size discrepancy");
			byte[] wholepacket = new byte[1+16+8+data.Length];
			wholepacket[0] = 0x46;
			Array.Copy(md4hash, 0, wholepacket, 1, 16);
			Array.Copy(Endian.GetBytes(range1, false), 0, wholepacket, 17, 4);
			Array.Copy(Endian.GetBytes(range2, false), 0, wholepacket, 21, 4);
			Array.Copy(data, 0, wholepacket, 25, data.Length);

			Packet pckt = new Packet(wholepacket.Length);
			pckt.outPayload = wholepacket;
			Sck.scks[sockNum].SendPacket(pckt);
			while(Sck.scks[sockNum].sendQueue.Count > 0)
				System.Threading.Thread.Sleep(20);
		}

		/// <summary>
		/// Outgoing info that we're done with the download.
		/// </summary>
		public static void EndOfDownload(byte[] fileHash, int sockNum)
		{
			byte[] ba = new byte[17];
			ba[0] = 0x49;
			Array.Copy(fileHash, 0, ba, 1, 16);

			Packet pckt = new Packet(ba.Length);
			pckt.outPayload = ba;
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing response to a view files request.
		/// </summary>
		public static void SendClientFiles(int sockNum)
		{
			MemoryStream ms = new MemoryStream();
			ms.WriteByte(0x4B);
			Routines.FileInfoList(ms);
			
			int bufSize = (int)ms.Position;
			ms.Close();
			Packet pckt = new Packet(bufSize);
			pckt.outPayload = ms.GetBuffer();
			Sck.scks[sockNum].SendPacket(pckt);
		}

		/// <summary>
		/// Outgoing udp request to see how our queued item is doing.
		/// </summary>
		public static void QueueStateCheck(Downloader elDLer)
		{
			byte[] msg = new byte[1+4+1+16];
			msg[0] = 0xE3;
			Array.Copy(Endian.GetBytes(17, false), 0, msg, 1, 4);
			msg[5] = 0x90;
			Array.Copy(elDLer.qho.md4sum, 0, msg, 6, 16);
			//method:
			//setup the header on that crap and send it udp-wise like in the udp-based find sources
			//but what is the port?
			//eMule finds the port by MuleInfoPacket in BaseClient
			//also, it sets the port when receiving udp messages, obviously
		}
	}
}
