// PongCache.cs
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
	public class PongCacheObject
	{
		//whole pong payload
		public byte[] pong;
		//socket number where this pong came from
		public int sckNumFrom;
		//timestamp to prevent outdated pongs
		public int timeStamp;
	}

	/// <summary>
	/// Used as a bandwidth reduction scheme as specified in the Pong Caching 0.1 scheme.
	/// </summary>
	public class PongCache
	{
		//stored pongs that are used in response to ping broadcasts
		public static ArrayList pongCache = new ArrayList();
		//timer to take care of things
		public static GoodTimer tmrTimeStamp;

		/// <summary>
		/// Sets up this class so that it's fully functional.
		/// </summary>
		public static void StartCache()
		{
			tmrTimeStamp = new GoodTimer();
			tmrTimeStamp.Interval = 3000;
			tmrTimeStamp.AddEvent(new ElapsedEventHandler(tmrTimeStamp_Tick));
			tmrTimeStamp.Start();
		}

		public static void tmrTimeStamp_Tick(object sender, ElapsedEventArgs e)
		{
			//System.Diagnostics.Debug.WriteLine("pong cache: " + pongCache.Count.ToString());
			//filter out all outdated objects
			lock(pongCache)
			{
				if(pongCache.Count > 1000)
					pongCache.Clear();
				for(int x = pongCache.Count - 1; x >= 0; x--)
				{
					PongCacheObject pco = (PongCacheObject)pongCache[x];
					if(Math.Abs((double)pco.timeStamp - DateTime.Now.TimeOfDay.TotalSeconds) > 15)
						pongCache.RemoveAt(x);
				}
			}
		}

		/// <summary>
		/// Add a pong to this cache.
		/// </summary>
		public static void AddPong(byte[] payload, int sockNum)
		{
			PongCacheObject pco = new PongCacheObject();
			pco.pong = payload;
			pco.sckNumFrom = sockNum;
			pco.timeStamp = (int)DateTime.Now.TimeOfDay.TotalSeconds;
			lock(pongCache)
				pongCache.Add(pco);
		}

		/// <summary>
		/// Take care of a broadcast ping.
		/// </summary>
		public static void HandlePing(Message theMessage, int sockNum)
		{
			if(pongCache.Count < 10)
			{
				/*
				 * our pong cache is depleted
				 * we need to broadcast this ping to get more pongs
				 */
				string nothingStr = "";
				theMessage.BroadcastPacket(sockNum, ref nothingStr);
				//System.Diagnostics.Debug.WriteLine("ping broadcast from: " + sockNum.ToString());
			}
			try
			{
				//send cached pongs
				for(int x = 0; x < 10; x++)
				{
					if(x >= pongCache.Count)
						return;
					PongCacheObject obj = (PongCacheObject)pongCache[x];
					//we don't send the pong where it came from
					if(obj.sckNumFrom != sockNum)
					{
						//send the packet
						Message pongPacket = new Message(theMessage.GetGUID(), 0x01, obj.pong, theMessage.GetHOPS());
						Sck.scks[sockNum].SendPacket(pongPacket);
					}
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("PongCache HandlePing");
			}
		}
	}
}
