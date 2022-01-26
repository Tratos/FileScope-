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

namespace FileScope.EDonkey
{
	/// <summary>
	/// Condition of a Sck.
	/// </summary>
	public enum Condition
	{
		Closed,			//disconnected socket
		Connecting,		//connecting to server
		Connected		//connected to server
	}

	/// <summary>
	/// Socket class dedicated to connecting to eDonkey networks and/or conducting file transfers.
	/// Each instance of this class has an instance of a blocking System.Net.Sockets.Socket.
	/// This class is reusable after a disconnect.
	/// </summary>
	public class Sck
	{
		//a max of 50,000 edonkey related connections
		public static Sck[] scks = new Sck[50000];
		//nick name for the filescope session
		public static string nick = "";
		//eDonkey clientID
		public static uint clientID = 0;

		public Socket sock1;							//socket instance
		public int sockNum;								//socket identifier
		public volatile Condition state;				//state of the connection
		public GoodTimer connectYet = new GoodTimer();	//10 seconds to connect to server
		public GoodTimer aliveTmr = new GoodTimer();	//40 seconds to check connection
		public Queue sendQueue = new Queue();			//queue of packets to send to server
		public System.Threading.Thread sendThread;		//thread for sending
		public System.Threading.Thread recvThread;		//thread for receiving
		public bool alive = false;						//keep track of connection life
		public bool server;								//true if server, false if client
		public int dmNum = -1, dlNum = -1;				//used when we're dealing with downloads
		public int upNum = -1;							//used when we're dealing with uploads
		public bool incoming;
		public string address;
		public int port;

