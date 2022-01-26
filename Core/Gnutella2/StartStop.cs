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

namespace FileScope.Gnutella2
{
	/// <summary>
	/// Used for starting and stopping G2 services.
	/// </summary>
	public class StartStop
	{
		//G2 network enabled or not
		public static bool enabled = false;

		/// <summary>
		/// Start/resume.
		/// </summary>
		public static void Start()
		{
			G2Data.SetupFuncTable();
			enabled = true;
			//connect to G2
			ConnectionManager.StartConnecting();
			//start checking for ultrapeer compatibility
			if(!Stats.settings.ultrapeerCapable)
				Ultrapeer.Start();
			//timer for filtering in HostCache
			HostCache.SetupTimer();
			//guid based routing tables
			Router.SetupRouter();
			//update the homepage
			GUIBridge.RefreshHomePageNetworks();
		}

		/// <summary>
		/// Stop.
		/// </summary>
		public static void Stop()
		{
			enabled = false;
			//stop connecting
			ConnectionManager.StopConnecting();
			//stop checking for ultrapeer compatibility
			Ultrapeer.Stop();
			Stats.Updated.Gnutella2.lastConnectionCount = 0;
			//update the homepage
			GUIBridge.RefreshHomePageNetworks();
		}
	}
}
