// Messages.cs
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
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace FileScope.Gnutella2
{
	/// <summary>
	/// Holds shared member variables used in Message and Child classes.
	/// Also contains the Read function which sets up children and finds the starting point of the payload.
	/// </summary>
	public abstract class G2Packet
	{
		public bool cf;						//compound flag; signifies child packets
		public int payLen;					//length of the payload of this packet
		public Child children = null;		//linked list of children that are mine
		public Child nextChild = null;		//linked list of children that are my parent's
		public Message root = null;			//store a reference to the root packet

		/// <summary>
		/// Iterate through all children and determine the beginning location of this packet's payload.
		/// </summary>
		public void Read(int buffIndex)
		{
			int start = buffIndex;
			//if we have any child packets
			if(this.cf)
			{
				Child curChild = null;
				while((start+this.payLen) - buffIndex > 0)
				{
					//if there is payload following children, a null byte terminates the set of children
					if(root.inPayload[buffIndex] == 0x00)
					{
						buffIndex++;
						break;
					}

					//control byte
					int llen = (int)root.inPayload[buffIndex] >> 6;
					int nlen = ((int)(root.inPayload[buffIndex] & 0x38) >> 3) + 1;

					buffIndex += 1+llen;

					//name
					string name = Encoding.ASCII.GetString(root.inPayload, buffIndex, nlen);
					if(curChild == null)
					{
						//start the linked list
						children = this.ChildType(ref name);
						curChild = children;
					}
					else
					{
						//continue the linked list
						curChild.nextChild = this.ChildType(ref name);
						curChild = curChild.nextChild;
					}

#if DEBUG
					if(curChild.GetType() == typeof(UnknownChild))
					{
//						if(name != "G1" && name != "NOG1" && name != "BH" && name != "UNSTA" && name != "SS")
//							System.Diagnostics.Debug.WriteLine("unknown G2 child: " + this.GetType().ToString() + "/" + name);
					}
#endif

					//ref to root
					curChild.root = root;

					buffIndex += nlen;
					curChild.offset = buffIndex;

					//length
					if(llen == 0)
						curChild.payLen = 0;
					else
						curChild.payLen = Endian.VarBytesToInt(root.inPayload, buffIndex-(llen+nlen), llen, root.be);

					//more of control byte
					curChild.cf = ((root.inPayload[buffIndex-(llen+nlen+1)] & 0x04) == 0x04);

					//shareaza bug
					if(curChild.payLen == 10 && curChild.cf && name == "CH")
						curChild.cf = false;

					buffIndex += curChild.payLen;
				}
			}

			//process the payload
			Process(buffIndex, (start+this.payLen) - buffIndex);
		}

		public abstract void Process(int offset, int len);

		public virtual Child ChildType(ref string name)
		{
			return new UnknownChild(ref name);
		}
	}

	/// <summary>
	/// Message is for encapsulating root packets in the Gnutella2 format.
	/// It is also the base class of many subclasses representing the different root packet types.
	/// </summary>
	public abstract class Message : G2Packet
	{
		public int sockNum;					//store where this message came from
		public IPEndPoint ipep = null;		//store where this message came from (if not over tcp)
		public bool be;						//big endian?
		public byte[] inPayload = null;		//payload of the entire root packet (including children / sub-children)

		/// <summary>
		/// Incoming Message.
		/// </summary>
		public void GetMsg(Sck sck)
		{
			this.root = this;
			this.sockNum = sck.sockNum;
			int bytesRec;
			//fill payload
			if(this.payLen > 0)
			{
				this.inPayload = new byte[this.payLen];
				bytesRec = sck.ReceiveData(this.inPayload, 0, this.payLen);
				if(bytesRec == 0)
				{
					sck.Disconnect("nethin' received (G2) GetMsg");
					return;
				}
			}
		}

		/// <summary>
		/// Incoming UDP Message.
		/// </summary>
		public void GetMsg(IncomingPacket udpip)
		{
			this.root = this;
			this.sockNum = -1;
			//fill payload
			if(this.payLen > 0)
			{
				this.inPayload = new byte[this.payLen];
				udpip.GetData(this.inPayload, 0, this.payLen);
			}
		}

		/// <summary>
		/// This function returns an instance of a subclass of Message.
		/// </summary>
		public static Message RootMsgType(ref string name)
		{
//System.Diagnostics.Debug.WriteLine("in: " + name);
			//ordered generally from most popular to least
			switch(name)
			{
				case "Q2":
					return new Query();
				case "QH2":
					return new QueryHit();
				case "QKR":
					return new QueryKeyRequest();
				case "QKA":
					return new QueryKeyAnswer();
				case "QA":
					return new QueryAck();
				case "PUSH":
					return new Push();
				case "PI":
					return new Ping();
				case "PO":
					return new Pong();
				case "LNI":
					return new LocalNodeInfo();
				case "KHL":
					return new KnownHubList();
				case "QHT":
					return new QueryHashTable();
				case "UPROC":
					return new UserProfileRequest();
				case "UPROD":
					return new UserProfileAnswer();
				default:
					return null;
			}
		}
	}

	/// <summary>
	/// Used to encapsulate child packets.
	/// This is also the base class for several subclasses representing G2 child packets.
	/// </summary>
	public abstract class Child : G2Packet
	{
		public int offset;		//offset of child payload in original root payload
	}




	/*   various subclasses of Message and Child   */




	public class HandshakeMsg : Message
	{
		public string handshake = "";

		public new void GetMsg(Sck sck)
		{
			int bytesRec;
			while(true)
			{
				if(sck.state == Condition.hndshk2 || sck.browseHost)
				{
					/*
					 * if we're receiving the last handshake from an incoming connection, then
					 * things are a little different because actual packets may follow right after
					 */
					if(!sck.incoming && !sck.browseHost)
						System.Diagnostics.Debug.WriteLine("G2 incoming final handshake problem");
					//we'll get straight up to the \r\n\r\n part, and stop right there
					int recLen = 4;
					byte[] hndshk = new byte[4];
					int endHndShk;
					while(true)
					{
						bytesRec = sck.ReceiveData(hndshk, 0, recLen);
						if(bytesRec == 0)
						{
							sck.Disconnect("nethin' received during handshake (G2)");
							return;
						}
						handshake += Encoding.ASCII.GetString(hndshk, 0, bytesRec);
						endHndShk = handshake.LastIndexOf("\r\n\r\n");
						if(endHndShk != -1)
							return;
						if(handshake[handshake.Length-1] == '\n')
							recLen = 2;
						else if(handshake[handshake.Length-1] == '\r')
							recLen = 1;
						else
							recLen = 4;
					}
				}
				else
				{
					//usual handshake receive
					byte[] hndshk = new byte[32768];
					bytesRec = sck.ReceiveData(hndshk);
					if(bytesRec == 0)
					{
						sck.Disconnect("nethin' received during handshake (G2)");
						return;
					}
					handshake += Encoding.ASCII.GetString(hndshk, 0, bytesRec);
					int endHndShk = handshake.LastIndexOf("\r\n\r\n");
					if(endHndShk != -1)
						break;
					else
						System.Diagnostics.Debug.WriteLine("G2 Handshake GetMsg didn't finish");
				}
			}
		}

		public override void Process(int offset, int len)
		{
			//
		}
	}

	public class UnknownChild : Child
	{
		public string name;

		public UnknownChild(ref string name)
		{
			this.name = name;
		}

		public override void Process(int offset, int len)
		{
			//
		}
	}

	public class Ping : Message
	{
		public override void Process(int offset, int len)
		{
			if(this.sockNum != -1)
			{
				UDP udp = null;
				bool relay = false;
				Child child = this.children;
				while(child != null)
				{
					if(child.GetType() == typeof(UDP))
						udp = (UDP)child;
					else if(child.GetType() == typeof(Relay))
						relay = true;
					child = child.nextChild;
				}
				if(!relay)
				{
					//add a relay child and forward to all ultrapeers if udp present
					if(udp != null)
					{
						OUdpPingRelay oupr = new OUdpPingRelay();
						Array.Copy(root.inPayload, udp.offset, oupr.endpoint, 0, 6);
						//in case there's an endian-ness difference between this host and us
						if(root.be != !Stats.Updated.le)
						{
							//reverse the byte order of the 16-bit port
							Array.Reverse(oupr.endpoint, 4, 2);
						}
						lock(HostCache.recentHubs)
						{
							foreach(ConnectionManager.G2Host g2h in ConnectionManager.ultrapeers.Keys)
								Sck.scks[g2h.sockNum].SendPacket(oupr);
						}
					}
					else
						Sck.scks[this.sockNum].SendPacket(new OPong());
				}
				else if(udp != null)
				{
					//we'll send a pong over udp with the relay child to the original host
					udp.Read(udp.offset);
					if(udp.ipep != null)
						UDPSR.SendOutgoingPacket(new OPongRelay(), udp.ipep);
				}
			}
			else
			{
				//ignoring any children, we'll send a plain pong back over udp
				UDPSR.SendOutgoingPacket(new OPong(), this.ipep);
			}
		}

		public override Child ChildType(ref string name)
		{
			if(name == "RELAY")
				return new Relay();
			else if(name == "UDP")
				return new UDP();
			else
				return new UnknownChild(ref name);
		}

		public class Relay : Child
		{
			public override void Process(int offset, int len)
			{
				//
			}
		}

		public class UDP : Child
		{
			public IPEndPoint ipep = null;

			public override void Process(int offset, int len)
			{
				if(len == 6)
					this.ipep = new IPEndPoint(Endian.GetIPAddress(root.inPayload, offset), Endian.ToUInt16(root.inPayload, offset+4, root.be));
				else
					System.Diagnostics.Debug.WriteLine("g2 ping udp len: " + len.ToString());
			}
		}
	}

	public class Pong : Message
	{
		public override void Process(int offset, int len)
		{
			//
		}

		public override Child ChildType(ref string name)
		{
			if(name == "RELAY")
				return new Relay();
			else
				return new UnknownChild(ref name);
		}

		public class Relay : Child
		{
			public override void Process(int offset, int len)
			{
				//
			}
		}
	}

	public class LocalNodeInfo : Message
	{
		public override void Process(int offset, int len)
		{
			if(root.sockNum == -1)
				return;
			Child child = this.children;
			while(child != null)
			{
				child.Read(child.offset);
				child = child.nextChild;
			}
		}

		public override Child ChildType(ref string name)
		{
			if(name == "NA")
				return new NodeAddress();
			else if(name == "GU")
				return new GuIdent();
			else if(name == "V")
				return new VendorCode();
			else if(name == "LS")
				return new LibraryStats();
			else if(name == "HS")
				return new HubStatus();
			else
				return new UnknownChild(ref name);
		}

		public class NodeAddress : Child
		{
			public override void Process(int offset, int len)
			{
				//port is important
				if(Sck.scks[root.sockNum].incoming && len == 6)
					Sck.scks[root.sockNum].port = Endian.ToUInt16(root.inPayload, offset+4, root.be);
			}
		}

		public class GuIdent : Child
		{
			public override void Process(int offset, int len)
			{
				if(!Stats.Updated.Gnutella2.ultrapeer)
					return;
				if(len == 16)
				{
					if(Sck.scks[root.sockNum].gitem == null)
					{
						GUIDitem gitem = new GUIDitem(root.inPayload, offset);
						lock(Router.htRoutes)
						{
							Sck.scks[root.sockNum].gitem = gitem;
							Router.RouteEntry re = new Router.RouteEntry();
							re.gitem_key = gitem;
							re.ipep = null;
							re.sckNum = root.sockNum;
							re.timeLeft = -9000;
							Router.htRoutes.Add(gitem, re);
						}
					}
				}
				else
					System.Diagnostics.Debug.WriteLine("G2 LNI GuIdent wrong length: " + len.ToString());
			}
		}

		public class VendorCode : Child
		{
			public override void Process(int offset, int len)
			{
				//
			}
		}

		public class LibraryStats : Child
		{
			public override void Process(int offset, int len)
			{
				Sck.scks[root.sockNum].numFiles = Endian.ToUInt32(root.inPayload, offset, root.be);
				Sck.scks[root.sockNum].numKB = Endian.ToUInt32(root.inPayload, offset+4, root.be);
			}
		}

		public class HubStatus : Child
		{
			public override void Process(int offset, int len)
			{
				Sck.scks[root.sockNum].leaves = Endian.ToUInt16(root.inPayload, offset, root.be);
				Sck.scks[root.sockNum].maxleaves = Endian.ToUInt16(root.inPayload, offset+2, root.be);
			}
		}
	}

	public class KnownHubList : Message
	{
		public override void Process(int offset, int len)
		{
			//since we're expecting neighboring hubs info, we'll clear the old info
			lock(HostCache.recentHubs)
				Sck.scks[root.sockNum].neighbors.Clear();
			int timeStamp = 0;
			Child child = this.children;
			while(child != null)
			{
				child.Read(child.offset);
				if(child.GetType() == typeof(CachedHub))
				{
					CachedHub ch = (CachedHub)child;
					if(ch.ipa != null)
					{
						HubInfo hi = new HubInfo();
						hi.port = ch.port;
						hi.timeKnown = Math.Abs(timeStamp - ch.timeStamp);
						HostCache.AddRecentAndCache(ch.ipa, hi);
					}
				}
				else if(child.GetType() == typeof(Timestamp))
					timeStamp = ((Timestamp)child).timeStamp;
				child = child.nextChild;
			}
		}

		public override Child ChildType(ref string name)
		{
			if(name == "NH")
				return new NeighborHub();
			else if(name == "CH")
				return new CachedHub();
			else if(name == "TS")
				return new Timestamp();
			else
				return new UnknownChild(ref name);
		}

		public class Timestamp : Child
		{
			public int timeStamp;

			public override void Process(int offset, int len)
			{
				this.timeStamp = Endian.ToInt32(root.inPayload, offset, root.be);
			}
		}

		public class NeighborHub : Child
		{
			public override void Process(int offset, int len)
			{
				if(len != 6)
				{
					Utils.Diag("G2 KHL/NH wrong length: " + len.ToString());
					return;
				}
				lock(HostCache.recentHubs)
					Sck.scks[root.sockNum].neighbors.Add(new IPEndPoint(Endian.GetIPAddress(root.inPayload, offset), Endian.ToUInt16(root.inPayload, offset+4, root.be)));
			}

			public override Child ChildType(ref string name)
			{
				if(name == "GU")
					return new GuIdent();
				else if(name == "V")
					return new VendorCode();
				else if(name == "LS")
					return new LibraryStats();
				else if(name == "HS")
					return new HubStatus();
				else
					return new UnknownChild(ref name);
			}

			public class GuIdent : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class VendorCode : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class LibraryStats : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class HubStatus : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}
		}

		public class CachedHub : Child
		{
			public IPAddress ipa;
			public ushort port;
			public int timeStamp;

			public override void Process(int offset, int len)
			{
				if(len == 10)
				{
					this.ipa = Endian.GetIPAddress(root.inPayload, offset);
					this.port = Endian.ToUInt16(root.inPayload, offset+4, root.be);
					this.timeStamp = Endian.ToInt32(root.inPayload, offset+len-4, root.be);
				}
				else
					Utils.Diag("G2 KHL/CH wrong length: " + len.ToString());
			}

			public override Child ChildType(ref string name)
			{
				if(name == "GU")
					return new GuIdent();
				else if(name == "V")
					return new VendorCode();
				else if(name == "LS")
					return new LibraryStats();
				else if(name == "HS")
					return new HubStatus();
				else
					return new UnknownChild(ref name);
			}

			public class GuIdent : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class VendorCode : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class LibraryStats : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class HubStatus : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}
		}
	}

	public class Push : Message
	{
		public int offsetGUID = -1;

		public override void Process(int offset, int len)
		{
			Child child = this.children;
			while(child != null)
			{
				child.Read(child.offset);
				child = child.nextChild;
			}
			if(offsetGUID != -1)
			{
				if(GUID.Compare(root.inPayload, offsetGUID, Stats.settings.myGUID, 0))
				{
					//this push was for us
					goto push_for_us;
				}
				else if(Stats.Updated.Gnutella2.ultrapeer)
				{
					//maybe we can route this push
					GUIDitem gitem = new GUIDitem(root.inPayload, offsetGUID);
					Router.RouteEntry re;
					lock(Router.htRoutes)
					{
						if(Router.htRoutes.ContainsKey(gitem))
							re = (Router.RouteEntry)Router.htRoutes[gitem];
#if	DEBUG
						else
						{
							Utils.Diag("G2 Push no route found");
							return;
						}
#else
						else
							return;
#endif
					}
					OCopy ocopy = new OCopy();
					ocopy.payload = root.inPayload;
					ocopy.CreateHeader("PUSH", root.be, root.cf);
					Router.RoutePacket(ocopy, re);
				}
			}
			if(this.children == null)
				goto push_for_us;

			return;

		push_for_us:
			if(len == 6)
			{
				IPAddress ipa = Endian.GetIPAddress(root.inPayload, offset);
				int port = Endian.ToUInt16(root.inPayload, offset+4, root.be);
				//the -5 indicates it's a G2, not G1 upload
				UploadManager.Outgoing(-5, ipa.ToString(), port);
			}
		}

		public override Child ChildType(ref string name)
		{
			if(name == "TO")
				return new To();
			else
				return new UnknownChild(ref name);
		}

		public class To : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 16)
					((Push)this.root).offsetGUID = offset;
				else
					System.Diagnostics.Debug.WriteLine("g2 Push guid is of wrong length: " + len.ToString());
			}
		}
	}

	public class QueryHashTable : Message
	{
		public override void Process(int offset, int len)
		{
			if(root.inPayload[offset] == 0x00)
			{
				lock(Sck.scks[root.sockNum])
				{
					if(len != 6)
						System.Diagnostics.Debug.WriteLine("g2 qht wrong size: " + len.ToString());
					Sck.scks[root.sockNum].inQHT = new BitArray(Endian.ToInt32(root.inPayload, offset+1, root.be), true);
				}
			}
			else if(root.inPayload[offset] == 0x01)
			{
				lock(Sck.scks[root.sockNum])
				{
					//first fragment?
					if(root.inPayload[offset+1] == 0x01)
					{
						Sck.scks[root.sockNum].inRawQHTindex = 0;
						Sck.scks[root.sockNum].inRawQHT = new byte[(len-5) * (int)root.inPayload[offset+2]];
					}
					Array.Copy(root.inPayload, offset+5, Sck.scks[root.sockNum].inRawQHT, Sck.scks[root.sockNum].inRawQHTindex, len-5);
					Sck.scks[root.sockNum].inRawQHTindex += (len-5);
					//last fragment?
					if(root.inPayload[offset+1] == root.inPayload[offset+2])
					{
						BitArray baPatch;
						bool compressed = (root.inPayload[offset+3] == 0x01);
						if(!compressed)
							baPatch = new BitArray(Sck.scks[root.sockNum].inRawQHT);
						else
						{
							byte[] unCompressed = new byte[Sck.scks[root.sockNum].inQHT.Length / 8];
							ICSharpCode.SharpZipLib.Zip.Compression.Inflater ifer = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
							ifer.SetInput(Sck.scks[root.sockNum].inRawQHT, 0, Sck.scks[root.sockNum].inRawQHTindex);
							ifer.Inflate(unCompressed);
							baPatch = new BitArray(unCompressed);
						}
						//apply the patch
						Sck.scks[root.sockNum].inQHT.Xor(baPatch);
					}
				}
			}
			else
				System.Diagnostics.Debug.WriteLine("q2 qht wrong command: " + root.inPayload[offset].ToString());
		}
	}

	public class QueryKeyRequest : Message
	{
		public override void Process(int offset, int len)
		{
			if(root.sockNum != -1)
			{
				System.Diagnostics.Debug.WriteLine("g2 qkr over tcp?");
				return;
			}
			if(!Stats.Updated.Gnutella2.ultrapeer)
				return;
			Child child = this.children;
			while(child != null)
			{
				child.Read(child.offset);
				child = child.nextChild;
			}
		}

		public override Child ChildType(ref string name)
		{
			if(name == "RNA")
				return new RNodeAddress();
			else
				return new UnknownChild(ref name);
		}

		public class RNodeAddress : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 6)
				{
					IPEndPoint ipep = new IPEndPoint(Endian.GetIPAddress(root.inPayload, offset), Endian.ToUInt16(root.inPayload, offset+4, root.be));
					OQKA oqka = new OQKA();
					oqka.queryKey = GUID.rand.Next();
					oqka.qk = true;
					oqka.sna = ipep.Address;
					HostCache.SentQueryKey(oqka.queryKey, ipep);
					UDPSR.SendOutgoingPacket(oqka, ipep);
				}
				else
					System.Diagnostics.Debug.WriteLine("RNodeAddress invalid len: " + len.ToString());
			}
		}
	}

	public class QueryKeyAnswer : Message
	{
		public int queryKey = -1;
		public bool qk = false;
		public IPAddress sna = null;
		public IPAddress qna = null;

		public override void Process(int offset, int len)
		{
			Child child = this.children;
			while(child != null)
			{
				child.Read(child.offset);
				child = child.nextChild;
			}
			try
			{
				if(!qk)
				{
#if DEBUG
					System.Diagnostics.Debug.WriteLine("g2 no query key received... perhaps our query key is no longer valid");
#endif
					HubInfo hi;
					lock(HostCache.hubCache)
					{
						if(qna == null)
							hi = (HubInfo)HostCache.hubCache[root.ipep.Address];
						else
							hi = (HubInfo)HostCache.hubCache[qna];
					}
					hi.timeKnown = 0;
					hi.mhi.qk = false;
					hi.mhi.queryKeyedHub = null;
				}
				else
				{
					//System.Diagnostics.Debug.WriteLine("query key present");
					HubInfo hi;
					lock(HostCache.hubCache)
					{
						if(qna == null)
						{
							hi = (HubInfo)HostCache.hubCache[root.ipep.Address];
							hi.mhi.queryKeyedHub = null;
						}
						else
						{
							hi = (HubInfo)HostCache.hubCache[qna];
							hi.mhi.queryKeyedHub = Sck.scks[root.sockNum].remoteIPA;
						}
					}
					hi.timeKnown = 0;
					hi.mhi.queryKey = this.queryKey;
					hi.mhi.qk = true;
					//we need to use this hub for either a normal query or a requery from a download
					int chance;
					int val = 1;
					if(ReQuery.g2requeried)
					{
						val = 2;
						ReQuery.g2requeried = false;
					}
					chance = GUID.rand.Next(0, val);
					if(chance == 1 || (Search.activeSearches.Count == 0 && val == 2))
					{
						if(qna == null)
							ReQuery.Gnutella2NewHub(root.ipep.Address, hi);
						else
							ReQuery.Gnutella2NewHub(qna, hi);
					}
					else if(chance == 0)
					{
						lock(Search.activeSearches)
						{
							if(Search.activeSearches.Count > 0)
							{
								int which = GUID.rand.Next(0, Search.activeSearches.Count);
								if(qna == null)
									((Search)Search.activeSearches[which]).NewHub(root.ipep.Address, hi);
								else
									((Search)Search.activeSearches[which]).NewHub(qna, hi);
							}
						}
					}
				}
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("g2 QKA: " + e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
			try
			{
				//routing?
				if(Stats.Updated.Gnutella2.ultrapeer && !this.sna.Equals(Stats.Updated.myIPA) && this.qna == null)
				{
					lock(HostCache.recentHubs)
					{
						if(ConnectionManager.htDirects.ContainsKey(this.sna))
						{
							int sockNum = ((ConnectionManager.G2Host)ConnectionManager.htDirects[this.sna]).sockNum;
							OQKA oqka = new OQKA();
							oqka.qna = root.ipep.Address;
							oqka.sna = this.sna;
							oqka.queryKey = this.queryKey;
							oqka.qk = this.qk;
							Sck.scks[sockNum].SendPacket(oqka);
						}
					}
				}
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("g2 QKA2: " + e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
		}

		public override Child ChildType(ref string name)
		{
			if(name == "QK")
				return new QueryKey();
			else if(name == "SNA")
				return new SNodeAddress();
			else if(name == "QNA")
				return new QNodeAddress();
			else
				return new UnknownChild(ref name);
		}

		public class QueryKey : Child
		{
			public override void Process(int offset, int len)
			{
				if(len >= 4)
				{
					((QueryKeyAnswer)this.root).queryKey = Endian.ToInt32(root.inPayload, offset, root.be);
					((QueryKeyAnswer)this.root).qk = true;
				}
			}
		}

		public class SNodeAddress : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 4 || len == 6)
					((QueryKeyAnswer)this.root).sna = Endian.GetIPAddress(root.inPayload, offset);
				else
					Utils.Diag("QKA SNA wrong size");
			}
		}

		public class QNodeAddress : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 4 || len == 6)
					((QueryKeyAnswer)this.root).qna = Endian.GetIPAddress(root.inPayload, offset);
				else
					Utils.Diag("QKA QNA wrong size");
			}
		}
	}

	public class Query : Message
	{
		//descriptive name
		public string dn = "";
		//udp or tcp
		public bool fromUdp = false;
		//return address and query key
		public IPEndPoint ipepReturn = null;
		public int queryKey;
		//urn linked list
		public URNinfo urns = null;
		public class URNinfo
		{
			/// <summary>
			/// range in root.inPayload... not this.fullurn
			/// </summary>
			public int loc, len;
			//full urn in string form (ex. urn:sha1:WIXYJFVJMIWNMUWPRPBGUTODIV52RMJA)
			public string fullurn;
			//next URNinfo object
			public URNinfo next = null;
		}
		//size restrictions:
		public bool sizeRestrict = false;
		public uint minSize;
		public uint maxSize;
		//interests; null means everything
		public string[] interests = null;

		public override void Process(int offset, int len)
		{
			if(len != 16)
			{
				System.Diagnostics.Debug.WriteLine("bad g2 query payload length: " + len.ToString());
				throw new Exception("bad g2 query payload length");
			}
			if(root.ipep != null)
				this.fromUdp = true;

			//whether or not we're an ultrapeer, it's necessary to make sure this isn't a duplicate query
			GUIDitem gitem = new GUIDitem(root.inPayload, offset);
			Router.RouteEntry re;
			lock(Router.htRoutes)
			{
				//query duplicate?
				if(Router.htRoutes.ContainsKey(gitem))
					return;
				re = new Router.RouteEntry();
				re.gitem_key = gitem;
				if(this.fromUdp)
				{
					//this may change if a udp child is found
					re.ipep = root.ipep;
					re.sckNum = -1;
				}
				else
				{
					re.ipep = null;
					re.sckNum = root.sockNum;
				}
				re.timeLeft = 120;
				Router.htRoutes.Add(gitem, re);
			}

			Child child = this.children;
			while(child != null)
			{
				child.Read(child.offset);
				child = child.nextChild;
			}

			//honor udp packet if present
			if(this.ipepReturn != null)
			{
				re.ipep = this.ipepReturn;
				re.sckNum = -1;
			}

			//check query key
			bool sendACK = false;
			if(Stats.Updated.Gnutella2.ultrapeer && this.ipepReturn != null && this.fromUdp)
			{
				if(!HostCache.QueryKeyOK(this.ipepReturn, this.queryKey))
				{
#if DEBUG
	Utils.Diag("someone's g2 key wasn't valid");
#endif
					//we need to notify the host that the query key is invalid
					OQKA oqka = new OQKA();
					oqka.sna = this.ipep.Address;
					oqka.qk = false;
					oqka.qna = null;
					UDPSR.SendOutgoingPacket(oqka, this.ipepReturn);
					return;
				}
				else
					sendACK = true;
			}
			if(Stats.Updated.Gnutella2.ultrapeer && !this.fromUdp)
				if(Sck.scks[this.sockNum].mode == G2Mode.Leaf)
					sendACK = true;
			if(sendACK)
			{
				//send qa either by udp or tcp
				OQA oqa = new OQA();
				oqa.sGUID = root.inPayload;
				oqa.gOffset = offset;
				oqa.tcp = (this.sockNum != -1);
				if(oqa.tcp)
					Sck.scks[this.sockNum].SendPacket(oqa);
				else
				{
					if(this.ipepReturn != null)
						UDPSR.SendOutgoingPacket(oqa, this.ipepReturn);
					else
						UDPSR.SendOutgoingPacket(oqa, root.ipep);
				}
			}

#if DEBUG
//System.Diagnostics.Debug.WriteLine("dn----> " + dn);
#endif

			if(dn.Length > 0)
				GUIBridge.QueryG2(ref dn);
			G2Word g2words = null;
			//check if we might have it
			if(dn.Length > 0 || urns != null)
			{
				ArrayList matches = null;
				if(this.urns != null)
				{
					bool hashQHTmatch = false;
					URNinfo curURN = this.urns;
					lock(QueryRouteTable.ourQHT)
					{
						while(curURN != null)
						{
							if(QueryRouteTable.ourQHT[Hashing.Hash(ref curURN.fullurn, curURN.fullurn.Length, 20)] == false)
							{
								hashQHTmatch = true;
								break;
							}
							else
								curURN = curURN.next;
						}
					}
					if(!hashQHTmatch)
						goto noLocalQHTMatch;

					//if passed qht check, check files
					lock(Stats.fileList)
					{
						foreach(FileObject fi in Stats.fileList)
						{
							bool match = false;
							curURN = this.urns;
							while(curURN != null)
							{
								if(curURN.fullurn.Substring(0, 8) == "urn:sha1")
								{
									if(Utils.SameArray(root.inPayload, curURN.loc, curURN.len, fi.sha1bytes, 0, fi.sha1bytes.Length))
									{
										match = true;
										break;
									}
								}
								else if(curURN.fullurn.Substring(0, 8) == "urn:ed2k")
								{
									if(Utils.SameArray(root.inPayload, curURN.loc, curURN.len, fi.md4, 0, fi.md4.Length))
									{
										match = true;
										break;
									}
								}
								curURN = curURN.next;
							}
							if(match && sizeRestrict)
							{
								if(fi.b < minSize || fi.b > maxSize)
									match = false;
							}
							if(match && fi.sha1.Length > 0)
							{
								if(matches == null)
									matches = new ArrayList();
								matches.Add(fi);
							}
						}
					}
				}
				else
				{
					G2Word g2wFirst = Keywords.GetG2Keywords(ref dn);
					g2words = g2wFirst;
					if(g2wFirst != null)
					{
						//first check our hash table
						G2Word g2wCurr1 = g2wFirst;
						lock(QueryRouteTable.ourQHT)
						{
							while(g2wCurr1 != null)
							{
								if(!g2wCurr1.negative)
								{
									g2wCurr1.subwords = g2wCurr1.word.Split(new char[] {' ', ','}, 15);
									for(int z = 0; z < g2wCurr1.subwords.Length; z++)
										//true means empty
										if(QueryRouteTable.ourQHT[Hashing.Hash(ref g2wCurr1.subwords[z], g2wCurr1.subwords[z].Length, 20)] == true)
											goto noLocalQHTMatch;
								}
								g2wCurr1 = g2wCurr1.next;
							}
						}

						//if passed qht check, check files
						lock(Stats.fileList)
						{
							foreach(FileObject fi in Stats.fileList)
							{
								G2Word g2wCurrent = g2wFirst;
								bool match = true;
								while(g2wCurrent != null)
								{
									if(g2wCurrent.negative)
									{
										if(fi.lcaseFileName.IndexOf(g2wCurrent.word) != -1)
										{
											match = false;
											break;
										}
									}
									else
									{
										if(fi.lcaseFileName.IndexOf(g2wCurrent.word) == -1)
										{
											match = false;
											break;
										}
									}
									g2wCurrent = g2wCurrent.next;
								}
								if(match && sizeRestrict)
								{
									if(fi.b < minSize || fi.b > maxSize)
										match = false;
								}
								if(match && fi.sha1.Length > 0)
								{
									if(matches == null)
										matches = new ArrayList();
									matches.Add(fi);
								}
							}
						}
					}
				}
				if(matches != null)
				{
					//send query hit
					OQH2 oqh2 = new OQH2();
					oqh2.searchGUID = root.inPayload;
					oqh2.searchGUIDoffset = offset;
					if(this.interests == null)
						oqh2.interests.all = true;
					else
					{
						foreach(string strInterest in this.interests)
						{
							if(strInterest == "URL")
								oqh2.interests.locations = true;
							else if(strInterest == "DN")
								oqh2.interests.descriptivenames = true;
							else if(strInterest == "MD")
								oqh2.interests.metadata = true;
							else if(strInterest == "COM")
								oqh2.interests.comments = true;
							else if(strInterest == "FS")
								oqh2.interests.partiallyavailableobjects = true;
							else if(strInterest.Length > 0)
								System.Diagnostics.Debug.WriteLine("unknown g2 interest: " + strInterest);
						}
					}
					oqh2.matches = matches;
					//if(this.ipepReturn != null && (Stats.Updated.Gnutella2.ultrapeer || (Stats.Updated.everIncoming && Stats.Updated.udpIncoming)))
					//we don't care about a "reliable" delivery; we just wanna conserve bandwidth; so instead of the line above, we have the one below
					if(this.ipepReturn != null)
						UDPSR.SendOutgoingPacket(oqh2, this.ipepReturn);
					else
					{
						if(this.fromUdp)
							System.Diagnostics.Debug.WriteLine("g2 oqh2 has no where to go");
						else
							Sck.scks[root.sockNum].SendPacket(oqh2);
					}
				}
			}

		noLocalQHTMatch:

			//route/broadcast it wherever
			if(Stats.Updated.Gnutella2.ultrapeer)
			{
				//both hubs and leaves, or just leaves
				bool both = false;
				if(!fromUdp)
				{
					if(Sck.scks[root.sockNum].mode == G2Mode.Leaf)
						both = true;
				}
				else
					both = true;

				//do anything you haven't already done
				if(g2words == null && this.dn.Length > 0)
					g2words = Keywords.GetG2Keywords(ref dn);
				G2Word g2currentWord = g2words;
				while(g2currentWord != null)
				{
					if(g2currentWord.subwords == null && !g2currentWord.negative)
							g2currentWord.subwords = g2currentWord.word.Split(new char[] {' ', ','}, 15);
					g2currentWord = g2currentWord.next;
				}

				//begin
				foreach(Sck sck in Sck.scks)
					if(sck != null)
						if(sck.inQHT != null)
							if(both || sck.mode == G2Mode.Leaf)
							{
								bool matched = false;
								lock(Sck.scks[sck.sockNum])
								{
									//query filtering
									if(urns != null)
									{
										URNinfo urniCurr = urns;
										while(urniCurr != null)
										{
											if(sck.inQHT[Hashing.Hash(ref urniCurr.fullurn, urniCurr.fullurn.Length, 20)] == false)
											{
												matched = true;
												break;
											}
											else
												urniCurr = urniCurr.next;
										}
									}
									if(!matched && g2words != null)
									{
										int found = 0, missing = 0;
										g2currentWord = g2words;
										while(g2currentWord != null)
										{
											if(!g2currentWord.negative)
											{
												if(g2currentWord.subwords == null)
												{
													if(sck.inQHT[Hashing.Hash(ref g2currentWord.word, g2currentWord.word.Length, 20)] == false)
														found++;
													else
														missing++;
												}
												else
												{
													for(int y = 0; y < g2currentWord.subwords.Length; y++)
													{
														if(sck.inQHT[Hashing.Hash(ref g2currentWord.subwords[y], g2currentWord.subwords[y].Length, 20)] == false)
															found++;
														else
															missing++;
													}
												}
											}
											g2currentWord = g2currentWord.next;
										}
										if(missing == 0)
										{
											if(found > 0)
												matched = true;
											else
												matched = false;
										}
										else
										{
											float perc = (float)found / (float)(found+missing);
											if(perc > 0.66)
												matched = true;
											else
												matched = false;
										}
									}
								}
								if(matched)
								{
									OCopy ocopy = new OCopy();
									ocopy.payload = root.inPayload;
									ocopy.CreateHeader("Q2", root.be, root.cf);
									sck.SendPacket2(ocopy);
								}
							}
			}
		}

		/// <summary>
		/// Interests found in a query.
		/// If no interest packet is found, all is set to true.
		/// URN is to always be included, regardless of interests.
		/// </summary>
		public struct Interests
		{
			public bool all;
			public bool locations;
			public bool descriptivenames;
			public bool metadata;
			public bool comments;
			public bool partiallyavailableobjects;
		}

		public override Child ChildType(ref string name)
		{
			if(name == "UDP")
				return new UDP();
			else if(name == "URN")
				return new URN();
			else if(name == "DN")
				return new DescName();
			else if(name == "MD")
				return new Metadata();
			else if(name == "SZR")
				return new SizeRestriction();
			else if(name == "I")
				return new Interest();
			else
				return new UnknownChild(ref name);
		}

		public class UDP : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 10)
				{
					((Query)root).ipepReturn = new IPEndPoint(Endian.GetIPAddress(root.inPayload, offset), Endian.ToUInt16(root.inPayload, offset+4, root.be));
					((Query)root).queryKey = Endian.ToInt32(root.inPayload, offset+6, root.be);
				}
				else if(len != 6)
					System.Diagnostics.Debug.WriteLine("Q2/UDP invalid len: " + len.ToString());
			}
		}

		public class URN : Child
		{
			public override void Process(int offset, int len)
			{
				if(len > 12)
				{
					int nullLoc = Utils.FindNull(root.inPayload, offset, len);
					if(nullLoc == -1)
					{
						Utils.Diag("G2 Q2 URN nullLoc problem");
						return;
					}
					string strUrn = Utils.GetString(root.inPayload, offset, nullLoc-offset);
					if(strUrn.Length == 11 && strUrn == "tree:tiger/" && len == 36)
					{
						URNinfo urni = ReturnNextURNinfo();
						urni.fullurn = "urn:tree:tiger/:" + Base32.Encode(root.inPayload, offset+12, 24);
						urni.len = 24;
						urni.loc = offset+12;
					}
					else if(strUrn.Length == 3 && strUrn == "ttr" && len == 28)
					{
						URNinfo urni = ReturnNextURNinfo();
						urni.fullurn = "urn:tree:tiger/:" + Base32.Encode(root.inPayload, offset+4, 24);
						urni.len = 24;
						urni.loc = offset+4;
					}
					else if(strUrn.Length == 4 && strUrn == "sha1" && len == 25)
					{
						URNinfo urni = ReturnNextURNinfo();
						urni.fullurn = "urn:sha1:" + Base32.Encode(root.inPayload, offset+5, 20);
						urni.len = 20;
						urni.loc = offset+5;
					}
					else if(strUrn.Length == 8 && strUrn == "bitprint" && len == 53)
					{
						URNinfo urni = ReturnNextURNinfo();
						urni.fullurn = "urn:sha1:" + Base32.Encode(root.inPayload, offset+9, 20);
						urni.len = 20;
						urni.loc = offset+9;
						urni.next = new URNinfo();
						urni = urni.next;
						urni.fullurn = "urn:tree:tiger/:" + Base32.Encode(root.inPayload, offset+29, 24);
						urni.len = 24;
						urni.loc = offset+29;
					}
					else if(strUrn.Length == 2 && strUrn == "bp" && len == 47)
					{
						URNinfo urni = ReturnNextURNinfo();
						urni.fullurn = "urn:sha1:" + Base32.Encode(root.inPayload, offset+3, 20);
						urni.len = 20;
						urni.loc = offset+3;
						urni.next = new URNinfo();
						urni = urni.next;
						urni.fullurn = "urn:tree:tiger/:" + Base32.Encode(root.inPayload, offset+23, 24);
						urni.len = 24;
						urni.loc = offset+23;
					}
					else if(strUrn.Length == 3 && strUrn == "md5" && len == 20)
					{
						URNinfo urni = ReturnNextURNinfo();
						urni.fullurn = "urn:md5:" + Base32.Encode(root.inPayload, offset+4, 16);
						urni.len = 16;
						urni.loc = offset+4;
					}
					else if(strUrn.Length == 4 && strUrn == "ed2k" && len == 21)
					{
						URNinfo urni = ReturnNextURNinfo();
						urni.fullurn = "urn:ed2k:" + Base32.Encode(root.inPayload, offset+5, 16);
						urni.len = 16;
						urni.loc = offset+5;
					}
				}
			}

			URNinfo ReturnNextURNinfo()
			{
				if(((Query)root).urns == null)
				{
					((Query)root).urns = new URNinfo();
					return ((Query)root).urns;
				}
				else
				{
					URNinfo cur = ((Query)root).urns;
					while(cur.next != null)
						cur = cur.next;
					cur.next = new URNinfo();
					return cur.next;
				}
			}
		}

		public class DescName : Child
		{
			public override void Process(int offset, int len)
			{
				if(len > 0)
					((Query)this.root).dn = Utils.GetString(root.inPayload, offset, len);
			}
		}

		public class Metadata : Child
		{
			public override void Process(int offset, int len)
			{
				//
			}
		}

		public class SizeRestriction : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 8)
				{
					((Query)root).minSize = Endian.ToUInt32(root.inPayload, offset, root.be);
					((Query)root).maxSize = Endian.ToUInt32(root.inPayload, offset+4, root.be);
					((Query)root).sizeRestrict = true;
				}
				else
					System.Diagnostics.Debug.WriteLine("we don't support g2 q2 SZR len of " + len.ToString());
			}
		}

		public class Interest : Child
		{
			static char[] delims = new char[]{'\0'};

			public override void Process(int offset, int len)
			{
				if(len > 0)
					((Query)root).interests = Utils.GetString(root.inPayload, offset, len).Split(delims, 600);
			}
		}
	}

	public class QueryAck : Message
	{
		int timeStamp = 0;
		int retry = 0;
		IPAddress remoteHub;

		public override void Process(int offset, int len)
		{
			if(root.sockNum == -1)
				this.remoteHub = root.ipep.Address;
			else
				this.remoteHub = Sck.scks[root.sockNum].remoteIPA;
			if(len < 16)
				throw new Exception("QA packet didn't have guid");
			GUIDitem gitem = new GUIDitem(root.inPayload, offset);

			//check if it's for us
			if(Search.activeSearches.Count > 0)
			{
				lock(Search.activeSearches)
				{
					foreach(Search search in Search.activeSearches)
						if(gitem.Equals(search.guid))
						{
							Child child = this.children;
							while(child != null)
							{
								child.Read(child.offset);
								if(child.GetType() == typeof(DoneHub))
									search.DoneHub(((DoneHub)child).ipa);
								child = child.nextChild;
							}
							if(this.retry > 0)
								HostCache.UpdateRetryTime(this.remoteHub, this.retry);
							return;
						}
				}
			}

			//if not for us...
			if(Stats.Updated.Gnutella2.ultrapeer)
			{
				Router.RouteEntry re;
				lock(Router.htRoutes)
				{
					if(Router.htRoutes.ContainsKey(gitem))
						re = (Router.RouteEntry)Router.htRoutes[gitem];
#if DEBUG
					else
					{
						Utils.Diag("G2 QA no route found");
						return;
					}
#else
					else
						return;
#endif
				}
				//make a copy... but insert an FR child
				OCopy ocopy = new OCopy();
				//make room for FR child
				ocopy.payload = new byte[root.inPayload.Length+(1+1+2+4)];
				//copy all children... but skip the null byte and the guid for now
				int tmpIndex = root.inPayload.Length-(1+16);
				Array.Copy(root.inPayload, 0, ocopy.payload, 0, tmpIndex);
				//start the FR child
				OMessage.Setllen(ocopy.payload, tmpIndex, 1);
				OMessage.Setnlen(ocopy.payload, tmpIndex, 2);
				//the below code is compatible with both little endian and big endian
				if(root.be)
					ocopy.payload[tmpIndex] |= 0x02;
				tmpIndex++;
				Endian.VarBytesFromInt(ocopy.payload, tmpIndex, 4, 1);
				tmpIndex++;
				ocopy.payload[tmpIndex] = (byte)'F';
				tmpIndex++;
				ocopy.payload[tmpIndex] = (byte)'R';
				tmpIndex++;
				if(root.ipep != null)
					Array.Copy(Endian.GetBytes(root.ipep.Address), 0, ocopy.payload, tmpIndex, 4);
				else
					Array.Copy(Endian.GetBytes(Sck.scks[root.sockNum].remoteIPA), 0, ocopy.payload, tmpIndex, 4);
				tmpIndex += 4;
				//end of the FR child... null packet and guid follow
				Array.Copy(root.inPayload, root.inPayload.Length-(1+16), ocopy.payload, tmpIndex, (1+16));
				ocopy.CreateHeader("FR", root.be, true);
				Router.RoutePacket(ocopy, re);
			}
		}

		public override Child ChildType(ref string name)
		{
			if(name == "D")
				return new DoneHub();
			else if(name == "S")
				return new SearchHub();
			else if(name == "TS")
				return new Timestamp();
			else if(name == "RA")
				return new RetryAfter();
			else if(name == "FR")
				return new FromAddress();
			else
				return new UnknownChild(ref name);
		}

		public class Timestamp : Child
		{
			public override void Process(int offset, int len)
			{
				((QueryAck)this.root).timeStamp = Endian.ToInt32(root.inPayload, offset, root.be);
			}
		}

		public class DoneHub : Child
		{
			public IPAddress ipa;

			public override void Process(int offset, int len)
			{
				this.ipa = Endian.GetIPAddress(root.inPayload, offset);
				//add these once in a while
				if(GUID.rand.Next(0, 20) == 1)
				{
					HubInfo hi = new HubInfo();
					hi.port = Endian.ToUInt16(root.inPayload, offset+4, root.be);
					hi.timeKnown = 0;
					HostCache.AddRecentAndCache(this.ipa, hi);
				}
			}
		}

		public class SearchHub : Child
		{
			public override void Process(int offset, int len)
			{
				if(len != 6 && len != 10)
				{
					System.Diagnostics.Debug.WriteLine("SearchHub paylen: " + len.ToString());
				}
				else
				{
					HubInfo hi = new HubInfo();
					hi.port = Endian.ToUInt16(root.inPayload, offset+4, root.be);
					if(len == 6)
						hi.timeKnown = 0;
					else
                        hi.timeKnown = Math.Abs(((QueryAck)root).timeStamp - Endian.ToInt32(root.inPayload, offset+6, root.be));
					HostCache.AddRecentAndCache(Endian.GetIPAddress(root.inPayload, offset), hi);
				}
			}
		}

		public class RetryAfter : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 2)
					((QueryAck)root).retry = (int)Endian.ToUInt16(root.inPayload, offset, root.be);
				else if(len == 4)
					((QueryAck)root).retry = Endian.ToInt32(root.inPayload, offset, root.be);
			}
		}

		public class FromAddress : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 6 || len == 4)
					((QueryAck)root).remoteHub = Endian.GetIPAddress(root.inPayload, offset);
				else
					Utils.Diag("G2 QA FromAddress is wrong length");
			}
		}
	}

	public class QueryHit : Message
	{
		ArrayList qhos = null;
		IPEndPoint ipepSender = null;
		IPEndPoint ipepNH = null;
		byte[] servguid = null;
		string vendor = "";
		bool supportsChat = false;
		bool supportsBrowse = false;
		bool szFound = false;

		public override void Process(int offset, int len)
		{
			if(len != 17)
				throw new Exception("g2 QH2 packet didn't have guid & hop count");

			//potential browse host response
			if(root.sockNum != -1)
				if(Sck.scks[root.sockNum].browseHost)
				{
					System.Threading.Thread.Sleep(200);
					Child child = this.children;
					while(child != null)
					{
						child.Read(child.offset);
						child = child.nextChild;
					}
					string searchname = "browse: " + Sck.scks[root.sockNum].address;
					if(this.qhos != null)
						foreach(QueryHitObject qho in this.qhos)
						{
							qho.browse = true;
							qho.chat = this.supportsChat;
							qho.guid = Utils.HexGuid(root.inPayload, offset+1, 16);
							qho.hops = (int)root.inPayload[offset];
							if(ipepSender == null)
							{
								System.Diagnostics.Debug.WriteLine("q2 qh2 no node address");
								return;
							}
							qho.ip = ipepSender.Address.ToString();
							qho.ipepNH = this.ipepNH;
							qho.networkType = NetworkType.Gnutella2;
							qho.port = ipepSender.Port;
							qho.servIdent = this.servguid;
							qho.sockWhereFrom = root.sockNum;
							qho.speed = 50;
							qho.vendor = this.vendor;
							GUIBridge.AddQueryHit(qho, null, ref searchname);
						}
					return;
				}

			//check if it's for us
			if(Search.activeSearches.Count > 0)
			{
				lock(Search.activeSearches)
					foreach(Search search in Search.activeSearches)
						if(GUID.Compare(search.guid, 0, root.inPayload, offset+1))
						{
							if(!search.active)
								return;
							Child child = this.children;
							while(child != null)
							{
								child.Read(child.offset);
								child = child.nextChild;
							}
							if(this.qhos != null)
								foreach(QueryHitObject qho in this.qhos)
								{
									qho.browse = this.supportsBrowse;
									qho.chat = this.supportsChat;
									qho.guid = Utils.HexGuid(search.guid);
									qho.hops = (int)root.inPayload[offset];
									if(ipepSender == null)
									{
										System.Diagnostics.Debug.WriteLine("q2 qh2 no node address");
										return;
									}
									qho.ip = ipepSender.Address.ToString();
									qho.ipepNH = this.ipepNH;
									qho.networkType = NetworkType.Gnutella2;
									qho.port = ipepSender.Port;
									qho.servIdent = this.servguid;
									qho.sockWhereFrom = root.sockNum;
									qho.speed = 50;
									qho.vendor = this.vendor;
									GUIBridge.AddQueryHit(qho, null, ref search.query);
								}
							return;
						}
			}

			//qh2 isn't for us
			if(Stats.Updated.Gnutella2.ultrapeer)
			{
				GUIDitem gitem = new GUIDitem(root.inPayload, offset+1);
				Router.RouteEntry re;
				lock(Router.htRoutes)
				{
					if(Router.htRoutes.ContainsKey(gitem))
						re = (Router.RouteEntry)Router.htRoutes[gitem];
#if	DEBUG
					else
					{
						Utils.Diag("G2 QH2 no route found... maybe requery response?");
						goto requery_response;
					}
#else
					else
						goto requery_response;
#endif
				}
				OCopy ocopy = new OCopy();
				ocopy.payload = root.inPayload;
				ocopy.payload[offset]++;	//hop count
				ocopy.CreateHeader("QH2", root.be, root.cf);
				Router.RoutePacket(ocopy, re);
				return;
			}

		requery_response:
			//maybe it's for a downloader... potential requery response
			if(Stats.Updated.downloadsNow == 0)
				return;
			Child child2 = this.children;
			while(child2 != null)
			{
				child2.Read(child2.offset);
				child2 = child2.nextChild;
			}
			if(this.qhos != null)
				foreach(QueryHitObject qho in this.qhos)
					foreach(DownloadManager dMEr in DownloadManager.dms)
						if(dMEr != null)
							if(dMEr.active)
								if(((Downloader)dMEr.downloaders[0]).qho.networkType == NetworkType.Gnutella2)
									if(((Downloader)dMEr.downloaders[0]).qho.sha1sum == qho.sha1sum && qho.sha1sum.Length > 0)
									{
										//for downloads started via the "Download URL" feature in FileScope
										if(dMEr.downloaders.Count == 1 && ((Downloader)dMEr.downloaders[0]).qho.ip == "0.0.0.0")
										{
											((Downloader)dMEr.downloaders[0]).qho.fileSize = qho.fileSize;
											((Downloader)dMEr.downloaders[0]).qho.fileName = qho.fileName;
										}
										qho.browse = this.supportsBrowse;
										qho.chat = this.supportsChat;
										qho.guid = Utils.HexGuid(root.inPayload, offset+1, 16);
										qho.hops = (int)root.inPayload[offset];
										if(ipepSender == null)
										{
											System.Diagnostics.Debug.WriteLine("q2 qh2 no node address");
											return;
										}
										qho.ip = ipepSender.Address.ToString();
										qho.ipepNH = this.ipepNH;
										qho.networkType = NetworkType.Gnutella2;
										qho.port = ipepSender.Port;
										qho.servIdent = this.servguid;
										qho.sockWhereFrom = root.sockNum;
										qho.speed = 50;
										qho.vendor = this.vendor;
										ReQuery.AddGnutellaReQueryFoundDownloader(qho, dMEr);
									}
		}

		QueryHitObject GetLatestQHO()
		{
			return (QueryHitObject)qhos[qhos.Count-1];
		}

		public override Child ChildType(ref string name)
		{
			if(name == "H")
				return new Hit();
			else if(name == "NH")
				return new NeighborHub();
			else if(name == "HG")
				return new HitGroup();
			else if(name == "GU")
				return new GuIdent();
			else if(name == "NA")
				return new NodeAddress();
			else if(name == "V")
				return new VendorCode();
			else if(name == "MD")
				return new UniMetadata();
			else if(name == "UPRO")
				return new UserProfile();
			else if(name == "BUP")
				return new BrowseTag();
			else if(name == "PCH")
				return new ChatTag();
			else
				return new UnknownChild(ref name);
		}

		public class GuIdent : Child
		{
			public override void Process(int offset, int len)
			{
				((QueryHit)root).servguid = new byte[16];
				Array.Copy(root.inPayload, offset, ((QueryHit)root).servguid, 0, 16);
			}
		}

		public class NodeAddress : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 6)
					((QueryHit)root).ipepSender = new IPEndPoint(Endian.GetIPAddress(root.inPayload, offset), Endian.ToUInt16(root.inPayload, offset+4, root.be));
				else
					System.Diagnostics.Debug.WriteLine("G2 QH2 node address was messed up: " + len.ToString());
			}
		}

		public class NeighborHub : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 6)
					((QueryHit)root).ipepNH = new IPEndPoint(Endian.GetIPAddress(root.inPayload, offset), Endian.ToUInt16(root.inPayload, offset+4, root.be));
				else
					System.Diagnostics.Debug.WriteLine("G2 QH2 neighboring hub address was messed up: " + len.ToString());
			}
		}

		public class VendorCode : Child
		{
			public override void Process(int offset, int len)
			{
				if(len == 4)
					((QueryHit)root).vendor = Gnutella.Vendor.GetVendor(Encoding.ASCII.GetString(root.inPayload, offset, 4));
			}
		}

		public class BrowseTag : Child
		{
			public override void Process(int offset, int len)
			{
				((QueryHit)root).supportsBrowse = true;
			}
		}

		public class ChatTag : Child
		{
			public override void Process(int offset, int len)
			{
				((QueryHit)root).supportsChat = true;
			}
		}

		public class HitGroup : Child
		{
			public override void Process(int offset, int len)
			{
				//
			}

			public override Child ChildType(ref string name)
			{
				if(name == "SS")
					return new ServerState();
				else
					return new UnknownChild(ref name);
			}

			public class ServerState : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}
		}

		public class Hit : Child
		{
			public override void Process(int offset, int len)
			{
				if(((QueryHit)root).qhos == null)
					((QueryHit)root).qhos = new ArrayList(2);
				((QueryHit)root).qhos.Add(new QueryHitObject());
				Child child = this.children;
				while(child != null)
				{
					child.Read(child.offset);
					child = child.nextChild;
				}
			}

			public override Child ChildType(ref string name)
			{
				if(name == "URN")
					return new URN();
				else if(name == "URL")
					return new URL();
				else if(name == "DN")
					return new DescName();
				else if(name == "MD")
					return new Metadata();
				else if(name == "SZ")
					return new Size();
				else if(name == "G")
					return new Group();
				else if(name == "ID")
					return new ID();
				else if(name == "CSC")
					return new CachedSourceCount();
				else if(name == "PART")
					return new Part();
				else if(name == "COM")
					return new Comment();
				else if(name == "PVU")
					return new Preview();
				else
					return new UnknownChild(ref name);
			}

			public class URN : Child
			{
				public override void Process(int offset, int len)
				{
					string strURN = Utils.GetString(root.inPayload, offset, len);
					if(strURN.Substring(0, 2) == "bp" && len == 47)
						((QueryHit)root).GetLatestQHO().sha1sum = "urn:sha1:" + Base32.Encode(root.inPayload, offset+3, 20);
					else if(strURN.Substring(0, 8) == "bitprint" && len == 53)
						((QueryHit)root).GetLatestQHO().sha1sum = "urn:sha1:" + Base32.Encode(root.inPayload, offset+9, 20);
					else if(strURN.Substring(0, 4) == "sha1" && len == 25)
						((QueryHit)root).GetLatestQHO().sha1sum = "urn:sha1:" + Base32.Encode(root.inPayload, offset+5, 20);
					else if(strURN.Substring(0, 4) == "ed2k" && len == 21)
					{
						((QueryHit)root).GetLatestQHO().md4sum = new byte[16];
						Array.Copy(root.inPayload, offset+5, ((QueryHit)root).GetLatestQHO().md4sum, 0, 16);
					}
				}
			}

			public class URL : Child
			{
				public override void Process(int offset, int len)
				{
					if(len > 0)
						System.Diagnostics.Debug.WriteLine("g2 url unsupported: " + Utils.GetString(root.inPayload, offset, len));
				}
			}

			public class DescName : Child
			{
				public override void Process(int offset, int len)
				{
					if(!((QueryHit)root).szFound)
					{
						((QueryHit)root).GetLatestQHO().fileSize = Endian.ToUInt32(root.inPayload, offset, root.be);
						offset += 4;
						len -= 4;
					}
					((QueryHit)root).GetLatestQHO().fileName = Utils.GetString(root.inPayload, offset, len);
				}
			}

			public class Metadata : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class Size : Child
			{
				public override void Process(int offset, int len)
				{
					if(len != 4 && len != 8)
						throw new Exception("g2 QH2/H/SZ len not supported: " + len.ToString());
					if(len == 4)
						((QueryHit)root).GetLatestQHO().fileSize = Endian.ToUInt32(root.inPayload, offset, root.be);
					else if(len == 8)
					{
						ulong tmpulong = Endian.ToUInt64(root.inPayload, offset, root.be);
						if(tmpulong < uint.MaxValue)
							((QueryHit)root).GetLatestQHO().fileSize = (uint)tmpulong;
						else
							throw new Exception("g2 QH2/H/SZ len too big!");
					}
					((QueryHit)root).szFound = true;
				}
			}

			public class Group : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class ID : Child
			{
				public override void Process(int offset, int len)
				{
					//as of right now, we don't touch ((QueryHit)root).GetLatestQHO().fileIndex, so it's available
				}
			}

			public class CachedSourceCount : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class Part : Child
			{
				public override void Process(int offset, int len)
				{
					System.Diagnostics.Debug.WriteLine("g2 partial file: " + Endian.ToUInt32(root.inPayload, offset, root.be).ToString());
				}
			}

			public class Comment : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}

			public class Preview : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}
		}

		public class UniMetadata : Child
		{
			public override void Process(int offset, int len)
			{
				//
			}
		}

		public class UserProfile : Child
		{
			public override void Process(int offset, int len)
			{
				//
			}

			public override Child ChildType(ref string name)
			{
				if(name == "NICK")
					return new NickName();
				else
					return new UnknownChild(ref name);
			}

			public class NickName : Child
			{
				public override void Process(int offset, int len)
				{
					//
				}
			}
		}
	}

	public class UserProfileRequest : Message
	{
		public override void Process(int offset, int len)
		{
			//
		}
	}

	public class UserProfileAnswer : Message
	{
		public override void Process(int offset, int len)
		{
			//
		}
	}
}
