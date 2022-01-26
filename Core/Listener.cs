// Listener.cs
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
using System.Threading;

namespace FileScope
{
	/// <summary>
	/// Listener class for every service we host.
	/// </summary>
	public class Listener
	{
		//actual tcp listener socket
		public static Socket sckListen;
		//actual udp listener socket
		public static Socket sckUdpListen;
		//receive buffer for udp
		public static byte[] udpRecBuff = new byte[4096];
		//endpoint for udp stuff
		public static System.Net.EndPoint ipep;
		//state
		public static bool enabled = false;

		public static void Start()
		{
			try
			{
				int maxPortTry = Stats.settings.port + 20;
				while(true)
				{
					try
					{
						sckListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
						sckListen.Bind(new IPEndPoint(IPAddress.Any, Stats.settings.port));
						break;
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine("port is used, moving up one");
						Stats.settings.port++;
						if(Stats.settings.port > maxPortTry)
							break;
					}
				}
				sckListen.Listen(-1);
				sckListen.BeginAccept(new AsyncCallback(OnAccept), sckListen);
				//attempt udp now that tcp's done
				try
				{
					sckUdpListen = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					ipep = new IPEndPoint(IPAddress.Any, Stats.settings.port);
					sckUdpListen.Bind(ipep);
					sckUdpListen.BeginReceiveFrom(udpRecBuff, 0, udpRecBuff.Length, SocketFlags.None, ref ipep, new AsyncCallback(OnUdpReceive), sckUdpListen);
				}
				catch(Exception eudp)
				{
					System.Diagnostics.Debug.WriteLine("Listen UDP problem: " + eudp.Message);
				}
				enabled = true;
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Listener Start: " + e.Message);
			}
		}

		public static void Abort()
		{
			enabled = false;
			try
			{
				if(sckListen != null)
				{
					if(sckListen.Connected)
						sckListen.Shutdown(SocketShutdown.Both);
					sckListen.Close();
				}
				if(sckUdpListen != null)
					sckUdpListen.Close();
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Listener Abort " + e.Message);
			}
		}

		static void OnAccept(IAsyncResult ar)
		{
			if(!enabled)
				return;
			try
			{
				new Dispatcher(sckListen.EndAccept(ar));
				sckListen.BeginAccept(new AsyncCallback(OnAccept), sckListen);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Listener OnAccept");
			}
		}

		static void OnUdpReceive(IAsyncResult ar)
		{
			if(!enabled)
				return;
			Socket tmpSock = sckUdpListen;
			try
			{
				tmpSock = (Socket)ar.AsyncState;
				int bytesRec = tmpSock.EndReceiveFrom(ar, ref ipep);
				Stats.Updated.inNetworkBandwidth += bytesRec;
				IPEndPoint remoteIP = (IPEndPoint)ipep;

				if(bytesRec > 0)
				{
					if(!Stats.Updated.udpIncoming)
					{
						//we need to make sure the udp packet wasn't from any directly connected host before setting udpIncoming to true
						bool isLocal = false;
						lock(Gnutella2.HostCache.recentHubs)
						{
							foreach(Gnutella2.ConnectionManager.G2Host g2h in Gnutella2.ConnectionManager.ultrapeers.Keys)
								if(Gnutella2.Sck.scks[g2h.sockNum].remoteIPA.Equals(remoteIP.Address))
									isLocal = true;
						}
						if(!isLocal)
						{
							System.Diagnostics.Debug.WriteLine("udp incoming set to true");
							Stats.Updated.udpIncoming = true;
						}
					}
					if(bytesRec > 4090)
						System.Diagnostics.Debug.WriteLine("udp receive buffer is running short");
					//determine what network this packet corresponds to
					if(udpRecBuff[0] == 0xE3 && bytesRec >= 2)
					{
						byte[] tempMsg = new byte[bytesRec];
						Array.Copy(udpRecBuff, 0, tempMsg, 0, bytesRec);
						if(tempMsg[1] == 0x90)
							EDonkey.Messages.QueueStateCheck(ipep, tempMsg);
						else if(tempMsg[1] == 0x91)
							EDonkey.Messages.QueueStateInfo(ipep, tempMsg);
						else if(tempMsg[1] == 0x9B)
							EDonkey.Messages.FoundSources(tempMsg, 2, -1);
						else
							System.Diagnostics.Debug.WriteLine("eDonkey unknown udp packet");
					}
					else if(udpRecBuff[0] == 'G' && udpRecBuff[1] == 'N' && udpRecBuff[2] == 'D' && bytesRec >= 8)
						Gnutella2.UDPSR.IncomingFragment(remoteIP, udpRecBuff, bytesRec);
					else
						System.Diagnostics.Debug.WriteLine("unknown udp packet");
				}
				else
					throw new Exception("received zero bytes");
				tmpSock.BeginReceiveFrom(udpRecBuff, 0, udpRecBuff.Length, SocketFlags.None, ref ipep, new AsyncCallback(OnUdpReceive), tmpSock);
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("OnUdpReceive: " + e.Message + " \n" + e.StackTrace);
				try
				{
					tmpSock.BeginReceiveFrom(udpRecBuff, 0, udpRecBuff.Length, SocketFlags.None, ref ipep, new AsyncCallback(OnUdpReceive), tmpSock);
				}
				catch{System.Diagnostics.Debug.WriteLine("couldn't continue receiving on udp");}
			}
		}

