// Query.cs
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
using System.Text;

namespace FileScope.Gnutella
{
	/// <summary>
	/// Class dedicated to query packets.
	/// Searches are created and processed here.
	/// </summary>
	public class Query
	{
		public static void HandleQuery(Message theMessage, int sockNum)
		{
			Stats.Updated.Gnutella.numQueries++;

			try
			{
				//check if we've seen it already
				if(Router.SeenYet(theMessage, sockNum))
					return;

				int minSpeed = (int)Endian.ToUInt16(theMessage.GetPayload(), 0, false);

				string total = Encoding.ASCII.GetString(theMessage.GetPayload());

				//locate the first null value in the string
				int firstNull = total.IndexOf((char)0x0, 2);
				if(firstNull == -1)
					return;

				//extract the non-rich keyword based search
				string query = total.Substring(2, firstNull-2);
				//System.Diagnostics.Debug.WriteLine("non-rich query: " + query);
				GUIBridge.Query(ref query);

				/*
				//find the rich portion
				if(total.Length - firstNull > 1)
				{
					byte[] rich = new byte[total.Length - firstNull - 2];
					Array.Copy(theMessage.GetPayload(), firstNull+1, rich, 0, rich.Length);
					string richQuery = Encoding.ASCII.GetString(rich);
					//System.Diagnostics.Debug.WriteLine("rich query: " + richQuery);
				}
				*/

				if(Stats.Updated.Gnutella.ultrapeer)
					//broadcast this query
					theMessage.BroadcastPacket(sockNum, ref query);

				//see if we have the file this guy/gal wants
				QueryHit.RespondToQuery(theMessage, ref query, sockNum);
			}
			catch(System.Threading.ThreadAbortException e)
			{e = e;}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("query error: " + e.Message);
			}
		}

		/// <summary>
		/// Send a query request to all appropriate connections.
		/// </summary>
		public static void BroadcastQuery(string query)
		{
			byte[] guid = GUID.newGUID();
			//add route to table
			WhereID wid = new WhereID(-1);
			Router.AddQueryHitRoute(new GUIDitem(guid), wid);
			//add search to arraylist
			ActiveSearch search = new ActiveSearch();
			search.guid = Utils.HexGuid(guid);
			search.query = query;
			lock(ActiveSearch.searches)
				ActiveSearch.searches.Add(search);
			//loop through all sockets to send the query to connected sockets
			foreach(Sck obj in Sck.scks)
				if(obj != null)
					if(obj.state == Condition.hndshk3)
						if(obj.ultrapeer)
							URNQuery(guid, query, obj.sockNum, "");
						else
						{
							if(obj.recQRT == null)
								URNQuery(guid, query, obj.sockNum, "");
							else
								if(obj.recQRT.CheckQuery(query))
									URNQuery(guid, query, obj.sockNum, "");
						}
		}

		public static void BroadcastReQuery(string query, string hash)
		{
			byte[] guid = GUID.newGUID();
			//add route to table
			WhereID wid = new WhereID(-1);
			Router.AddQueryHitRoute(new GUIDitem(guid), wid);
			//loop through all sockets to send the query to connected sockets
			foreach(Sck obj in Sck.scks)
				if(obj != null)
					if(obj.state == Condition.hndshk3)
						if(obj.ultrapeer)
							URNQuery(guid, query, obj.sockNum, hash);
						else
						{
							if(obj.recQRT == null)
								URNQuery(guid, query, obj.sockNum, hash);
							else
								if(obj.recQRT.CheckQuery(query))
								URNQuery(guid, query, obj.sockNum, hash);
						}
		}

		/// <summary>
		/// Sends simple query request without any meta data.
		/// </summary>
		public static void PlainQuery(byte[] guid, string query, int sockNum)
		{
			//convert string to byte array
			byte[] byteQuery = System.Text.Encoding.ASCII.GetBytes(query);
			//first two bytes in a query is the minimum speed
			byte[] minSpeed = new byte[2];
			minSpeed = Endian.GetBytes((ushort)0, false);

			//payload
			byte[] payload = new byte[minSpeed.Length+byteQuery.Length+1];
			Array.Copy(minSpeed, 0, payload, 0, 2);
			Array.Copy(byteQuery, 0, payload, 2, byteQuery.Length);
			//null terminated
			payload[payload.Length-1] = (byte)0;

			//setup message for outgoing, ttl 7, 0x80 packet
			Message message = new Message(guid, 0x80, payload, 7);
			Sck.scks[sockNum].SendPacket(message);
			//System.Diagnostics.Debug.WriteLine("plain search");
		}

		/// <summary>
		/// Sends simple query request with URN extension.
		/// </summary>
		public static void URNQuery(byte[] guid, string query, int sockNum, string hash)
		{
			//convert string to byte array
			byte[] byteQuery = System.Text.Encoding.ASCII.GetBytes(query);
			//first two bytes in a query is the minimum speed
			byte[] minSpeed = new byte[2];
			minSpeed = Endian.GetBytes((ushort)0, false);

			//payload
			byte[] payload = new byte[minSpeed.Length+byteQuery.Length+1+4+hash.Length+1];
			Array.Copy(minSpeed, 0, payload, 0, 2);
			Array.Copy(byteQuery, 0, payload, 2, byteQuery.Length);
			//first null
			payload[minSpeed.Length+byteQuery.Length] = (byte)0;

			//URN extension
			byte[] urn = new byte[4+hash.Length];
			urn = System.Text.Encoding.ASCII.GetBytes("urn:" + hash);
			Array.Copy(urn, 0, payload, minSpeed.Length+byteQuery.Length+1, urn.Length);
			//last null
			payload[payload.Length-1] = (byte)0;

			//setup message for outgoing, ttl 7, 0x80 packet
			Message message = new Message(guid, 0x80, payload, 7);
			Sck.scks[sockNum].SendPacket(message);
			//System.Diagnostics.Debug.WriteLine("urn search");
		}
	}
}
