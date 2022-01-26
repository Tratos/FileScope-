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
using System.Net;
using System.Net.Sockets;

namespace FileScope.EDonkey
{
	/// <summary>
	/// Used to encapsulate data exchanged in the eDonkey protocol.
	/// </summary>
	public class Packet
	{
		//** Incoming vars
		public int len;				//payload length
		public byte[] payload;		//payload
		public bool good = false;	//good or bad(malformed) packet

		//** Outgoing vars
		public byte[] outHeader = new byte[5];
		public byte[] outPayload;
		public int outpayLen;

		/// <summary>
		/// Incoming packet.
		/// We already received the data and good is set to true.
		/// </summary>
		public Packet()
		{
			this.good = true;
		}

		/// <summary>
		/// Incoming packet.
		/// </summary>
		public Packet(Sck sck)
		{
			//header: [(byte)protocol mod][(dword)payload length][(byte)type]
			byte[] bytesHeader = new byte[6];
			for(int pos = 0; pos < 6;)
			{
				int bytesRec = 0;
				try
				{
					bytesRec = sck.sock1.Receive(bytesHeader, pos, 6-pos, SocketFlags.None);
				}
				catch
				{return;}
				if(bytesRec == 0)
					return;
				Stats.Updated.inNetworkBandwidth += bytesRec;
				pos += bytesRec;
			}
			if(bytesHeader[0] != 0xE3)
			{
				//we don't understand the protocol mod
				System.Diagnostics.Debug.WriteLine("eDonkey bad protocol mod");
				return;
			}
			this.len = Endian.ToInt32(bytesHeader, 1, false);
			if(this.len < 1 || this.len > 500000)
			{
				System.Diagnostics.Debug.WriteLine("eDonkey malformed packet");
				return;
			}

			if(bytesHeader[5] == 0x46)
			{
				//System.Diagnostics.Debug.WriteLine("ed2k dling");
				this.payload = new byte[1];
				this.payload[0] = 0x46;
				//hash and the ranges (16+8)
				byte[] first24 = new byte[24];
				for(int pos = 0; pos < 24;)
				{
					int bytesRec = 0;
					try
					{
						bytesRec = sck.sock1.Receive(first24, pos, 24-pos, SocketFlags.None);
					}
					catch
					{return;}
					if(bytesRec == 0)
						return;
					Stats.Updated.inTransferBandwidth += bytesRec;
					pos += bytesRec;
				}
				//download data
				int bytesLeft = this.len - 25;
				int bufSize = 4096;
				if(sck.dmNum != -1 && sck.dlNum != -1)
				{
					Downloader dler = (Downloader)DownloadManager.dms[sck.dmNum].downloaders[sck.dlNum];
					while(bytesLeft > 0)
					{
						if(dler.receiveBuff == null)
						{
							System.Diagnostics.Debug.WriteLine("eDonkey dl problem in Packet.cs: receiveBuff isn't available");
							return;
						}
						int bytesRec = 0;
						try
						{
							if(bytesLeft < bufSize)
								bytesRec = sck.sock1.Receive(dler.receiveBuff, 0, bytesLeft, SocketFlags.None);
							else
								bytesRec = sck.sock1.Receive(dler.receiveBuff, 0, bufSize, SocketFlags.None);
						}
						catch
						{return;}
						if(bytesRec == 0)
							return;
						Stats.Updated.inTransferBandwidth += bytesRec;
						sck.alive = true;
						bytesLeft -= bytesRec;
						dler.DataReceived(bytesRec);
					}
				}
				else
				{
					//we're not going to disconnect because the host might stop after a few
					System.Diagnostics.Debug.WriteLine("eDonkey Packet: not expecting more download data");
				}
			}
			else
			{
				//payload
				this.payload = new byte[this.len];
				this.payload[0] = bytesHeader[5];
				for(int pos = 1; pos < this.len;)
				{
					int bytesRec = 0;
					try
					{
						bytesRec = sck.sock1.Receive(this.payload, pos, this.len-pos, SocketFlags.None);
					}
					catch
					{return;}
					if(bytesRec == 0)
						return;
					Stats.Updated.inNetworkBandwidth += bytesRec;
					pos += bytesRec;
				}
			}
			this.good = true;
		}

		/// <summary>
		/// Outgoing packet.
		/// </summary>
		public Packet(int payloadLength)
		{
			this.outHeader[0] = 0xE3;
			Array.Copy(Endian.GetBytes(payloadLength, false), 0, this.outHeader, 1, 4);
			this.outpayLen = payloadLength;
		}
	}
}
