// Search.cs
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
using System.Collections;
using System.Timers;

namespace FileScope.Gnutella2
{
	/// <summary>
	/// One instance of this per G2 query.
	/// </summary>
	public class Search
	{
		//store instances of Search class
		public static ArrayList activeSearches = new ArrayList(4);
		//stores [IPAddress][HubInfo] of hubs we haven't received keys from
		public static SortedList hubsWithoutKeys = new SortedList(new IPAComparer(), 30);

		/// <summary>
		/// Starts a new search.
		/// </summary>
		public static void BeginSearch(string query)
		{
			byte[] newguid = new byte[16];
			GUID.rand.NextBytes(newguid);
			Search search = new Search(query, newguid);
			lock(activeSearches)
				activeSearches.Add(search);
		}

		public bool active = false;
		public string query;
		public byte[] guid;
		//hubs we searched in this particular search
		Hashtable searchedHubs = new Hashtable(20);
		//stores [IPAddress][HubInfo] of hubs we have received keys from
		SortedList hubsWithKeys = new SortedList(new IPAComparer(), 30);
		GoodTimer tmrTakeCareOfBusiness = new GoodTimer(350);

		public Search(string query, byte[] guid)
		{
			//System.Diagnostics.Debug.WriteLine("beginning g2 search for " + query + ", udpIncoming: " + Stats.Updated.udpIncoming.ToString());
			this.query = query;
			this.guid = guid;
			this.active = true;
			//first do tcp searches before udp
			lock(HostCache.recentHubs)
			{
				//at some point in time we should also check retryCountDown for these hosts

				//send to ultrapeers
				foreach(ConnectionManager.G2Host g2h in ConnectionManager.ultrapeers.Keys)
				{
					OQ2 oq2 = new OQ2();
					oq2.query = this.query;
					oq2.guid = this.guid;
					oq2.tcpQuery = true;
					Sck.scks[g2h.sockNum].SendPacket(oq2);
				}
				//send to leaves if you're an ultrapeer
				if(Stats.Updated.Gnutella2.ultrapeer)
					foreach(ConnectionManager.G2Host g2h in ConnectionManager.leaves.Keys)
					{
						OQ2 oq2 = new OQ2();
						oq2.query = this.query;
						oq2.guid = this.guid;
						oq2.tcpQuery = true;
						Sck.scks[g2h.sockNum].SendPacket(oq2);
					}
			}
			//use recent hubs (warning: lock on recentHubs has other nested locks)
			lock(HostCache.recentHubs)
			{
				for(int x = 0; x < HostCache.recentHubs.Count; x++)
				{
					IPAddress ipa = (IPAddress)HostCache.recentHubs.GetKey(x);
					HubInfo hi = (HubInfo)HostCache.recentHubs.GetByIndex(x);
					if(hi.mhi == null)
						continue;
				again:
					if(!hi.mhi.qk)
						NewHubNoKey(ipa, hi);
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
						//it's ready
						NewHub(ipa, hi);
					}
				}
			}
			//timers
			tmrTakeCareOfBusiness.AddEvent(new ElapsedEventHandler(tmrTakeCareOfBusiness_Tick));
			tmrTakeCareOfBusiness.Start();
		}

		public void TerminateSearch()
		{
			if(!active)
				return;
			tmrTakeCareOfBusiness.Stop();
			tmrTakeCareOfBusiness.RemoveEvent(new ElapsedEventHandler(tmrTakeCareOfBusiness_Tick));
			this.active = false;
		}

		/// <summary>
		/// A new hub just directly connected to us via tcp.
		/// </summary>
		public static void NewDirectHub(int sockNum)
		{
			foreach(Search search in activeSearches)
				if(search.active)
				{
					OQ2 oq2 = new OQ2();
					oq2.query = search.query;
					oq2.guid = search.guid;
					oq2.tcpQuery = true;
					Sck.scks[sockNum].SendPacket(oq2);
				}
		}

		/// <summary>
		/// A recent hub was just discovered without a key.
		/// </summary>
		public static void NewHubNoKey(IPAddress ipa, HubInfo hi)
		{
			lock(hubsWithoutKeys)
			{
				if(!hubsWithoutKeys.Contains(ipa))
					hubsWithoutKeys.Add(ipa, hi);
			}
		}

		/// <summary>
		/// In case an ultrapeer disconnects while we're udp-firewalled, this function should be called.
		/// Since this ultrapeer could have been used as a udp proxy, we might have already requested query keys for it.
		/// These query keys become useless now because the hub that would proxy udp packets to us via tcp is now gone.
		/// So now this function will loop through hubsWithKeys and remove any query keys obtained for this proxy hub.
		/// </summary>
		public static void PotentialProxyGone(IPAddress ipa)
		{
			lock(activeSearches)
			{
				foreach(Search search in activeSearches)
					lock(search)
					{
						for(int x = search.hubsWithKeys.Count-1; x >= 0; x--)
						{
							HubInfo hi = (HubInfo)search.hubsWithKeys.GetByIndex(x);
							if(hi.mhi != null)
								if(hi.mhi.qk)
									if(hi.mhi.queryKeyedHub.Equals(ipa))
									{
										hi.mhi.qk = false;
										hi.mhi.queryKeyedHub = null;
										search.hubsWithKeys.RemoveAt(x);
									}
						}
					}
			}
		}

