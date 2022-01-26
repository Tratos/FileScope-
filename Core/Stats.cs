// Stats.cs
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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Net;
using System.Timers;
using System.Drawing;

namespace FileScope
{
	/// <summary>
	/// Used in fileList to store all necessary information about each file.
	/// </summary>
	public class FileObject
	{
		//full file path and name
		public string location;
		//store a copy of the filename in lowercase
		public string lcaseFileName;
		//size of file in bytes
		public uint b;
		//file index
		public int fileIndex;
		//md4 hash value for the file
		public byte[] md4;
		//sha1 hash value for the file
		public string sha1;		//in base32
		public byte[] sha1bytes;
		//temporary variable
		public int tempOne;
	}

	/// <summary>
	/// Used in lastFileSet to store all necessary information used to recover
	/// previously generated hash values.
	/// </summary>
	[Serializable]
	public class FileObject2
	{
		public uint bytes;
		public byte[] md4;
		public string sha1;
		public byte[] sha1bytes;
	}

	//connection type
	public enum EnumConnectionType
	{
		dialUp, cable, t1, t3
	}

	/// <summary>
	/// Class for storing the states, stats, and settings for FileScope.
	/// </summary>
	public class Stats
	{
		//FileScope version
		public static string version = "0.5.0";
		//stores all of FileScope settings
		public static FileScopeSettings settings = new FileScopeSettings();
		//list of all shared directories
		public static ArrayList shareList = new ArrayList();
		//file system watchers for each shared directory
		public static FileSystemWatcher[] fsws;
		//timer to check to see if all shared directories are still existent
		public static GoodTimer tmrCheckDirs = new GoodTimer(5000);
		//carry out any 1 second chores
		public static GoodTimer tmrChores = new GoodTimer(1000);
		//list of all shared files
		public static ArrayList fileList = new ArrayList();
		//store the last filelist's sha1 values in this list to speed up the background hashing thread
		public static Hashtable lastFileSet = new Hashtable();
		//list of all blocked chat hosts
		public static ArrayList blockedChatHosts = new ArrayList();

		//list of all cached gnutella hosts
		public static ArrayList gnutellaHosts = new ArrayList();
		//queue that stores index values of sockets that just received data for processing
		public static Queue gnutellaQueue = new Queue(110);
		//gnutella routing table for routing pongs
		public static Gnutella.GoodHashtable gnutellaPongRouteTable = new Gnutella.GoodHashtable(GUID.guidComparer, 4001);
		//gnutella routing table for routing query hits
		public static Gnutella.GoodHashtable gnutellaQueryHitRouteTable = new Gnutella.GoodHashtable(GUID.guidComparer, 12001);
		//gnutella routing table for push requests
		public static Gnutella.GoodHashtable gnutellaPushRouteTable = new Gnutella.GoodHashtable(GUID.guidComparer, 4001);
		//gnutella web cache servers
		public static ArrayList	gnutellaWebCache = new ArrayList();

		//gwc2 servers
		public static ArrayList gnutella2WebCache = new ArrayList();

		//list of all OpenNap servers
		public static SortedList opennapHosts = new SortedList(GUID.stringComparer);

		//list of all eDonkey servers
		public static SortedList edonkeyHosts = new SortedList(GUID.stringComparer);
		//eDonkey file part size
		public static uint eDonkeyPartSize = 9728000;
		//eDonkey lowip indicator
		public static uint eDonkeyLowIP = 16777216;
		/// <summary>
		/// This class is embedded into edonkeyHosts and contains info for an eDonkey server.
		/// </summary>
		public class EDonkeyServerInfo
		{
			public string servName = " ";
			public string servDesc = " ";
			public uint maxUsers = 0;
			public uint curUsers = 0;
			public uint curFiles = 0;
		}

