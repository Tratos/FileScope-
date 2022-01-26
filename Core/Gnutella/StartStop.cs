// StartStop.cs
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

namespace FileScope.Gnutella
{
	/// <summary>
	/// Used for starting and stopping gnutella services.
	/// </summary>
	public class StartStop
	{
		//gnutella network enabled or not
		public static bool enabled = false;

		/// <summary>
		/// Start connecting to gnutella network.
		/// </summary>
		public static void Start()
		{
			enabled = true;
			//start processing packets
			ProcessThread.Start();
			//setup Pong Cache
			if(PongCache.tmrTimeStamp == null)
				PongCache.StartCache();
			//connect to gnutella
			ConnectionManager.StartConnecting();
			//update the homepage
			GUIBridge.RefreshHomePageNetworks();
		}

		/// <summary>
		/// Pause connecting to gnutella network.
		/// </summary>
		public static void Stop()
		{
			enabled = false;
			//stop connecting
			ConnectionManager.StopConnecting();
			//stop processing packets
			ProcessThread.Abort();
			//clear queue
			Stats.gnutellaQueue.Clear();
			Stats.Updated.Gnutella.lastConnectionCount = 0;
			//update the homepage
			GUIBridge.RefreshHomePageNetworks();
		}

		/// <summary>
		/// Abort all gnutella processes.
		/// </summary>
		public static void Abort()
		{
			enabled = false;
			//stop connecting
			ConnectionManager.StopConnecting();
			//finally abort our threads
			ProcessThread.Abort();
		}
	}
}
