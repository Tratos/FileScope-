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

namespace FileScope.OpenNap
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
	/// Socket class dedicated to connecting to OpenNap networks.
	/// Each instance of this class has an instance of System.Net.Sockets.Socket.
	/// This is a fully asynchronous socket with no hop flow scheme.
	/// This class is reusable after a disconnect.
	/// </summary>
	public class Sck
	{
		//a maximum of 40 opennap sockets
		public static Sck[] scks = new Sck[40];

		Socket sock1;									//socket instance
		public int sockNum;								//socket identifier
		byte[] receiveBuff = new byte[4096];			//receive buffer
		public ArrayList buf = new ArrayList();			//handy receive buffer
		public volatile Condition state;				//state of the connection
		public GoodTimer connectYet = new GoodTimer();	//10 seconds to connect to server
		public string address;
		public int port;
		public string nick;								//store nickname

		public Sck(int sckIndex)
		{
			connectYet.Interval = 10000;
			connectYet.AddEvent(new ElapsedEventHandler(connectYet_Tick));
			this.sockNum = sckIndex;
			this.state = Condition.Closed;
		}

		public void Reset(string server)
		{
			try
			{
				sock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				state = Condition.Connecting;
				//parse *.*.*.*:* into IP address and port
				Utils.AddrParse(server, out address, out port, 8888);
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
				Disconnect();
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
				Disconnect();
			}
		}

		void BeginConnection(IPAddress ip)
		{
			try
			{
				IPEndPoint endPoint = new IPEndPoint(ip, port);
				sock1.BeginConnect(endPoint, new AsyncCallback(OnConnect), sock1);
				connectYet.Start();
			}
			catch
			{
				Disconnect();
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
				//stop asynchronous connect
				Socket tmpSock = (Socket)ar.AsyncState;
				tmpSock.EndConnect(ar);

				//connected
				connectYet.Stop();
				state = Condition.Connected;
				Stats.Updated.OpenNap.lastConnectionCount++;
				tmpSock.BeginReceive(receiveBuff, 0, receiveBuff.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), tmpSock);
				//send login
				GUIBridge.OJustConnected(sockNum);
				Messages.Login(sockNum);
			}
			catch
			{
				Disconnect();
			}
		}

		void OnReceiveData(IAsyncResult ar)
		{
			try
			{
				//end asynchronous receive
				Socket tmpSock = (Socket)ar.AsyncState;
				int bytesRec = tmpSock.EndReceive(ar);
				Stats.Updated.inNetworkBandwidth += bytesRec;

				if(bytesRec > 0)
				{
					//create a smaller image of the buffer
					byte[] tempMsg = new byte[bytesRec];
					Array.Copy(receiveBuff, 0, tempMsg, 0, bytesRec);

					//buf is the buffer we actually read from and work with
					//if a message is incomplete during processing, it's put back into buf
					lock(buf)
						buf.AddRange(tempMsg);

					//process new data
					ProcessData.NewData(this.sockNum);

					//start receiving again
					tmpSock.BeginReceive(receiveBuff, 0, receiveBuff.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), tmpSock);
				}
				else
				{
					//connection is probably dead
					Disconnect();
				}
			}
			catch
			{
				Disconnect();
			}
		}

		void OnSendData(IAsyncResult ar)
		{
			try
			{
				//stop asynchronous send
				Socket tmpSock = (Socket)ar.AsyncState;
				int bytesSent = tmpSock.EndSend(ar);
				Stats.Updated.outNetworkBandwidth += bytesSent;
			}
			catch
			{
				Disconnect();
			}
		}

		/// <summary>
		/// Disconnect from this OpenNap server.
		/// </summary>
		public void Disconnect()
		{
			//update gui
			GUIBridge.OJustDisconnected(sockNum);
			if(Sck.scks[sockNum].state == Condition.Connected)
				Stats.Updated.OpenNap.lastConnectionCount--;

			//if(state == Condition.Connected)
			//	System.Diagnostics.Debug.WriteLine(sockNum.ToString() + " just disconnected");

			try
			{
				this.state = Condition.Closed;
				connectYet.Stop();
				lock(buf)
					buf.Clear();
				if(sock1 != null)
				{
					if(sock1.Connected)
						sock1.Shutdown(SocketShutdown.Both);
					sock1.Close();
					sock1 = null;
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("OpenNap Sck Disconnect sock1 shutdown");
			}
		}

		/// <summary>
		/// Send a packet to the OpenNap server.
		/// </summary>
		public void SendPacket(Packet packet)
		{
			try
			{
				sock1.BeginSend(packet.packet, 0, packet.packet.Length, SocketFlags.None, new AsyncCallback(OnSendData), sock1);
			}
			catch
			{
				Disconnect();
			}
		}

		/// <summary>
		/// Spawn an outgoing connection to an OpenNap server.
		/// </summary>
		public static void Outgoing(string server)
		{
			scks[GetSck()].Reset(server);
		}

		/// <summary>
		/// Return an OpenNap socket not in use.
		/// </summary>
		public static int GetSck()
		{
			try
			{
				for (int x = 0; x < scks.Length; x++)
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
				System.Diagnostics.Debug.WriteLine("OpenNap GetSck error");
			}
			return -1;
		}

		//10 seconds to connect to server; otherwise we drop connection
		void connectYet_Tick(object sender, ElapsedEventArgs e)
		{
			connectYet.Stop();
			Disconnect();
		}
	}
}
