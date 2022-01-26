// PacketFactory.cs
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
using System.Text;
using System.Collections;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace FileScope.Gnutella2
{
	/*
	 * anything involving crafting packets for departure is done in PacketFactory.cs
	 * reading packets should be totally OOP-based to allow clean & easy & flexible processing
	 * when crafting packets, we care less about flexibility, and more about speed
	 * there are static packets that are cached, packets that are part cached, and totally dynamic packets
	 * all of these forms are covered and dealt with in this file
	 */

	/// <summary>
	/// Encapsulate all outgoing G2 root packets.
	/// Many subclasses representing packet types will inherit from this class.
	/// </summary>
	public abstract class OMessage
	{
		/// <summary>
		/// Set the llen in the control byte.
		/// </summary>
		public static void Setllen(byte[] byt, int loc, int llen)
		{
			byt[loc] = 0x00;
			byt[loc] |= (byte)(llen << 6);
		}

		/// <summary>
		/// Set the nlen in the control byte.
		/// </summary>
		public static void Setnlen(byte[] byt, int loc, int nlen)
		{
			byt[loc] |= (byte)((nlen-1) << 3);
		}

		/// <summary>
		/// Set the CF and BE flags in the control byte.
		/// </summary>
		public static void SetRest(byte[] byt, int loc, bool children)
		{
			if(children)
				byt[loc] |= 0x04;
			if(!Stats.Updated.le)
				byt[loc] |= 0x02;
		}

		public abstract bool FillSendBuff(Sck sck, ref int buffIndex);

		/// <summary>
		/// If the OMessage subclass doesn't have a custom way of filling the send buffer, it should use this routine.
		/// Hopefully the JITer will inline this function.
		/// pckt is copied into sck.sendBuff
		/// </summary>
		public static bool StandardFill(Sck sck, ref int buffIndex, byte[] pckt)
		{
			int lenLeft = sck.sendBuff.Length - buffIndex;
			if(pckt.Length > lenLeft)
			{
				if(buffIndex == 0)
					sck.sendBuff = new byte[pckt.Length];
				else
					return false;
			}
			Array.Copy(pckt, 0, sck.sendBuff, buffIndex, pckt.Length);
			buffIndex += pckt.Length;
			return true;
		}

		/// <summary>
		/// If sameArrayRefIfTooBig is false, the contents of pckt will be copied into sck.sendBuff.
		/// If sameArrayRefIfTooBig is true, sck.sendBuff will be set to pckt.
		/// </summary>
		public static bool StandardFill(Sck sck, ref int buffIndex, byte[] pckt, bool sameArrayRefIfTooBig)
		{
			int lenLeft = sck.sendBuff.Length - buffIndex;
			if(pckt.Length > lenLeft)
			{
				if(buffIndex == 0)
				{
					if(sameArrayRefIfTooBig)
					{
						sck.sendBuff = pckt;
						buffIndex += pckt.Length;
						return true;
					}
					else
						sck.sendBuff = new byte[pckt.Length];
				}
				else
					return false;
			}
			Array.Copy(pckt, 0, sck.sendBuff, buffIndex, pckt.Length);
			buffIndex += pckt.Length;
			return true;
		}

		/// <summary>
		/// pckt will be copied into sck.sendBuff.
		/// </summary>
		public static bool StandardFill(Sck sck, ref int buffIndex, byte[] pckt, int pcktLen)
		{
			int lenLeft = sck.sendBuff.Length - buffIndex;
			if(pcktLen > lenLeft)
			{
				if(buffIndex == 0)
					sck.sendBuff = new byte[pcktLen];
				else
					return false;
			}
			Array.Copy(pckt, 0, sck.sendBuff, buffIndex, pcktLen);
			buffIndex += pcktLen;
			return true;
		}

		/// <summary>
		/// Just find out if the packet can fit.
		/// </summary>
		public static bool CanFit(Sck sck, ref int buffIndex, int len)
		{
			if(len > (sck.sendBuff.Length - buffIndex))
			{
				if(buffIndex == 0)
					sck.sendBuff = new byte[len];
				else
					return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Any OMessage aiming to be sent over a udp medium must implement this interface.
	/// </summary>
	public interface IUdpMessage
	{
		void SetupUdpPacket(OutgoingPacket op);
	}

	public class OHandshake : OMessage
	{
		public string handshake = "";

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			if(CanFit(sck, ref buffIndex, handshake.Length))
			{
				Encoding.ASCII.GetBytes(handshake, 0, handshake.Length, sck.sendBuff, buffIndex);
				buffIndex += handshake.Length;
				return true;
			}
			else
				return false;
		}
	}

	public class OKeepAlivePing : OMessage, IUdpMessage
	{
		public static byte[] pckt = null;

		public void SetupUdpPacket(OutgoingPacket op)
		{
			CreatePacket();
			byte[] udpPckt = new byte[pckt.Length+8];
			Array.Copy(pckt, 0, udpPckt, 8, pckt.Length);
			op.deflate = false;
			//whatever byte[] op.pcktData is set to shouldn't be static because doing so is dangerous
			op.pcktData = udpPckt;
			op.acks = null;
			op.parts = new BitArray(UDPSR.GetPartCount(udpPckt.Length-8), false);
			op.len = udpPckt.Length-8;
		}

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			CreatePacket();
			return OMessage.StandardFill(sck, ref buffIndex, pckt);
		}

		void CreatePacket()
		{
			if(pckt == null)
			{
				pckt = new byte[1+2];
				Setllen(pckt, 0, 0);
				Setnlen(pckt, 0, 2);
				SetRest(pckt, 0, false);
				pckt[1] = (byte)'P';
				pckt[2] = (byte)'I';
			}
		}
	}

	public class OUdpPing : OMessage
	{
		public static byte[] pckt = new byte[0];
		public byte[] endpoint = new byte[6];

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			lock(pckt)
			{
				if(pckt.Length == 0)
				{
					pckt = new byte[1+1+2 + 1+1+3+6];
					Setllen(pckt, 0, 1);
					Setnlen(pckt, 0, 2);
					SetRest(pckt, 0, true);
					Endian.VarBytesFromInt(pckt, 1, pckt.Length-4, 1);
					pckt[2] = (byte)'P';
					pckt[3] = (byte)'I';
					Setllen(pckt, 4, 1);
					Setnlen(pckt, 4, 3);
					SetRest(pckt, 4, false);
					Endian.VarBytesFromInt(pckt, 5, 6, 1);
					pckt[6] = (byte)'U';
					pckt[7] = (byte)'D';
					pckt[8] = (byte)'P';
					//[endpoint]
				}
				Array.Copy(this.endpoint, 0, pckt, 9, 6);
				return OMessage.StandardFill(sck, ref buffIndex, pckt);
			}
		}
	}

	public class OUdpPingRelay : OMessage
	{
		public static byte[] pckt = new byte[0];
		public byte[] endpoint = new byte[6];

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			lock(pckt)
			{
				if(pckt.Length == 0)
				{
					pckt = new byte[1+1+2 + 1+1+3+6 + 1+5];
					Setllen(pckt, 0, 1);
					Setnlen(pckt, 0, 2);
					SetRest(pckt, 0, true);
					Endian.VarBytesFromInt(pckt, 1, pckt.Length-4, 1);
					pckt[2] = (byte)'P';
					pckt[3] = (byte)'I';
					Setllen(pckt, 4, 1);
					Setnlen(pckt, 4, 3);
					SetRest(pckt, 4, false);
					Endian.VarBytesFromInt(pckt, 5, 6, 1);
					pckt[6] = (byte)'U';
					pckt[7] = (byte)'D';
					pckt[8] = (byte)'P';
					//[endpoint]
					Setllen(pckt, 15, 0);
					Setnlen(pckt, 15, 5);
					SetRest(pckt, 15, false);
					pckt[16] = (byte)'R';
					pckt[17] = (byte)'E';
					pckt[18] = (byte)'L';
					pckt[19] = (byte)'A';
					pckt[20] = (byte)'Y';
				}
				Array.Copy(this.endpoint, 0, pckt, 9, 6);
				return OMessage.StandardFill(sck, ref buffIndex, pckt);
			}
		}
	}

	public class OPong : OMessage, IUdpMessage
	{
		public static byte[] pckt = null;

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			CreatePacket();
			return OMessage.StandardFill(sck, ref buffIndex, pckt);
		}

		public void SetupUdpPacket(OutgoingPacket op)
		{
			CreatePacket();
			byte[] udpPckt = new byte[pckt.Length+8];
			Array.Copy(pckt, 0, udpPckt, 8, pckt.Length);
			op.deflate = false;
			op.pcktData = udpPckt;
			op.acks = null;
			op.parts = new BitArray(UDPSR.GetPartCount(udpPckt.Length-8), false);
			op.len = udpPckt.Length-8;
		}

		void CreatePacket()
		{
			if(pckt == null)
			{
				pckt = new byte[1+2];
				Setllen(pckt, 0, 0);
				Setnlen(pckt, 0, 2);
				SetRest(pckt, 0, false);
				pckt[1] = (byte)'P';
				pckt[2] = (byte)'O';
			}
		}
	}

	public class OPongRelay : OMessage, IUdpMessage
	{
		public static byte[] pckt = null;

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			CreatePacket();
			return OMessage.StandardFill(sck, ref buffIndex, pckt);
		}

		public void SetupUdpPacket(OutgoingPacket op)
		{
			CreatePacket();
			byte[] udpPckt = new byte[pckt.Length+8];
			Array.Copy(pckt, 0, udpPckt, 8, pckt.Length);
			op.deflate = false;
			op.pcktData = udpPckt;
			op.acks = null;
			op.parts = new BitArray(UDPSR.GetPartCount(udpPckt.Length-8), false);
			op.len = udpPckt.Length-8;
		}

		void CreatePacket()
		{
			if(pckt == null)
			{
				pckt = new byte[1+1+2 + 1+5];
				Setllen(pckt, 0, 1);
				Setnlen(pckt, 0, 2);
				SetRest(pckt, 0, true);
				Endian.VarBytesFromInt(pckt, 1, pckt.Length-4, 1);
				pckt[2] = (byte)'P';
				pckt[3] = (byte)'O';
				Setllen(pckt, 4, 0);
				Setnlen(pckt, 4, 5);
				SetRest(pckt, 4, false);
				pckt[5] = (byte)'R';
				pckt[6] = (byte)'E';
				pckt[7] = (byte)'L';
				pckt[8] = (byte)'A';
				pckt[9] = (byte)'Y';
			}
		}
	}

	public class OLNI : OMessage
	{
		public static byte[] pckt = new byte[0];

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			lock(pckt)
			{
				if(pckt.Length == 0)
				{
					pckt = new byte[1+1+3 + 1+1+2+6 + 1+1+2+16 + 1+1+1+4 + 1+1+2+8 + 1+1+2+4];

					Setllen(pckt, 0, 1);
					Setnlen(pckt, 0, 3);
					SetRest(pckt, 0, true);
					//[len]
					pckt[2] = (byte)'L';
					pckt[3] = (byte)'N';
					pckt[4] = (byte)'I';

					Setllen(pckt, 5, 1);
					Setnlen(pckt, 5, 2);
					SetRest(pckt, 5, false);
					Endian.VarBytesFromInt(pckt, 6, 6, 1);
					pckt[7] = (byte)'N';
					pckt[8] = (byte)'A';
					//[endpoint]

					Setllen(pckt, 15, 1);
					Setnlen(pckt, 15, 2);
					SetRest(pckt, 15, false);
					Endian.VarBytesFromInt(pckt, 16, 16, 1);
					pckt[17] = (byte)'G';
					pckt[18] = (byte)'U';
					Array.Copy(Stats.settings.myGUID, 0, pckt, 19, 16);

					Setllen(pckt, 35, 1);
					Setnlen(pckt, 35, 1);
					SetRest(pckt, 35, false);
					Endian.VarBytesFromInt(pckt, 36, 4, 1);
					pckt[37] = (byte)'V';
					pckt[38] = (byte)'F';
					pckt[39] = (byte)'S';
					pckt[40] = (byte)'C';
					pckt[41] = (byte)'P';

					Setllen(pckt, 42, 1);
					Setnlen(pckt, 42, 2);
					SetRest(pckt, 42, false);
					Endian.VarBytesFromInt(pckt, 43, 8, 1);
					pckt[44] = (byte)'L';
					pckt[45] = (byte)'S';
					//[library status (8)]

					Setllen(pckt, 54, 1);
					Setnlen(pckt, 54, 2);
					SetRest(pckt, 54, false);
					Endian.VarBytesFromInt(pckt, 55, 4, 1);
					pckt[56] = (byte)'H';
					pckt[57] = (byte)'S';
					//[hub status (4)]
				}
				int pcktLen;
				if(Stats.Updated.Gnutella2.ultrapeer)
				{
					pcktLen = pckt.Length;
					//library status
					uint filesTotal = (uint)Stats.Updated.filesShared;
					uint kbTotal = (uint)Stats.Updated.kbShared;
					foreach(Sck elsck in Sck.scks)
						if(elsck != null)
							if(elsck.mode == G2Mode.Leaf)
							{
								filesTotal += elsck.numFiles;
								kbTotal += elsck.numKB;
							}
					Array.Copy(Endian.GetBytes(filesTotal, !Stats.Updated.le), 0, pckt, 46, 4);
					Array.Copy(Endian.GetBytes(kbTotal, !Stats.Updated.le), 0, pckt, 50, 4);
					//hub status
					Array.Copy(Endian.GetBytes((ushort)ConnectionManager.leaves.Count, !Stats.Updated.le), 0, pckt, 58, 2);
					Array.Copy(Endian.GetBytes((ushort)Stats.settings.gConnectionsToKeep, !Stats.Updated.le), 0, pckt, 60, 2);
				}
				else
				{
					pcktLen = pckt.Length - 8;
					//library status
					Array.Copy(Endian.GetBytes(Stats.Updated.filesShared, !Stats.Updated.le), 0, pckt, 46, 4);
					Array.Copy(Endian.GetBytes(Stats.Updated.kbShared, !Stats.Updated.le), 0, pckt, 50, 4);
				}
				Endian.VarBytesFromInt(pckt, 1, pcktLen-5, 1);
				Array.Copy(Endian.BigEndianIP(Stats.settings.ipAddress), 0, pckt, 9, 4);
				Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, !Stats.Updated.le), 0, pckt, 13, 2);
				return OMessage.StandardFill(sck, ref buffIndex, pckt, pcktLen);
			}
		}
	}

	public class OKHL : OMessage
	{
		int chCount;

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			lock(HostCache.recentHubs)
			{
				int total = SizeTimestamp()+SizeNHs()+SizeCHs();
				int numBytesLen = Endian.NumBytesFromInt(total);
				if(!OMessage.CanFit(sck, ref buffIndex, 1+numBytesLen+3+total))
					return false;

				Setllen(sck.sendBuff, buffIndex, numBytesLen);
				Setnlen(sck.sendBuff, buffIndex, 3);
				SetRest(sck.sendBuff, buffIndex, true);
				buffIndex++;
				Endian.VarBytesFromInt(sck.sendBuff, buffIndex, total, numBytesLen);
				buffIndex += numBytesLen;
				sck.sendBuff[buffIndex] = (byte)'K';
				buffIndex++;
				sck.sendBuff[buffIndex] = (byte)'H';
				buffIndex++;
				sck.sendBuff[buffIndex] = (byte)'L';
				buffIndex++;

				//timestamp
				Setllen(sck.sendBuff, buffIndex, 1);
				Setnlen(sck.sendBuff, buffIndex, 2);
				SetRest(sck.sendBuff, buffIndex, false);
				buffIndex++;
				Endian.VarBytesFromInt(sck.sendBuff, buffIndex, 4, 1);
				buffIndex++;
				sck.sendBuff[buffIndex] = (byte)'T';
				buffIndex++;
				sck.sendBuff[buffIndex] = (byte)'S';
				buffIndex++;
				Array.Copy(Endian.GetBytes(Stats.Updated.timestamp, !Stats.Updated.le), 0, sck.sendBuff, buffIndex, 4);
				buffIndex += 4;

				//neighboring hubs
				for(int x = 0; x < ConnectionManager.ultrapeers.Count; x++)
				{
					Setllen(sck.sendBuff, buffIndex, 1);
					Setnlen(sck.sendBuff, buffIndex, 2);
					SetRest(sck.sendBuff, buffIndex, false);
					buffIndex++;
					Endian.VarBytesFromInt(sck.sendBuff, buffIndex, 6, 1);
					buffIndex++;
					sck.sendBuff[buffIndex] = (byte)'N';
					buffIndex++;
					sck.sendBuff[buffIndex] = (byte)'H';
					buffIndex++;
					Array.Copy(Endian.GetBytes(Sck.scks[((ConnectionManager.G2Host)ConnectionManager.ultrapeers.GetKey(x)).sockNum].remoteIPA), 0, sck.sendBuff, buffIndex, 4);
					buffIndex += 4;
					Array.Copy(Endian.GetBytes((ushort)Sck.scks[((ConnectionManager.G2Host)ConnectionManager.ultrapeers.GetKey(x)).sockNum].port, !Stats.Updated.le), 0, sck.sendBuff, buffIndex, 2);
					buffIndex += 2;
				}

				//cached hubs
				int pos = GUID.rand.Next(0, HostCache.recentHubs.Count);
				while(this.chCount > 0)
				{
					Setllen(sck.sendBuff, buffIndex, 1);
					Setnlen(sck.sendBuff, buffIndex, 2);
					SetRest(sck.sendBuff, buffIndex, false);
					buffIndex++;
					Endian.VarBytesFromInt(sck.sendBuff, buffIndex, 10, 1);
					buffIndex++;
					sck.sendBuff[buffIndex] = (byte)'C';
					buffIndex++;
					sck.sendBuff[buffIndex] = (byte)'H';
					buffIndex++;
					Array.Copy(Endian.GetBytes((IPAddress)HostCache.recentHubs.GetKey(pos)), 0, sck.sendBuff, buffIndex, 4);
					buffIndex += 4;
					Array.Copy(Endian.GetBytes(((HubInfo)HostCache.recentHubs.GetByIndex(pos)).port, !Stats.Updated.le), 0, sck.sendBuff, buffIndex, 2);
					buffIndex += 2;
					Array.Copy(Endian.GetBytes(Stats.Updated.timestamp - ((HubInfo)HostCache.recentHubs.GetByIndex(pos)).timeKnown, !Stats.Updated.le), 0, sck.sendBuff, buffIndex, 4);
					buffIndex += 4;
					pos++;
					if(pos == HostCache.recentHubs.Count)
						pos = 0;
					this.chCount--;
				}
				return true;
			}
		}

		int SizeTimestamp()
		{
			return (1+1+2+4);
		}

		int SizeNHs()
		{
			int eachNH = 1+1+2+6;
			return (eachNH*ConnectionManager.ultrapeers.Count);
		}

		int SizeCHs()
		{
			int eachCH = 1+1+2+10;
			this.chCount = GUID.rand.Next(15, 35);
			if(HostCache.recentHubs.Count < this.chCount)
				this.chCount = HostCache.recentHubs.Count;
			return (eachCH*this.chCount);
		}
	}

	public class OQHT : OMessage
	{
		public bool reset;
		public byte deflate;
		public byte fragNum;
		public byte fragCount;

		//indexindata and lenofdata make things more ugly, but much faster (less buffer copies)
		public int indexindata;
		public int lenofdata;
		public byte[] data;

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			if(reset)
			{
				byte[] pckt = new byte[1+1+3+6];
				Setllen(pckt, 0, 1);
				Setnlen(pckt, 0, 3);
				SetRest(pckt, 0, false);
				Endian.VarBytesFromInt(pckt, 1, 6, 1);
				pckt[2] = (byte)'Q';
				pckt[3] = (byte)'H';
				pckt[4] = (byte)'T';
				pckt[5] = 0x00;
				Array.Copy(Endian.GetBytes(QueryRouteTable.ourQHT.Length, !Stats.Updated.le), 0, pckt, 6, 4);
				pckt[10] = 0x01;
				return StandardFill(sck, ref buffIndex, pckt);
			}
			else
			{
				int payLen = 5+lenofdata;
				int llen = Endian.NumBytesFromInt(payLen);
				if(!CanFit(sck, ref buffIndex, 1+llen+3+payLen))
					return false;
				Setllen(sck.sendBuff, buffIndex, llen);
				Setnlen(sck.sendBuff, buffIndex, 3);
				SetRest(sck.sendBuff, buffIndex, false);
				buffIndex++;
				Endian.VarBytesFromInt(sck.sendBuff, buffIndex, payLen, llen);
				buffIndex += llen;
				sck.sendBuff[buffIndex] = (byte)'Q';
				sck.sendBuff[buffIndex+1] = (byte)'H';
				sck.sendBuff[buffIndex+2] = (byte)'T';
				buffIndex += 3;
				sck.sendBuff[buffIndex] = 0x01;
				sck.sendBuff[buffIndex+1] = fragNum;
				sck.sendBuff[buffIndex+2] = fragCount;
				sck.sendBuff[buffIndex+3] = deflate;
				sck.sendBuff[buffIndex+4] = 0x01;
				buffIndex += 5;
				Array.Copy(data, indexindata, sck.sendBuff, buffIndex, lenofdata);
				buffIndex += lenofdata;
				return true;
			}
		}
	}

	public class OQKR : OMessage, IUdpMessage
	{
		public static byte[] pckt = new byte[0];
		public static byte[] localIP = null;

		public void SetupUdpPacket(OutgoingPacket op)
		{
			byte[] udpPckt;
			lock(pckt)
			{
				if(pckt.Length == 0)
				{
					pckt = new byte[1+1+3 + 1+1+3+6];
					Setllen(pckt, 0, 1);
					Setnlen(pckt, 0, 3);
					SetRest(pckt, 0, true);
					Endian.VarBytesFromInt(pckt, 1, 11, 1);
					pckt[2] = (byte)'Q';
					pckt[3] = (byte)'K';
					pckt[4] = (byte)'R';
					Setllen(pckt, 5, 1);
					Setnlen(pckt, 5, 3);
					SetRest(pckt, 5, false);
					Endian.VarBytesFromInt(pckt, 6, 6, 1);
					pckt[7] = (byte)'R';
					pckt[8] = (byte)'N';
					pckt[9] = (byte)'A';
				}
				udpPckt = new byte[pckt.Length+8];
				Array.Copy(pckt, 0, udpPckt, 8, pckt.Length);
			}
			//key for either us or proxy hub
			if(Stats.Updated.udpIncoming)
			{
				if(OQKR.localIP == null)
					OQKR.localIP = Endian.BigEndianIP(Stats.settings.ipAddress);
				Array.Copy(OQKR.localIP, 0, udpPckt, 18, 4);
				Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, !Stats.Updated.le), 0, udpPckt, 22, 2);
			}
			else
			{
				if(ConnectionManager.ultrapeers.Count == 0)
					op.cancel = true;
				else
				{
					int sockNum = ((ConnectionManager.G2Host)ConnectionManager.ultrapeers.GetKey(GUID.rand.Next(0, ConnectionManager.ultrapeers.Count))).sockNum;
					if(Sck.scks[sockNum].remoteIPA == null)
						op.cancel = true;
					else
					{
						Array.Copy(Endian.GetBytes(Sck.scks[sockNum].remoteIPA), 0, udpPckt, 18, 4);
						Array.Copy(Endian.GetBytes((ushort)Sck.scks[sockNum].port, !Stats.Updated.le), 0, udpPckt, 22, 2);
					}
				}
			}
			op.deflate = false;
			op.pcktData = udpPckt;
			op.acks = null;
			op.parts = new BitArray(UDPSR.GetPartCount(udpPckt.Length-8), false);
			op.len = udpPckt.Length-8;
		}

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			System.Diagnostics.Debug.WriteLine("G2 OQKR: this shouldn't have been called");
			return true;
		}
	}

	public class OQKA : OMessage, IUdpMessage
	{
		public bool qk = false;
		public int queryKey = -1;
		public IPAddress sna = null;
		public IPAddress qna = null;
		public static byte[] header = new byte[0];

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			lock(header)
			{
				FillHeader();
				int size = SizeRest();
				if(CanFit(sck, ref buffIndex, header.Length+size))
				{
					Array.Copy(header, 0, sck.sendBuff, buffIndex, header.Length);
					buffIndex += header.Length;
					FillRest(sck.sendBuff, ref buffIndex);
					return true;
				}
				else
					return false;
			}
		}

		public void SetupUdpPacket(OutgoingPacket op)
		{
			lock(header)
			{
				FillHeader();
				int size = SizeRest();
				byte[] payload = new byte[8+header.Length+size];
				int buffIndex = 8+header.Length;
				FillRest(payload, ref buffIndex);
				Array.Copy(header, 0, payload, 8, header.Length);
				op.deflate = false;
				op.pcktData = payload;
				op.acks = null;
				op.parts = new BitArray(UDPSR.GetPartCount(payload.Length-8), false);
				op.len = payload.Length-8;
			}
		}

		void FillHeader()
		{
			if(header.Length == 0)
			{
				header = new byte[1+1+3];
				Setllen(header, 0, 1);
				Setnlen(header, 0, 3);
				SetRest(header, 0, true);
				//skip length for now
				header[2] = (byte)'Q';
				header[3] = (byte)'K';
				header[4] = (byte)'A';
			}
		}

		int SizeRest()
		{
			int size = (1+1+3+4);	//sna
			if(qna != null)
				size += (1+1+3+4);	//qna
			if(this.qk)
				size += (1+1+2+4);	//query key
			Endian.VarBytesFromInt(header, 1, size, 1);
			return size;
		}

		void FillRest(byte[] payload, ref int buffIndex)
		{
			//query key
			if(this.qk)
			{
				Setllen(payload, buffIndex, 1);
				Setnlen(payload, buffIndex, 2);
				SetRest(payload, buffIndex, false);
				buffIndex++;
				Endian.VarBytesFromInt(payload, buffIndex, 4, 1);
				buffIndex++;
				payload[buffIndex] = (byte)'Q';
				buffIndex++;
				payload[buffIndex] = (byte)'K';
				buffIndex++;
				Array.Copy(Endian.GetBytes(queryKey, !Stats.Updated.le), 0, payload, buffIndex, 4);
				buffIndex += 4;
			}
			//sna
			Setllen(payload, buffIndex, 1);
			Setnlen(payload, buffIndex, 3);
			SetRest(payload, buffIndex, false);
			buffIndex++;
			Endian.VarBytesFromInt(payload, buffIndex, 4, 1);
			buffIndex++;
			payload[buffIndex] = (byte)'S';
			buffIndex++;
			payload[buffIndex] = (byte)'N';
			buffIndex++;
			payload[buffIndex] = (byte)'A';
			buffIndex++;
			Array.Copy(Endian.GetBytes(sna), 0, payload, buffIndex, 4);
			buffIndex += 4;
			//qna
			if(qna != null)
			{
				Setllen(payload, buffIndex, 1);
				Setnlen(payload, buffIndex, 3);
				SetRest(payload, buffIndex, false);
				buffIndex++;
				Endian.VarBytesFromInt(payload, buffIndex, 4, 1);
				buffIndex++;
				payload[buffIndex] = (byte)'Q';
				buffIndex++;
				payload[buffIndex] = (byte)'N';
				buffIndex++;
				payload[buffIndex] = (byte)'A';
				buffIndex++;
				Array.Copy(Endian.GetBytes(qna), 0, payload, buffIndex, 4);
				buffIndex += 4;
			}
		}
	}

	public class OQ2 : OMessage, IUdpMessage
	{
		public byte[] pckt = null;
		//descriptive name
		public string query = "";
		//associated guid
		public byte[] guid = null;
		//should this be sent directly over a tcp connection to a hub
		public bool tcpQuery = false;
		//do we need a proxy hub because we're udp-firewalled
		public IPAddress ipaProxyHub = null;
		public int proxyPort;
		//hub info for reference
		public HubInfo hi;
		//any urn?
		public byte[] urn = null;
		//interest packet combinations: 0=none, 1=URL&DN
		public int interest = 0;

		public void SetupUdpPacket(OutgoingPacket op)
		{
			CreatePacket(8);
			op.deflate = false;
			op.pcktData = pckt;
			op.acks = null;
			op.parts = new BitArray(UDPSR.GetPartCount(pckt.Length-8), false);
			op.len = pckt.Length-8;
		}

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			CreatePacket(0);
			return OMessage.StandardFill(sck, ref buffIndex, pckt, true);
		}

		void CreatePacket(int i)
		{
			if(pckt == null)
			{
				//the 1 is for the null control byte to end the series of children... and the 16 is the guid payload
				int total = SizeUDP() + SizeURN() + SizeDN() + SizeI() + 1 + 16;
				int numBytesLen = Endian.NumBytesFromInt(total);
				this.pckt = new byte[i+1+numBytesLen+2+total];
				Setllen(pckt, i, numBytesLen);
				Setnlen(pckt, i, 2);
				SetRest(pckt, i, true);
				i++;
				Endian.VarBytesFromInt(pckt, i, total, numBytesLen);
				i += numBytesLen;
				pckt[i] = (byte)'Q';
				i++;
				pckt[i] = (byte)'2';
				i++;
				//udp
				if(!tcpQuery)
				{
					Setllen(pckt, i, 1);
					Setnlen(pckt, i, 3);
					SetRest(pckt, i, false);
					i++;
					Endian.VarBytesFromInt(pckt, i, 10, 1);
					i++;
					pckt[i] = (byte)'U';
					i++;
					pckt[i] = (byte)'D';
					i++;
					pckt[i] = (byte)'P';
					i++;
					if(this.ipaProxyHub == null)
					{
						Array.Copy(Endian.GetBytes(Stats.Updated.myIPA), 0, pckt, i, 4);
						i += 4;
						Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, !Stats.Updated.le), 0, pckt, i, 2);
						i += 2;
					}
					else
					{
						Array.Copy(Endian.GetBytes(this.ipaProxyHub), 0, pckt, i, 4);
						i += 4;
						Array.Copy(Endian.GetBytes((ushort)proxyPort, !Stats.Updated.le), 0, pckt, i, 2);
						i += 2;
					}
					Array.Copy(Endian.GetBytes(hi.mhi.queryKey, !Stats.Updated.le), 0, pckt, i, 4);
					i += 4;
				}
				//urn
				if(this.urn != null)
				{
					Setllen(pckt, i, 1);
					Setnlen(pckt, i, 3);
					SetRest(pckt, i, false);
					i++;
					Endian.VarBytesFromInt(pckt, i, this.urn.Length, 1);
					i++;
					pckt[i] = (byte)'U';
					i++;
					pckt[i] = (byte)'R';
					i++;
					pckt[i] = (byte)'N';
					i++;
					Array.Copy(this.urn, 0, pckt, i, this.urn.Length);
					i += this.urn.Length;
				}
				//dn
				if(this.query.Length > 0)
				{
					Setllen(pckt, i, 1);
					Setnlen(pckt, i, 2);
					SetRest(pckt, i, false);
					i++;
					Endian.VarBytesFromInt(pckt, i, this.query.Length, 1);
					i++;
					pckt[i] = (byte)'D';
					i++;
					pckt[i] = (byte)'N';
					i++;
					Array.Copy(Encoding.ASCII.GetBytes(this.query), 0, pckt, i, this.query.Length);
					i += this.query.Length;
				}
				//i
				if(this.interest == 1)
				{
					Setllen(pckt, i, 1);
					Setnlen(pckt, i, 1);
					SetRest(pckt, i, false);
					i++;
					Endian.VarBytesFromInt(pckt, i, 7, 1);
					i++;
					pckt[i] = (byte)'I';
					i++;
					pckt[i] = (byte)'U';
					i++;
					pckt[i] = (byte)'R';
					i++;
					pckt[i] = (byte)'L';
					i++;
					pckt[i] = (byte)'\0';
					i++;
					pckt[i] = (byte)'D';
					i++;
					pckt[i] = (byte)'N';
					i++;
					pckt[i] = (byte)'\0';
					i++;
				}
				//the rest
				pckt[i] = 0x00;
				i++;
				Array.Copy(this.guid, 0, pckt, i, 16);
				i += 16;
			}
		}

		int SizeUDP()
		{
			if(this.tcpQuery)
				return 0;
			else
				return (1+1+3+10);
		}

		int SizeURN()
		{
			if(this.urn == null)
				return 0;
			else
				return (1+1+3+this.urn.Length);
		}

		int SizeDN()
		{
			if(this.query.Length == 0)
				return 0;
			else
				return (1+1+2+this.query.Length);
		}

		int SizeI()
		{
			if(this.interest == 0)
				return 0;
			else if(this.interest == 1)
				return (1+1+1+7);
			else
				return 0;
		}
	}

	public class OQA : OMessage, IUdpMessage
	{
		public bool tcp;	//tcp or not
		public byte[] sGUID;//search guid
		public int gOffset;	//offset in search guid
		int dCount = 0;		//amount of Done hubs
		int sCount1 = 0;	//amount of local cluster Search hubs
		int sCount2 = 0;	//amount of cached Search hubs
		int numBytesLen;
		int total;

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			lock(HostCache.recentHubs)
			{
				total = SizeTimestamp()+SizeDs()+SizeSs()+1+16;
				numBytesLen = Endian.NumBytesFromInt(total);
				if(!OMessage.CanFit(sck, ref buffIndex, 1+numBytesLen+2+total))
					return false;

				FillBuff(sck.sendBuff, ref buffIndex);
				return true;
			}
		}

		public void SetupUdpPacket(OutgoingPacket op)
		{
			lock(HostCache.recentHubs)
			{
				total = SizeTimestamp()+SizeDs()+SizeSs()+1+16;
				numBytesLen = Endian.NumBytesFromInt(total);
				byte[] pckt = new byte[8+1+numBytesLen+2+total];
				int buffIndex = 8;
				FillBuff(pckt, ref buffIndex);
				op.deflate = false;
				op.pcktData = pckt;
				op.acks = null;
				op.parts = new BitArray(UDPSR.GetPartCount(pckt.Length-8), false);
				op.len = pckt.Length-8;
			}
		}

		void FillBuff(byte[] bytarr, ref int buffIndex)
		{
			Setllen(bytarr, buffIndex, numBytesLen);
			Setnlen(bytarr, buffIndex, 2);
			SetRest(bytarr, buffIndex, true);
			buffIndex++;
			Endian.VarBytesFromInt(bytarr, buffIndex, total, numBytesLen);
			buffIndex += numBytesLen;
			bytarr[buffIndex] = (byte)'Q';
			buffIndex++;
			bytarr[buffIndex] = (byte)'A';
			buffIndex++;

			//timestamp
			Setllen(bytarr, buffIndex, 1);
			Setnlen(bytarr, buffIndex, 2);
			SetRest(bytarr, buffIndex, false);
			buffIndex++;
			Endian.VarBytesFromInt(bytarr, buffIndex, 4, 1);
			buffIndex++;
			bytarr[buffIndex] = (byte)'T';
			buffIndex++;
			bytarr[buffIndex] = (byte)'S';
			buffIndex++;
			Array.Copy(Endian.GetBytes(Stats.Updated.timestamp, !Stats.Updated.le), 0, bytarr, buffIndex, 4);
			buffIndex += 4;

			//done hubs
			for(int x = 0; x <= ConnectionManager.ultrapeers.Count; x++)
			{
				Setllen(bytarr, buffIndex, 1);
				Setnlen(bytarr, buffIndex, 1);
				SetRest(bytarr, buffIndex, false);
				buffIndex++;
				Endian.VarBytesFromInt(bytarr, buffIndex, 8, 1);
				buffIndex++;
				bytarr[buffIndex] = (byte)'D';
				buffIndex++;
				if(x < ConnectionManager.ultrapeers.Count)
				{
					int tmpSckNum = ((ConnectionManager.G2Host)ConnectionManager.ultrapeers.GetKey(x)).sockNum;
					Array.Copy(Endian.GetBytes(Sck.scks[tmpSckNum].remoteIPA), 0, bytarr, buffIndex, 4);
					buffIndex += 4;
					Array.Copy(Endian.GetBytes((ushort)Sck.scks[tmpSckNum].port, !Stats.Updated.le), 0, bytarr, buffIndex, 2);
					buffIndex += 2;
					Array.Copy(Endian.GetBytes(Sck.scks[tmpSckNum].leaves, !Stats.Updated.le), 0, bytarr, buffIndex, 2);
					buffIndex += 2;
				}
				else
				{
					Array.Copy(Endian.GetBytes(Stats.Updated.myIPA), 0, bytarr, buffIndex, 4);
					buffIndex += 4;
					Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, !Stats.Updated.le), 0, bytarr, buffIndex, 2);
					buffIndex += 2;
					Array.Copy(Endian.GetBytes((ushort)ConnectionManager.leaves.Count, !Stats.Updated.le), 0, bytarr, buffIndex, 2);
					buffIndex += 2;
				}
			}

			//search hubs
			foreach(ConnectionManager.G2Host g2h in ConnectionManager.ultrapeers.Keys)
				if(Sck.scks[g2h.sockNum].neighbors.Count > 0)
					foreach(IPEndPoint ipep in Sck.scks[g2h.sockNum].neighbors)
					{
						Setllen(bytarr, buffIndex, 1);
						Setnlen(bytarr, buffIndex, 1);
						SetRest(bytarr, buffIndex, false);
						buffIndex++;
						Endian.VarBytesFromInt(bytarr, buffIndex, 6, 1);
						buffIndex++;
						bytarr[buffIndex] = (byte)'S';
						buffIndex++;
						Array.Copy(Endian.GetBytes(ipep.Address), 0, bytarr, buffIndex, 4);
						buffIndex += 4;
						Array.Copy(Endian.GetBytes((ushort)ipep.Port, !Stats.Updated.le), 0, bytarr, buffIndex, 2);
						buffIndex += 2;
					}
			int pos = GUID.rand.Next(0, HostCache.recentHubs.Count);
			while(this.sCount2 > 0)
			{
				Setllen(bytarr, buffIndex, 1);
				Setnlen(bytarr, buffIndex, 1);
				SetRest(bytarr, buffIndex, false);
				buffIndex++;
				Endian.VarBytesFromInt(bytarr, buffIndex, 10, 1);
				buffIndex++;
				bytarr[buffIndex] = (byte)'S';
				buffIndex++;
				Array.Copy(Endian.GetBytes((IPAddress)HostCache.recentHubs.GetKey(pos)), 0, bytarr, buffIndex, 4);
				buffIndex += 4;
				Array.Copy(Endian.GetBytes(((HubInfo)HostCache.recentHubs.GetByIndex(pos)).port, !Stats.Updated.le), 0, bytarr, buffIndex, 2);
				buffIndex += 2;
				Array.Copy(Endian.GetBytes(Stats.Updated.timestamp - ((HubInfo)HostCache.recentHubs.GetByIndex(pos)).timeKnown, !Stats.Updated.le), 0, bytarr, buffIndex, 4);
				buffIndex += 4;
				pos++;
				if(pos == HostCache.recentHubs.Count)
					pos = 0;
				this.sCount2--;
			}

			//null and guid
			bytarr[buffIndex] = 0x00;
			buffIndex++;
			Array.Copy(sGUID, gOffset, bytarr, buffIndex, 16);
			buffIndex += 16;
		}

		int SizeTimestamp()
		{
			return (1+1+2+4);
		}

		int SizeDs()
		{
			int eachD = 1+1+1+8;
			//we include the local hub (+1)
			this.dCount = ConnectionManager.ultrapeers.Count+1;
			return (eachD*this.dCount);
		}

		int SizeSs()
		{
			int eachS1 = 1+1+1+6;
			int eachS2 = 1+1+1+10;
			foreach(ConnectionManager.G2Host g2h in ConnectionManager.ultrapeers.Keys)
				if(Sck.scks[g2h.sockNum].neighbors.Count > 0)
					this.sCount1 += Sck.scks[g2h.sockNum].neighbors.Count;
			int num2 = this.sCount1 > 20 ? 6 : 10;
			this.sCount2 = GUID.rand.Next(3, num2);
			if(HostCache.recentHubs.Count < this.sCount2)
				this.sCount2 = HostCache.recentHubs.Count;
			return ((eachS1*this.sCount1) + (eachS2*this.sCount2));
		}
	}

	public class OQH2 : OMessage, IUdpMessage
	{
		static byte[] pcktStaticPart = new byte[0];
		public byte[] searchGUID;
		public int searchGUIDoffset;
		public Query.Interests interests = new Query.Interests();
		public ArrayList matches;

		public void SetupUdpPacket(OutgoingPacket op)
		{
			//payload inside qh2 is static parts, dynamic parts, null, hop count + guid
			int payloadSize;
			byte[] wholepacket;
			int i;
			lock(pcktStaticPart)
			{
				payloadSize = this.GetSizeStaticParts() + this.GetSizeDynamicParts() + 1 + 17;
				wholepacket = new byte[8+this.GetSizeQH2Header(payloadSize)+payloadSize];
				i = 8;
				FillQH2Header(wholepacket, ref i, payloadSize);
				Array.Copy(pcktStaticPart, 0, wholepacket, i, pcktStaticPart.Length);
				i += pcktStaticPart.Length;
				FillDynamicPart(wholepacket, ref i);
				FillRest(wholepacket, ref i);
			}
#if DEBUG
			if(i != wholepacket.Length)
				System.Diagnostics.Debug.WriteLine("G2 oqh2 length problem");
#endif

			op.deflate = false;
			op.pcktData = wholepacket;
			op.acks = null;
			op.parts = new BitArray(UDPSR.GetPartCount(wholepacket.Length-8), false);
			op.len = wholepacket.Length-8;
		}

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			//payload inside qh2 is static parts, dynamic parts, null, hop count + guid
			int payloadSize;
			lock(pcktStaticPart)
			{
				payloadSize = this.GetSizeStaticParts() + this.GetSizeDynamicParts() + 1 + 17;
				if(!OMessage.CanFit(sck, ref buffIndex, this.GetSizeQH2Header(payloadSize)+payloadSize))
				{
					if(buffIndex != 0)
						return false;
					else
						sck.sendBuff = new byte[this.GetSizeQH2Header(payloadSize)+payloadSize];
				}
				FillQH2Header(sck.sendBuff, ref buffIndex, payloadSize);
				Array.Copy(pcktStaticPart, 0, sck.sendBuff, buffIndex, pcktStaticPart.Length);
				buffIndex += pcktStaticPart.Length;
				FillDynamicPart(sck.sendBuff, ref buffIndex);
				FillRest(sck.sendBuff, ref buffIndex);
			}
			return true;
		}

		public void BrowseHostData(out byte[] dataQH2s)
		{
			if(this.matches.Count == 0)
			{
				dataQH2s = new byte[0];
				return;
			}
			//payload inside qh2 is static parts, dynamic parts, null, hop count + guid
			int payloadSize;
			int i;
			lock(pcktStaticPart)
			{
				payloadSize = this.GetSizeStaticParts() + this.GetSizeDynamicParts() + 1 + 17;
				dataQH2s = new byte[this.GetSizeQH2Header(payloadSize)+payloadSize];
				i = 0;
				FillQH2Header(dataQH2s, ref i, payloadSize);
				Array.Copy(pcktStaticPart, 0, dataQH2s, i, pcktStaticPart.Length);
				i += pcktStaticPart.Length;
				FillDynamicPart(dataQH2s, ref i);
				FillRest(dataQH2s, ref i);
			}
		}

		void FillQH2Header(byte[] bytarr, ref int i, int payloadSize)
		{
			int numBytesLen = Endian.NumBytesFromInt(payloadSize);
			Setllen(bytarr, i, numBytesLen);
			Setnlen(bytarr, i, 3);
			SetRest(bytarr, i, true);
			i++;
			Endian.VarBytesFromInt(bytarr, i, payloadSize, numBytesLen);
			i += numBytesLen;
			bytarr[i] = (byte)'Q';
			i++;
			bytarr[i] = (byte)'H';
			i++;
			bytarr[i] = (byte)'2';
			i++;
		}

		public static void ResetStaticPart()
		{
			lock(pcktStaticPart)
				pcktStaticPart = new byte[0];
		}

		void FillStaticPart()
		{
			lock(pcktStaticPart)
			{
				int i = 0;
				//for neighboring hub scenario
				int sockNum = -1;
				if(!Stats.Updated.Gnutella2.ultrapeer && ConnectionManager.ultrapeers.Count > 0)
				{
					sockNum = ((ConnectionManager.G2Host)ConnectionManager.ultrapeers.GetKey(GUID.rand.Next(0, ConnectionManager.ultrapeers.Count))).sockNum;
					if(Sck.scks[sockNum].remoteIPA == null)
						sockNum = -1;
				}
				//guid, node address, neighboring hub, vendor code, browse host, chat tag
				if(sockNum != -1)
					pcktStaticPart = new byte[1+1+2+16 + 1+1+2+6 + 1+1+2+6 + 1+1+1+4 + 1+0+3+0 + 1+0+3+0];
				else
					pcktStaticPart = new byte[1+1+2+16 + 1+1+2+6 + 0+0+0+0 + 1+1+1+4 + 1+0+3+0 + 1+0+3+0];
				//GU
				Setllen(pcktStaticPart, i, 1);
				Setnlen(pcktStaticPart, i, 2);
				SetRest(pcktStaticPart, i, false);
				i++;
				Endian.VarBytesFromInt(pcktStaticPart, i, 16, 1);
				i++;
				pcktStaticPart[i] = (byte)'G';
				i++;
				pcktStaticPart[i] = (byte)'U';
				i++;
				Array.Copy(Stats.settings.myGUID, 0, pcktStaticPart, i, 16);
				i += 16;
				//NA
				Setllen(pcktStaticPart, i, 1);
				Setnlen(pcktStaticPart, i, 2);
				SetRest(pcktStaticPart, i, false);
				i++;
				Endian.VarBytesFromInt(pcktStaticPart, i, 6, 1);
				i++;
				pcktStaticPart[i] = (byte)'N';
				i++;
				pcktStaticPart[i] = (byte)'A';
				i++;
				Array.Copy(Endian.GetBytes(Stats.Updated.myIPA), 0, pcktStaticPart, i, 4);
				i += 4;
				Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, !Stats.Updated.le), 0, pcktStaticPart, i, 2);
				i += 2;
				//NH
				if(sockNum != -1)
				{
					Setllen(pcktStaticPart, i, 1);
					Setnlen(pcktStaticPart, i, 2);
					SetRest(pcktStaticPart, i, false);
					i++;
					Endian.VarBytesFromInt(pcktStaticPart, i, 6, 1);
					i++;
					pcktStaticPart[i] = (byte)'N';
					i++;
					pcktStaticPart[i] = (byte)'H';
					i++;
					Array.Copy(Endian.GetBytes(Sck.scks[sockNum].remoteIPA), 0, pcktStaticPart, i, 4);
					i += 4;
					Array.Copy(Endian.GetBytes((ushort)Sck.scks[sockNum].port, !Stats.Updated.le), 0, pcktStaticPart, i, 2);
					i += 2;
				}
				//V
				Setllen(pcktStaticPart, i, 1);
				Setnlen(pcktStaticPart, i, 1);
				SetRest(pcktStaticPart, i, false);
				i++;
				Endian.VarBytesFromInt(pcktStaticPart, i, 4, 1);
				i++;
				pcktStaticPart[i] = (byte)'V';
				i++;
				pcktStaticPart[i] = (byte)'F';
				i++;
				pcktStaticPart[i] = (byte)'S';
				i++;
				pcktStaticPart[i] = (byte)'C';
				i++;
				pcktStaticPart[i] = (byte)'P';
				i++;
				//BUP
				Setllen(pcktStaticPart, i, 0);
				Setnlen(pcktStaticPart, i, 3);
				SetRest(pcktStaticPart, i, false);
				i++;
				pcktStaticPart[i] = (byte)'B';
				i++;
				pcktStaticPart[i] = (byte)'U';
				i++;
				pcktStaticPart[i] = (byte)'P';
				i++;
				//PCH
				Setllen(pcktStaticPart, i, 0);
				Setnlen(pcktStaticPart, i, 3);
				SetRest(pcktStaticPart, i, false);
				i++;
				pcktStaticPart[i] = (byte)'P';
				i++;
				pcktStaticPart[i] = (byte)'C';
				i++;
				pcktStaticPart[i] = (byte)'H';
				i++;
			}
		}

		void FillDynamicPart(byte[] bytarr, ref int i)
		{
			foreach(FileObject fo in this.matches)
			{
				int numBytesLen = Endian.NumBytesFromInt(fo.tempOne);
				Setllen(bytarr, i, numBytesLen);
				Setnlen(bytarr, i, 1);
				SetRest(bytarr, i, true);
				i++;
				Endian.VarBytesFromInt(bytarr, i, fo.tempOne, numBytesLen);
				i += numBytesLen;
				bytarr[i] = (byte)'H';
				i++;
				//urn
				Setllen(bytarr, i, 1);
				Setnlen(bytarr, i, 3);
				SetRest(bytarr, i, false);
				i++;
				Endian.VarBytesFromInt(bytarr, i, 25, 1);
				i++;
				bytarr[i] = (byte)'U';
				i++;
				bytarr[i] = (byte)'R';
				i++;
				bytarr[i] = (byte)'N';
				i++;
				bytarr[i] = (byte)'s';
				i++;
				bytarr[i] = (byte)'h';
				i++;
				bytarr[i] = (byte)'a';
				i++;
				bytarr[i] = (byte)'1';
				i++;
				bytarr[i] = (byte)'\0';
				i++;
				Array.Copy(fo.sha1bytes, 0, bytarr, i, 20);
				i += 20;
				//url
				if(interests.all || interests.locations)
				{
					Setllen(bytarr, i, 0);
					Setnlen(bytarr, i, 3);
					SetRest(bytarr, i, false);
					i++;
					bytarr[i] = (byte)'U';
					i++;
					bytarr[i] = (byte)'R';
					i++;
					bytarr[i] = (byte)'L';
					i++;
				}
				if(interests.all || interests.descriptivenames)
				{
					//sz
					Setllen(bytarr, i, 1);
					Setnlen(bytarr, i, 2);
					SetRest(bytarr, i, false);
					i++;
					Endian.VarBytesFromInt(bytarr, i, 4, 1);
					i++;
					bytarr[i] = (byte)'S';
					i++;
					bytarr[i] = (byte)'Z';
					i++;
					Array.Copy(Endian.GetBytes(fo.b, !Stats.Updated.le), 0, bytarr, i, 4);
					i += 4;
					//dn
					int numBytesLen2 = Endian.NumBytesFromInt(fo.lcaseFileName.Length);
					Setllen(bytarr, i, numBytesLen2);
					Setnlen(bytarr, i, 2);
					SetRest(bytarr, i, false);
					i++;
					Endian.VarBytesFromInt(bytarr, i, fo.lcaseFileName.Length, numBytesLen2);
					i += numBytesLen2;
					bytarr[i] = (byte)'D';
					i++;
					bytarr[i] = (byte)'N';
					i++;
					Encoding.ASCII.GetBytes(fo.location, fo.location.Length-fo.lcaseFileName.Length, fo.lcaseFileName.Length, bytarr, i);
					i += fo.lcaseFileName.Length;
				}
			}
		}

		/// <summary>
		/// This will add on the null to signify the end of child packets.
		/// And it will also add the qh2 payload: hop count + guid.
		/// </summary>
		void FillRest(byte[] bytarr, ref int i)
		{
			bytarr[i] = 0x00;
			i++;
			bytarr[i] = 0x00;
			i++;
			Array.Copy(this.searchGUID, searchGUIDoffset, bytarr, i, 16);
			i += 16;
		}

		//five segments: query hit header, static parts, dynamic hits, null, hop count + guid

		int GetSizeQH2Header(int payloadSize)
		{
			return (1+Endian.NumBytesFromInt(payloadSize)+3);
		}

		int GetSizeStaticParts()
		{
			if(pcktStaticPart.Length == 0)
				FillStaticPart();
			return pcktStaticPart.Length;
		}

		int GetSizeDynamicParts()
		{
			int total = 0;
			foreach(FileObject fo in this.matches)
			{
				//first deal with the /H/? children
				int payloadSize = 0;
				//urn... don't forget the null char that follows the string identifying the urn family
				payloadSize += (1+1+3+(5+20));
				//url
				if(interests.all || interests.locations)
					payloadSize += (1+0+3+0);
				if(interests.all || interests.descriptivenames)
				{
					//sz
					payloadSize += (1+1+2+4);
					//dn
					payloadSize += (1+Endian.NumBytesFromInt(fo.lcaseFileName.Length)+2+fo.lcaseFileName.Length);
				}
				//store size
				fo.tempOne = payloadSize;

				//now header
				total += (1+Endian.NumBytesFromInt(payloadSize)+1+payloadSize);
			}
			return total;
		}
	}

	public class OPush : OMessage, IUdpMessage
	{
		static byte[] pckt = new byte[0];
		public GUIDitem gitem;

		public static void SendPush(QueryHitObject qho)
		{
			if(qho.servIdent == null)
				return;
			if(!Stats.Updated.Gnutella2.ultrapeer && !Stats.Updated.udpIncoming && !Stats.Updated.everIncoming)
				return;
			OPush opush = new OPush();
			opush.gitem = new GUIDitem(qho.servIdent);
			if((qho.hops == 0 || qho.hops == 1) && qho.sockWhereFrom != -1)
				Sck.scks[qho.sockWhereFrom].SendPacket(opush);
			else
			{
				if(qho.ipepNH != null)
					UDPSR.SendOutgoingPacket(opush, qho.ipepNH);
			}
		}

		public void SetupUdpPacket(OutgoingPacket op)
		{
			CreatePacket();
			byte[] pcktCopy = new byte[8+pckt.Length];
			Array.Copy(pckt, 0, pcktCopy, 8, pckt.Length);
			op.deflate = false;
			op.pcktData = pcktCopy;
			op.acks = null;
			op.parts = new BitArray(UDPSR.GetPartCount(pcktCopy.Length-8), false);
			op.len = pcktCopy.Length-8;
		}

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			CreatePacket();
			return OMessage.StandardFill(sck, ref buffIndex, pckt);
		}

		void CreatePacket()
		{
			if(pckt.Length != 0)
			{
				Array.Copy(this.gitem.gUiD, this.gitem.loc, pckt, 10, 16);
				return;
			}

			int i = 0;
			pckt = new byte[1+1+4 + 1+1+2+16 + 1+6];
			Setllen(pckt, i, 1);
			Setnlen(pckt, i, 4);
			SetRest(pckt, i, true);
			i++;
			Endian.VarBytesFromInt(pckt, i, 27, 1);
			i++;
			pckt[i] = (byte)'P';
			i++;
			pckt[i] = (byte)'U';
			i++;
			pckt[i] = (byte)'S';
			i++;
			pckt[i] = (byte)'H';
			i++;

			//TO
			Setllen(pckt, i, 1);
			Setnlen(pckt, i, 2);
			SetRest(pckt, i, false);
			i++;
			Endian.VarBytesFromInt(pckt, i, 16, 1);
			i++;
			pckt[i] = (byte)'T';
			i++;
			pckt[i] = (byte)'O';
			i++;
			Array.Copy(this.gitem.gUiD, this.gitem.loc, pckt, i, 16);
			i += 16;

			pckt[i] = 0x00;
			i++;
			Array.Copy(Endian.GetBytes(Stats.Updated.myIPA), 0, pckt, i, 4);
			i += 4;
			Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, !Stats.Updated.le), 0, pckt, i, 2);
			i += 2;
		}
	}

	/// <summary>
	/// If we're routing a packet, we just use this class to copy it verbatim.
	/// </summary>
	public class OCopy : OMessage, IUdpMessage
	{
		public byte[] header;
		public byte[] payload;

		public override bool FillSendBuff(Sck sck, ref int buffIndex)
		{
			if(CanFit(sck, ref buffIndex, header.Length+payload.Length))
			{
				Array.Copy(header, 0, sck.sendBuff, buffIndex, header.Length);
				buffIndex += header.Length;
				Array.Copy(payload, 0, sck.sendBuff, buffIndex, payload.Length);
				buffIndex += payload.Length;
				return true;
			}
			else
				return false;
		}

		public void SetupUdpPacket(OutgoingPacket op)
		{
			byte[] pckt = new byte[header.Length+payload.Length];
			Array.Copy(header, 0, pckt, 0, header.Length);
			Array.Copy(payload, 0, pckt, header.Length, payload.Length);
			op.deflate = false;
			op.pcktData = pckt;
			op.acks = null;
			op.parts = new BitArray(UDPSR.GetPartCount(pckt.Length-8), false);
			op.len = pckt.Length-8;
		}

		public void CreateHeader(string name, bool be, bool cf)
		{
			int llen = Endian.NumBytesFromInt(payload.Length);
			this.header = new byte[1+llen+name.Length];
			this.header[0] = 0x00;
			this.header[0] |= (byte)(llen << 6);
			this.header[0] |= (byte)((name.Length-1) << 3);
			if(cf)
				this.header[0] |= 0x04;
			if(be)
				this.header[0] |= 0x02;
			Endian.VarBytesFromInt(this.header, 1, payload.Length, llen);
			Encoding.ASCII.GetBytes(name, 0, name.Length, this.header, 1+llen);
		}
	}
}