		public Sck(int sckIndex)
		{
			//create a nick if one doesn't exist
			if(Sck.nick == "")
			{
				int num1 = GUID.rand.Next(10, 2000000000);
				Sck.nick = "FileScope" + num1.ToString();
			}

			connectYet.Interval = 10000;
			connectYet.AddEvent(new ElapsedEventHandler(connectYet_Tick));
			aliveTmr.Interval = 40000;
			aliveTmr.AddEvent(new ElapsedEventHandler(aliveTmr_Tick));

			this.sockNum = sckIndex;
			this.state = Condition.Closed;

			sendThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncSend));
			sendThread.IsBackground = true;
			sendThread.Start();
			recvThread = new System.Threading.Thread(new System.Threading.ThreadStart(FuncRecv));
			recvThread.IsBackground = true;
			recvThread.Start();
		}

		public void Reset(string server, bool isServer)
		{
			try
			{
				alive = false;
				aliveTmr.Start();
				sock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				state = Condition.Connecting;
				incoming = false;
				this.server = isServer;
				//parse *.*.*.*:* into IP address and port
				Utils.AddrParse(server, out address, out port, 4661);
				//asynchronous get host by name
				if(Utils.IsHostName(address))
					Dns.BeginGetHostByName(address, new AsyncCallback(OnGetHostByName), null);
				else
				{
					IPHostEntry IPHost = Dns.GetHostByName(address);
					BeginConnection(IPHost.AddressList[0]);
				}
			}
			catch
			{
				Disconnect("Reset");
			}
		}

		public void ResetIncoming(Socket elsck, Packet pckt)
		{
			try
			{
				if(elsck == null)
					System.Diagnostics.Debug.WriteLine("eDonkey ResetIncoming problem");
				alive = false;
				aliveTmr.Start();
				sock1 = elsck;
				state = Condition.Connected;
				incoming = true;
				server = false;
				//process the message
				ProcessData.NewData(pckt, this.sockNum);
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("eDonkey ResetIncoming: " + e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
				Disconnect("ResetIncoming");
			}
		}

		void OnGetHostByName(IAsyncResult ar)
		{
			try
			{
				IPHostEntry ipHost = Dns.EndGetHostByName(ar);
				BeginConnection(ipHost.AddressList[0]);
			}
			catch
			{
				Disconnect("OnGetHostByName");
			}
		}

		void BeginConnection(IPAddress ip)
		{
			try
			{
				this.address = ip.ToString();
				IPEndPoint endPoint = new IPEndPoint(ip, port);
				sock1.BeginConnect(endPoint, new AsyncCallback(OnConnect), sock1);
				connectYet.Start();
			}
			catch
			{
				Disconnect("BeginConnection");
			}
		}

		void OnConnect(IAsyncResult ar)
		{
			//make sure we're not already connected
			if(state == Condition.Connected)
				return;
			if(state != Condition.Connecting)
				return;

			try
			{
				Socket tmpSock = (Socket)ar.AsyncState;
				tmpSock.EndConnect(ar);

				connectYet.Stop();
				if(server)
				{
					lock(this)
					{
						state = Condition.Connected;
						Stats.Updated.EDonkey.lastConnectionCount++;
					}
				}
				else
					state = Condition.Connected;
				//update gui
				GUIBridge.EJustConnected(sockNum);
				//some login procedures
				//System.Threading.Thread.Sleep(2000);
				Messages.Login(sockNum);
				if(server)
					Messages.GetServerList(sockNum);
			}
			catch
			{
				Disconnect("OnConnect");
			}
		}

		void FuncSend()
		{
			while(true)
			{
				if(Stats.Updated.closing)
					return;
				if(this.state != Condition.Connected)
				{
					System.Threading.Thread.Sleep(200);
					continue;
				}

				//send packets
				while(sendQueue.Count > 0)
				{
					Packet pckt;
					lock(sendQueue)
						pckt = (Packet)sendQueue.Dequeue();
					try
					{
						sock1.Send(pckt.outHeader);
						//we gotta specify byte offsets cause we use MemoryStream.GetBuffer()
						sock1.Send(pckt.outPayload, pckt.outpayLen, SocketFlags.None);
						if(!this.server)
							Stats.Updated.outTransferBandwidth += (pckt.outHeader.Length + pckt.outpayLen);
						else
							Stats.Updated.outNetworkBandwidth += (pckt.outHeader.Length + pckt.outpayLen);
						alive = true;
					}
					catch
					{
						Disconnect("FuncSend");
						continue;
					}
				}
				System.Threading.Thread.Sleep(200);
			}
		}

		void FuncRecv()
		{
			while(true)
			{
				if(Stats.Updated.closing)
					return;
				bool connected = IsConnected();
				if(!connected)
				{
					System.Threading.Thread.Sleep(200);
					continue;
				}

				//receive packets
				Packet packt = new Packet(this);
				alive = true;
				try
				{
					if(packt.good)
					{
						if(this.dlNum != -1 && this.dmNum != -1)
						{
							if(DownloadManager.dms[dmNum] != null)
							{
								lock(DownloadManager.dms[dmNum].endpoints)
									ProcessData.NewData(packt, this.sockNum);
							}
							else
								ProcessData.NewData(packt, this.sockNum);
						}
						else
							ProcessData.NewData(packt, this.sockNum);
					}
					else
						Disconnect("FuncRec");
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("eDonkey FuncRecv: " + e.Message);
					System.Diagnostics.Debug.WriteLine(e.StackTrace);
					Disconnect("FuncRecv");
				}
				System.Threading.Thread.Sleep(200);
			}
		}

		bool IsConnected()
		{
			if(this.dlNum != -1 && this.dmNum != -1)
				if(DownloadManager.dms[dmNum] != null)
				{
					lock(DownloadManager.dms[dmNum].endpoints)
						return (this.state == Condition.Connected);
				}

			lock(this)
				return (this.state == Condition.Connected);
		}

		bool dlDisconnect = false;
		bool upDisconnect = false;

		/// <summary>
		/// Only disconnect the download portion.
		/// </summary>
		public void DLend()
		{
			dlDisconnect = true;
			Disconnect("DLend");
		}

		/// <summary>
		/// Only disconnect the upload portion.
		/// </summary>
		public void UPend()
		{
			upDisconnect = true;
			Disconnect("UPend");
		}

		public void Disconnect(string what)
		{
			//System.Diagnostics.Debug.WriteLine("eDonkey(" + this.sockNum.ToString() + "): " + what);
			if(this.dlNum != -1 && this.dmNum != -1)
				if(DownloadManager.dms[dmNum] != null)
				{
					lock(DownloadManager.dms[dmNum].endpoints)
						Disconnect(true);
					return;
				}

			lock(this)
				Disconnect(false);
		}

		/// <summary>
		/// Disconnect from this EDonkey server.
		/// </summary>
		public void Disconnect(bool DLlocks)
		{
			aliveTmr.Stop();
			if(state == Condition.Closed)
			{
				dlDisconnect = false;
				upDisconnect = false;
				return;
			}
			//these indicates which transfer objects we should release
			bool closeDL = false;
			bool closeUP = false;
			if(!dlDisconnect && !upDisconnect)
				closeDL = closeUP = true;
			else
			{
				closeDL = dlDisconnect;
				closeUP = upDisconnect;
			}
			//indicates whether this actual Sck should also be disconnected
			bool closeSck = false;
			if(closeDL && closeUP)
				closeSck = true;
			if(closeDL && upNum == -1)
				closeSck = true;
			if(closeUP && dlNum == -1 && dmNum == -1)
				closeSck = true;
			//if Uploader's StopEverything() called us, then we won't call it back
			if(upDisconnect == true && dlDisconnect == false && upNum != -1)
				upNum = -1;
			//reset these two
			dlDisconnect = false;
			upDisconnect = false;

			if(server)
			{
				GUIBridge.EJustDisconnected(sockNum);
				if(this.state == Condition.Connected)
					Stats.Updated.EDonkey.lastConnectionCount--;
			}

			//dler
			if(dmNum != -1 && dlNum != -1 && !server && closeDL)
				if(DownloadManager.dms[dmNum] != null)
					if(DownloadManager.dms[dmNum].downloaders[dlNum] != null)
					{
						Downloader dler = (Downloader)DownloadManager.dms[dmNum].downloaders[dlNum];
						if(dler.edsck != null)
							dler.edsck = null;
						//if we're uploading, we'll do some cleanup stuff
						if(dler.lastMessage == "Queued" && upNum != -1 && !closeUP)
							Messages.SlotRelease(this.sockNum);
						if(dler.state == DLState.Downloading && upNum != -1 && !closeUP)
							Messages.EndOfDownload(dler.qho.md4sum, this.sockNum);
						dler.Disconnect("eDonkey");
						dlNum = -1;
						dmNum = -1;
					}

			//uper
			if(upNum != -1 && !server && closeUP)
			{
				if(UploadManager.ups[upNum] != null)
				{
					Uploader uper = (Uploader)UploadManager.ups[upNum];
					if(uper.edsck != null)
						uper.edsck = null;
					uper.StopEverything("edsck");
					upNum = -1;
				}
			}

			//sck
			if(!closeSck)
				return;
			try
			{
				state = Condition.Closed;
				connectYet.Stop();
				if(sock1 != null)
				{
					if(sock1.Connected)
						sock1.Shutdown(SocketShutdown.Both);
					sock1.Close();
					sock1 = null;
				}
				lock(sendQueue)
					sendQueue.Clear();
				if(Stats.Updated.closing)
				{
					try
					{if(sendThread != null) if(sendThread.IsAlive) sendThread.Abort();}
					catch(System.Threading.ThreadAbortException tae)
					{tae=tae;}
					catch(Exception e)
					{System.Diagnostics.Debug.WriteLine("eDonkey sendThread: " + e.Message);}
					try
					{if(recvThread != null) if(recvThread.IsAlive) recvThread.Abort();}
					catch(System.Threading.ThreadAbortException tae)
					{tae=tae;}
					catch(Exception e)
					{System.Diagnostics.Debug.WriteLine("eDonkey recvThread: " + e.Message);}
				}
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("EDonkey Sck Disconnect sock1 shutdown " + e.StackTrace);
			}
		}

		/// <summary>
		/// Send a packet to the EDonkey server.
		/// </summary>
		public void SendPacket(Packet packet)
		{
			lock(this.sendQueue)
				this.sendQueue.Enqueue(packet);
		}

		/// <summary>
		/// Spawn an outgoing connection to an eDonkey server.
		/// </summary>
		public static void Outgoing(string server)
		{
			try
			{
				scks[GetSck()].Reset(server, true);
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("eDonkey Outgoing: " + e.Message);
			}
		}

		/// <summary>
		/// Spawn a client->client connection for the purpose of a download.
		/// </summary>
		public static Sck DLOutgoing(int dmNum, int dlNum, string server)
		{
			//check to see if we're already connected to the server
			string justIP = server.Substring(0, server.IndexOf(":"));
			int sckNum = -1;
			for(int x = 0; x < scks.Length; x++)
				if(scks[x] != null)
					if(scks[x].state == Condition.Connected && !scks[x].server)
						if(scks[x].RemoteIP() == justIP)
						{
							if(scks[x].dlNum == -1 && scks[x].dmNum == -1)
							{
								sckNum = x;
								break;
							}
							else
								return null;
						}
			//if not, spawn a new connection
			if(sckNum == -1)
				sckNum = GetSck();
			if(sckNum == -1)
				return null;
			scks[sckNum].dlNum = dlNum;
			scks[sckNum].dmNum = dmNum;
			if(scks[sckNum].state != Condition.Connected)
				scks[sckNum].Reset(server, false);
			return scks[sckNum];
		}

		/// <summary>
		/// Take an incoming client->client eDonkey connection.
		/// </summary>
		public static void Incoming(Socket elsck, byte[] data)
		{
			Packet pckt = new Packet();
			if(data.Length <= 5)
			{
				pckt.good = false;
				Utils.Diag("ed2k packet bad 1");
			}
			if(pckt.good)
			{
				pckt.len = Endian.ToInt32(data, 1, false);
				if(pckt.len != data.Length - 5)
				{
					pckt.good = false;
					Utils.Diag("ed2k packet bad 2");
				}
				else
				{
					pckt.payload = new byte[pckt.len];
					Array.Copy(data, 5, pckt.payload, 0, (int)pckt.len);
				}
			}

			if(pckt.good)
			{
				try
				{
					scks[GetSck()].ResetIncoming(elsck, pckt);
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("eDonkey Incoming: " + e.Message);
					goto problem;
				}
				return;
			}
		problem:
			System.Diagnostics.Debug.WriteLine("eDonkey Incoming packet problem");
			try
			{
				if(elsck != null)
				{
					if(elsck.Connected)
						elsck.Shutdown(SocketShutdown.Both);
					elsck.Close();
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("eDonkey Incoming sckt problem");
			}
		}

		public string RemoteIP()
		{
			string tempEnd = "";
			try
			{
				tempEnd = sock1.RemoteEndPoint.ToString();
			}
			catch(Exception exc)
			{
				System.Diagnostics.Debug.WriteLine("eDonkey RemoteIP: " + state.ToString() + " : " + server.ToString() + " : " + exc.ToString());
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				for(int i = 0; i < st.FrameCount; i++)
				{
					System.Diagnostics.StackFrame sf = st.GetFrame(i);
					System.Diagnostics.Debug.WriteLine(" File: " + sf.GetFileName() + 
						" Line: " + sf.GetFileLineNumber() + 
						" Method: " + sf.GetMethod());
				}
				Disconnect("remoteip problem");
				return "";
			}
			int xPos = tempEnd.IndexOf(":");
			if(xPos == -1)
				return "";
			return tempEnd.Substring(0, xPos);
		}

		/// <summary>
		/// Return an EDonkey socket not in use.
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
				System.Diagnostics.Debug.WriteLine("EDonkey GetSck error");
			}
			return -1;
		}

		//10 seconds to connect to server; otherwise we drop connection
		void connectYet_Tick(object sender, ElapsedEventArgs e)
		{
			connectYet.Stop();
			Disconnect("connectYet");
		}

		//40 seconds to check if connection is alive
		void aliveTmr_Tick(object sender, ElapsedEventArgs e)
		{
			if(this.state == Condition.Connected && !server)
			{
				if(!alive)
					Disconnect("aliveTmr");
				else
					alive = false;
			}
		}
	}
}
