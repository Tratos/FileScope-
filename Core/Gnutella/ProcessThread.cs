// ProcessThread.cs
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
using System.Threading;
using System.Text;

namespace FileScope.Gnutella
{
	/// <summary>
	/// Class with thread dedicated to extensive gnutella data processing.
	/// Every message from the gnutella network will be processed here.
	/// </summary>
	public class ProcessThread
	{
		//the actual thread
		public static Thread thread;

		public static void Start()
		{
			thread = new Thread(new ThreadStart(FuncDataProcess));
			thread.IsBackground = true;
			thread.Start();
		}

		public static void Abort()
		{
			try
			{
				if(thread != null)
					thread.Abort();
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("ProcessThread Abort: " + e.Message);
			}
		}

		public static void FuncDataProcess()
		{
			while(true)
			{
				if(Stats.gnutellaQueue.Count > 0)
				{
					int sockNum;
					//get the socket index value
					lock(Stats.gnutellaQueue)
						sockNum = (int)Stats.gnutellaQueue.Dequeue();

					//check if we're still processing handshakes
					if(Sck.scks[sockNum].state != Condition.hndshk3)
					{
						byte[] msg;
						//copy the entire buffer to a byte array
						lock(Sck.scks[sockNum].buf)
						{
							msg = new byte[Sck.scks[sockNum].buf.Count];
							Sck.scks[sockNum].buf.CopyTo(msg);
							Sck.scks[sockNum].buf.Clear();
						}
						if(msg.Length == 0)
							continue;

						//we wait for the "\r\n\r\n" or "\n\n" to signalize the end of the handshake
						string hndBuf = Encoding.ASCII.GetString(msg);
						if(hndBuf.IndexOf("\r\n\r\n") != -1 || hndBuf.IndexOf("\n\n") != -1)
						{
							//System.Diagnostics.Debug.WriteLine("handshake:\r\n" + hndBuf);
							/*
							 * a handshake can potentially have actual gnutella data appended to it
							 * taking this into account, we need to parse everything carefully
							 */
							int endPos = Handshake.EndOfHandshake(ref hndBuf);
							Handshake handshake = new Handshake(hndBuf.Substring(0, endPos), sockNum);
							handshake.Respond();
							//check for extra data
							if(endPos < msg.Length)
							{
								System.Diagnostics.Debug.WriteLine("extra data from gnutella 1 handshake");
								byte[] newData = new byte[msg.Length - endPos];
								Array.Copy(msg, endPos, newData, 0, newData.Length);
								//put the new data, back into the buffer
								lock(Sck.scks[sockNum].buf)
									Sck.scks[sockNum].buf.InsertRange(0, newData);
							}
						}
						else
						{
							//handshake not complete; put it back into buffer
							lock(Sck.scks[sockNum].buf)
								Sck.scks[sockNum].buf.InsertRange(0, msg);
						}
						continue;
					}
					else
					{
						//process all packets waiting in the Sck object
						Message message;
						for(;;)
						{
							lock(Sck.scks[sockNum].pckBuf)
							{
								if(Sck.scks[sockNum].pckBuf.Count == 0)
									break;
								message = (Message)Sck.scks[sockNum].pckBuf[0];
								Sck.scks[sockNum].pckBuf.RemoveAt(0);
							}
							switch(message.GetPayloadDescriptor())
							{
								case 0x00://Ping
									//System.Diagnostics.Debug.WriteLine("Ping ttl: "+message.GetTTL().ToString());
									Ping.HandlePing(message, sockNum);
									break;
								case 0x01://Pong
									//System.Diagnostics.Debug.WriteLine("pong ttl: "+message.GetTTL().ToString());
									Pong.HandlePong(message, sockNum);
									break;
								case 0x30://QRP table update
									//System.Diagnostics.Debug.WriteLine("qrt ttl: "+message.GetTTL().ToString());
									QueryRouteTable.HandleQueryRouteTable(message, sockNum);
									break;
								case 0x80://Query
									//System.Diagnostics.Debug.WriteLine("query ttl: "+message.GetTTL().ToString());
									Query.HandleQuery(message, sockNum);
									break;
								case 0x81://Query hit
									//System.Diagnostics.Debug.WriteLine("queryhit ttl: "+message.GetTTL().ToString());
									QueryHit.HandleQueryHit(message, sockNum);
									break;
								case 0x40://Push request
									//System.Diagnostics.Debug.WriteLine("push ttl: "+message.GetTTL().ToString());
									Push.HandlePush(message, sockNum);
									break;
								case 0x02://Bye packet
									//System.Diagnostics.Debug.WriteLine("bye ttl: "+message.GetTTL().ToString());
									Bye.HandleBye(message, sockNum);
									break;
								case 0x31://unknown packet
									break;
								default:
									System.Diagnostics.Debug.WriteLine("UNKNOWN:\r\n" + message.GetPayloadDescriptor().ToString() + "\r\n");
									break;
							}
						}
					}
				}
				else
					//nothing on the queue
					//so the thread can take a brake for a fraction of a second
					Thread.Sleep(10);
			}
		}
	}
}
