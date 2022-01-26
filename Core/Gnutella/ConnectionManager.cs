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
using System.Timers;
using System.Collections;

namespace FileScope.Gnutella
{
	/// <summary>
	/// Responsible for maintaining the proper connections to the Gnutella network.
	/// Takes care of Sck.scks
	/// </summary>
	public class ConnectionManager
	{
		//timer for checking connection count
		public static GoodTimer tmrCheck;
		//keep track of all hosts we're connected or connecting to
		public static SortedList activeHosts = new SortedList();

		/// <summary>
		/// Take note of a host we're connecting or connected to.
		/// </summary>
		public static void AddActiveHost(string host, int sckNum)
		{
			if(ActiveHostFound(host))
				activeHosts[host] = sckNum;
			else
				activeHosts.Add(host, sckNum);
		}

		/// <summary>
		/// Check to see if we're already connected or connecting to this host.
		/// </summary>
		public static bool ActiveHostFound(string host)
		{
			return activeHosts.Contains(host);
		}

		/// <summary>
		/// We just disconnected from this host.
		/// </summary>
		public static void RemoveActiveHost(int sckNum)
		{
			for(int x = 0; x < activeHosts.Count; x++)
				if(Convert.ToInt32(activeHosts.GetByIndex(x)) == sckNum)
				{
					activeHosts.RemoveAt(x);
					break;
				}
		}

		//used in HomePage
		public static string status = "Disconnected";

		/// <summary>
		/// Start connecting to Gnutella.
		/// </summary>
		public static void StartConnecting()
		{
			status = "Connecting";
			if(tmrCheck == null)
				tmrCheck = new GoodTimer(600);
			tmrCheck.AddEvent(new ElapsedEventHandler(tmrCheck_Tick));
			tmrCheck.Start();
		}

		/// <summary>
		/// Stop connecting to Gnutella.
		/// </summary>
		public static void StopConnecting()
		{
			status = "Disconnected";
			//keep the good hosts
			if(!Stats.Updated.Gnutella.ultrapeer)
			{
				lock(Stats.gnutellaHosts)
				{
					for(int x = 0; x < activeHosts.Count; x++)
						if(activeHosts.GetKey(x).ToString().IndexOf(":") != -1 && activeHosts.GetKey(x).ToString().IndexOf("http") == -1)
							Stats.gnutellaHosts.Insert(0, activeHosts.GetKey(x));
				}
			}
			if(tmrCheck != null)
			{
				tmrCheck.Stop();
				tmrCheck.RemoveEvent(new ElapsedEventHandler(tmrCheck_Tick));
			}
			//disconnect all gnutella sockets
			foreach(Sck d in Sck.scks)
				if(d != null)
					d.Disconnect("stopped connecting");
		}

		/// <summary>
		/// For some reason, we have to switch from ultrapeer to leaf node mode.
		/// </summary>
		public static void SwitchMode()
		{
			//disconnect all gnutella sockets
			foreach(Sck d in Sck.scks)
				if(d != null)
					d.Disconnect("switching mode");
			Stats.Updated.Gnutella.ultrapeer = false;
			//inform gui
			GUIBridge.SwitchedMode();
		}

		/// <summary>
		/// Switch from ultrapeer to leaf node mode but spare one socket.
		/// </summary>
		public static void SwitchMode(int sckNum)
		{
			//disconnect all gnutella sockets except sckNum
			foreach(Sck d in Sck.scks)
				if(d != null)
					if(d.sockNum != sckNum)
						d.Disconnect("switching mode");
			Stats.Updated.Gnutella.ultrapeer = false;
			//inform gui
			GUIBridge.SwitchedMode();
		}

		//try to connect to 7 ultrapeers when in ultrapeer mode
		public static int numNonLeafs = 7;