		/// <summary>
		/// Class for all of the settings in FileScope.
		/// </summary>
		[Serializable]
		public class FileScopeSettings
		{
			public volatile int maxUploads;						//the maximum simultaneous uploads
			public volatile string ipAddress;					//public IP Address
			public volatile bool ultrapeerCapable;				//fulfilled gnutella ultrapeer requirements
			public volatile string dlDirectory;					//download directory
			public volatile EnumConnectionType connectionType;	//EnumConnectionType will take care of this
			public volatile byte[] myGUID = new byte[16];		//permanent GUID for this computer
			public volatile int gConnectionsToKeep;				//gnutella connections to keep when ultrapeer
			public volatile bool updateNotify;					//notify user of new updates
			public volatile bool alwaysOnTop;					//window always on top
			public volatile bool switchTransfers;				//switch to transfers page on new download
			public volatile bool closeNormal;					//exit program / send to tray
			public volatile bool minimizeNormal;				//minimize program / send to tray
			public volatile bool firewall;						//are we behind a firewall?
			public volatile int port;							//port for incoming connections
			public volatile bool allowUltrapeer;				//allow ultrapeer
			public volatile bool autoGnutella;					//automatically connect to gnutella on startup
			public volatile bool autoGnutella2;					//automatically connect to gnutella2 on startup
			public volatile bool autoEDonkey;					//automatically connect to eDonkey on startup
			public volatile bool autoOpenNap;					//automatically connect to opennap on startup
			public volatile bool fileAlert;						//alert when downloading dangerous file types
			public volatile bool clearDl;						//clear completed downloads
			public volatile bool clearUp;						//clear completed uploads
			public volatile bool cancelDLAlert;					//alert when attempting to cancel downloads?
			public volatile bool allowChats;					//allow incoming chats?
			public volatile int mainWidth;						//width of main window
			public volatile int mainHeight;						//height of main window
			public volatile float transSplitPerc;				//% of the transfers control the splitter is at
			public volatile bool mainMax;						//whether the main window should be maximized
			public volatile string theme;						//current color scheme
			public Color clFormsBack;							//backcolor for forms, tabpages, etc.
			public Color clLabelFore;							//forecolor for labels, linklabels
			public Color clLabelFore2;							//another forecolor that's good with the backcolor
			public Color clButtonBack;							//backcolor for command buttons
			public Color clButtonFore;							//forecolor for command buttons
			public Color clTextBoxBack;							//backcolor for textboxes, combos, updowns
			public Color clTextBoxFore;							//forecolor for textboxes, combos, updowns
			public Color clCheckBoxFore;						//forecolor for checkboxes and radiobuttons
			public Color clGroupBoxBack;						//backcolor for groupboxes
			public Color clGroupBoxFore;						//forecolor for groupboxes
			public Color clListBoxBack;							//backcolor for listboxes, list/tree views
			public Color clListBoxFore;							//forecolor for listboxes, list/tree views
			public Color clRichTextBoxBack;						//backcolor for richtextboxes
			public Color clChatHeader;							//header for chats
			public Color clChatYou;								//color of your text
			public Color clChatPeer;							//color of a peer's text
			public Color clHighlight1;							//general purpose highlight color
			public Color clHighlight2;							//general purpose highlight color
			public Color clHighlight3;							//general purpose highlight color
			public Color clHighlight4;							//general purpose highlight color
			public Color clMenuHighlight1;						//hovering menu color
			public Color clMenuHighlight2;						//selected menu color
			public Color clMenuBox;								//menu box color
			public Color clMenuBorder;							//menu hovering border color
			public Color clHomeTL;								//the color at the top-left of the homepage
			public Color clHomeBR;								//the color of the bottom-right logo in the homepage
			public bool clGridLines;							//show gridlines?
		}

		/// <summary>
		/// Updated statistics during runtime.
		/// </summary>
		public class Updated
		{
			public static volatile bool le = true;				//little or big endian
			public static volatile int timestamp;
			public static volatile int kbShared;				//kilobytes you're sharing
			public static volatile int filesShared;				//# of files you're sharing
			public static volatile bool everIncoming;			//accepted an incoming tcp connection?
			public static volatile bool udpIncoming;			//ever received incoming udp packets
			public static volatile IPAddress myIPA;				//our ip address
			public static volatile int uploads;					//# of files you've uploaded
			public static volatile int uploadsNow2;				//# of files currently uploading
			public static int uploadsNow
			{
				get{return uploadsNow2;}
				set{uploadsNow2 = value; GUIBridge.RefreshHomePageTransfers();}
			}
			public static volatile int downloadsNow2;			//# of files currently downloading
			public static int downloadsNow
			{
				get{return downloadsNow2;}
				set{downloadsNow2 = value; GUIBridge.RefreshHomePageTransfers();}
			}
			public static volatile int numChats;				//number of active chats
			public static volatile int trueSpeed;				//"real" speed of connection
			public static volatile bool closing;				//are we closing FileScope?
			public static volatile bool opened;					//did we fully open FileScope?
			public static volatile int inNetworkBandwidth;		//b/s inward network bandwidth
			public static volatile int outNetworkBandwidth;		//b/s outward network bandwidth
			public static volatile int inTransferBandwidth;		//b/s inward transfers bandwidth
			public static volatile int outTransferBandwidth;	//b/s outward transfers bandwidth

			public class Gnutella
			{
				public static volatile bool ultrapeer;			//are we an ultrapeer now or not?
				public static volatile int lastConnectionCount2;//stores the last number of connections
				public static int lastConnectionCount
				{
					get{return lastConnectionCount2;}
					set
					{
						bool sameCount = (lastConnectionCount2 == value);
						lastConnectionCount2 = value;
						if(!sameCount)
							GUIBridge.RefreshHomePageNetworks();
					}
				}
				public static volatile int numPings;
				public static volatile int numPongs;
				public static volatile int numQueries;
				public static volatile int numQueryHits;
				public static volatile int numPushes;
			}

