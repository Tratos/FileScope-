// Push.cs
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
using System.Text;

namespace FileScope.Gnutella
{
	/// <summary>
	/// Class dedicated to push request packets.
	/// Allows nodes to download from firewalled hosts.
	/// </summary>
	public class Push
	{
		public static void HandlePush(Message theMessage, int sockNum)
		{
			Stats.Updated.Gnutella.numPushes++;

			byte[] bytesServi = new byte[16];
			Array.Copy(theMessage.GetPayload(), 0, bytesServi, 0, 16);

			//is it for us
			if(Utils.HexGuid(Stats.settings.myGUID) == Utils.HexGuid(bytesServi))
			{
				//System.Diagnostics.Debug.WriteLine("g1 PUSH FOR US");

				int fileIndex = Endian.ToInt32(theMessage.GetPayload(), 16, false);
				string ip = Endian.BigEndianIP(theMessage.GetPayload(), 20);
				int port = (int)Endian.ToUInt16(theMessage.GetPayload(), 24, false);

				//spawn the upload
				UploadManager.Outgoing(fileIndex, ip, port);
				return;
			}

			if(Stats.Updated.Gnutella.ultrapeer)
			{
				//where to route it
				int route = Router.GetPushRoute(new GUIDitem(bytesServi));
				if(route == -2 || route == -1)
				{
					System.Diagnostics.Debug.WriteLine("HandlePush NO PUSH ROUTE");
					return;
				}
				theMessage.RoutePacket(route);
			}
		}

		/// <summary>
		/// Craft our own push request and send.
		/// </summary>
		public static void CraftPush(QueryHitObject qho)
		{
			//make sure we can receive incoming connections
			if(Stats.settings.firewall)
				return;

			//generate payload
			byte[] bytesPayload = new byte[26];
			//node's servent identifier
			Array.Copy(qho.servIdent, 0, bytesPayload, 0, 16);
			//node's file index
			Array.Copy(Endian.GetBytes(qho.fileIndex, false), 0, bytesPayload, 16, 4);
			//our IP
			Array.Copy(Endian.BigEndianIP(Stats.settings.ipAddress), 0, bytesPayload, 20, 4);
			//our port
			Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, false), 0, bytesPayload, 24, 2);

			//deliver it
			int route = Router.GetPushRoute(new GUIDitem(qho.servIdent));
			if(route == -2 || route == -1)
			{
				//System.Diagnostics.Debug.WriteLine("CraftPush NO PUSH ROUTE ");
				return;
			}
			Message pckt = new Message(0x40, bytesPayload, qho.hops);
			Sck.scks[route].SendPacket(pckt);
		}
	}
}
