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

namespace FileScope.OpenNap
{
	/// <summary>
	/// This class is used for handling all messages.
	/// </summary>
	public class Messages
	{
		/// <summary>
		/// Incoming error message.
		/// </summary>
		public static void Error(Packet packet, int sockNum)
		{
			System.Diagnostics.Debug.WriteLine("error: "+packet.payload);
			GUIBridge.OMessage(sockNum, "error: "+packet.payload);
			if(packet.payload.IndexOf("server is full") != -1)
				Sck.scks[sockNum].Disconnect();
		}

		/// <summary>
		/// Incoming login confirmation.
		/// </summary>
		public static void LoginAck(Packet packet, int sockNum)
		{
			System.Diagnostics.Debug.WriteLine("login ack: "+packet.payload);
			ServerStats(sockNum);
			NotifySharedFiles(sockNum);
		}

		/// <summary>
		/// Incoming search response.
		/// </summary>
		public static void SearchResponse(Packet packet, int sockNum)
		{
			//System.Diagnostics.Debug.WriteLine("search response: "+packet.payload);
			//parse results; we only want the first 7 parameters
			try
			{
				int afterFilenameIndex = packet.payload.LastIndexOf("\"") + 2;
				string takenOutFilename = packet.payload.Substring(afterFilenameIndex, packet.payload.Length - afterFilenameIndex);
				string filename = packet.payload.Substring(1, afterFilenameIndex-3);
				string[] msgs = new string[8];//md5, size, bitrate, frequency, length, nick, ip, link-type
				char[] delimeters = new char[1]{' '};
				msgs = takenOutFilename.Split(delimeters, 8);
				string[] appendix = msgs[7].Split(delimeters);
				msgs[7] = appendix[0];
				//convert from hz to kHz
				msgs[3] = Convert.ToString((Math.Round(Convert.ToDouble(msgs[3]) / 1024)));

				QueryHitObject qho = new QueryHitObject();
				qho.networkType = NetworkType.OpenNap;
				qho.extensions = new string[0];
				qho.fileIndex = 0;
				qho.filePath = filename;
				qho.fileName = System.IO.Path.GetFileName(filename);
				qho.fileSize = Convert.ToUInt32(msgs[1]);
				qho.ip = msgs[5];
				qho.port = -1;
				qho.sockWhereFrom = sockNum;
				qho.nick = Sck.scks[sockNum].nick;
				switch(Convert.ToInt32(msgs[7]))
				{
					case 0:
						qho.speed = 10;
						break;
					case 1:
						qho.speed = 2;
						break;
					case 2:
						qho.speed = 3;
						break;
					case 3:
						qho.speed = 3;
						break;
					case 4:
						qho.speed = 4;
						break;
					case 5:
						qho.speed = 10;
						break;
					case 6:
						qho.speed = 22;
						break;
					case 7:
						qho.speed = 100;
						break;
					case 8:
						qho.speed = 100;
						break;
					case 9:
						qho.speed = 1500;
						break;
					case 10:
						qho.speed = 55000;
						break;
					default:
						System.Diagnostics.Debug.WriteLine("yeah this shouldn't have happened (link-type)");
						qho.speed = 10;
						break;
				}
				qho.unseenHosts = 0;
				qho.vendor = "OpenNap";
				qho.xml = "";

				QueryHitTable table = new QueryHitTable();
				table.address = msgs[5];
				table.busy = false;
				table.chat = false;
				table.unseenHosts = 0;
				table.mp3info = msgs[2] + " kbps " + msgs[3] + " kHz " + Utils.TimeFormatFromSeconds(Convert.ToInt32(msgs[4]));
				table.push = false;
				table.speed = qho.speed;
				table.type = QHOStuff.GetType(qho);
				table.sha1 = "";
				table.queryHitObjects.Add(qho);

				//this might not be a search response to the user's query... but a response to a ReQuery made by the ReQuery class
				foreach(DownloadManager dmer in DownloadManager.dms)
					if(dmer != null)
						if(dmer.active)
						{
							if(((Downloader)dmer.downloaders[0]).qho.fileSize == qho.fileSize)
								if(QHOStuff.Match2(((Downloader)dmer.downloaders[0]).qho.fileName, qho.fileName))
								{
									//System.Diagnostics.Debug.WriteLine("opennap match found");
									if(((Downloader)dmer.downloaders[0]).state == DLState.CouldNotConnect || ((Downloader)dmer.downloaders[0]).state == DLState.Waiting)
									{
										Download(qho);
										//add the qho if we don't know about it
										foreach(QueryHitObject elQhO in ((Downloader)dmer.downloaders[0]).otroQHOs)
											if(elQhO.ip == qho.ip)
												return;
										qho.fileName = ((Downloader)dmer.downloaders[0]).qho.fileName;
										((Downloader)dmer.downloaders[0]).otroQHOs.Add(qho);
									}
									return;
								}
						}

				//find the appropriate ActiveSearch this search response corresponds to
				lock(ActiveSearch.searches)
					foreach(ActiveSearch search in ActiveSearch.searches)
						if(Match(filename.ToLower(), search.query.ToLower()) && search.guid != "stopped")
						{
							//same method as in Gnutella
							GUIBridge.AddQueryHit(qho, table, ref search.query);
							return;
						}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("opennap messages search response error");
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
		/// Incoming queue limit message.
		/// </summary>
		public static void QueueLimit(Packet packet, int sockNum)
		{
			try
			{
				//System.Diagnostics.Debug.WriteLine("Queue: "+packet.payload);
				string elpay = packet.payload;
				string elnick = elpay.Substring(0, elpay.IndexOf("\"")-1);
				string elfinm = elpay.Substring(elpay.IndexOf("\"")+1, elpay.LastIndexOf("\"") - elpay.IndexOf("\"")-1);
				//find matching download
				foreach(DownloadManager dmer in DownloadManager.dms)
					if(dmer != null)
						if(dmer.active)
							if(System.IO.Path.GetFileName(elfinm) == ((Downloader)dmer.downloaders[0]).qho.fileName)
								if(elnick == ((Downloader)dmer.downloaders[0]).qho.ip)
								{
									((Downloader)dmer.downloaders[0]).state = DLState.Waiting;
									return;
								}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("opennap messages Queue Limit");
			}
		}

		/// <summary>
		/// Incoming message of the day.
		/// </summary>
		public static void MOD(Packet packet, int sockNum)
		{
			System.Diagnostics.Debug.WriteLine("MOD: "+packet.payload);
			GUIBridge.OMessage(sockNum, "MOD: "+packet.payload);
		}

		/// <summary>
		/// Incoming general 404 error.
		/// </summary>
		public static void Error404(Packet packet, int sockNum)
		{
			System.Diagnostics.Debug.WriteLine("error: "+packet.payload);
			GUIBridge.OMessage(sockNum, "error: "+packet.payload);
		}

		/// <summary>
		/// Incoming download response.
		/// </summary>
		public static void DownloadAck(Packet packet, int sockNum)
		{
			System.Diagnostics.Debug.WriteLine("download ack: "+packet.payload);
			try
			{
				string payload = packet.payload;
				string filename = payload.Substring(payload.IndexOf("\"") + 1, payload.LastIndexOf("\"") - payload.IndexOf("\"") - 1);
				string rest = payload.Substring(0, payload.IndexOf("\"") - 1);
				char[] delimeters = new char[1]{' '};
				string[] msgs = rest.Split(delimeters, 3);
				string nick = msgs[0];
				long ip = Convert.ToInt64(msgs[1]);
				int port = (int)Convert.ToUInt16(msgs[2]);

				//find corresponding download
				for(int x = 0; x < DownloadManager.dms.Length; x++)
				{
					if(DownloadManager.dms[x] == null)
						continue;
					if(!DownloadManager.dms[x].active)
						continue;
					Downloader elDLer = (Downloader)DownloadManager.dms[x].downloaders[0];

					//normal
					if(elDLer.qho.filePath == filename && elDLer.qho.ip == nick && elDLer.state == DLState.Connecting)
					{
						elDLer.qho.port = (int)port;
						elDLer.qho.longIP = ip;
						elDLer.Reset();
						return;
					}

					//through requery algorithm
					if(elDLer.state == DLState.CouldNotConnect || elDLer.state == DLState.Waiting)
					{
						//find matching nick and filename
						foreach(QueryHitObject elqHo in elDLer.otroQHOs)
							if(elqHo.filePath == filename && elqHo.ip == nick)
							{
								elDLer.qho = elqHo;
								elDLer.qho.port = (int)port;
								elDLer.qho.longIP = ip;
								elDLer.Reset();
								return;
							}
					}
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("opennap messages DownloadAck");
			}
		}

		/// <summary>
		/// Incoming server stats response.
		/// </summary>
		public static void ServerStatsResponse(Packet packet, int sockNum)
		{
			System.Diagnostics.Debug.WriteLine("server stats response: "+packet.payload);
			try
			{
				//parse the server stats response: [users][#files][size]
				string[] msgs = new string[3];
				char[] delimeters = new char[1]{' '};
				msgs = packet.payload.Split(delimeters, 3);
				GUIBridge.OUpdateStats(sockNum, msgs[0], msgs[1], msgs[2]);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("opennap messages ServerStatsResponse");
			}
		}

		/// <summary>
		/// Incoming response to one of our Resume Requests.
		/// </summary>
		public static void ResumeRequestResponse(Packet packet, int sockNum)
		{
			//servers stopped implementing this
			System.Diagnostics.Debug.WriteLine("resume request response: "+packet.payload);
		}

		/// <summary>
		/// Incoming request for an upload of one of our files.
		/// </summary>
		public static void UploadRequest(Packet packet, int sockNum)
		{
			System.Diagnostics.Debug.WriteLine("upload request: "+packet.packet);
			if(packet.payload.Length > 2)
			{
				if(!packet.payload.EndsWith("\""))
					packet.payload = packet.payload.Substring(0, packet.payload.Length-2);
				AcceptUpload(sockNum, ref packet.payload);
			}
		}

		/// <summary>
		/// Outgoing login.
		/// </summary>
		public static void Login(int sockNum)
		{
			//generate random username/password
			int num1 = GUID.rand.Next(10, 1000000);
			int num2 = GUID.rand.Next(10, 1000000);

			//generate login text
			string login = "scope"+num1.ToString()+" scope"+num2.ToString()+" "+Stats.settings.port.ToString()+" \"Napster 2.0 Beta 8\" 7";

			//create and send packet
			Packet packet = new Packet(login.Length, 2, login);
			Sck.scks[sockNum].SendPacket(packet);
			Sck.scks[sockNum].nick = "scope"+num1.ToString();
		}

		/// <summary>
		/// Outgoing broadcast search.
		/// </summary>
		public static void Search(string query)
		{
			string payload = "FILENAME CONTAINS \"" + query + "\"";
			//create packet
			Packet packet = new Packet(payload.Length, 200, payload);
			//loop through all connected sockets
			foreach(Sck obj in Sck.scks)
				if(obj != null)
					if(obj.state == Condition.Connected)
						obj.SendPacket(packet);
		}

		/// <summary>
		/// Outgoing download request.
		/// </summary>
		public static void Download(QueryHitObject qho)
		{
			string payload = qho.ip + " \"" + qho.filePath + "\"";
			Packet packet = new Packet(payload.Length, 203, payload);
			Sck.scks[qho.sockWhereFrom].SendPacket(packet);
		}

		/// <summary>
		/// Allows for the download to be pushed.
		/// </summary>
		public static void AlternateDownloadRequest(QueryHitObject qho)
		{
			string payload = qho.ip + " \"" + qho.filePath + "\"";
			Packet packet = new Packet(payload.Length, 500, payload);
			Sck.scks[qho.sockWhereFrom].SendPacket(packet);
		}

		/// <summary>
		/// Outgoing server stats request.
		/// </summary>
		public static void ServerStats(int sockNum)
		{
			//no payload
			Packet packet = new Packet(0, 214, "");
			Sck.scks[sockNum].SendPacket(packet);
		}

		/// <summary>
		/// Outgoing client notification of shared files.
		/// </summary>
		public static void NotifySharedFiles(int sockNum)
		{
			//we won't send over 100 shared file notifications
			for(int x = 0; x < 100; x++)
			{
				if(Stats.fileList.Count == x)
					return;
				string payload = "\"" + System.IO.Path.GetFileName(((FileObject)Stats.fileList[x]).location) + "\"" + " 00000000000000000000000000000000 " + ((FileObject)Stats.fileList[x]).b.ToString();
				Packet packet = new Packet(payload.Length, 100, payload);
				Sck.scks[sockNum].SendPacket(packet);
			}
		}

		/// <summary>
		/// Accept the request for an upload from the server.
		/// </summary>
		public static void AcceptUpload(int sockNum, ref string payload)
		{
			System.Diagnostics.Debug.WriteLine("Accepting Upload: "+payload);
			Packet packet = new Packet(payload.Length, 608, payload);
			Sck.scks[sockNum].SendPacket(packet);
		}
	}
}