			public class OpenNap
			{
				public static volatile int lastConnectionCount2;//stores the number of connections
				public static int lastConnectionCount
				{
					get{return lastConnectionCount2;}
					set
					{
						bool sameCount = (lastConnectionCount2 == value);
						lastConnectionCount2 = value;
						if(!sameCount)
							GUIBridge.RefreshHomePageNetworks();
					}
				}
			}

			public class EDonkey
			{
				public static volatile int lastConnectionCount2;//stores the number of connections
				public static int lastConnectionCount
				{
					get{return lastConnectionCount2;}
					set
					{
						bool sameCount = (lastConnectionCount2 == value);
						lastConnectionCount2 = value;
						if(!sameCount)
							GUIBridge.RefreshHomePageNetworks();
					}
				}
			}

			public class Gnutella2
			{
				public static volatile bool ultrapeer;			//are we an ultrapeer now or not?
				public static volatile int lastConnectionCount2;//stores the number of connections
				public static volatile bool enableDeflate;		//determines whether or not deflate should be used between g2 connections
				public static int lastConnectionCount
				{
					get{return lastConnectionCount2;}
					set
					{
						bool sameCount = (lastConnectionCount2 == value);
						lastConnectionCount2 = value;
						if(!sameCount)
							GUIBridge.RefreshHomePageNetworks();
					}
				}
				public static volatile int numPI;
				public static volatile int numPO;
				public static volatile int numQ2;
				public static volatile int numQA;
				public static volatile int numQH2;
				public static volatile int numLNI;
				public static volatile int numKHL;
				public static volatile int numPUSH;
				public static volatile int numQHT;
				public static volatile int numQKR;
				public static volatile int numQKA;
			}
		}

		public static void InitializeVariables()
		{
			Updated.Gnutella2.enableDeflate = true;
			if(Stats.settings.ultrapeerCapable && !Stats.settings.firewall && Stats.settings.allowUltrapeer)
				Updated.Gnutella2.ultrapeer = true;
			FileType.FillExt();
			Updated.Gnutella.lastConnectionCount = 0;
			Updated.Gnutella2.lastConnectionCount = 0;
			Updated.OpenNap.lastConnectionCount = 0;
			Updated.EDonkey.lastConnectionCount = 0;
			Updated.uploads = 0;
			Updated.uploadsNow = 0;
			Updated.downloadsNow = 0;
			Updated.trueSpeed = -1;
			Updated.numChats = 0;
			Updated.closing = false;
			Updated.everIncoming = false;
			Updated.udpIncoming = false;
			try
			{
				Updated.myIPA = Endian.GetIPAddress(Endian.BigEndianIP(Stats.settings.ipAddress), 0);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("couldn't set myIPA in Stats.InitializeVariables");
				Updated.myIPA = null;
			}
			Updated.timestamp = 50;
			Updated.Gnutella.numPings = 0;
			Updated.Gnutella.numPongs = 0;
			Updated.Gnutella.numQueries = 0;
			Updated.Gnutella.numQueryHits = 0;
			Updated.Gnutella.numPushes = 0;
			Updated.Gnutella2.numPI = 0;
			Updated.Gnutella2.numPO = 0;
			Updated.Gnutella2.numLNI = 0;
			Updated.Gnutella2.numKHL = 0;
			Updated.Gnutella2.numQ2 = 0;
			Updated.Gnutella2.numQA = 0;
			Updated.Gnutella2.numQH2 = 0;
			Updated.Gnutella2.numPUSH = 0;
			Updated.Gnutella2.numQHT = 0;
			Updated.Gnutella2.numQKR = 0;
			Updated.Gnutella2.numQKA = 0;
			Gnutella2.G2Data.SetupFuncTable();
			Updated.le = BitConverter.IsLittleEndian;
			tmrCheckDirs.AddEvent(new ElapsedEventHandler(tmrCheckDirs_Tick));
			tmrCheckDirs.Start();
			tmrChores.AddEvent(new ElapsedEventHandler(tmrChores_Tick));
			tmrChores.Start();
		}

		/// <summary>
		/// Return the speed of this connection.
		/// </summary>
		public static int GetSpeed()
		{
			if(Updated.trueSpeed != -1)
				return Updated.trueSpeed;
			else
				switch(settings.connectionType)
				{
					case EnumConnectionType.dialUp:
						return 5;
					case EnumConnectionType.cable:
						return 100;
					case EnumConnectionType.t1:
						return 1000;
					case EnumConnectionType.t3:
						return 50000;
					default:
						return 100;
				}
		}

