// Router.cs
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

namespace FileScope.Gnutella
{
	/// <summary>
	/// Used to get the routing value for a given guid.
	/// It also checks to see if guids have been seen before.
	/// Routing values:
	/// -2 means no entry.
	/// -1 means it's for us.
	/// Anything else is the socket number to route to.
	/// </summary>
	public class Router
	{
		/// <summary>
		/// Each individual broadcast packet has it's own guid.
		/// This function makes sure we haven't already "seen" a packet based on guid.
		/// Also adds the route if the packet hasn't been seen.
		/// </summary>
		public static bool SeenYet(Message theMessage, int sockNum)
		{
			WhereID whereID = new WhereID(sockNum);
			bool conkey;
			if(theMessage.GetPayloadDescriptor() == 0x00)
			{
				lock(Stats.gnutellaPongRouteTable)
					conkey = Stats.gnutellaPongRouteTable.ContainsKey(theMessage.gitem);
				if(conkey)
				{
					//System.Diagnostics.Debug.WriteLine("OLD Ping");
					return true;
				}
				else
				{
					AddPongRoute(theMessage.gitem, whereID);
					return false;
				}
			}
			else if(theMessage.GetPayloadDescriptor() == 0x80)
			{
				lock(Stats.gnutellaQueryHitRouteTable)
					conkey = Stats.gnutellaQueryHitRouteTable.ContainsKey(theMessage.gitem);
				if(conkey)
				{
					//System.Diagnostics.Debug.WriteLine("OLD Query");
					return true;
				}
				else
				{
					AddQueryHitRoute(theMessage.gitem, whereID);
					return false;
				}
			}
			else
			{
				//servent identifier for routing pushes
				byte[] servi = new byte[16];
				Array.Copy(theMessage.GetPayload(), theMessage.GetPayload().Length-16, servi, 0, 16);
				GUIDitem guitem = new GUIDitem(servi);
				lock(Stats.gnutellaPushRouteTable)
					conkey = Stats.gnutellaPushRouteTable.ContainsKey(guitem);
				if(conkey)
				{
					lock(Stats.gnutellaPushRouteTable)
						Stats.gnutellaPushRouteTable[guitem] = whereID;
					return true;
				}
				else
				{
					AddPushRoute(guitem, whereID);
					return false;
				}
			}
		}

		public static int GetPongRoute(GUIDitem guid)
		{
			lock(Stats.gnutellaPongRouteTable)
			{
				WhereID elWhereID = Stats.gnutellaPongRouteTable[guid] as WhereID;
				if(elWhereID != null)
					return elWhereID.GetLoc();
				//-2 indicates no table entry
				return -2;
			}
		}

		public static int GetQueryHitRoute(GUIDitem guid)
		{
			lock(Stats.gnutellaQueryHitRouteTable)
			{
				WhereID elWhereID = Stats.gnutellaQueryHitRouteTable[guid] as WhereID;
				if(elWhereID != null)
					return elWhereID.GetLoc();
				//-2 indicates no table entry
				return -2;
			}
		}

		public static int GetPushRoute(GUIDitem guid)
		{
			lock(Stats.gnutellaPushRouteTable)
			{
				WhereID elWhereID = Stats.gnutellaPushRouteTable[guid] as WhereID;
				if(elWhereID != null)
					return elWhereID.GetLoc();
				//-2 indicates no table entry
				return -2;
			}
		}

		public static void AddPongRoute(GUIDitem guid, WhereID where)
		{
			lock(Stats.gnutellaPongRouteTable)
				Stats.gnutellaPongRouteTable.ADD(guid, where);
		}

		public static void AddQueryHitRoute(GUIDitem guid, WhereID where)
		{
			lock(Stats.gnutellaQueryHitRouteTable)
				Stats.gnutellaQueryHitRouteTable.ADD(guid, where);
		}

		public static void AddPushRoute(GUIDitem guid, WhereID where)
		{
			lock(Stats.gnutellaPushRouteTable)
				Stats.gnutellaPushRouteTable.ADD(guid, where);
		}
	}

	/// <summary>
	/// Wrapper over an integer just so that we have a reference type.
	/// This way no boxing/unboxing lags us down when we insert this into the hashtable.
	/// This class represents where (socket number) a packet came from.
	/// </summary>
	public class WhereID
	{
		public int where;

		public WhereID(int loc)
		{
			this.where = loc;
		}

		public int GetLoc()
		{
			return where;
		}
	}

	public class GoodHashtable : Hashtable
	{
		//keeps original order so we can remove old entries easily
		ArrayList originalOrder;
		//max items
		int max;

		public GoodHashtable(IComparer var1, int var2) : base(var2, null, var1)
		{
			originalOrder = new ArrayList(var2);
			max = var2;
		}

		public void ADD(GUIDitem guid, WhereID whereID)
		{
			try
			{
				//prevent from getting too big
				if(originalOrder.Count > max)
					RemoveOldest();

				//add to both collections
				base.Add(guid, whereID);
				originalOrder.Add(guid);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("this never happens");
			}
		}

		public void RemoveOldest()
		{
			try
			{
				base.Remove((GUIDitem)originalOrder[0]);
				originalOrder.RemoveAt(0);
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("this also never happens: " + e.Message);
			}
		}
	}
}
