// Ultrapeer.cs
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

namespace FileScope.Gnutella2
{
	/// <summary>
	/// Determines whether or not this node's connection is good enough to become an ultrapeer.
	/// Criteria:
	/// bandwidth, ability to accept incoming connections, and uptime
	/// </summary>
	public class Ultrapeer
	{
		//60,000 b/sec transfers in any direction is good enough
		public static int minBandwidth = 60000;
		//6 hours sounds good
		public static int minMinutes = 360;
		//webcache update time (1:08)
		public static int minWCupdate = 68;
		//timer for checking every 4 seconds for bandwidth
		static GoodTimer tmrCheck;
		//timer for countdown of uptime
		static GoodTimer tmrUpCheck;
		//check for bandwidth first
		static bool metBandwidthRequirement = false;

		/// <summary>
		/// Start monitoring for ultrapeer capability.
		/// </summary>
		public static void Start()
		{
			//if timers aren't instantiated
			if(tmrCheck == null && tmrUpCheck == null)
			{
				tmrCheck = new GoodTimer(4000);
				tmrUpCheck = new GoodTimer(60000);
			}
			tmrCheck.AddEvent(new ElapsedEventHandler(tmrCheck_Tick));
			tmrUpCheck.AddEvent(new ElapsedEventHandler(tmrUpCheck_Tick));
			tmrCheck.Start();
			tmrUpCheck.Start();
		}

		/// <summary>
		/// Stop monitoring for ultrapeer capability.
		/// </summary>
		public static void Stop()
		{
			if(tmrCheck == null && tmrUpCheck == null)
				return;
			tmrCheck.Stop();
			tmrCheck.RemoveEvent(new ElapsedEventHandler(tmrCheck_Tick));
			tmrUpCheck.Stop();
			tmrUpCheck.RemoveEvent(new ElapsedEventHandler(tmrUpCheck_Tick));
		}

		public static void tmrCheck_Tick(object sender, ElapsedEventArgs e)
		{
			if(Stats.Updated.outTransferBandwidth + Stats.Updated.inTransferBandwidth > minBandwidth)
				metBandwidthRequirement = true;
		}

		public static void tmrUpCheck_Tick(object sender, ElapsedEventArgs e)
		{
			//System.Diagnostics.Debug.WriteLine("Ultrapeer metBandwidthRequirement:  " + metBandwidthRequirement.ToString());
			minMinutes--;
			if(minMinutes <= 0 && metBandwidthRequirement && Stats.Updated.everIncoming && Stats.Updated.udpIncoming)
			{
				System.Diagnostics.Debug.WriteLine("Just Became Ultrapeer Capable");
				Stats.settings.ultrapeerCapable = true;
			}
			//let's also update a webcache
			minWCupdate--;
			if(minWCupdate <= 0)
			{
				if(ConnectionManager.ultrapeers.Count > 3 && ConnectionManager.leaves.Count > 30 && Stats.gnutella2WebCache.Count > 0)
					Sck.OutgoingWebCache((string)Stats.gnutella2WebCache[GUID.rand.Next(0, Stats.gnutella2WebCache.Count)]);
				minWCupdate = 68;
			}
#if DEBUG
			//output some debug data related to ultrapeer mode
			if(Stats.Updated.Gnutella2.ultrapeer)
			{
				Utils.Diag("g2 ultrapeer collections check:");
				Utils.Diag("recentHubs: " + HostCache.recentHubs.Count.ToString() + " (320)");
				Utils.Diag("hubCache: " + HostCache.hubCache.Count.ToString() + " (80000)");
				Utils.Diag("sentQueryKeys: " + HostCache.sentQueryKeys.Count.ToString() + " (20000)");
				Utils.Diag("htRoutes: " + Router.htRoutes.Count.ToString());
				Utils.Diag("slIn: " + UDPSR.slIn.Count.ToString() + " (2000)");
				Utils.Diag("slOut: " + UDPSR.slOut.Count.ToString() + " (2000)");
			}
#endif
		}
	}
}