		/// <summary>
		/// Class for loading and saving stuff.
		/// </summary>
		public class LoadSave
		{
			public static void LoadSettings()
			{
				FileStream fStream = new FileStream(Utils.GetCurrentPath("settings.fscp"), FileMode.Open, FileAccess.Read);
				try
				{
					BinaryFormatter crip = new BinaryFormatter();
					settings = (FileScopeSettings)crip.Deserialize(fStream);
					fStream.Close();
				}
				catch
				{
					fStream.Close();
					System.Diagnostics.Debug.WriteLine("LoadSettings");
					try{File.Delete(Utils.GetCurrentPath("settings.fscp"));}
					catch{System.Diagnostics.Debug.WriteLine("Deleting settings.fscp");}
					System.Windows.Forms.Application.Exit();
				}
			}

			public static void SaveSettings()
			{
				BinaryFormatter crip = new BinaryFormatter();
				FileStream fStream = new FileStream(Utils.GetCurrentPath("settings.fscp"), FileMode.Create, FileAccess.Write);
				crip.Serialize(fStream, settings);
				fStream.Close();
			}

			public static void LoadShares()
			{
				FileStream fStream = new FileStream(Utils.GetCurrentPath("shares.fscp"), FileMode.Open, FileAccess.Read);
				try
				{
					BinaryFormatter crip = new BinaryFormatter();
					shareList = (ArrayList)crip.Deserialize(fStream);
					fStream.Close();
					UpdateShares();
				}
				catch
				{
					fStream.Close();
					System.Diagnostics.Debug.WriteLine("LoadShares");
					try{File.Delete(Utils.GetCurrentPath("shares.fscp"));}
					catch{System.Diagnostics.Debug.WriteLine("Deleting shares.fscp");}
					System.Windows.Forms.Application.Exit();
				}
			}

			public static void SaveShares()
			{
				BinaryFormatter crip = new BinaryFormatter();
				FileStream fStream = new FileStream(Utils.GetCurrentPath("shares.fscp"), FileMode.Create, FileAccess.Write);
				crip.Serialize(fStream, shareList);
				fStream.Close();
			}

			public static void LoadHosts()
			{
				FileStream fStream = new FileStream(Utils.GetCurrentPath("gnuthosts.fscp"), FileMode.Open, FileAccess.Read);
				FileStream fStream2 = new FileStream(Utils.GetCurrentPath("gnut2hosts.fscp"), FileMode.Open, FileAccess.Read);
				FileStream fStream3 = new FileStream(Utils.GetCurrentPath("gnut2cache.fscp"), FileMode.Open, FileAccess.Read);
				FileStream fStream4 = new FileStream(Utils.GetCurrentPath("gnut2keys.fscp"), FileMode.Open, FileAccess.Read);
				try
				{
					BinaryFormatter crip = new BinaryFormatter();
					//gnutella first
					lock(gnutellaHosts)
					{
						gnutellaHosts = (ArrayList)crip.Deserialize(fStream);
						fStream.Close();
					}

					//gnutella2
					lock(Gnutella2.HostCache.recentHubs)
					{
						Gnutella2.HostCache.recentHubs = (SortedList)crip.Deserialize(fStream2);
						fStream2.Close();
						//we've been gone probably for a while
						foreach(Gnutella2.HubInfo hi in Gnutella2.HostCache.recentHubs.Values)
							hi.timeKnown += 650;
					}
					lock(Gnutella2.HostCache.hubCache)
					{
						Gnutella2.HostCache.hubCache = (Hashtable)crip.Deserialize(fStream3);
						fStream3.Close();
						foreach(Gnutella2.HubInfo hi in Gnutella2.HostCache.hubCache.Values)
							hi.timeKnown += 650;
					}
					lock(Gnutella2.HostCache.sentQueryKeys)
					{
						Gnutella2.HostCache.sentQueryKeys = (Hashtable)crip.Deserialize(fStream4);
						fStream4.Close();
						foreach(Gnutella2.QKeyInfo qki in Gnutella2.HostCache.sentQueryKeys.Values)
							qki.timeKnown += 650;
					}

					//opennap
					if(File.Exists(Utils.GetCurrentPath("nap.wsx")))
						LoadOpenNapWSXFile(Utils.GetCurrentPath("nap.wsx"));

					//edonkey
					if(File.Exists(Utils.GetCurrentPath("donkey.met")))
						LoadEDonkeyMETFile(Utils.GetCurrentPath("donkey.met"));
				}
				catch(Exception e)
				{
					fStream.Close();
					fStream2.Close();
					fStream3.Close();
					fStream4.Close();
					System.Diagnostics.Debug.WriteLine("LoadHosts " + e.Message);
					System.Diagnostics.Debug.WriteLine(e.StackTrace);
					try{File.Delete(Utils.GetCurrentPath("gnuthosts.fscp"));}
					catch{System.Diagnostics.Debug.WriteLine("Deleting gnuthosts.fscp");}
					HashEngine.Stop();
					System.Windows.Forms.Application.Exit();
				}
			}

