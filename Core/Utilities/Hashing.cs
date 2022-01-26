// Hashing.cs
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
	/// Class containing any hashing functions that are network specific.
	/// </summary>
	public class Hashing
	{
		/// <summary>
		/// Hashing function for use in Gnutella (1 and 2)'s Query Routing Protocol.
		/// We xor every little-endian 4 byte array together.
		/// Then we multiply by this nice 0x4F1BBCDC constant.
		/// Then just take the rightmost 32 bits as an int... our hash.
		/// </summary>
		public static int Hash(ref string query, int len, int bits)
		{
			int num = 0;
			int pos = 0;
			for(int i = 0; i < len; i++)
			{
				int val = (int)char.ToLower(query[i]) & 0xFF;
				val <<= (pos * 8);
				pos = (pos + 1) & 3;
				num ^= val;
			}
			UInt64 nProduct = (UInt64)num * (UInt64)0x4F1BBCDC;
			UInt64 nHash = (nProduct << 32) >> (32 + (32 - bits));
			return (int)nHash;
		}
	}
}