		/// <summary>
		/// Send a message with the available udp socket.
		/// </summary>
		public static void UdpSend(System.Net.EndPoint ep, byte[] bytesMsg, int start, int len)
		{
			try
			{
				if(ep == null)
				{
					Utils.Diag("Listener UdpSend had null ep");
					return;
				}
				sckUdpListen.BeginSendTo(bytesMsg, start, len, SocketFlags.None, ep, null, sckUdpListen);
				Stats.Updated.outNetworkBandwidth += len;
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("UdpSend problem");
			}
		}

		public static void UdpSend(ref string ip, int port, byte[] bytesMsg, int start, int len)
		{
			try
			{
				UdpSend(new IPEndPoint(IPAddress.Parse(ip), port), bytesMsg, start, len);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("UdpSend2 problem");
			}
		}

		/// <summary>
		/// Determine what service this connection seeks and dispatch it accordingly.
		/// </summary>
		public class Dispatcher
		{
			public Thread thread;
			Socket tmpSock;

			public Dispatcher(Socket tmpSock)
			{
				//copy socket
				this.tmpSock = tmpSock;
				//create and start the thread
				thread = new Thread(new ThreadStart(FuncThread));
				thread.Start();
			}

			public void FuncThread()
			{
//System.Diagnostics.Debug.WriteLine("- INCOMING -");
				//we're able to accept incoming connections
				Stats.Updated.everIncoming = true;

				//we have two possible states because OpenNap sometimes requires the uploader to send a '1' character
				int recState = 1;
				byte[] msg;
				int bytesRec;

			RECEIVE:

				//receive timeout
				if(recState == 1)
					tmpSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);
				else
					tmpSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);
				Thread.Sleep(500);

				//get the first message to determine what service this is for
				msg = new byte[8192];
				try
				{
					bytesRec = tmpSock.Receive(msg);
				}
				catch
				{
					//first we check to see if we're only on state 1
					if(recState == 1)
					{
						recState = 2;
						try
						{
							tmpSock.Send(new byte[] { (byte)'1' });
							System.Diagnostics.Debug.WriteLine("sent a 1 to host");
						}
						catch
						{
							goto DISCONNECT;
						}
						goto RECEIVE;
					}

				DISCONNECT:

					try
					{
						if(tmpSock != null)
						{
							if(tmpSock.Connected)
								tmpSock.Shutdown(SocketShutdown.Both);
							tmpSock.Close();
						}
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine("Dispatcher FuncThread 1");
					}
					tmpSock = null;
					return;
				}

