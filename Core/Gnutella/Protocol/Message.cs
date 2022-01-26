// Message.cs
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
using System.Net.Sockets;

namespace FileScope.Gnutella
{
	/// <summary>
	/// Class that creates and reads actual gnutella packets.
	/// Message will fabricate gnutella headers and attach payloads.
	/// The constructor for this class is overloaded for incoming and outgoing packets.
	/// The payload is encapsulated in Message, but created or read somewhere else.
	/// </summary>
	public class Message
	{
		public GUIDitem gitem;
		byte[] gUId;
		byte payloadDescriptor;
		byte ttl;
		byte hops;
		byte[] payloadLength;

		byte[] payload;			//we don't know the size yet
		public int headerType;	//0 for ok, 1 for disconnected, 2 for illegal packet
		public int payLength = -1;	//holds the value of the payload length of an incoming packet

		/*
		 * this Message class is used not only for sending packets
		 * when we want to send a packet, it's encapsulated in this class
		 * when we want to send any random byte[] (ex. handshake), we also embed it in this class
		 * so the boolean packet specifies whether it's a gnutella packet, or byte[] of data
		 * one of the outgoing Message constructor overloads sets this to false
		 * that same Message constructor embeds the message into nonPacketMessage
		 */
		public bool packet = true;
		public byte[] nonPacketMessage;

		/// <summary>
		/// This is a special receive function in case Sck.buf isn't empty.
		/// This can happen if more data was received while in handshake mode.
		/// </summary>
		public int GoodReceive(Sck sck, byte[] bytes, int pos, int len)
		{
			int totalB = 0;
			if(sck.buf.Count > 0)
			{
				lock(sck.buf)
				{
					if(sck.buf.Count < len)
					{
						sck.buf.CopyTo(bytes);
						pos += sck.buf.Count;
						len -= sck.buf.Count;
						totalB += sck.buf.Count;
						sck.buf.Clear();
						totalB += sck.sock1.Receive(bytes, pos, len, SocketFlags.None);
						Stats.Updated.inNetworkBandwidth += len;
						return totalB;
					}
					else
					{
						sck.buf.CopyTo(0, bytes, pos, len);
						sck.buf.RemoveRange(0, len);
						totalB += len;
						return totalB;
					}
				}
			}
			else
			{
				totalB += sck.sock1.Receive(bytes, pos, len, SocketFlags.None);
				Stats.Updated.inNetworkBandwidth += len;
				return totalB;
			}
		}

		/// <summary>
		/// Incoming packet.
		/// </summary>
		public Message(Sck sck, byte[] midbuf)
		{
			//start with guid
			gUId = new byte[16];
			for(int pos = 0; pos < 16;)
			{
				int bytesRec = 0;
				try
				{
					bytesRec = GoodReceive(sck, gUId, pos, 16-pos);
					sck.bytesIn += bytesRec;
				}
				catch(Exception e)
				{
					sck.Disconnect("in Message " + e.Message);
					this.headerType = 1;
					return;
				}
				if(bytesRec == 0)
				{
					//we're probably disconnected right about now
					sck.Disconnect("zero length message");
					this.headerType = 1;
					return;
				}
				pos += bytesRec;
			}
			gitem = new GUIDitem(this.gUId);

			//next do payload descriptor, ttl, hops, and payload length
			for(int pos = 0; pos < 7;)
			{
				int bytesRec = 0;
				try
				{
					bytesRec = GoodReceive(sck, midbuf, pos, 7-pos);
					sck.bytesIn += bytesRec;
				}
				catch(Exception e)
				{
					sck.Disconnect("in Message " + e.Message);
					this.headerType = 1;
					return;
				}
				if(bytesRec == 0)
				{
					//we're probably disconnected right about now
					sck.Disconnect("zero length message");
					this.headerType = 1;
					return;
				}
				pos += bytesRec;
			}
			payloadDescriptor = midbuf[0];
			ttl = midbuf[1];
			hops = midbuf[2];
			payLength = Endian.ToInt32(midbuf, 3, false);
			this.payloadLength = Endian.GetBytes(payLength, false);
			if(payLength < 0 || payLength > 150000)
			{
				sck.Disconnect("illegal payload length: " + payLength.ToString());
				this.headerType = 2;
				return;
			}

			//now do payload
			payload = new byte[payLength];
			for(int pos = 0; pos < payLength;)
			{
				int bytesRec = 0;
				try
				{
					bytesRec = GoodReceive(sck, payload, pos, payLength-pos);
					sck.bytesIn += bytesRec;
				}
				catch(Exception e)
				{
					sck.Disconnect("in Message " + e.Message);
					this.headerType = 1;
					return;
				}
				if(bytesRec == 0)
				{
					//we're probably disconnected right about now
					sck.Disconnect("zero length message");
					this.headerType = 1;
					return;
				}
				pos += bytesRec;
			}

			//final checks
			if((int)ttl < 0 || (int)ttl > 12)
			{
				//System.Diagnostics.Debug.WriteLine("illegal ttl: " + ttl.ToString());
				this.headerType = 2;
				return;
			}
			if((int)hops < 0 || (int)hops > 12)
			{
				//System.Diagnostics.Debug.WriteLine("illegal hop count: " + hops.ToString());
				this.headerType = 2;
				return;
			}

			//this is a good packet
			this.headerType = 0;
			//update the status of this packet
			deincrementTTL();
			IncrementHops();
		}

