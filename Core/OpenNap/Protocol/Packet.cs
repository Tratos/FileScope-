// Packet.cs
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

namespace FileScope.OpenNap
{
	/// <summary>
	/// Used to represent a "packet" from OpenNap.
	/// These characteristic packets are fabricated in this order:
	/// [length][command][payload]
	/// </summary>
	public class Packet
	{
		//** Incoming vars
		public int len;
		public int cmd;
		public string payload;
		public int type;//0 for ok, 1 for not finished (fragment), 2 for illegal packet

		//** Outgoing vars
		public byte[] packet;

		/// <summary>
		/// Create an outgoing packet.
		/// </summary>
		public Packet(int len, int cmd, string payload)
		{
			//create that packet... it's len size plus 4 bytes: [len][cmd]
			packet = new byte[len+4];

			//setup the first 4 bytes
			byte[] bytesLen = Endian.GetBytes((ushort)len, false);
			byte[] bytesCmd = Endian.GetBytes((ushort)cmd, false);
			Array.Copy(bytesLen, 0, this.packet, 0, 2);
			Array.Copy(bytesCmd, 0, this.packet, 2, 2);

			if(len == 0)
				return;

			//create the rest of the packet
			byte[] bytesPayload = Encoding.ASCII.GetBytes(payload);
			Array.Copy(bytesPayload, 0, this.packet, 4, len);
		}

		/// <summary>
		/// Take care of an incoming packet.
		/// </summary>
		public Packet(byte[] packets, ref int buffIndex)
		{
			try
			{
				//check size of packet
				if(packets.Length - buffIndex < 4)
					//we have to receive the rest of this packet
					type = 1;
				else
				{
					//fix those first 4 bytes
					byte[] bytesLen = new byte[2];
					byte[] bytesCmd = new byte[2];
					Array.Copy(packets, buffIndex, bytesLen, 0, 2);
					Array.Copy(packets, buffIndex+2, bytesCmd, 0, 2);
					this.len = (int)Endian.ToUInt16(bytesLen, 0, false);
					this.cmd = (int)Endian.ToUInt16(bytesCmd, 0, false);

					//payload
					if(len > 50000)
					{
						//bad packet
						System.Diagnostics.Debug.WriteLine("bad packet: " + len.ToString());
						type = 2;
					}
					else if(len > (packets.Length - (buffIndex + 4)))
					{
						type = 1;
					}
					else
					{
						//this is a good packet
						type = 0;
						byte[] payload = new byte[len];
						Array.Copy(packets, buffIndex+4, payload, 0, len);
						//OpenNap payloads are always clear text
						this.payload = Encoding.ASCII.GetString(payload);
						buffIndex += 4+len;
					}
				}
			}
			catch
			{
				type = 2;
				System.Diagnostics.Debug.WriteLine("incoming packet error");
			}
		}
	}
}
