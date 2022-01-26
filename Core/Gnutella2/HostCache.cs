// HostCache.cs
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
	[Serializable]
	public class HubInfo
	{
		public ushort port;
		//how long have we known this host without being refreshed
		public int timeKnown = 0;
		public MoreHubInfo mhi = null;
	}

	[Serializable]
	public class MoreHubInfo
	{
		public IPAddress ipa = null;
		//if we're behind a firewall, we need query keys for one of our directly connected hubs
		public IPAddress queryKeyedHub = null;
		//query key for us... or a directly connected hub
		public bool qk = false;
		public int queryKey;
		//limit before this hub can be queried again
		public int retryCountDown = 0;
	}

	[Serializable]
	public class QKeyInfo
	{
		public IPEndPoint ipep = null;
		public int timeKnown = 0;
		public int queryKey = -1;
	}

	[Serializable]
	public class IPAComparer : IComparer
	{
		public int Compare(object obj1, object obj2)
		{
			return (int)(((IPAddress)obj1).Address - ((IPAddress)obj2).Address);
		}
	}

	/// <summary>
	/// Anything regarding the storage of g2 hosts and their attributes is dealt with here.
	/// </summary>
	public class HostCache
	{
		//the infamous hub cache of G2 hubs and associated query keys, timestamps, etc. [IPAddress, HubInfo]
		public static Hashtable hubCache = new Hashtable(100);
		//the most recently discovered "fresh" hubs [IPAddress, HubInfo]
		public static SortedList recentHubs = new SortedList(new IPAComparer(), 120);
		//keep track of query keys we assign [IPEndPoint, QKeyInfo]
		public static Hashtable sentQueryKeys = new Hashtable(500);
		//timer for doing some filtering stuff
		public static GoodTimer timerFilter = new GoodTimer(20000);

		/// <summary>
		/// Add simultaneously to recentHubs and hubCache.
		/// If already in hubCache, timeKnown is refreshed.
		/// </summary>
		public static void AddRecentAndCache(IPAddress ipa, HubInfo hi)
		{
			if(ConnectionManager.InOurCluster(ipa))
				return;
			bool inRecents = recentHubs.Contains(ipa);
			lock(hubCache)
			{
				if(!hubCache.Contains(ipa))
				{
					hi.mhi = new MoreHubInfo();
					hi.mhi.ipa = ipa;
					hubCache.Add(ipa, hi);
				}
				else
				{
					//refresh timestamp
					HubInfo hi2 = (HubInfo)hubCache[ipa];
					if(hi2.timeKnown > hi.timeKnown)
						hi2.timeKnown = hi.timeKnown;
					hi2.port = hi.port;
					hi = hi2;
				}
			}
			lock(recentHubs)
				if(!inRecents && hi.timeKnown < 2000)
					recentHubs.Add(ipa, hi);
			if(!inRecents && Search.activeSearches.Count > 0)
			{
			again:
				if(!hi.mhi.qk)
					Search.NewHubNoKey(ipa, hi);
				else
				{
					//check first
					if(!Stats.Updated.udpIncoming)
					{
						if(ConnectionManager.SckNumForRouting(hi.mhi.queryKeyedHub) == -1)
						{
							hi.mhi.qk = false;
							hi.mhi.queryKeyedHub = null;
							goto again;
						}
					}
					else
					{
						if(hi.mhi.queryKeyedHub != null)
						{
							hi.mhi.qk = false;
							hi.mhi.queryKeyedHub = null;
							goto again;
						}
					}
					lock(Search.activeSearches)
					{
						int which = GUID.rand.Next(0, Search.activeSearches.Count);
						((Search)Search.activeSearches[which]).NewHub(ipa, hi);
					}
				}
			}
		}

		/// <summary>
		/// Keep track of the keys we send out.
		/// </summary>
		public static void SentQueryKey(int queryKey, IPEndPoint ipep)
		{
			lock(sentQueryKeys)
			{
				if(sentQueryKeys.ContainsKey(ipep))
				{
					((QKeyInfo)sentQueryKeys[ipep]).queryKey = queryKey;
					((QKeyInfo)sentQueryKeys[ipep]).timeKnown = 0;
				}
				else
				{
					QKeyInfo qki = new QKeyInfo();
					qki.ipep = ipep;
					qki.queryKey = queryKey;
					qki.timeKnown = 0;
					sentQueryKeys.Add(ipep, qki);
				}
			}
		}

		public static bool QueryKeyOK(IPEndPoint ipep, int queryKey)
		{
			lock(sentQueryKeys)
			{
				if(sentQueryKeys.ContainsKey(ipep))
				{
					if(((QKeyInfo)sentQueryKeys[ipep]).queryKey == queryKey)
						return true;
					else
						return false;
				}
				else
					return false;
			}
		}

		public static void UpdateRetryTime(IPAddress ipa, int time)
		{
			try
			{
				lock(hubCache)
					((HubInfo)hubCache[ipa]).mhi.retryCountDown = time;
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("trouble with G2 UpdateRetryTime: " + e.Message);
			}
		}

		public static void SetupTimer()
		{
			if(timerFilter.Enabled)
				return;
			timerFilter.AddEvent(new ElapsedEventHandler(timerFilter_Tick));
			timerFilter.Start();
		}

		static ArrayList oldlist = new ArrayList(20);

		static void timerFilter_Tick(object sender, ElapsedEventArgs e)
		{
#if DEBUG
			if(hubCache.Count > 80000)
			{
				System.Diagnostics.Debug.WriteLine("hubCache size: " + hubCache.Count.ToString());
				hubCache.Clear();
			}
			if(sentQueryKeys.Count > 20000)
			{
				System.Diagnostics.Debug.WriteLine("sentQueryKeys size: " + sentQueryKeys.Count.ToString());
				sentQueryKeys.Clear();
			}
			if(recentHubs.Count > 320)
			{
				System.Diagnostics.Debug.WriteLine("recentHubs size: " + recentHubs.Count.ToString());
				recentHubs.Clear();
			}
#endif

			int timePassed = (int)timerFilter.Interval / 1000;
			lock(hubCache)
			{
				foreach(HubInfo hi in hubCache.Values)
				{
					hi.timeKnown += timePassed;
					if(hi.mhi != null)
						if(hi.mhi.retryCountDown > 0)
							hi.mhi.retryCountDown -= Math.Min(hi.mhi.retryCountDown, 20);
					//four hours
					if(hi.timeKnown > 14400)
						oldlist.Add(hi.mhi.ipa);
				}
				foreach(IPAddress ipa in oldlist)
				{
					hubCache.Remove(ipa);
					Utils.Diag("hubCache removed");
				}
				oldlist.Clear();
			}
			lock(sentQueryKeys)
			{
				foreach(QKeyInfo qki in sentQueryKeys.Values)
				{
					qki.timeKnown += timePassed;
					//four hours
					if(qki.timeKnown > 14400)
						oldlist.Add(qki.ipep);
				}
				foreach(IPEndPoint ipep in oldlist)
					sentQueryKeys.Remove(ipep);
				oldlist.Clear();
			}
			lock(recentHubs)
			{
				if(recentHubs.Count > 140)
				{
					for(int x = 0; x < 60; x++)
						recentHubs.RemoveAt(GUID.rand.Next(0, recentHubs.Count));
				}
				else if(recentHubs.Count > 90)
				{
					for(int x = 0; x < 10; x++)
						recentHubs.RemoveAt(GUID.rand.Next(0, recentHubs.Count));
				}
			}
		}
	}

	/// <summary>
	/// Anything regarding guid-based routing is dealt with here.
	/// </summary>
	public class Router
	{
		public class RouteEntry
		{
			public GUIDitem gitem_key;
			public int sckNum;
			public IPEndPoint ipep;
			public int timeLeft;		//set to -9000 when it's a directly connected host
		}

		public static Hashtable htRoutes = new Hashtable(200);
		public static GoodTimer tmrRoutes = new GoodTimer(20000);
		static LinkedGUIDs lguids = new LinkedGUIDs();

		public static void SetupRouter()
		{
			if(tmrRoutes.Enabled)
				return;
			tmrRoutes.AddEvent(new ElapsedEventHandler(tmrRoutes_Tick));
			tmrRoutes.Start();
		}

		class LinkedGUIDs
		{
			public GUIDitem gitem;
			public LinkedGUIDs next = null;
		}

		/// <summary>
		/// If we have the packet itself and the route entry, this function will take care of things.
		/// It's sent under the high priority send buffer... usually any packet being routed is high priority.
		/// </summary>
		public static void RoutePacket(OMessage omsg, RouteEntry re)
		{
			try
			{
				if(re.ipep != null)
					UDPSR.SendOutgoingPacket((IUdpMessage)omsg, re.ipep);
				else
					Sck.scks[re.sckNum].SendPacket(omsg);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("G2 RoutePacket");
			}
		}

		static void tmrRoutes_Tick(object sender, ElapsedEventArgs e)
		{
			//remove old entries
			lock(htRoutes)
			{
				int removeCount = 0;
				LinkedGUIDs lguidCurr = lguids;
				foreach(RouteEntry re in htRoutes.Values)
				{
					if(re.timeLeft != -9000)
					{
						re.timeLeft -= 20;
						if(re.timeLeft <= 0)
						{
							if(lguidCurr.next == null)
								lguidCurr.next = new LinkedGUIDs();
							lguidCurr.next.gitem = re.gitem_key;
							lguidCurr = lguidCurr.next;
							removeCount++;
						}
					}
				}
				lguidCurr = lguids;
				while(removeCount > 0)
				{
					htRoutes.Remove(lguidCurr.next.gitem);
					lguidCurr = lguidCurr.next;
					removeCount--;
				}
			}
		}
	}
}
