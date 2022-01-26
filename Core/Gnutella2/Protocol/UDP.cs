// UDP.cs
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
using System.Timers;
using System.Collections;
using System.Net;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace FileScope.Gnutella2
{
	public class IncomingKey
	{
		public static IncomingKeyComparer ikc = new IncomingKeyComparer();

		public int ip;
		public int seq;

		public IncomingKey(long ip, short seq)
		{
			this.ip = (int)ip;
			this.seq = (int)seq;
		}

		public override int GetHashCode()
		{
			return ip^seq;
		}

		public override bool Equals(object obj)
		{
			IncomingKey ik = (IncomingKey)obj;
			return (ik.ip == this.ip && ik.seq == this.seq);
		}
	}

	public class IncomingKeyComparer : IComparer
	{
		public int Compare(object ik1, object ik2)
		{
			return (((IncomingKey)ik1).GetHashCode() - ((IncomingKey)ik2).GetHashCode());
		}
	}

	public class IncomingPacket
	{
		public int secPassed = 0;
		public bool deflate;
		public InflaterInputStream iis = null;
		public BitArray parts;
		public byte[] pcktData;
		public int startPoint;
		public int bytesLeft;

		public void ProcessMe(IPEndPoint ipep)
		{
			if(deflate)
				iis = new InflaterInputStream(new System.IO.MemoryStream(pcktData, startPoint, bytesLeft, false));
			byte[] byt = new byte[12];
		again:
			bool proceed = GetData(byt, 0, 1);
			if(!proceed)
				return;
			int llen = (int)byt[0] >> 6;
			int nlen = ((int)(byt[0] & 0x38) >> 3) + 1;
			GetData(byt, 1, llen+nlen);
			string name = System.Text.Encoding.ASCII.GetString(byt, 1+llen, nlen);
			//System.Diagnostics.Debug.WriteLine("udp name: " + name + "  deflate: " + deflate.ToString());
			//System.Diagnostics.Debug.WriteLine("udp:");
			Message msg = Message.RootMsgType(ref name);
			if(msg == null || msg.GetType() == typeof(Message))
			{
				System.Diagnostics.Debug.WriteLine("udp: unknown G2 root packet: " + name);
				return;
			}
			msg.ipep = ipep;
			msg.cf = ((byt[0] & 0x04) == 0x04);
			msg.be = ((byt[0] & 0x02) == 0x02);
			if(msg.be)
				System.Diagnostics.Debug.WriteLine("g2 udp: some host is sending BigEndian order");
			if(llen == 0)
				msg.payLen = 0;
			else
				msg.payLen = Endian.VarBytesToInt(byt, 1, llen, msg.be);
			try
			{
				msg.GetMsg(this);
				G2Data.ProcessMessage(msg);
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("G2 udp incoming ProcessMe problem: " + e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
			if(!deflate)
			{
				if(bytesLeft > 0)
					goto again;
			}
			else
			{
				//since we can't tell the end of the stream until we read... we'll try a read
				goto again;
			}
		}

		public bool GetData(byte[] byt, int loc, int len)
		{
			if(iis != null)
			{
				try
				{
					int bytesRec = iis.Read(byt, loc, len);
					if(bytesRec == 0)
					{
						return false;
					}
					else if(bytesRec == -5)
					{
						System.Diagnostics.Debug.WriteLine("g2 udp: deflate stream ended");
						return false;
					}
				}
				catch
				{
					if(!Stats.Updated.closing)
						System.Diagnostics.Debug.WriteLine("g2 udp: deflate stream corrupt");
					return false;
				}
			}
			else
			{
				Array.Copy(pcktData, startPoint, byt, loc, len);
				startPoint += len;
			}
			bytesLeft -= len;
			return true;
		}
	}

	public class OutgoingKey
	{
		public static ushort nextSeq = 1500;
		public static OutgoingKeyComparer okc = new OutgoingKeyComparer();

		public System.Net.EndPoint ep;
		public int seq;

		public OutgoingKey(ushort seq)
		{
			this.seq = seq;
		}

		public OutgoingKey(System.Net.EndPoint ep)
		{
			this.ep = ep;
			nextSeq += 1;
			this.seq = nextSeq;
		}
	}

	public class OutgoingKeyComparer : IComparer
	{
		public int Compare(object ok1, object ok2)
		{
			return (((OutgoingKey)ok1).seq - ((OutgoingKey)ok2).seq);
		}
	}

	public class OutgoingPacket
	{
		public int secPassed = 0;
		public bool cancel = false;
		public bool deflate;
		public BitArray parts;
		public BitArray acks;
		public int len;
		public byte[] pcktData;
	}

	/// <summary>
	/// G2 UDP send and receive.
	/// </summary>
	public class UDPSR
	{
		//sortedlist of all incoming udp packets
		public static SortedList slIn = new SortedList(IncomingKey.ikc, 50);
		//sortedlist of all outgoing udp packets
		public static SortedList slOut = new SortedList(OutgoingKey.okc, 50);
		//timer regarding timeouts
		static GoodTimer tmrTimeOuts = new GoodTimer(5000);
		//timer controlling sending
		static GoodTimer tmrSend = new GoodTimer(200);

		static void tmrTimeOuts_Tick(object sender, ElapsedEventArgs e)
		{
			lock(slIn)
			{
#if DEBUG
				if(slIn.Count > 2000)
					System.Diagnostics.Debug.WriteLine("g2 udp slIn overflow");
#endif

				//find and remove old packets
				for(int x = slIn.Count-1; x >= 0; x--)
				{
					IncomingPacket ipckt = (IncomingPacket)slIn.GetByIndex(x);
					ipckt.secPassed += 5;
					if(ipckt.secPassed > 25)
						slIn.RemoveAt(x);
				}
			}

			lock(slOut)
			{
#if DEBUG
				if(slOut.Count > 2000)
					System.Diagnostics.Debug.WriteLine("g2 udp slOut overflow");
#endif

				//find and remove old packets
				for(int x = slOut.Count-1; x >= 0; x--)
				{
					OutgoingPacket opckt = (OutgoingPacket)slOut.GetByIndex(x);
					opckt.secPassed += 5;
					if(opckt.secPassed > 25)
						slOut.RemoveAt(x);
				}
			}
		}

		static void tmrSend_Tick(object sender, ElapsedEventArgs e)
		{
			lock(slOut)
			{
				if(slOut.Count == 0)
					return;
				int randPckt = GUID.rand.Next(0, slOut.Count);
				OutgoingKey ok = (OutgoingKey)slOut.GetKey(randPckt);
				OutgoingPacket opckt = (OutgoingPacket)slOut.GetByIndex(randPckt);

				//if we haven't received acks, then we never "really" sent the fragments
				if(opckt.acks != null)
					if(opckt.secPassed == 10 || opckt.secPassed == 21)
					{
						opckt.secPassed += 1;
						for(int x = 0; x < opckt.acks.Count; x++)
							if(!opckt.acks[x])
								opckt.parts[x] = false;
					}

				//send an unsent fragment
				for(int x = 0; x < opckt.parts.Count; x++)
					if(!opckt.parts[x])
					{
						SendFragment(ok, opckt, x+1);
						break;
					}
			}
		}

		static void SendFragment(OutgoingKey ok, OutgoingPacket opckt, int nPart)
		{
			if(nPart == 1)
			{
				//header
				opckt.pcktData[0] = (byte)'G';
				opckt.pcktData[1] = (byte)'N';
				opckt.pcktData[2] = (byte)'D';
				//acknowledgements?
				if(opckt.acks != null)
					opckt.pcktData[3] |= 0x02;
				//deflate?
				if(opckt.deflate)
					opckt.pcktData[3] |= 0x01;
				Array.Copy(BitConverter.GetBytes((ushort)ok.seq), 0, opckt.pcktData, 4, 2);
				opckt.pcktData[7] = (byte)opckt.parts.Count;
			}
			opckt.pcktData[6] = (byte)nPart;

			//send targeted area
			if(opckt.len <= 492)
			{
				Listener.UdpSend(ok.ep, opckt.pcktData, 0, 8+opckt.len);
			}
			else
			{
				int elSize = 492;
				if(nPart == opckt.parts.Count)
					elSize = opckt.len;
				byte[] buf = new byte[8+elSize];
				Array.Copy(opckt.pcktData, 0, buf, 0, 8);
				Array.Copy(opckt.pcktData, (8+((nPart-1)*492)), buf, 8, elSize);
				Listener.UdpSend(ok.ep, buf, 0, buf.Length);
			}
			opckt.parts[nPart-1] = true;

			//check if not finished
			if(opckt.acks == null)
				if(nPart == opckt.parts.Count)
					IsOutgoingPacketDone(ok);
		}

		/// <summary>
		/// A certain part was confirmed (acked), check if the packet can be flushed from cache now.
		/// </summary>
		static void IsOutgoingPacketDone(ushort seqNum, int nPart)
		{
			OutgoingKey ok = new OutgoingKey(seqNum);
			lock(slOut)
			{
				OutgoingPacket opckt = (OutgoingPacket)slOut[ok];
				if(opckt == null)
					return;
				opckt.acks[nPart-1] = true;
				opckt.parts[nPart-1] = true;
				opckt.secPassed = 0;
				bool ready = true;
				for(int x = 0; x < opckt.acks.Count; x++)
					if(!opckt.acks[x])
					{
						ready = false;
						break;
					}
				if(ready)
					slOut.Remove(ok);
			}
		}

		/// <summary>
		/// An outgoing packet finished sending and no acks are necessary; just flush the packet now.
		/// </summary>
		static void IsOutgoingPacketDone(OutgoingKey ok)
		{
			lock(slOut)
				slOut.Remove(ok);
		}

		public static int GetPartCount(int len)
		{
			int nCount = (len / 492) + 1;
			if(len % 492 == 0)
				nCount--;
			return nCount;
		}

		public static void SendOutgoingPacket(IUdpMessage omsg, System.Net.EndPoint ep)
		{
			//System.Diagnostics.Debug.WriteLine("g2 udp send: " + omsg.GetType().ToString());
			try
			{
				if(!tmrTimeOuts.hasEvent)
				{
					tmrTimeOuts.AddEvent(new ElapsedEventHandler(tmrTimeOuts_Tick));
					tmrTimeOuts.Start();
				}
				if(!tmrSend.hasEvent)
				{
					tmrSend.AddEvent(new ElapsedEventHandler(tmrSend_Tick));
					tmrSend.Start();
				}
				OutgoingPacket opckt = new OutgoingPacket();
				omsg.SetupUdpPacket(opckt);
				if(opckt.cancel)
					return;
				//super high tech hop flow
				if(slOut.Count > 18000 && omsg.GetType() == typeof(OQA))
				{
					System.Diagnostics.Debug.WriteLine("UDPSR hop flow in effect");
					return;
				}
				lock(slOut)
					slOut.Add(new OutgoingKey(ep), opckt);
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("g2 UDPSR SendOutgoingPacket problem");
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
		}

		public static void IncomingFragment(IPEndPoint ipep, byte[] byt, int bytesRec)
		{
			if(!FileScope.Gnutella2.StartStop.enabled)
				return;
			if(!tmrTimeOuts.hasEvent)
			{
				tmrTimeOuts.AddEvent(new ElapsedEventHandler(tmrTimeOuts_Tick));
				tmrTimeOuts.Start();
			}
			//System.Diagnostics.Debug.WriteLine("G2 udp recv: " + ipep.Address.ToString());
			byte nCount = byt[7];
			if(nCount == 0x00)
				IsOutgoingPacketDone(BitConverter.ToUInt16(byt, 4), byt[6]);
			else
			{
				//check critical flags that we don't allow
				if((byt[3] & 0x0C) != 0x00)
				{
					System.Diagnostics.Debug.WriteLine("G2 udp unknown flags");
					return;
				}
				IncomingKey ik = new IncomingKey(ipep.Address.Address, BitConverter.ToInt16(byt, 4));
				IncomingPacket ipckt;
				bool ready = true;
				bool wantsAck = ((byt[3] & 0x02) == 0x02);
				lock(slIn)
				{
					ipckt = slIn[ik] as IncomingPacket;
					int nPart = byt[6];
					if(ipckt == null)
					{
						//we can't start with the last fragment unless it's the only one
						if(nCount > 1 && nPart == nCount)
							return;
						SendAck(byt, ipep);
						ipckt = new IncomingPacket();
						ipckt.deflate = ((byt[3] & 0x01) == 0x01);
						ipckt.parts = new BitArray(nCount, false);
						ipckt.parts[nPart-1] = true;
						if(nCount == 1)
						{
							ipckt.pcktData = byt;
							ipckt.startPoint = 8;
							ipckt.bytesLeft = bytesRec-8;
						}
						else
						{
							ipckt.pcktData = new byte[nCount*(bytesRec-8)];
							Array.Copy(byt, 8, ipckt.pcktData, (nPart-1)*(ipckt.pcktData.Length/ipckt.parts.Count), bytesRec-8);
							ipckt.startPoint = 0;
							//a second number will be added on when we see how big the last fragment is
							ipckt.bytesLeft = (nCount-1)*(bytesRec-8);
						}
						ipckt.secPassed = 0;
						slIn.Add(ik, ipckt);
					}
					else
					{
						SendAck(byt, ipep);
						//if parts is null, it means the packet was marked as done (see below)
						if(ipckt.parts == null)
							return;
						if(ipckt.parts.Count != (int)nCount)
							return;
						if(ipckt.parts[nPart-1])
							return;
						ipckt.parts[nPart-1] = true;
						ipckt.secPassed = 0;
						if(ipckt.pcktData.Length % ipckt.parts.Count != 0)
							System.Diagnostics.Debug.WriteLine("g2 udp IncomingFragment division problem");
						Array.Copy(byt, 8, ipckt.pcktData, (nPart-1)*(ipckt.pcktData.Length/ipckt.parts.Count), bytesRec-8);
						if(nPart == nCount)
							ipckt.bytesLeft += (bytesRec-8);
						else if((ipckt.pcktData.Length/ipckt.parts.Count) != (bytesRec-8))
						{
							System.Diagnostics.Debug.WriteLine("some idiot is sending mixed fragment sizes on G2 via UDP");
							ipckt.parts[nPart-1] = false;
						}
					}
					for(int x = 0; x < ipckt.parts.Count; x++)
						if(!ipckt.parts[x])
						{
							ready = false;
							break;
						}
					/*
					 * if we're not sending back acknowledgements, there won't be any retransmissions
					 * if we are sending back acks, we'll keep the packet in the cache, but mark it as done by setting parts to null
					 * the timer will remove if anyways, after the timeout passes by
					 */
					if(ready && !wantsAck)
						slIn.Remove(ik);
					else if(ready && wantsAck)
						ipckt.parts = null;
				}
				if(ready)
					ipckt.ProcessMe(ipep);
			}
		}

		static void SendAck(byte[] byt, IPEndPoint ipep)
		{
			if((byt[3] & 0x02) == 0x02)
			{
				//send acknowledgement; reuse this byte[] for efficiency
				byt[7] = 0x00;
				byt[3] = 0x00;
				Listener.UdpSend(ipep, byt, 0, 8);
			}
		}
	}
}