			public static void LoadOpenNapWSXFile(string path)
			{
				StreamReader rStream = new StreamReader(path);
				string line = rStream.ReadLine();
				int count = 0;
				while(line != null)
				{
					count++;
					try
					{
						if(line.ToLower().IndexOf("a: ") != -1)
						{
							string addr; int port;
							string host = line.Substring(3, line.Length-3);
							if(host.Length > 5)
							{
								Utils.AddrParse(host, out addr, out port, 8888);
								host = addr.ToLower()+":"+port.ToString();
								if(!Stats.opennapHosts.ContainsKey(host))
									Stats.opennapHosts.Add(host, null);
							}
						}
						line = rStream.ReadLine();
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine("invalid file at line " + count.ToString());
						break;
					}
				}
				rStream.Close();
			}

			public static void LoadEDonkeyMETFile(string path)
			{
				FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
				BinaryReader br = new BinaryReader(fs);
				//check the first header byte
				byte headerByte = br.ReadByte();
				if(headerByte != 0xE0 && headerByte != 0x0E)
				{
					System.Diagnostics.Debug.WriteLine("Invalid eDonkey server list file");
					br.Close();
					fs.Close();
					return;
				}
				//process the rest of the file
				uint numServers = br.ReadUInt32();
				for(uint x = 0; x < numServers; x++)
				{
					EDonkeyServerInfo edsi = new EDonkeyServerInfo();
					string address = Endian.BigEndianIP(br.ReadBytes(4), 0);
					address += ":" + br.ReadUInt16();
					uint metaCount = br.ReadUInt32();
					for(uint y = 0; y < metaCount; y++)
					{
						byte metaType = br.ReadByte();
						ushort metaLen = br.ReadUInt16();
						byte[] meta = br.ReadBytes(metaLen);
						string metaName = "";
						if(metaLen > 1)
							metaName = System.Text.Encoding.ASCII.GetString(meta);
						string strPayload = "";
						uint intPayload = 0;
						if(metaType == 0x02)
							strPayload = System.Text.Encoding.ASCII.GetString(br.ReadBytes(br.ReadUInt16()));
						else if(metaType == 0x03)
							intPayload = br.ReadUInt32();
						//check the special tag
						switch(meta[0])
						{
							case 0x01://server name
								edsi.servName = strPayload;
								break;
							case 0x0B://description
								edsi.servDesc = strPayload;
								break;
							case 0x87://max users
								edsi.maxUsers = intPayload;
								break;
							default:
								if(metaName.ToLower().IndexOf("users") != -1)
									edsi.curUsers = intPayload;
								else if(metaName.ToLower().IndexOf("files") != -1)
									edsi.curFiles = intPayload;
								break;
						}
					}
					if(!Stats.edonkeyHosts.Contains(address))
						Stats.edonkeyHosts.Add(address, edsi);
				}
				br.Close();
				fs.Close();
			}

