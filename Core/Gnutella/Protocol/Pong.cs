// Pong.cs
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

namespace FileScope.Gnutella
{
	/// <summary>
	/// Class dedicated to pong packets.
	/// </summary>
	public class Pong
	{
		public static void HandlePong(Message theMessage, int sockNum)
		{
			Stats.Updated.Gnutella.numPongs++;

			try
			{
				//extract some stuff
				byte[] thePong = theMessage.GetPayload();
				int port = (int)Endian.ToUInt16(thePong, 0, false);
				string address = Endian.BigEndianIP(thePong, 2);
				int filesShared = Endian.ToInt32(thePong, 6, false);
				int kbShared = Endian.ToInt32(thePong, 10, false);

				//some people are idiots; thus this must do this
				if(filesShared == 0)
					kbShared = 0;

				//ultrapeers handle pongs differently
				if(Stats.Updated.Gnutella.ultrapeer)
				{
					int route = Router.GetPongRoute(theMessage.gitem);
					//is the pong for us or someone else
					if(route == -1 || route == -2)
					{
						string host = address + ":" + port.ToString();
						//check to see if we know about this host already
						if(ConnectionManager.ActiveHostFound(host))
							return;
						for(int x = 0; x < 30; x++)
							if(x < Stats.gnutellaHosts.Count)
								if((string)Stats.gnutellaHosts[x] == host)
									return;
						lock(Stats.gnutellaHosts)
							Stats.gnutellaHosts.Insert(0, host);
					}
					else
					{
						//add to the cache if it's a good pong
						if(theMessage.GetHOPS() > 1)
							PongCache.AddPong(thePong, sockNum);
						//route it properly
						theMessage.RoutePacket(route);
					}
				}
				else
				{
					string host = address + ":" + port.ToString();
					//check to see if we know about this host already
					if(ConnectionManager.ActiveHostFound(host))
						return;
					for(int x = 0; x < 30; x++)
						if(Stats.gnutellaHosts.Count > x)
							if((string)Stats.gnutellaHosts[x] == host)
								return;
					lock(Stats.gnutellaHosts)
						Stats.gnutellaHosts.Insert(0, host);
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("pong error");
			}
		}

		/// <summary>
		/// Here we send a pong with the same ttl as the corresponding ping.
		/// </summary>
		public static void RespondToPing(Message theMessage, int sockNum)
		{
			//make sure we at least have an IP
			if(Stats.settings.ipAddress == "")
				return;

			//create payload first
			byte[] ourPayload = new byte[14];
			Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, false), 0, ourPayload, 0, 2);
			Array.Copy(Endian.BigEndianIP(Stats.settings.ipAddress), 0, ourPayload, 2, 4);
			Array.Copy(Endian.GetBytes(Stats.Updated.filesShared, false), 0, ourPayload, 6, 4);
			Array.Copy(Endian.GetBytes(Stats.Updated.kbShared, false), 0, ourPayload, 10, 4);

			//create the message and embed payload
			Message message = new Message(theMessage.GetGUID(), 0x01, ourPayload, theMessage.GetHOPS());
			Sck.scks[sockNum].SendPacket(message);
			//System.Diagnostics.Debug.WriteLine("sent pong, TTL " + theMessage.GetTTL().ToString() + " to " + sockNum.ToString());
		}
	}
}
