// HashSums.cs
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
using System.Security.Cryptography;
using System.IO;

namespace FileScope
{
	/// <summary>
	/// Class with various hash functions.
	/// </summary>
	public class HashSums
	{
		/// <summary>
		/// Quickly generate a 20 byte hash value for a file.
		/// And return the 32 byte string base32 encoded equivalent.
		/// </summary>
		public static string CalcSha1(string file, ref byte[] sha1bytes)
		{
			FileStream fileStream = null;
			SHA1Managed sha1 = null;
			try
			{
				//create a managed sha1 generator
				sha1 = new SHA1Managed();
				//the 20 byte result hash
				byte[] shaResults = null;
				//filestream for the given file
				fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
				//generate the hash
				shaResults = sha1.ComputeHash(fileStream);
				//release resources
				fileStream.Close();
				sha1.Clear();
				//convert and return the base32 equivalent
				sha1bytes = shaResults;
				return Base32.Encode(shaResults, 0, shaResults.Length);
			}
			catch(System.Threading.ThreadAbortException tae)
			{tae=tae;}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Sha1 error: " + e.Message);
			}
			//whether an error occurs or not, the file will be closed
			if(fileStream != null)
				fileStream.Close();
			if(sha1 != null)
				sha1.Clear();
			return "";
		}

		/// <summary>
		/// Return the 16 byte md4 hash sum for the given file.
		/// </summary>
		public static byte[] CalcMD4(string file)
		{
			FileStream fileStream = null;
			try
			{
				//create a managed md4 generator
				MD4Hash.MD4 md4gen = new MD4Hash.MD4();
				//the 16 byte result hash
				byte[] md4Results = null;
				//filestream for the given file
				fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
				//generate the hash
				md4Results = md4gen.GetByteHashCrapFromFileStream(fileStream);
				//release resources
				fileStream.Close();
				//return the hash
				return md4Results;
			}
			catch(System.Threading.ThreadAbortException tae)
			{tae=tae;}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("MD4 error: " + e.Message);
			}
			//whether an error occurs or not, the file will be closed
			if(fileStream != null)
				fileStream.Close();
			return null;
		}
	}
}