			public static void SaveHosts()
			{
				//gnutella first
				BinaryFormatter crip = new BinaryFormatter();
				lock(gnutellaHosts)
				{
					FileStream fStream = new FileStream(Utils.GetCurrentPath("gnuthosts.fscp"), FileMode.Create, FileAccess.Write);
					crip.Serialize(fStream, gnutellaHosts);
					fStream.Close();
				}

				//g2
				lock(Gnutella2.HostCache.recentHubs)
				{
					FileStream fStream2 = new FileStream(Utils.GetCurrentPath("gnut2hosts.fscp"), FileMode.Create, FileAccess.Write);
					crip.Serialize(fStream2, Gnutella2.HostCache.recentHubs);
					fStream2.Close();
				}
				lock(Gnutella2.HostCache.hubCache)
				{
					FileStream fStream3 = new FileStream(Utils.GetCurrentPath("gnut2cache.fscp"), FileMode.Create, FileAccess.Write);
					crip.Serialize(fStream3, Gnutella2.HostCache.hubCache);
					fStream3.Close();
				}
				lock(Gnutella2.HostCache.sentQueryKeys)
				{
					FileStream fStream4 = new FileStream(Utils.GetCurrentPath("gnut2keys.fscp"), FileMode.Create, FileAccess.Write);
					crip.Serialize(fStream4, Gnutella2.HostCache.sentQueryKeys);
					fStream4.Close();
				}

				//opennap
				StreamWriter wStream = new StreamWriter(Utils.GetCurrentPath("nap.wsx"));
				wStream.WriteLine("# I am excessively smart");
				wStream.WriteLine();
				for(int x = 0; x < opennapHosts.Count; x++)
					wStream.WriteLine("A: " + opennapHosts.GetKey(x).ToString());
				wStream.Close();

				//edonkey
				FileStream fs = new FileStream(Utils.GetCurrentPath("donkey.met"), FileMode.Create, FileAccess.Write);
				BinaryWriter bw = new BinaryWriter(fs);
				bw.Write((byte)0xE0);
				bw.Write(edonkeyHosts.Count);
				for(int x = 0; x < Stats.edonkeyHosts.Count; x++)
				{
					string address = (string)edonkeyHosts.GetKeyList()[x];
					EDonkeyServerInfo edsi = (EDonkeyServerInfo)edonkeyHosts.GetValueList()[x];
					bw.Write(Endian.BigEndianIP(address.Substring(0, address.IndexOf(":"))));
					int pos = address.IndexOf(":")+1;
					bw.Write(Convert.ToUInt16(address.Substring(pos, address.Length-pos)));
					bw.Write((uint)5);
					//strings first: server name, server description
					bw.Write((byte)0x02);
					bw.Write((ushort)1);
					bw.Write((byte)0x01);
					byte[] bytesPayload = System.Text.Encoding.ASCII.GetBytes(edsi.servName);
					bw.Write((ushort)bytesPayload.Length);
					bw.Write(bytesPayload);
					bw.Write((byte)0x02);
					bw.Write((ushort)1);
					bw.Write((byte)0x0B);
					bytesPayload = System.Text.Encoding.ASCII.GetBytes(edsi.servDesc);
					bw.Write((ushort)bytesPayload.Length);
					bw.Write(bytesPayload);
					//ints next: max # users, current # users, # files
					bw.Write((byte)0x03);
					bw.Write((ushort)1);
					bw.Write((byte)0x87);
					bw.Write(edsi.maxUsers);
					bw.Write((byte)0x03);
					bw.Write((ushort)5);
					bw.Write(System.Text.Encoding.ASCII.GetBytes("users"));
					bw.Write(edsi.curUsers);
					bw.Write((byte)0x03);
					bw.Write((ushort)5);
					bw.Write(System.Text.Encoding.ASCII.GetBytes("files"));
					bw.Write(edsi.curFiles);
				}
				bw.Close();
				fs.Close();
			}

			public static void LoadWebCache()
			{
				FileStream fStream = new FileStream(Utils.GetCurrentPath("webcache.fscp"), FileMode.Open, FileAccess.Read);
				FileStream fStream2 = new FileStream(Utils.GetCurrentPath("webcache2.fscp"), FileMode.Open, FileAccess.Read);
				try
				{
					BinaryFormatter crip = new BinaryFormatter();
					gnutellaWebCache = (ArrayList)crip.Deserialize(fStream);
					fStream.Close();
					gnutella2WebCache = (ArrayList)crip.Deserialize(fStream2);
					fStream2.Close();
				}
				catch
				{
					fStream.Close();
					fStream2.Close();
					System.Diagnostics.Debug.WriteLine("LoadWebCache");
					try{File.Delete(Utils.GetCurrentPath("webcache.fscp"));}
					catch{System.Diagnostics.Debug.WriteLine("Deleting webcache.fscp");}
					try{File.Delete(Utils.GetCurrentPath("webcache2.fscp"));}
					catch{System.Diagnostics.Debug.WriteLine("Deleting webcache2.fscp");}
					HashEngine.Stop();
					System.Windows.Forms.Application.Exit();
				}
			}

			public static void SaveWebCache()
			{
				BinaryFormatter crip = new BinaryFormatter();
				FileStream fStream = new FileStream(Utils.GetCurrentPath("webcache.fscp"), FileMode.Create, FileAccess.Write);
				FileStream fStream2 = new FileStream(Utils.GetCurrentPath("webcache2.fscp"), FileMode.Create, FileAccess.Write);
				crip.Serialize(fStream, gnutellaWebCache);
				crip.Serialize(fStream2, gnutella2WebCache);
				fStream.Close();
				fStream2.Close();
			}

			public static void LoadBlockedChatHosts()
			{
				FileStream fStream = new FileStream(Utils.GetCurrentPath("chatblocks.fscp"), FileMode.Open, FileAccess.Read);
				try
				{
					BinaryFormatter crip = new BinaryFormatter();
					blockedChatHosts = (ArrayList)crip.Deserialize(fStream);
					fStream.Close();
				}
				catch
				{
					fStream.Close();
					System.Diagnostics.Debug.WriteLine("LoadBlockedChatHosts");
					try{File.Delete(Utils.GetCurrentPath("chatblocks.fscp"));}
					catch{System.Diagnostics.Debug.WriteLine("Deleting chatblocks.fscp");}
					HashEngine.Stop();
					System.Windows.Forms.Application.Exit();
				}
			}