		/// <summary>
		/// Outgoing packet; specify Payload Descriptor, Payload.
		/// </summary>
		public Message(byte payloadDescriptor, byte[] payload)
		{
			this.gUId = GUID.newGUID();
			gitem = new GUIDitem(this.gUId);
			this.ttl = (byte)7;
			this.hops = (byte)0;
			this.payloadLength = Endian.GetBytes(payload.Length, false);
			this.payloadDescriptor = payloadDescriptor;
			this.payload = payload;
		}

		/// <summary>
		/// Outgoing packet; specify Payload Descriptor, Payload, TTL.
		/// </summary>
		public Message(byte payloadDescriptor, byte[] payload, int theTTL)
		{
			this.gUId = GUID.newGUID();
			gitem = new GUIDitem(this.gUId);
			this.ttl = (byte)theTTL;
			this.hops = (byte)0;
			this.payloadLength = Endian.GetBytes(payload.Length, false);
			this.payloadDescriptor = payloadDescriptor;
			this.payload = payload;
		}

		/// <summary>
		/// Outgoing packet; specify GUID, Payload Descriptor, Payload, TTL.
		/// </summary>
		public Message(byte[] gUId, byte payloadDescriptor, byte[] payload, int theTTL)
		{
			this.gUId = gUId;
			gitem = new GUIDitem(this.gUId);
			this.ttl = (byte)theTTL;
			this.hops = (byte)0;
			this.payloadLength = Endian.GetBytes(payload.Length, false);
			this.payloadDescriptor = payloadDescriptor;
			this.payload = payload;
		}

		/// <summary>
		/// We're not sending a packet.
		/// Instead we send a raw message (maybe a handshake or something).
		/// </summary>
		public Message(byte[] message)
		{
			this.packet = false;
			this.nonPacketMessage = message;
			//random payload descriptor initialization
			this.payloadDescriptor = 0x99;
		}

		/// <summary>
		/// Broadcast this entire packet after receiving it.
		/// </summary>
		public void BroadcastPacket(int sockWhereFrom, ref string anyQuery)
		{
			if(GetTTL() > 0)
			{
				//loop through all sockets to send the packet to connected sockets
				foreach(Sck obj in Sck.scks)
					if(obj != null)
						if(obj.state == Condition.hndshk3 && obj.sockNum != sockWhereFrom)
							if(this.GetPayloadDescriptor() == 0x80)
							{
								//broadcast query to the right nodes
								if(obj.ultrapeer)
									obj.SendPacket(this);
								else
								{
									//no QRT, no luck
									if(obj.recQRT == null)
										obj.SendPacket(this);
									else
									{
										//respect the QRT
										if(obj.recQRT.CheckQuery(anyQuery))
											obj.SendPacket(this);
									}
								}
							}
							else if(this.GetPayloadDescriptor() == 0x00)
							{
								//this is a ping; only broadcast to ultrapeers
								if(obj.ultrapeer)
									obj.SendPacket(this);
							}
			}
		}

		/// <summary>
		/// Route the packet wherever it should go.
		/// </summary>
		public void RoutePacket(int route)
		{
			if(GetTTL() > 0)
			{
				if(Sck.scks[route].state == Condition.hndshk3)
					Sck.scks[route].SendPacket(this);
			}
			else
			{
				/*
				if(this.payloadDescriptor == 0x81)
					System.Diagnostics.Debug.WriteLine("queryhit route error");
				else if(this.payloadDescriptor == 0x01)
					System.Diagnostics.Debug.WriteLine("pong route error");
				else if(this.payloadDescriptor == 0x40)
					System.Diagnostics.Debug.WriteLine("push route error");
				*/
			}
		}

		/// <summary>
		/// Add one to the hop count of this header.
		/// </summary>
		public void IncrementHops()
		{
			hops = (byte)((int)hops + 1);
		}

		/// <summary>
		/// Subtract one from the ttl of this header.
		/// </summary>
		public void deincrementTTL()
		{
			ttl = (byte)((int)ttl - 1);
		}

		public byte[] GetGUID()
		{
			return gUId;
		}

		public byte GetPayloadDescriptor()
		{
			return payloadDescriptor;
		}

		public int GetTTL()
		{
			return (int)ttl;
		}

		public int GetHOPS()
		{
			return (int)hops;
		}

		public int GetPayloadLength()
		{
			if(payLength != -1)
				return payLength;
			else
				return Endian.ToInt32(payloadLength, 0, false);
		}

		public byte[] GetPayload()
		{
			return payload;
		}

		/// <summary>
		/// Return the entire packet with header and payload.
		/// Used for outgoing packets.
		/// </summary>
		public byte[] GetWholePacket()
		{
			if(this.packet)
			{
				byte[] packet = new byte[23+payload.Length];
				Array.Copy(this.gUId, 0, packet, 0, 16);
				packet[16] = this.payloadDescriptor;
				packet[17] = this.ttl;
				packet[18] = this.hops;
				Array.Copy(this.payloadLength, 0, packet, 19, 4);
				Array.Copy(this.payload, 0, packet, 23, payload.Length);
				return packet;
			}
			else
			{
				return this.nonPacketMessage;
			}
		}
	}
}
