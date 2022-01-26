// guid.cs
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

namespace FileScope
{
	/// <summary>
	/// To be embedded in stuff (ex. Hashtables, etc.).
	/// </summary>
	public class GUIDitem
	{
		//store the original guid for reference
		public byte[] gUiD;
		//starting location in the above byte[]... length is always 16
		public int loc;
		//cache the hash code
		int hashcode;

		public GUIDitem(byte[] guid)
		{
			//copy ref
			this.gUiD = guid;
			this.loc = 0;
			//generate hash code and cache it
			this.hashcode = BitConverter.ToInt32(guid, 0)^BitConverter.ToInt32(guid, 4)^BitConverter.ToInt32(guid, 8)^BitConverter.ToInt32(guid, 12);
		}

		public GUIDitem(byte[] guid, int loc)
		{
			//copy ref
			this.gUiD = guid;
			this.loc = loc;
			//generate hash code and cache it
			this.hashcode = BitConverter.ToInt32(guid, loc+0)^BitConverter.ToInt32(guid, loc+4)^BitConverter.ToInt32(guid, loc+8)^BitConverter.ToInt32(guid, loc+12);
		}

		public override int GetHashCode()
		{
			return this.hashcode;
		}

		public override bool Equals(object obj)
		{
			GUIDitem gitem = (GUIDitem)obj;
			for(int x = 0; x < 16; x++)
				if(this.gUiD[this.loc+x] != gitem.gUiD[gitem.loc+x])
					return false;
			return true;
		}

		public bool Equals(byte[] guid)
		{
			for(int x = 0; x < 16; x++)
				if(this.gUiD[this.loc+x] != guid[x])
					return false;
			return true;
		}
	}

	/// <summary>
	/// GUID stuff.
	/// </summary>
	public class GUID
	{
		//randomizer
		public static Random rand = new Random();
		//comparers
		public static GuidComparer guidComparer = new GuidComparer();
		public static StringComparer stringComparer = new StringComparer();

		/// <summary>
		/// Return a new GUID.
		/// </summary>
		public static byte[] newGUID()
		{
			//allocate the GUID
			byte[] tempGuid = new byte[16];
			//fill with random crap
			rand.NextBytes(tempGuid);

			//new stuff
			tempGuid[8]=(byte)0xFF; //Mark as "new" gnutella client
			tempGuid[15]=(byte)0x00;//Future use

			return tempGuid;
		}

		/// <summary>
		/// Returns true if both guids are the same.
		/// </summary>
		public static bool Compare(byte[] byt1, int start1, byte[] byt2, int start2)
		{
			for(int x = 0; x < 16; x++)
				if(byt1[start1+x] != byt2[start2+x])
					return false;
			return true;
		}

		/// <summary>
		/// Compare two guids together.
		/// </summary>
		public class GuidComparer : System.Collections.IComparer
		{
			public int Compare(object gUid1, object gUid2)
			{
				GUIDitem gitem1 = (GUIDitem)gUid1;
				GUIDitem gitem2 = (GUIDitem)gUid2;
				for(int x = 0; x < 16; x++)
				{
					int diff = gitem1.gUiD[gitem1.loc+x] - gitem2.gUiD[gitem2.loc+x];
					if(diff != 0)
						return diff;
				}
				return 0;
			}
		}

		/// <summary>
		/// General purpose string comparer.
		/// </summary>
		public class StringComparer : System.Collections.IComparer
		{
			public int Compare(object obj1, object obj2)
			{
				string ob1 = (string)obj1;
				string ob2 = (string)obj2;
				return ob1.CompareTo(ob2);
			}
		}
	}
}