			public static void SaveBlockedChatHosts()
			{
				BinaryFormatter crip = new BinaryFormatter();
				FileStream fStream = new FileStream(Utils.GetCurrentPath("chatblocks.fscp"), FileMode.Create, FileAccess.Write);
				crip.Serialize(fStream, blockedChatHosts);
				fStream.Close();
			}

			public static void LoadLastFileSet()
			{
				if(File.Exists(Utils.GetCurrentPath("hashlist.fscp")))
				{
					FileStream fStream = new FileStream(Utils.GetCurrentPath("hashlist.fscp"), FileMode.Open, FileAccess.Read);
					try
					{
						BinaryFormatter crip = new BinaryFormatter();
						lastFileSet = (Hashtable)crip.Deserialize(fStream);
						fStream.Close();
					}
					catch
					{
						fStream.Close();
						System.Diagnostics.Debug.WriteLine("LoadLastFileSet");
						try{File.Delete(Utils.GetCurrentPath("hashlist.fscp"));}
						catch{System.Diagnostics.Debug.WriteLine("Deleting hashlist.fscp");}
						HashEngine.Stop();
						System.Windows.Forms.Application.Exit();
					}
				}
			}

			public static void SaveLastFileSet()
			{
				//copy everything first
				CopyHashes();

				//normal procedure
				BinaryFormatter crip = new BinaryFormatter();
				FileStream fStream = new FileStream(Utils.GetCurrentPath("hashlist.fscp"), FileMode.Create, FileAccess.Write);
				crip.Serialize(fStream, lastFileSet);
				fStream.Close();
			}

			//this is used to stop running UpdateShares for an amount of time
			private static bool supUp = false;
			public static bool suppressUpdate
			{
				get
				{
					return supUp;
				}
				set
				{
					supUp = value;
					if(value)
						StopFSWs();
					else
						StartFSWs();
				}
			}
			/*
			 * flag used to prevent copying our filelist into lastFileSet during UpdateShares
			 * it's set to true because the first time we UpdateShares, we load lastFileSet
			 */
			public static bool suppressCopy = true;
			/*
			 * when the HashEngine doesn't finish, hashEngineAborted is set to true
			 * when true, this variable tells us that all the values in lastFileSet weren't copied
			 * because we'll clear them normally in CopyHashes, those hash values will have to be recalculated
			 * this boolean will allow the CopyHashes function to keep those hash values and reduce overhead later on
			 */
			public static bool hashEngineAborted = false;

			/// <summary>
			/// This function should be called any time the share list is changed.
			/// It should also be called if a change in files occurs (deletion, creation, etc.).
			/// It will scan all of the shared directories and record filenames.
			/// Then it will create an appropriate query routing table.
			/// </summary>
			public static void UpdateShares()
			{
				if(suppressUpdate)
					return;

				//dispose/disable any current file system watchers
				StopFSWs();

				//stop hashing for now
				HashEngine.Stop();
				while(HashEngine.IsAlive())
					System.Threading.Thread.Sleep(20);

				CopyHashes();

				//run the rest of the routine
				Gnutella.QueryRouteTable.PrepareTable();
				Gnutella2.QueryRouteTable.PrepareTable();
				lock(fileList)
					fileList.Clear();
				Updated.filesShared = 0;
				Updated.kbShared = 0;
				if(shareList.Count > 0)
					lock(shareList)
					{
						//loop through each shared directory
						foreach(object item in shareList)
						{
							try
							{
								//add all of the files in the directory to fileList
								string[] files = Directory.GetFiles(item.ToString());
								//loop through each file
								foreach(object file in files)
								{
									FileInfo inf = new FileInfo(file.ToString());
									FileObject fileProps = new FileObject();
									//record path and filesize
									fileProps.location = file.ToString();
									fileProps.lcaseFileName = Path.GetFileName(fileProps.location).ToLower();
									fileProps.b = (uint)inf.Length;
									fileProps.fileIndex = Updated.filesShared;
									fileProps.md4 = null;
									fileProps.sha1bytes = null;
									fileProps.sha1 = "";
									//update variables
									Updated.filesShared++;
									Updated.kbShared += Convert.ToInt32(Math.Round((double)fileProps.b / 1024));
									//add the class object to the ArrayList
									lock(fileList)
										fileList.Add(fileProps);
									Gnutella.QueryRouteTable.Add(Path.GetFileName(fileProps.location));
									Gnutella2.QueryRouteTable.AddWords(Path.GetFileName(fileProps.location));
								}
							}
							catch
							{
								System.Diagnostics.Debug.WriteLine("directory doesn't exist");
							}
						}
					}
				Gnutella.QueryRouteTable.CreatePatches();
				Gnutella.QueryRouteTable.SendQRTables();
				Gnutella2.QueryRouteTable.SendQRT();

				//let the GUI know that we altered the shared directories / files
				GUIBridge.ChangeShared();

				//start file system watchers
				StartFSWs();

				//restart what we ended before
				HashEngine.Start();

				//update the HomePage
				GUIBridge.RefreshHomePageLibrary();
			}

