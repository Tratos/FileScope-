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
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace FileScope.Gnutella2
{
	/// <summary>
	/// Condition of a Sck.
	/// </summary>
	public enum Condition
	{
		Closed,		//disconnected
		Connecting,	//connecting
		Connected,	//connected superficially: only at tcp/ip level
		hndshk,		//first handshake sent or received
		hndshk2,	//response handshake sent or received
		hndshk3		//final response handshake sent or received; this is the "official" connected state
	}

	/// <summary>
	/// Modes of operation.
	/// </summary>
	public enum G2Mode
	{
		Leaf,		//shielded leaf node
		Ultrapeer	//ultrapeer / supernode
	}

	/// <summary>
	/// This class is just like the Sck class in Gnutella1's namespace.
	/// This G2 bridge is just a wrapper over System.Net.Sockets.Socket.
	/// It follows a blocking socket model to implement hop flow.
	/// Connecting is the only asynchronous thing.
	/// WebCache connections are also spawned and managed here.
	/// Each instance of this class is reusable after a disconnect.
	/// </summary>
	public class Sck
	{
		//a max of 910 G2 sockets
		public static Sck[] scks = new Sck[910];

		public Socket sock1;							//socket instance
		public NetworkStream ns;						//used as wrapper over sock1; works with deflate algo
		public InflaterInputStream iis;					//used when decompressing incoming deflated data
		public DeflaterOutputStream dos;				//used when deflating outgoing data
		public int sockNum;								//socket identifier
		HttpWebRequest httpRequest;						//for connecting to web cache servers
		public byte[] sendBuff = new byte[4096];		//send buffer
		public byte[] inHeaderBuff = new byte[12];		//reusable incoming header buffer
		public ArrayList oBuf1 = new ArrayList();		//high priority send buffer (hop flow)
		public ArrayList oBuf2 = new ArrayList();		//low priority send buffer (hop flow)
		public GUIDitem gitem = null;					//host's guid
		public uint numFiles = 0, numKB = 0;			//number of host's shared files and library size in KB
		public ushort leaves = 0, maxleaves = 0;		//number of leaves and max number of leaves
		public ArrayList neighbors = new ArrayList();	//if the host is a hub, keep track of its neighboring hubs (IPEndPoints)
		public BitArray lastQHT = null;					//last qht sent
		public BitArray inQHT = null;					//last qht received
		public byte[] inRawQHT = null;					//raw qht in byte[]
		public int inRawQHTindex;						//position we're receiving at in inRawQHT
		public IPAddress remoteIPA;
		public string address;
		public int port = 6346;
		public volatile bool incoming;					//is it an incoming connection?
		public volatile G2Mode mode;					//remotehost's connection mode
		public volatile Condition state;				//state of the connection
		public volatile string vendor;					//vendor name
		public volatile bool addedHost = true;			//synchronization variable
		public volatile bool removedHost = true;		//synchronization variable
		public volatile bool deflateIn, deflateOut;		//which directions have compression enabled
		public volatile bool ultrapeerNeeded = true;	//does the host need an ultrapeer?
		public volatile bool browseHost = false;		//is this a connection for library browsing purposes?
		public volatile bool webCache;					//are we connecting to a web cache server?
		public volatile bool wcUpdate;					//are we updating a web cache server?
		public volatile int bytesOut=0;					//outgoing bytes sent in last second
		public volatile int bytesIn=0;					//incoming bytes received in last second
		public GoodTimer connectYet = new GoodTimer();	//12 seconds to connect w/ handshake
		public GoodTimer connband = new GoodTimer();	//check bandwidth and if connection's alive
		public System.Threading.Thread sendThread;		//thread that sends data
		public System.Threading.Thread receiveThread;	//thread that receives data
		public System.Threading.Thread webCacheThread;	//thread connecting to web cache

		public Sck(int sockNum)
		{
			//timers + event handlers
			connectYet.Interval = 10000;
			connectYet.AddEvent(new ElapsedEventHandler(connectYet_Tick));
			connband.Interval = 1000;
			connband.AddEvent(new ElapsedEventHandler(connband_Tick));
			//other stuff
			state = Condition.Closed;
			mode = G2Mode.Ultrapeer;
			this.sockNum = sockNum;
			this.ns = null;
			this.iis = null;
			this.dos = null;
			this.deflateIn = false;
			this.deflateOut = false;
			//threads
			sendThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncSend));
			sendThread.IsBackground = true;
			sendThread.Start();
			receiveThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncRecv));
			receiveThread.IsBackground = true;
			receiveThread.Start();
		}

		/// <summary>
		/// Outgoing connection starts here.
		/// </summary>
		public void Reset(string addr)
		{
			try
			{
				lock(this)
				{
					if(state != Condition.Closed)
						return;
					while(!removedHost)
						System.Threading.Thread.Sleep(10);
					sock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					state = Condition.Connecting;
					//reset these
					mode = G2Mode.Ultrapeer;
					ultrapeerNeeded = true;
					incoming = false;
					webCache = false;
					vendor = "Unknown";
					addedHost = false;
					//parse *.*.*.*:* into IP address and port
					Utils.AddrParse(addr, out address, out port, 6346);
					if(!this.browseHost)
						GUIBridge.G2NewConnection(addr, this.sockNum);
					IPHostEntry IPHost = Dns.GetHostByName(address);
					IPEndPoint endPoint = new IPEndPoint(IPHost.AddressList[0], port);
					this.remoteIPA = endPoint.Address;
					//connect
					sock1.BeginConnect(endPoint, new AsyncCallback(OnConnect), sock1);
					connband.Start();
					connectYet.Start();
				}
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
				lock(this)
				{
					if(state != Condition.Closed)
						throw new Exception("G2 ResetIncoming state problem");
					while(!removedHost)
						System.Threading.Thread.Sleep(10);
					sock1 = tmpSock;
					ultrapeerNeeded = true;
					incoming = true;
					webCache = false;
					vendor = "Unknown";
					state = Condition.Connected;
					addedHost = false;
					if(!browseHost)
						GUIBridge.G2NewConnection(RemoteIP(), this.sockNum);
					connband.Start();
					connectYet.Start();
				}
				if(!StartStop.enabled && !browseHost)
				{
					Disconnect("g2 is turned off");
					return;
				}
				if(!browseHost)
					if(ConnectionManager.DirectlyConnected(((IPEndPoint)tmpSock.RemoteEndPoint).Address))
					{
						Disconnect("one connection per IP allowed");
						return;
					}
				lock(this)
				{
					HandshakeMsg msg = new HandshakeMsg();
					msg.handshake = Encoding.ASCII.GetString(bytesMsg);
					if(msg.handshake.IndexOf("\r\n\r\n") == -1)
					{
						System.Diagnostics.Debug.WriteLine("G2 initial incoming handshake is truncated: " + msg.handshake);
						Disconnect("ResetIncoming truncated handshake");
						return;
					}
					if(!browseHost)
						Handshake.Incoming(this, msg);
					else
					{
						//make sure it's g2
						if(msg.handshake.ToLower().IndexOf("application/x-gnutella2") == -1)
						{
							Disconnect("not g2 browse host: " + msg.handshake);
							return;
						}
						this.state = Condition.hndshk3;
						JustFinishedHandshake();
						//create the data
						byte[] dataQH2s;
						OQH2 oqh2 = new OQH2();
						oqh2.searchGUID = GUID.newGUID();
						oqh2.searchGUIDoffset = 0;
						oqh2.interests.all = true;
						oqh2.matches = Stats.fileList;
						oqh2.BrowseHostData(out dataQH2s);
						//create the initial response
						string resp = "HTTP/1.1 200 OK\r\n";
						resp += "Server: FileScope " + Stats.version + "\r\n";
						resp += "Remote-IP: " + RemoteIP() + "\r\n";
						resp += "Connection: Close\r\n";
						resp += "Content-Type: application/x-gnutella2\r\n";
						resp += "Content-Length: " + dataQH2s.Length.ToString() + "\r\n\r\n";
						byte[] bytResp = Encoding.ASCII.GetBytes(resp);
						SendData(bytResp, 0, bytResp.Length);
						if(dataQH2s.Length > 0)
							SendData(dataQH2s, 0, dataQH2s.Length);
						Disconnect("done with browse host");
					}
				}
			}
			catch(Exception excp23)
			{
				if(this.browseHost)
				{
					System.Diagnostics.Debug.WriteLine("incoming g2 browse host problem: " + excp23.Message.ToString());
					System.Diagnostics.Debug.WriteLine(excp23.StackTrace);
				}
				else
					Utils.Diag(excp23.ToString());
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
				lock(this)
				{
					if(state != Condition.Closed)
						return;
					state = Condition.Connecting;
					mode = G2Mode.Ultrapeer;
					incoming = false;
					webCache = true;
					vendor = "WebCache";
					address = server;
					//notify UI thread of this
					GUIBridge.G2NewConnection(server, this.sockNum);
					connectYet.Start();
					webCacheThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncWebCache));
					webCacheThread.Start();
					//update or request
					if(ConnectionManager.ultrapeers.Count > 3 && ConnectionManager.leaves.Count > 30 && HostCache.recentHubs.Count > 30)
						wcUpdate = true;
					else
						wcUpdate = false;
				}
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
				if(wcUpdate)
					httpRequest = (HttpWebRequest)WebRequest.Create(address + "?client=FSCP" + Stats.version.Substring(0, 3) + "&net=gnutella2&update=1&ip=" + Stats.Updated.myIPA.ToString() + @"%3A" + Stats.settings.port.ToString());
				else
					httpRequest = (HttpWebRequest)WebRequest.Create(address + "?client=FSCP" + Stats.version.Substring(0, 3) + "&net=gnutella2&get=1");
				httpRequest.UserAgent = "FileScope";
				//begin asynchronous get request
				httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), httpRequest);
			}
			catch
			{
				Disconnect("FuncWebCache");
			}
		}

		/// <summary>
		/// Response from web cache.
		/// </summary>
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
				//if we updated
				if(host.IndexOf("|update|") != -1)
				{
					if(host.ToLower().IndexOf("warning") != -1)
						System.Diagnostics.Debug.WriteLine("GWC2 update warning: " + host);
					goto donewithhosts;
				}
				int overflowCheck = 0;
				//one per customer
				bool hostAdded = false;
				while(host != null)
				{
					//used for security purposes
					overflowCheck++;
					if(overflowCheck > 50)
						break;

					if(host.Length > 9 && host[0] == 'h' && host[1] == '|')
					{
						string elHost = host.Substring(2, host.IndexOf('|', 5)-2);
						ushort elPort;
						int colon = elHost.IndexOf(":");
						if(colon == -1)
							elPort = 6346;
						else
						{
							elPort = Convert.ToUInt16(elHost.Substring(colon+1, elHost.Length-colon-1));
							elHost = elHost.Substring(0, colon);
						}
						IPAddress ipa = IPAddress.Parse(elHost);
						HubInfo hi = new HubInfo();
						hi.port = elPort;
						hi.timeKnown = 0;
						HostCache.AddRecentAndCache(ipa, hi);
					}
					else if(host.Length > 9 && host[0] == 'u' && host[1] == '|' && !hostAdded)
					{
						object strwc = host.Substring(2, host.IndexOf('|', 5)-2);
						lock(Stats.gnutella2WebCache)
							if(!Stats.gnutella2WebCache.Contains(strwc) && Stats.gnutella2WebCache.Count < 60)
							{
								Stats.gnutella2WebCache.Add(strwc);
								hostAdded = true;
							}
					}
					host = stream.ReadLine();
				}
			donewithhosts:
				stream.Close();
				Disconnect("webcache done");
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("g2 webcache: " + e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
				bool remove = false;
				if(e.Message.IndexOf("404") != -1 || e.Message.IndexOf("403") != -1)
					remove = true;
				if(remove || !removedWebCacheToday)
				{
					lock(Stats.gnutella2WebCache)
					{
						for(int posgwc = 0; posgwc < Stats.gnutella2WebCache.Count; posgwc++)
							if((string)Stats.gnutella2WebCache[posgwc] == this.address)
							{
								System.Diagnostics.Debug.WriteLine("gwc2 removed: " + this.address);
								Stats.gnutella2WebCache.RemoveAt(posgwc);
								removedWebCacheToday = true;
								return;
							}
					}
				}
				Disconnect("OnGetResponse");
			}
		}

		//we use this flag to prevent too many webcaches to be removed
		static bool removedWebCacheToday = false;

		void OnConnect(IAsyncResult ar)
		{
			try
			{
				lock(this)
				{
					if(state != Condition.Connecting)
						return;

					Socket tmpSock = (Socket)ar.AsyncState;
					tmpSock.EndConnect(ar);

					state = Condition.Connected;
					//start handshake
					if(!this.browseHost)
					{
						OMessage msg = Handshake.Outgoing(this, false);
						SendPacket(msg);
					}
					else
					{
						string req = "GET / HTTP/1.1\r\n";
						req += "User-Agent: FileScope " + Stats.version + "\r\n";
						req += "Accept: application/x-gnutella2\r\n";
						req += "Connection: Close\r\n";
						req += "Host: " + address + ":" + port.ToString() + "\r\n\r\n";
						byte[] bytReq = Encoding.ASCII.GetBytes(req);
						SendData(bytReq, 0, bytReq.Length);
					}
				}
			}
			catch// (Exception e)
			{
				Disconnect("OnConnect routine");
			}
		}

		public void SendPacket(OMessage msg)
		{
			if(state == Condition.Closed || this.webCache)
				return;

			lock(oBuf1)
				oBuf1.Add(msg);
		}

		public void SendPacket2(OMessage msg)
		{
			if(state == Condition.Closed || this.webCache)
				return;

			lock(oBuf2)
				oBuf2.Add(msg);
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
				//make sure we're connected first
				if(state == Condition.Closed || state == Condition.Connecting)
				{
					System.Threading.Thread.Sleep(200);
					continue;
				}

				//keep track of index in the buffer
				int buffIndex = 0;
				ArrayList alBuf = oBuf1;

			oBufs:

				lock(alBuf)
					for(int x = 0; x < alBuf.Count; x++)
					{
						OMessage pckt = (OMessage)alBuf[0];
						try
						{
							bool contin = pckt.FillSendBuff(this, ref buffIndex);
							if(!contin)
								goto ready;
							alBuf.RemoveAt(0);
						}
						catch(Exception e)
						{
							System.Diagnostics.Debug.WriteLine("G2 FuncSend: " + e.Message);
							System.Diagnostics.Debug.WriteLine(e.StackTrace);
							Disconnect("FuncSend");
							if(sendBuff.Length != 4096)
								sendBuff = new byte[4096];
							buffIndex = 0;
							goto ready;
						}
					}
				if(alBuf == oBuf1)
				{
					//we just did high priority, we'll jump to the next priority
					alBuf = oBuf2;
					goto oBufs;
				}

			ready:

				//anything to send?
				if(buffIndex > 0)
				{
					SendData(sendBuff, 0, buffIndex);
					if(sendBuff.Length != 4096)
						sendBuff = new byte[4096];
					//cleanup anything that's not being sent
					lock(oBuf1)
					{
						if(oBuf1.Count > 300)
						{
							oBuf1.RemoveRange(0, 20);
							System.Diagnostics.Debug.WriteLine("g2 oBuf1 trimming");
						}
					}
					lock(oBuf2)
					{
						if(oBuf2.Count > 300)
						{
							oBuf2.RemoveRange(0, 20);
							System.Diagnostics.Debug.WriteLine("g2 oBuf2 trimming");
						}
					}
				}
				else
					System.Threading.Thread.Sleep(150);
			}
		}

		//used below
		public byte[] dbgpckt;

		/// <summary>
		/// Blocking receive thread.
		/// </summary>
		void FuncRecv()
		{
			int bytesRec;
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

				try
				{
					bool process = true;
					Message msg;
					if(state != Condition.hndshk3)
					{
						msg = new HandshakeMsg();
						((HandshakeMsg)msg).GetMsg(this);
					}
					else
					{
						//receive data
						bytesRec = ReceiveData(this.inHeaderBuff, 0, 1);
						if(bytesRec == 0)
						{
							Disconnect("nethin' received (G2)");
							return;
						}

						//control byte
						int llen = (int)this.inHeaderBuff[0] >> 6;
						int nlen = ((int)(this.inHeaderBuff[0] & 0x38) >> 3) + 1;

						//receive data
						bytesRec = ReceiveData(this.inHeaderBuff, 1, llen+nlen);
						if(bytesRec == 0)
						{
							Disconnect("nethin' received (G2)");
							return;
						}

						//determine the root packet type
						string name = Encoding.ASCII.GetString(this.inHeaderBuff, 1+llen, nlen);
						msg = Message.RootMsgType(ref name);
						if(msg == null || msg.GetType() == typeof(Message))
						{
//							System.Diagnostics.Debug.WriteLine("unknown G2 root packet: " + name);
							process = false;
						}

						if(process)
						{
							//more of control byte
							msg.cf = ((this.inHeaderBuff[0] & 0x04) == 0x04);
							msg.be = ((this.inHeaderBuff[0] & 0x02) == 0x02);

#if DEBUG
							//cause we haven't seen one before
							if(msg.be)
								System.Diagnostics.Debug.WriteLine("some host is sending BigEndian on G2");
#endif

							//length
							if(llen == 0)
								msg.payLen = 0;
							else
								msg.payLen = Endian.VarBytesToInt(this.inHeaderBuff, 1, llen, msg.be);

							//get the rest
							msg.GetMsg(this);

#if DEBUG_PACKETS
							if(msg.payLen > 0)
							{
								this.dbgpckt = new byte[msg.inPayload.Length];
								Array.Copy(msg.inPayload, 0, this.dbgpckt, 0, this.dbgpckt.Length);
							}
#endif
						}
						else if(llen > 0)
						{
							int payLen = Endian.VarBytesToInt(this.inHeaderBuff, 1, llen, ((this.inHeaderBuff[0] & 0x02) == 0x02));
							byte[] temp = new byte[payLen];
							bytesRec = ReceiveData(temp, 0, payLen);
							if(bytesRec == 0)
							{
								Disconnect("nethin' received (G2)");
								return;
							}
						}
					}
					if(msg is HandshakeMsg)
					{
						if(!this.browseHost)
							Handshake.Incoming(this, (HandshakeMsg)msg);
						else
						{
							if(((HandshakeMsg)msg).handshake.ToLower().IndexOf("200 ok") != -1)
							{
								this.state = Condition.hndshk3;
								this.JustFinishedHandshake();
							}
							else
								Disconnect("browse host request failed: " + ((HandshakeMsg)msg).handshake);
						}
					}
					else
					{
						//msg.Read(0);	//output children
						if(!process)
							process = true;
						else
							G2Data.ProcessMessage(msg);
						if(this.browseHost && msg.GetType() == typeof(QueryHit))
						{
							System.Threading.Thread.Sleep(200);
							this.Disconnect("done browsing");
						}
					}
				}
				catch(Exception e)
				{
					e=e;
#if DEBUG
					if(this.state == Condition.hndshk3)
					//if(this.state != Condition.Closed)
					{
						System.Diagnostics.Debug.WriteLine("G2 FuncRecv: (" + this.sockNum.ToString() + ") " + e.Message);
						System.Diagnostics.Debug.WriteLine(e.StackTrace);
					}
#endif
					Disconnect("FuncRecv");
				}
			}
		}

		public bool allow1withoutStream = false;

		public void SendData(byte[] byt, int loc, int len)
		{
			try
			{
				if(this.ns != null && !allow1withoutStream)
				{
					if(dos != null)
						dos.Write(byt, loc, len);
					else
						ns.Write(byt, loc, len);
				}
				else
				{
					sock1.Send(byt, loc, len, SocketFlags.None);
					allow1withoutStream = false;
				}
				if(browseHost)
					Stats.Updated.outTransferBandwidth += len;
				else
					Stats.Updated.outNetworkBandwidth += len;
				bytesOut += len;
			}
			catch(Exception e)
			{
				Disconnect("g2 send error: " + e.Message);
			}
		}

		public int ReceiveData(byte[] byt, int loc, int len)
		{
			int bytesRec;
			while(true)
			{
				if(this.ns != null)
				{
					if(iis != null)
					{
						try
						{
							bytesRec = iis.Read(byt, loc, len);
							if(bytesRec == 0)
							{
								System.Diagnostics.Debug.WriteLine("g2: deflate stream returned zero bytes?");
								return 0;
							}
							else if(bytesRec == -5)
							{
								System.Diagnostics.Debug.WriteLine("g2: deflate stream ended");
								return 0;
							}
						}
						catch
						{
							if(!Stats.Updated.closing)
								System.Diagnostics.Debug.WriteLine("g2: deflate stream is corrupt");
							return 0;
						}
					}
					else
						bytesRec = ns.Read(byt, loc, len);
				}
				else
				{
					bytesRec = sock1.Receive(byt, loc, len, SocketFlags.None);
				}
				if(browseHost)
					Stats.Updated.inTransferBandwidth += bytesRec;
				else
					Stats.Updated.inNetworkBandwidth += bytesRec;
				bytesIn += bytesRec;
				if(bytesRec == len || bytesRec == 0)
					return bytesRec;
				else
				{
					len -= bytesRec;
					loc += bytesRec;
				}
			}
		}

		public int ReceiveData(byte[] byt)
		{
			int bytesRec;
			if(this.ns != null)
			{
				System.Diagnostics.Debug.WriteLine("G2 ReceiveData ns should have been null");
				bytesRec = 0;
			}
			else
			{
				bytesRec = sock1.Receive(byt);
			}
			if(browseHost)
				Stats.Updated.inTransferBandwidth += bytesRec;
			else
				Stats.Updated.inNetworkBandwidth += bytesRec;
			bytesIn += bytesRec;
			return bytesRec;
		}

		public void Disconnect(string why)
		{
#if DEBUG
			if(why != "webcache done" && why.IndexOf("connectYet_Tick") == -1 && why.IndexOf("OnConnect routine") == -1)
				System.Diagnostics.Debug.WriteLine("(" + this.sockNum.ToString() + ") " + why);
#endif

			bool removeHost = false;
			lock(this)
			{
				//if(state == Condition.hndshk3)
				//	System.Diagnostics.Debug.WriteLine("--Disconnected--" + sockNum.ToString() + "--" + why);
				if(state == Condition.Closed)
					return;
				if(!browseHost && !Stats.Updated.Gnutella2.ultrapeer)
					OQH2.ResetStaticPart();
				if(state == Condition.hndshk3 && !this.browseHost)
					removeHost = true;
				//wait until we add the host before removing it
				if(removeHost)
				{
					while(true)
					{
						if(addedHost)
							break;
						else
							System.Threading.Thread.Sleep(10);
					}
					removedHost = false;
				}
				this.state = Condition.Closed;
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
					if(!Stats.Updated.closing && !this.browseHost)
						GUIBridge.G2JustDisconnected(this.sockNum);
					connectYet.Stop();
					connband.Stop();
					deflateIn = false;
					deflateOut = false;
					allow1withoutStream = false;
					sendLNIandKHL = false;
					ultrapeerNeeded = true;
					browseHost = false;
					addedHost = true;
					lastQHT = null;
					inQHT = null;
					inRawQHT = null;
					numFiles = 0;
					numKB = 0;
					leaves = 0;
					maxleaves = 0;
					bytesOut = 0;
					bytesIn = 0;
					wcUpdate = false;
					lock(oBuf1)
						oBuf1.Clear();
					lock(oBuf2)
						oBuf2.Clear();
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("g2 disconnect problem: " + e.Message);
				}
				try
				{
					if(dos != null)
					{
						dos.Close();
						dos = null;
					}
					if(iis != null)
					{
						iis.Close();
						iis = null;
					}
					if(ns != null)
					{
						ns.Close();
						ns = null;
					}
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
					System.Diagnostics.Debug.WriteLine("Gnutella2 Sck sock1 shutdown: " + e.Message);
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
			}
			if(removeHost)
			{
				ConnectionManager.G2Host g2h = new ConnectionManager.G2Host();
				g2h.sockNum = this.sockNum;
				ConnectionManager.RemoveG2Host(g2h, mode);
			}
			remoteIPA = null;
			lock(HostCache.recentHubs)
				neighbors.Clear();
			lock(Router.htRoutes)
			{
				if(this.gitem != null)
				{
					try
					{
						Router.htRoutes.Remove(this.gitem);
						this.gitem = null;
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine("g2 sck problem removing gitem");
						this.gitem = null;
					}
				}
			}
			removedHost = true;
		}

		/// <summary>
		/// Return a Sck not in use.
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
			try
			{
				IPEndPoint ipep = (IPEndPoint)sock1.RemoteEndPoint;
				this.remoteIPA = ipep.Address;
				string tempEnd = ipep.Address.ToString();
				//no error by now, copy the contents
				this.address = tempEnd;
				return tempEnd;
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("g2 Sck RemoteIP");
				return "";
			}
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
		/// Spawn an outgoing connection for the sole purpose of a browse host.
		/// </summary>
		public static void OutgoingBrowseHost(string server)
		{
			int tmpSckNum = GetSck();
			Sck.scks[tmpSckNum].browseHost = true;
			Sck.scks[tmpSckNum].Reset(server);
		}

		/// <summary>
		/// Spawn outgoing connection to a web cache server.
		/// </summary>
		public static void OutgoingWebCache(string server)
		{
			scks[GetSck()].ResetWebCache(server);
		}

		/// <summary>
		/// This is called after the transition to hndshk3 state is over.
		/// </summary>
		public void JustFinishedHandshake()
		{
			lock(this)
			{
				connectYet.Stop();
				if(state == Condition.Closed)
					return;
				//we don't need to go further if it's browseHost
				if(browseHost)
					return;
				//we use this wrapper to make things easier
				this.ns = new NetworkStream(this.sock1, FileAccess.ReadWrite, false);
				if(this.deflateIn)
					this.iis = new InflaterInputStream(this.ns);
				if(this.deflateOut)
					this.dos = new DeflaterOutputStream(this.ns);
			}
			lock(HostCache.recentHubs)
			{
				ConnectionManager.G2Host g2h = new ConnectionManager.G2Host();
				g2h.sockNum = this.sockNum;
				ConnectionManager.AddG2Host(g2h, mode);
				GUIBridge.G2JustConnected(this.sockNum);
				this.addedHost = true;
			}
			lock(this)
			{
				//something could have happened since we released the lock
				if(this.state != Condition.hndshk3)
					return;
				System.Threading.Thread.Sleep(300);
				if(!Stats.Updated.udpIncoming && !Stats.Updated.Gnutella2.ultrapeer && this.mode == G2Mode.Ultrapeer)
				{
					OUdpPing oup = new OUdpPing();
					Array.Copy(Endian.GetBytes(Stats.Updated.myIPA), 0, oup.endpoint, 0, 4);
					Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, !Stats.Updated.le), 0, oup.endpoint, 4, 2);
					SendPacket(oup);
				}
				else
					SendPacket2(new OKeepAlivePing());
				if(this.mode == G2Mode.Ultrapeer)
					Search.NewDirectHub(this.sockNum);
				sendLNIandKHL = true;
			}
			//send qht while we're at it
			QueryRouteTable.FillWithPatches(this);
		}

		void connectYet_Tick(object sender, ElapsedEventArgs e)
		{
			connectYet.Stop();
			this.Disconnect("connectYet_Tick");
		}

		bool sendLNIandKHL = false;

		void connband_Tick(object sender, ElapsedEventArgs e)
		{
			if(state == Condition.Closed || state == Condition.Connecting || this.browseHost)
				return;

			//lni + khl
			int randres;
			if(Stats.Updated.Gnutella2.ultrapeer)
				randres = GUID.rand.Next(0, 40);
			else
				randres = GUID.rand.Next(0, 90);
			if((randres == 1 || sendLNIandKHL) && state == Condition.hndshk3)
			{
				sendLNIandKHL = false;
				SendPacket2(new OLNI());
				SendPacket2(new OKHL());
			}

			//pings
			if(GUID.rand.Next(0, 25) == 0 && state == Condition.hndshk3)
				SendPacket2(new OKeepAlivePing());

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
			GUIBridge.G2Update(this.sockNum, ref kbInVal, ref kbOutVal);
		}
	}
}
