// Chat.cs
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
using System.Collections;

namespace FileScope
{
	public enum ChatState
	{
		Closed,
		Connecting,
		Connected,	//connected and in handshake process
		HndShk		//connected and handshake is finished
	}

	/// <summary>
	/// Single managed asynchronous chat with a peer.
	/// </summary>
	public class Chat
	{
		public ChatState state;							//state of the chat
		Socket sock1;									//socket instance we use
		public int chatNum;								//chat identifier
		byte[] receiveBuff = new byte[32768];			//receive buffer
		public ArrayList buf = new ArrayList();			//handy receive buffer
		public GoodTimer connectYet = new GoodTimer();	//10 seconds to finish handshake
		public bool incoming = false;					//is this an incoming connection?
		public string address;
		public int port;

		public Chat(int chatNum)
		{
			this.chatNum = chatNum;
			state = ChatState.Closed;
			connectYet.Interval = 10000;
			connectYet.AddEvent(new ElapsedEventHandler(connectYet_Tick));
		}

		/// <summary>
		/// Connect and request a chat with peer.
		/// </summary>
		public void Outgoing(ref string peer)
		{
			try
			{
				Stats.Updated.numChats++;
				state = ChatState.Connecting;
				sock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				//parse *.*.*.*:* into IP address and port
				Utils.AddrParse(peer, out address, out port, 6346);
				IPHostEntry ipHost = Dns.GetHostByName(address);
				IPEndPoint endPoint = new IPEndPoint(ipHost.AddressList[0], port);
				//notify gui of our new chat
				GUIBridge.NewChat(chatNum);
				//connect
				sock1.BeginConnect(endPoint, new AsyncCallback(OnConnect), sock1);
				connectYet.Start();
			}
			catch
			{
				Disconnect();
			}
		}

		//receive flag
		bool beginReceive = false;

		/// <summary>
		/// Handle an incoming connection.
		/// </summary>
		public void Incoming(Socket elSock)
		{
			try
			{
				Stats.Updated.numChats++;
				sock1 = elSock;
				incoming = true;
				state = ChatState.Connected;

				//make sure that we're not blocking this host
				if(Stats.blockedChatHosts.Contains(this.RemoteIP()))
				{
					Disconnect();
					return;
				}

				beginReceive = true;
				ChatHandShake.SendResponse(this.chatNum);
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
			if(state == ChatState.Connected || state == ChatState.HndShk)
				return;
			if(state != ChatState.Connecting)
				return;

			try
			{
				//stop asynchronous connect
				Socket tmpSock = (Socket)ar.AsyncState;
				tmpSock.EndConnect(ar);

				state = ChatState.Connected;
				tmpSock.BeginReceive(receiveBuff, 0, receiveBuff.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), tmpSock);
				ChatHandShake.SendHandshake(chatNum);
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

				if(bytesRec > 0)
				{
					//create a smaller image of the buffer
					byte[] tempMsg = new byte[bytesRec];
					Array.Copy(receiveBuff, 0, tempMsg, 0, bytesRec);

					//buf is the buffer we actually read from and work with
					//if a message is incomplete during processing, it's put back into buf
					lock(buf)
						buf.AddRange(tempMsg);

					ChatProcessData.NewData(chatNum);

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

		/// <summary>
		/// Send a string message to our peer.
		/// </summary>
		public void SendString(ref string msg)
		{
			byte[] bytesMsg = System.Text.Encoding.ASCII.GetBytes(msg);
			try
			{
				sock1.BeginSend(bytesMsg, 0, bytesMsg.Length, SocketFlags.None, new AsyncCallback(OnSendData), sock1);
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
				if(beginReceive)
				{
					beginReceive = false;
					sock1.BeginReceive(receiveBuff, 0, receiveBuff.Length, SocketFlags.None, new AsyncCallback(OnReceiveData), sock1);
				}
			}
			catch
			{
				Disconnect();
			}
		}

		public void Disconnect()
		{
			try
			{
				if(state != ChatState.Closed)
					Stats.Updated.numChats--;
				GUIBridge.DisconnectedChat(chatNum);
				state = ChatState.Closed;
				connectYet.Stop();
				buf.Clear();
				if(sock1 != null)
				{
					if(sock1.Connected)
						sock1.Shutdown(SocketShutdown.Both);
					sock1.Close();
				}
				StartApp.main.BeginInvoke(new ChatManager.nullifyMe(ChatManager.NullifyMe), new object[]{this.chatNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Chat Disconnect");
			}
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
				//System.Diagnostics.Debug.WriteLine("Chat RemoteIP");
			}
			int xPos = tempEnd.IndexOf(":");
			if(xPos == -1)
				return "";
			else
				return tempEnd.Substring(0, xPos);
		}

		/// <summary>
		/// Take care of some stuff so this instance can be collected when the GC fires.
		/// </summary>
		public void CleanUp()
		{
			connectYet.RemoveEvent(new ElapsedEventHandler(connectYet_Tick));
			connectYet.Stop();
			connectYet.Close();
		}

		//7 seconds to finish handshake otherwise we give up
		void connectYet_Tick(object sender, ElapsedEventArgs e)
		{
			connectYet.Stop();
			Disconnect();
		}
	}
}