		/// <summary>
		/// This is where the magic happens.
		/// This timer makes sure everything regarding connections is perfect.
		/// </summary>
		public static void tmrCheck_Tick(object sender, ElapsedEventArgs e)
		{
			//first take care of the host cache
			lock(Stats.gnutellaHosts)
			{
				if(Stats.gnutellaHosts.Count > 160)
					Stats.gnutellaHosts.RemoveRange(Stats.gnutellaHosts.Count-20, 20);
			}

			//hold the number of fully connected sockets
			int connectedCount = 0;
			//hold the number of connecting sockets
			int connectingCount = 0;
			//hold the number of non-leafnode connections (peers and ultrapeers are ultrapeers)
			int ultrapeerCount = 0;
			//hold the number of incoming connections
			int leafCount = 0;
			//hold the number of connecting sockets specifically connecting to ultrapeers
			int connectingToUltrapeerCount = 0;

			//tally up everything
			foreach(Sck d in Sck.scks)
				if(d != null)
				{
					if(d.state == Condition.hndshk3)
					{
						if(d.ultrapeer || !d.newHost)
							ultrapeerCount++;
						else
							leafCount++;
						//increment connected count
						connectedCount++;
					}
					else if(d.state != Condition.Closed)
					{
						connectingCount++;
						if(d.incoming == false)
							connectingToUltrapeerCount++;
					}
				}

			Stats.Updated.Gnutella.lastConnectionCount = connectedCount;

			//System.Diagnostics.Debug.WriteLine("connectedCount: " + connectedCount.ToString());
			//System.Diagnostics.Debug.WriteLine("connectingCount: " + connectingCount.ToString());
			//System.Diagnostics.Debug.WriteLine("ultrapeerCount: " + ultrapeerCount.ToString());
			//System.Diagnostics.Debug.WriteLine("leafCount: " + leafCount.ToString());
			//System.Diagnostics.Debug.WriteLine("connectingToUltrapeerCount:" + connectingToUltrapeerCount.ToString());

			if(Stats.Updated.Gnutella.ultrapeer)
			//ultrapeer mode
			{
				//the "desired" amount of non-leaf connections during ultrapeer mode
				int goodCount = numNonLeafs;

				//add ultrapeer connections if we don't have enough
				if(ultrapeerCount < goodCount)
					SpawnConnections(goodCount - (ultrapeerCount+connectingToUltrapeerCount) + 2, connectedCount+connectingCount);
			}
			else
			//we're running in leaf node mode
			{
				//the "desired" amount of connections
				int goodCount;
				if(Stats.settings.connectionType == EnumConnectionType.dialUp)
					goodCount = 2;
					//goodCount = 8;
				else
					goodCount = 4;
					//goodCount = 12;

				//take care of an extra connection if one exists
				//next time this routine is called, another superfluous connection will close
				if(connectedCount > goodCount)
					foreach(Sck d in Sck.scks)
						if(d != null)
						{
							if(d.state == Condition.hndshk3)
							{
								d.Disconnect("too many connections");
								break;
							}
							else if(d.state != Condition.Closed)
								d.Disconnect("too many connections");
						}

				//time to add connections if we don't have enough
				if(connectedCount < goodCount)
					SpawnConnections(goodCount - (connectedCount+connectingCount) + 2, connectedCount+connectingCount);
			}
		}

		/// <summary>
		/// Start a given number of outgoing connections.
		/// </summary>
		public static void SpawnConnections(int count, int otherCount)
		{
			if(count <= 0)
				return;

			//find out whether we have cached hosts or not
			if(Stats.gnutellaHosts.Count > 0)
			//we have cached hosts; connect to them
			{
				lock(Stats.gnutellaHosts)
				{
					for(int x = 0; x < count && Stats.gnutellaHosts.Count > 0; x++)
					{
						string host = (string)Stats.gnutellaHosts[0];
						Stats.gnutellaHosts.RemoveAt(0);
						if(ActiveHostFound(host))
							continue;
						Sck.Outgoing(host);
					}
				}
			}
			else
			//we don't have cached hosts; connect to Host Cache servers
			{
				try
				{
					for(int x = 0; x < 5; x++)
					{
						string cacheServer = (string)Stats.gnutellaWebCache[GUID.rand.Next(0, Stats.gnutellaWebCache.Count)];
						if(ActiveHostFound(cacheServer))
							continue;
						Sck.OutgoingWebCache(cacheServer);
					}
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("ConnectionManager SpawnConnections");
				}
			}
		}
	}
}