		/// <summary>
		/// A recent hub (ready to be searched) was just recently discovered.
		/// </summary>
		public void NewHub(IPAddress ipa, HubInfo hi)
		{
			if(hi.mhi == null)
			{
				System.Diagnostics.Debug.WriteLine("g2 search new hub problem");
				return;
			}
			else if(hi.mhi.retryCountDown > 0)
				return;

			lock(this)
			{
				if(this.searchedHubs.ContainsKey(ipa) || this.hubsWithKeys.ContainsKey(ipa))
					return;
				this.hubsWithKeys.Add(ipa, hi);
			}
		}

		/// <summary>
		/// A done hub indicated in a query acknowledgement packet.
		/// </summary>
		public void DoneHub(IPAddress ipa)
		{
			//we don't want to query this hub again... so take two steps:

			lock(this)
			{
				//1.
				if(hubsWithKeys.ContainsKey(ipa))
					hubsWithKeys.Remove(ipa);

				//2.
				if(!searchedHubs.ContainsKey(ipa))
					searchedHubs.Add(ipa, null);
			}
		}

		void tmrTakeCareOfBusiness_Tick(object sender, ElapsedEventArgs e)
		{
			lock(hubsWithoutKeys)
			{
				int count = 0;
			again:
				if(hubsWithoutKeys.Count > 0)
				{
					int randIndex = GUID.rand.Next(0, hubsWithoutKeys.Count);
					IPAddress ipa = (IPAddress)hubsWithoutKeys.GetKey(randIndex);
					HubInfo hi = (HubInfo)hubsWithoutKeys.GetByIndex(randIndex);
					hubsWithoutKeys.RemoveAt(randIndex);
					//check once more if it doesn't have a query key
					if(hi.mhi != null)
						if(!hi.mhi.qk)
							UDPSR.SendOutgoingPacket(new OQKR(), new IPEndPoint(ipa, hi.port));
					count++;
					if(count < 2)
					{
						System.Threading.Thread.Sleep(10);
						goto again;
					}
				}
			}

			bool searchIt = false;
			IPAddress ipa2 = null;
			HubInfo hi2 = null;
			lock(this)
			{
				if(this.hubsWithKeys.Count > 0)
				{
					int randIndex = GUID.rand.Next(0, this.hubsWithKeys.Count);
					ipa2 = (IPAddress)this.hubsWithKeys.GetKey(randIndex);
					hi2 = (HubInfo)this.hubsWithKeys.GetByIndex(randIndex);
					this.hubsWithKeys.RemoveAt(randIndex);
					if(hi2.mhi == null)
						return;
					if(!hi2.mhi.qk || hi2.mhi.retryCountDown > 0)
						return;
					try
					{
						searchedHubs.Add(ipa2, null);
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine("G2 hub somehow got searched twice in the same query");
						return;
					}
					searchIt = true;
				}
#if DEBUG
				else
				{
					Utils.Diag("G2 no hubsWithKeys found");
				}
#endif
			}
			if(searchIt)
				SearchHub(ipa2, hi2);
		}

		/// <summary>
		/// Sends q2 over udp.
		/// </summary>
		void SearchHub(IPAddress ipa, HubInfo hi)
		{
			OQ2 oq2 = new OQ2();
			oq2.guid = this.guid;
			oq2.query = this.query;
			oq2.hi = hi;
			//System.Diagnostics.Debug.WriteLine("searching " + ipa.ToString());
			SearchHub(ipa, hi, oq2);
		}

		public static void SearchHub(IPAddress ipa, HubInfo hi, OQ2 oq2)
		{
#if DEBUG
			Utils.Diag("SearchHub: " + ipa.ToString());
#endif
			if(ipa == null)
			{
				Utils.Diag("g2 searchhub ipa null problem");
				return;
			}
			if(!Stats.Updated.udpIncoming)
			{
				int sckNum = ConnectionManager.SckNumForRouting(hi.mhi.queryKeyedHub);
				if(sckNum == -1)
				{
					hi.mhi.qk = false;
					hi.mhi.queryKeyedHub = null;
					return;
				}
				IPAddress proxy = Sck.scks[sckNum].remoteIPA;
				if(proxy == null)
					return;
				oq2.ipaProxyHub = proxy;
				oq2.proxyPort = Sck.scks[sckNum].port;
			}
			else
			{
				if(hi.mhi.queryKeyedHub != null)
				{
					hi.mhi.qk = false;
					hi.mhi.queryKeyedHub = null;
					return;
				}
			}			
			UDPSR.SendOutgoingPacket(oq2, new IPEndPoint(ipa, hi.port));
		}
	}
}
