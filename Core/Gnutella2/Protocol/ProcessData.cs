// ProcessData.cs
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

namespace FileScope.Gnutella2
{
	/// <summary>
	/// Class with static functions for dealing with G2 packets.
	/// </summary>
	public class G2Data
	{
		public static Hashtable funcTable = null;
		public delegate void handleFunc(Message msg);

		/// <summary>
		/// Setup our funcTable Hashtable to have delegates to functions dealing with certain types of G2 packets.
		/// </summary>
		public static void SetupFuncTable()
		{
			if(funcTable == null)
			{
				funcTable = new Hashtable(13);
				funcTable.Add(typeof(Ping), new handleFunc(HandlePing));
				funcTable.Add(typeof(Pong), new handleFunc(HandlePong));
				funcTable.Add(typeof(LocalNodeInfo), new handleFunc(HandleLNI));
				funcTable.Add(typeof(KnownHubList), new handleFunc(HandleKHL));
				funcTable.Add(typeof(Push), new handleFunc(HandlePush));
				funcTable.Add(typeof(QueryHashTable), new handleFunc(HandleQHT));
				funcTable.Add(typeof(QueryKeyRequest), new handleFunc(HandleQKR));
				funcTable.Add(typeof(QueryKeyAnswer), new handleFunc(HandleQKA));
				funcTable.Add(typeof(Query), new handleFunc(HandleQuery));
				funcTable.Add(typeof(QueryAck), new handleFunc(HandleQueryAck));
				funcTable.Add(typeof(QueryHit), new handleFunc(HandleQueryHit));
				funcTable.Add(typeof(UserProfileRequest), new handleFunc(HandleUPR));
				funcTable.Add(typeof(UserProfileAnswer), new handleFunc(HandleUPA));
			}
		}

		/// <summary>
		/// Deal with a root packet and its children.
		/// </summary>
		public static void ProcessMessage(Message msg)
		{
			((handleFunc)funcTable[msg.GetType()])(msg);
		}

		public static void HandlePing(Message msg)
		{
			Ping ping = (Ping)msg;
			ping.Read(0);
			Stats.Updated.Gnutella2.numPI++;
		}

		public static void HandlePong(Message msg)
		{
			Pong pong = (Pong)msg;
			pong.Read(0);
			Stats.Updated.Gnutella2.numPO++;
		}

		public static void HandleLNI(Message msg)
		{
			LocalNodeInfo lni = (LocalNodeInfo)msg;
			lni.Read(0);
			Stats.Updated.Gnutella2.numLNI++;
		}

		public static void HandleKHL(Message msg)
		{
			KnownHubList khl = (KnownHubList)msg;
			khl.Read(0);
			Stats.Updated.Gnutella2.numKHL++;
		}

		public static void HandlePush(Message msg)
		{
			Push push = (Push)msg;
			push.Read(0);
			Stats.Updated.Gnutella2.numPUSH++;
		}

		public static void HandleQHT(Message msg)
		{
			if(Stats.Updated.Gnutella2.ultrapeer)
			{
				QueryHashTable qht = (QueryHashTable)msg;
				qht.Read(0);
			}
			else
				System.Diagnostics.Debug.WriteLine("g2 receiving qht when leaf");
			Stats.Updated.Gnutella2.numQHT++;
		}

		public static void HandleQKR(Message msg)
		{
			QueryKeyRequest qkr = (QueryKeyRequest)msg;
			qkr.Read(0);
			Stats.Updated.Gnutella2.numQKR++;
		}

		public static void HandleQKA(Message msg)
		{
			QueryKeyAnswer qka = (QueryKeyAnswer)msg;
			qka.Read(0);
			Stats.Updated.Gnutella2.numQKA++;
		}

		public static void HandleQuery(Message msg)
		{
			Query query = (Query)msg;
			query.Read(0);
			Stats.Updated.Gnutella2.numQ2++;
		}

		public static void HandleQueryAck(Message msg)
		{
			QueryAck qa = (QueryAck)msg;
			qa.Read(0);
			Stats.Updated.Gnutella2.numQA++;
		}

		public static void HandleQueryHit(Message msg)
		{
			QueryHit qh = (QueryHit)msg;
			qh.Read(0);
			Stats.Updated.Gnutella2.numQH2++;
		}

		public static void HandleUPR(Message msg)
		{
			UserProfileRequest upr = (UserProfileRequest)msg;
			upr.Read(0);
		}

		public static void HandleUPA(Message msg)
		{
			UserProfileAnswer upa = (UserProfileAnswer)msg;
			upa.Read(0);
		}
	}
}
