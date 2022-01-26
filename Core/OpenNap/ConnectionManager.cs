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

namespace FileScope.OpenNap
{
	/// <summary>
	/// Keeps connecting to OpenNap servers until we're connected to a desired amount.
	/// </summary>
	public class ConnectionManager
	{
		//timer for checking connection count
		public static GoodTimer tmrCheck = null;
		//used in HomePage
		public static string status = "Disconnected";

		/// <summary>
		/// Start connecting to OpenNap network.
		/// </summary>
		public static void StartConnecting()
		{
			status = "Connecting";
			if(tmrCheck == null)
				tmrCheck = new GoodTimer(4000);
			tmrCheck.AddEvent(new ElapsedEventHandler(tmrCheck_Tick));
			tmrCheck.Start();
		}

		/// <summary>
		/// Stop connecting to OpenNap network.
		/// </summary>
		public static void StopConnecting()
		{
			status = "Disconnected";
			if(tmrCheck != null)
			{
				tmrCheck.Stop();
				tmrCheck.RemoveEvent(new ElapsedEventHandler(tmrCheck_Tick));
			}
			//disconnect all sockets
			foreach(Sck d in Sck.scks)
				if(d != null)
					d.Disconnect();
		}

		public static void tmrCheck_Tick(object sender, ElapsedEventArgs e)
		{
			//we optimally want to connect to 2 OpenNap servers
			if(Stats.Updated.OpenNap.lastConnectionCount >= 2)
				return;

			GUIBridge.OConnectRandom();
		}
	}
}