			/// <summary>
			/// Here we're going to take a shortcut.
			/// We don't want to redundantly generate sha1 values again and waste cpu.
			/// We'll copy the hashes we already have and then HashEngine will take care of them.
			/// </summary>
			public static void CopyHashes()
			{
				if(!suppressCopy)
				{
					try
					{
						lock(fileList)
						{
							if(!Stats.LoadSave.hashEngineAborted)
								lastFileSet.Clear();
							//find files that have hashes already calculated
							foreach(FileObject fo in fileList)
								if(fo.sha1 != "")
								{
									FileObject2 fo2 = new FileObject2();
									fo2.bytes = fo.b;
									fo2.md4 = fo.md4;
									fo2.sha1 = fo.sha1;
									fo2.sha1bytes = fo.sha1bytes;
									if(Stats.LoadSave.hashEngineAborted)
									{
										if(lastFileSet.ContainsKey(fo.location))
											lastFileSet[fo.location] = fo2;
										else
											lastFileSet.Add(fo.location, fo2);
									}
									else
										lastFileSet.Add(fo.location, fo2);
								}
						}
					}
					catch(Exception e)
					{
						System.Diagnostics.Debug.WriteLine("Stats CopyHashes: " + e.Message);
					}
					if(Stats.LoadSave.hashEngineAborted)
						Stats.LoadSave.hashEngineAborted = false;
				}
				else
					suppressCopy = false;
			}
		}

		/// <summary>
		/// Turn on the file system watchers.
		/// </summary>
		static void StartFSWs()
		{
			if(shareList.Count > 0)
			{
				//instantiate each watcher
				fsws = new FileSystemWatcher[shareList.Count];
				//setup everything accordingly
				for(int x = 0; x < shareList.Count; x++)
				{
					try
					{
						fsws[x] = new FileSystemWatcher(shareList[x].ToString());
						fsws[x].NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
						fsws[x].Created += new FileSystemEventHandler(OnChanged);
						fsws[x].Deleted += new FileSystemEventHandler(OnChanged);
						fsws[x].Renamed += new RenamedEventHandler(OnRenamed);
						fsws[x].Error += new ErrorEventHandler(OnError);
						fsws[x].EnableRaisingEvents = true;
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine("Stats UpdateShares system watchers");
					}
				}
			}
		}

		/// <summary>
		/// Turn off the file system watchers.
		/// </summary>
		public static void StopFSWs()
		{
			if(fsws != null)
				if(fsws.Length > 0)
					foreach(FileSystemWatcher watcher in fsws)
					{
						try
						{
							watcher.EnableRaisingEvents = false;
							watcher.Created -= new FileSystemEventHandler(OnChanged);
							watcher.Deleted -= new FileSystemEventHandler(OnChanged);
							watcher.Renamed -= new RenamedEventHandler(OnRenamed);
							watcher.Error -= new ErrorEventHandler(OnError);
							watcher.Dispose();
						}
						catch
						{
							System.Diagnostics.Debug.WriteLine("Stats UpdateShares watcher dispose");
						}
					}
		}

		/// <summary>
		/// A shared directory was probably deleted.
		/// </summary>
		static void OnError(object sender, ErrorEventArgs e)
		{
			LoadSave.UpdateShares();
		}

		/// <summary>
		/// A file/directory was added or deleted.
		/// </summary>
		static void OnChanged(object source, FileSystemEventArgs e)
		{
			LoadSave.UpdateShares();
		}

		/// <summary>
		/// A file/directory was renamed.
		/// </summary>
		static void OnRenamed(object source, RenamedEventArgs e)
		{
			LoadSave.UpdateShares();
		}

		static void tmrCheckDirs_Tick(object sender, ElapsedEventArgs e)
		{
			bool changeMade = false;
			lock(shareList)
			{
				if(shareList.Count > 0)
					for(int x = shareList.Count - 1; x >= 0; x--)
						if(!Directory.Exists((string)shareList[x]))
						{
							changeMade = true;
							shareList.RemoveAt(x);
						}
			}
			if(changeMade)
				LoadSave.UpdateShares();
		}

		static void tmrChores_Tick(object sender, ElapsedEventArgs e)
		{
			if(Updated.timestamp == int.MaxValue)
			{
				Updated.timestamp = int.MinValue;
				return;
			}
			Updated.timestamp++;
		}
	}
}
