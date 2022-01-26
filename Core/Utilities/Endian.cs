// Endian.cs
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

namespace FileScope
{
	/// <summary>
	/// Any byte order related stuff.
	/// </summary>
	public class Endian
	{
		/// <summary>
		/// Convert 4 bytes into a string IP Address.
		/// </summary>
		public static string BigEndianIP(byte[] ip, int elIndex)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(ip[0+elIndex].ToString() + ".");
			sb.Append(ip[1+elIndex].ToString() + ".");
			sb.Append(ip[2+elIndex].ToString() + ".");
			sb.Append(ip[3+elIndex].ToString());
			return sb.ToString();
		}

		/// <summary>
		/// Convert string IP Address into 4 bytes.
		/// </summary>
		public static byte[] BigEndianIP(string ip)
		{
			byte[] final = new byte[4];
			char[] delimiter = new char[1];
			delimiter[0] = '.';
			//we split up the IP by each period
			string[] split = ip.Split(delimiter, 4);
			for(int x = 0; x < 4; x++)
				final[x] = Convert.ToByte(split[x]);
			return final;
		}

		/// <summary>
		/// Convert 4 bytes to a int taking into account endianness on the architecture.
		/// </summary>
		public static int ToInt32(byte[] var, int start, bool be)
		{
			if(!be && !Stats.Updated.le || be && Stats.Updated.le)
			{
				Array.Reverse(var, start, 4);
				int retu = BitConverter.ToInt32(var, start);
				Array.Reverse(var, start, 4);
				return retu;
			}
			else
				return BitConverter.ToInt32(var, start);
		}

		/// <summary>
		/// Convert 4 bytes to a uint taking into account endianness on the architecture.
		/// </summary>
		public static uint ToUInt32(byte[] var, int start, bool be)
		{
			if(!be && !Stats.Updated.le || be && Stats.Updated.le)
			{
				Array.Reverse(var, start, 4);
				uint retu = BitConverter.ToUInt32(var, start);
				Array.Reverse(var, start, 4);
				return retu;
			}
			else
				return BitConverter.ToUInt32(var, start);
		}

		/// <summary>
		/// Convert 8 bytes to a ulong taking into account endianness on the architecture.
		/// </summary>
		public static ulong ToUInt64(byte[] var, int start, bool be)
		{
			if(!be && !Stats.Updated.le || be && Stats.Updated.le)
			{
				Array.Reverse(var, start, 8);
				ulong retu = BitConverter.ToUInt64(var, start);
				Array.Reverse(var, start, 8);
				return retu;
			}
			else
				return BitConverter.ToUInt64(var, start);
		}

		/// <summary>
		/// Convert 2 bytes to a ushort taking into account endianness on the architecture.
		/// </summary>
		public static ushort ToUInt16(byte[] var, int start, bool be)
		{
			if(!be && !Stats.Updated.le || be && Stats.Updated.le)
			{
				Array.Reverse(var, start, 2);
				ushort retu = BitConverter.ToUInt16(var, start);
				Array.Reverse(var, start, 2);
				return retu;
			}
			else
				return BitConverter.ToUInt16(var, start);
		}

		/// <summary>
		/// Get bytes in little or big endian (defined in be).
		/// </summary>
		public static byte[] GetBytes(int var, bool be)
		{
			byte[] newbytearr = BitConverter.GetBytes(var);
			if(!be && !Stats.Updated.le || be && Stats.Updated.le)
				Array.Reverse(newbytearr, 0, 4);
			return newbytearr;
		}

		/// <summary>
		/// Get bytes in little or big endian (defined in be).
		/// </summary>
		public static byte[] GetBytes(uint var, bool be)
		{
			byte[] newbytearr = BitConverter.GetBytes(var);
			if(!be && !Stats.Updated.le || be && Stats.Updated.le)
				Array.Reverse(newbytearr, 0, 4);
			return newbytearr;
		}

		/// <summary>
		/// Get bytes in little or big endian (defined in be).
		/// </summary>
		public static byte[] GetBytes(ushort var, bool be)
		{
			byte[] newbytearr = BitConverter.GetBytes(var);
			if(!be && !Stats.Updated.le || be && Stats.Updated.le)
				Array.Reverse(newbytearr, 0, 2);
			return newbytearr;
		}

		/// <summary>
		/// Convert a variable number of bytes, in a given order, to an integer.
		/// </summary>
		public static int VarBytesToInt(byte[] byt, int loc, int num, bool be)
		{
			if(num == 0 || num >= 4)
			{
				System.Diagnostics.Debug.WriteLine("Endian.VarBytesToInt has invalid byte count");
				return 0;
			}
			int b0, b1, b2;
		ready:
			if(!be)
			{
				b0 = byt[loc] & 0x000000FF;
				if(num > 1)
					b1 = (byt[loc+1]<<8) & 0x0000FF00;
				else
					b1 = 0;
				if(num > 2)
					b2 = (byt[loc+2]<<16) & 0x00FF0000;
				else
					b2 = 0;
				return (b0|b1|b2);
			}
			else
			{
				//obviously not optimized for big endian
				byte[] byt2 = new byte[4];
				Array.Copy(byt, loc, byt2, 4-num, num);
				Array.Reverse(byt2);
				loc = 0;
				byt = byt2;
				be = !be;
				goto ready;
			}
		}

		/// <summary>
		/// How many bytes can this integer fit into?
		/// </summary>
		public static int NumBytesFromInt(int val)
		{
			if(val <= 0)
				return 0;
			else if(val < 256)
				return 1;
			else if(val < 65536)
				return 2;
			else
				return 3;
		}

		/// <summary>
		/// Convert an integer into a variable number of bytes, in whatever byte order of this machine.
		/// </summary>
		public static void VarBytesFromInt(byte[] dest, int loc, int val, int numBytes)
		{
			if(numBytes == 0)
				return;
			dest[loc] = (byte)(val & 0x000000FF);
			if(numBytes > 1)
			{
				dest[loc+1] = (byte)((val>>8) & 0x000000FF);
				if(numBytes > 2)
					dest[loc+2] = (byte)((val>>16) & 0x000000FF);
			}
			if(numBytes > 1 && !Stats.Updated.le)
				Array.Reverse(dest, loc, numBytes);
		}

		/// <summary>
		/// Return an IPAddress from 4 bytes in a byte[].
		/// </summary>
		public static System.Net.IPAddress GetIPAddress(byte[] byt, int loc)
		{
			return (new System.Net.IPAddress(BitConverter.ToUInt32(byt, loc)));
		}

		/// <summary>
		/// Return a byte[] from an IPAddress.
		/// </summary>
		public static byte[] GetBytes(System.Net.IPAddress ipa)
		{
			return BitConverter.GetBytes((uint)ipa.Address);
		}
	}
}