				//if no bytes were received, the connection is probably dead
				if(bytesRec == 0)
				{
					try
					{
						if(tmpSock != null)
						{
							if(tmpSock.Connected)
								tmpSock.Shutdown(SocketShutdown.Both);
							tmpSock.Close();
						}
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine("Dispatcher FuncThread 2");
					}
					tmpSock = null;
					return;
				}

				//we don't need a timeout anymore
				tmpSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
				Stats.Updated.inNetworkBandwidth += bytesRec;

				byte[] bytesMsg = new byte[bytesRec];
				Array.Copy(msg, 0, bytesMsg, 0, bytesRec);
				string strMsg = System.Text.Encoding.ASCII.GetString(bytesMsg);

				try
				{
					//what kind of service
					if(strMsg.Substring(0, 3).ToLower() == "get")
					{
						//upload request; in case OpenNap, we need the second chunk of the get statement
						if(strMsg.Length < 9)
						{
							try
							{
								bytesRec = tmpSock.Receive(msg);
								bytesMsg = new byte[bytesRec];
								Array.Copy(msg, 0, bytesMsg, 0, bytesRec);
								strMsg += System.Text.Encoding.ASCII.GetString(bytesMsg);
							}
							catch
							{
								try
								{
									if(tmpSock != null)
									{
										if(tmpSock.Connected)
											tmpSock.Shutdown(SocketShutdown.Both);
										tmpSock.Close();
									}
								}
								catch
								{
									System.Diagnostics.Debug.WriteLine("Dispatcher FuncThread 3");
								}
								tmpSock = null;
								return;
							}
						}
						//might be a "browse host" request
						if(strMsg.Substring(0, 8).ToLower() == "get / ht")
						{
							int tmpSckNum = Gnutella2.Sck.GetSck();
							Gnutella2.Sck.scks[tmpSckNum].browseHost = true;
							Gnutella2.Sck.scks[tmpSckNum].ResetIncoming(tmpSock, System.Text.Encoding.ASCII.GetBytes(strMsg));
						}
						else
							UploadManager.Incoming(tmpSock, ref strMsg);
					}
					else if(strMsg.Substring(0, 4).ToLower() == "chat")
					{
						//chat request
						ChatManager.Incoming(tmpSock);
					}
					else if(strMsg.Substring(0, 3).ToLower() == "giv")
					{
						//pushed gnutella download
						DownloadManager.Incoming(tmpSock, ref strMsg);
					}
					else if(strMsg.Substring(0, 4).ToLower() == "push")
					{
						//pushed gnutella 2 download
						DownloadManager.Incoming(tmpSock, ref strMsg);
					}
					else if(strMsg.Substring(0, 4).ToLower() == "send")
					{
						//pushed opennap download; we need to receive a certain chunk of info
						if(strMsg.Length < 9)
						{
							try
							{
								bytesRec = tmpSock.Receive(msg);
								bytesMsg = new byte[bytesRec];
								Array.Copy(msg, 0, bytesMsg, 0, bytesRec);
								strMsg += System.Text.Encoding.ASCII.GetString(bytesMsg);
							}
							catch
							{
								try
								{
									if(tmpSock != null)
									{
										if(tmpSock.Connected)
											tmpSock.Shutdown(SocketShutdown.Both);
										tmpSock.Close();
									}
								}
								catch
								{
									System.Diagnostics.Debug.WriteLine("Dispatcher FuncThread 4");
								}
								tmpSock = null;
								return;
							}
						}
						DownloadManager.Incoming(tmpSock, ref strMsg);
					}
					else if(bytesMsg[0] == 0xE3)
					{
						//eDonkey direct connection
						EDonkey.Sck.Incoming(tmpSock, bytesMsg);
					}
					else
						//anything else must be a gnutella peer
						Gnutella2.Sck.Incoming(tmpSock, bytesMsg);
				}
				catch(Exception eee)
				{
					System.Diagnostics.Debug.WriteLine("Dispatcher thread error: " + eee.Message);
					System.Diagnostics.Debug.WriteLine(eee.StackTrace);
				}
				tmpSock = null;
			}
		}
	}
}
