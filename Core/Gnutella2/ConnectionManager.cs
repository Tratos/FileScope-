// ConnectionManager.cs
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
using System.Timers;
using System.Collections;

namespace FileScope.Gnutella2
{
	/// <summary>
	/// Maintains a general connection to the gnutella 2 network.
	/// </summary>
	public class ConnectionManager
	{
		//timer for checking connection count
		public static GoodTimer tmrCheck;
		//these two keep track of active Sck objects that are connected [G2Host,null]
		public static SortedList ultrapeers = new SortedList(new HostComparer(), 20);
		public static SortedList leaves = new SortedList(new HostComparer(), 200);
		//sort by ip; directly connected hosts [IPAddress,G2Host]
		public static Hashtable htDirects = new Hashtable(50);

		public class G2Host
		{
			public int sockNum;
		}

		public class HostComparer : IComparer
		{
			public int Compare(object obj1, object obj2)
			{
				return ((G2Host)obj1).sockNum - ((G2Host)obj2).sockNum;
			}
		}

		public static void AddG2Host(G2Host g2h, G2Mode g2m)
		{
			try
			{
				lock(HostCache.recentHubs)
				{
					if(g2m == G2Mode.Ultrapeer)
						ultrapeers.Add(g2h, null);
					else
						leaves.Add(g2h, null);
					htDirects.Add(Sck.scks[g2h.sockNum].remoteIPA, g2h);
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("AddG2Host: " + e.Message);
			}
		}

		public static void RemoveG2Host(G2Host g2h, G2Mode g2m)
		{
			try
			{
				if(g2m == G2Mode.Ultrapeer)
				{
					lock(HostCache.recentHubs)
					{
						ultrapeers.Remove(g2h);
						htDirects.Remove(Sck.scks[g2h.sockNum].remoteIPA);
					}
					//the recentHubs lock isn't over this because of a deadlock scenario involving activeSearches
					if(!Stats.Updated.udpIncoming)
						Search.PotentialProxyGone(Sck.scks[g2h.sockNum].remoteIPA);
				}
				else
				{
					lock(HostCache.recentHubs)
					{
						leaves.Remove(g2h);
						htDirects.Remove(Sck.scks[g2h.sockNum].remoteIPA);
					}
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("RemoveG2Host");
			}
		}

		public static bool DirectlyConnected(IPAddress ipa)
		{
			lock(HostCache.recentHubs)
				return htDirects.ContainsKey(ipa);
		}

		/// <summary>
		/// Returns true if host is already in our hub cluster (up to 2 hops away).
		/// </summary>
		public static bool InOurCluster(IPAddress ipa)
		{
			lock(HostCache.recentHubs)
			{
				foreach(G2Host g2h in ultrapeers.Keys)
				{
					if(Sck.scks[g2h.sockNum].remoteIPA.Equals(ipa))
						return true;
					foreach(IPEndPoint ipep in Sck.scks[g2h.sockNum].neighbors)
						if(ipep.Address.Equals(ipa))
							return true;
				}
				return false;
			}
		}

		/// <summary>
		/// If we cannot receive udp, we need an ultrapeer to do routing for us.
		/// This function will return the Sck number of the ultrapeer with the ipa.
		/// If we're no longer connected to that ultrapeer, it returns -1.
		/// </summary>
		public static int SckNumForRouting(IPAddress ipa)
		{
			if(ipa == null)
				return -1;
			lock(HostCache.recentHubs)
			{
				foreach(G2Host g2h in ultrapeers.Keys)
					if(Sck.scks[g2h.sockNum].remoteIPA.Equals(ipa))
						return g2h.sockNum;
				return -1;
			}
		}

		//used in HomePage
		public static string status = "Disconnected";

		/// <summary>
		/// Start connecting to Gnutella 2.
		/// </summary>
		public static void StartConnecting()
		{
			status = "Connecting";
			//set to a brand new timer
			if(tmrCheck == null)
				tmrCheck = new GoodTimer(800);
			tmrCheck.AddEvent(new ElapsedEventHandler(tmrCheck_Tick));
			tmrCheck.Start();
		}

		/// <summary>
		/// Stop connecting to Gnutella 2.
		/// </summary>
		public static void StopConnecting()
		{
			status = "Disconnected";
			lock(HostCache.recentHubs)
			{
				//keep some good hosts
				try
				{
					if(!Stats.Updated.Gnutella2.ultrapeer)
						for(int x = 0; x < ultrapeers.Count; x++)
						{
							IPAddress ipa = IPAddress.Parse(Sck.scks[((G2Host)ultrapeers.GetKey(x)).sockNum].address);
							HubInfo hi = new HubInfo();
							hi.timeKnown = 0;
							hi.port = (ushort)Sck.scks[((G2Host)ultrapeers.GetKey(x)).sockNum].port;
							if(!HostCache.recentHubs.Contains(ipa))
								HostCache.recentHubs.Add(ipa, hi);
						}
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("G2 StopConnecting problem: " + e.Message);
				}
			}
			if(tmrCheck != null)
			{
				tmrCheck.Stop();
				tmrCheck.RemoveEvent(new ElapsedEventHandler(tmrCheck_Tick));
			}
			//disconnect all g2 sockets
			foreach(Sck d in Sck.scks)
				if(d != null)
					d.Disconnect("stopped connecting");
		}

		/// <summary>
		/// For some reason, we have to switch from ultrapeer to leaf node mode.
		/// </summary>
		public static void SwitchMode()
		{
			//disconnect all gnutella2 sockets
			foreach(Sck d in Sck.scks)
				if(d != null)
					d.Disconnect("switching mode");
			Stats.Updated.Gnutella2.ultrapeer = false;
			OQH2.ResetStaticPart();
			//inform gui
			GUIBridge.SwitchedMode();
		}

		/// <summary>
		/// Switch from ultrapeer to leaf node mode but spare one socket.
		/// </summary>
		public static void SwitchMode(int sckNum)
		{
			//disconnect all gnutella2 sockets except sckNum
			foreach(Sck d in Sck.scks)
				if(d != null)
					if(d.sockNum != sckNum)
						d.Disconnect("switching mode");
			Stats.Updated.Gnutella2.ultrapeer = false;
			OQH2.ResetStaticPart();
			//inform gui
			GUIBridge.SwitchedMode();
		}

		public static int numHubsWhenHubMax = 8;
		public static int numHubsWhenHub = 6;
		static int numHubsWhenLeaf = 3;

		/// <summary>
		/// This is where the magic happens.
		/// This timer makes sure everything regarding connections is perfect.
		/// </summary>
		public static void tmrCheck_Tick(object sender, ElapsedEventArgs e)
		{
			Stats.Updated.Gnutella2.lastConnectionCount = ultrapeers.Count + leaves.Count;
			int goodCount;
			if(Stats.Updated.Gnutella2.ultrapeer)
			{
				if(ultrapeers.Count >= numHubsWhenHub)
					return;
				goodCount = numHubsWhenHub;
			}
			else
			{
				if(ultrapeers.Count == numHubsWhenLeaf)
					return;
				else if(ultrapeers.Count > numHubsWhenLeaf)
				{
					foreach(Sck sck in Sck.scks)
						if(sck != null)
							if(sck.state != Condition.Closed)
							{
								sck.Disconnect("too many connections");
								return;
							}
				}
				goodCount = numHubsWhenLeaf;
			}
			//we need more hubs at this point
			int connCount = 0;
			foreach(Sck sck in Sck.scks)
				if(sck != null)
					if(sck.state == Condition.Connecting)
						connCount++;
			SpawnConnections(goodCount - (ultrapeers.Count+connCount) + 2);
		}

		static bool AlreadyActive(ref string host)
		{
			foreach(Sck sck in Sck.scks)
				if(sck != null)
					if(sck.state != Condition.Closed)
						if(host.IndexOf(sck.address) != -1)
							return true;
			return false;
		}

		static bool AlreadyActive(IPAddress ipa)
		{
			foreach(Sck sck in Sck.scks)
				if(sck != null)
					if(sck.state != Condition.Closed)
						if(sck.remoteIPA.Equals(ipa))
							return true;
			return false;
		}

		static void SpawnConnections(int num)
		{
			if(num <= 0)
				return;

			if(HostCache.recentHubs.Count > 0)
			{
				for(int x = 0; x < num; x++)
				{
					IPAddress tmpIPA;
					string host;
					lock(HostCache.recentHubs)
					{
						tmpIPA = (IPAddress)HostCache.recentHubs.GetKey(x);
						host = tmpIPA.ToString() + ":" + ((HubInfo)HostCache.recentHubs.GetByIndex(x)).port.ToString();
						HostCache.recentHubs.RemoveAt(0);
					}
					if(AlreadyActive(tmpIPA))
						continue;
					Sck.Outgoing(host);
				}
			}
			else
			{
				try
				{
					if(Stats.gnutella2WebCache.Count == 0)
					{
						System.Diagnostics.Debug.WriteLine("no more webcache servers");
						return;
					}
					for(int x = 0; x < 4; x++)
					{
						string cacheServer = (string)Stats.gnutella2WebCache[GUID.rand.Next(0, Stats.gnutella2WebCache.Count)];
						if(AlreadyActive(ref cacheServer))
							continue;
						Sck.OutgoingWebCache(cacheServer);
					}
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("g2 ConnectionManager SpawnConnections");
				}
			}
		}
	}
}
