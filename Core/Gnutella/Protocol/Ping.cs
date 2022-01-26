// Ping.cs
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
	/// Class dedicated to ping packets.
	/// </summary>
	public class Ping
	{
		public static void HandlePing(Message theMessage, int sockNum)
		{
			Stats.Updated.Gnutella.numPings++;

			//leaf nodes and ultrapeers handle pings in different ways
			if(Stats.Updated.Gnutella.ultrapeer)
			{
				//it's a broadcast packet, check if we've seen it before
				if(Router.SeenYet(theMessage, sockNum))
					return;

				//is this a handshake ping or a broadcast ping
				if(theMessage.GetTTL() == 0 && theMessage.GetHOPS() == 1)
					Pong.RespondToPing(theMessage, sockNum);
				else if(theMessage.GetTTL() > 0)
				{
					//System.Diagnostics.Debug.WriteLine("incoming broadcast ping");

					//we only help out good citizens (ultrapeer compatible hosts)
					if(!Sck.scks[sockNum].newHost)
						return;

					//we're dealing with a broadcast ping
					//we handle the situation in compliance with Pong-Cache 0.1 specifications
					PongCache.HandlePing(theMessage, sockNum);
					if(Stats.settings.gConnectionsToKeep > Stats.Updated.Gnutella.lastConnectionCount)
						Pong.RespondToPing(theMessage, sockNum);
				}
			}
			else
			{
				//since this is a leaf node, we are not going to route or cache pongs
				//FileScope leafs only respond to the "handshake" ping of 1 TTL
				if(theMessage.GetTTL() == 0 && theMessage.GetHOPS() == 1)
					Pong.RespondToPing(theMessage, sockNum);
			}
		}

		/// <summary>
		/// Send ping with TTL of only 1.
		/// </summary>
		public static void HandshakePing(int sockNum)
		{
			Message pingMessage = new Message(0x00, new byte[0], 1);
			Sck.scks[sockNum].SendPacket(pingMessage);
			//System.Diagnostics.Debug.WriteLine("sent handshake ping");
		}

		/// <summary>
		/// Send ping with TTL of 7.
		/// </summary>
		public static void BroadcastPing(int sockNum)
		{
			Message pingMessage = new Message(0x00, new byte[0], 7);
			Sck.scks[sockNum].SendPacket(pingMessage);
			//System.Diagnostics.Debug.WriteLine("send broadcast ping");
		}
	}
}
