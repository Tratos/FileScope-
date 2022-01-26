// Bye.cs
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

namespace FileScope.Gnutella
{
	/// <summary>
	/// This class processes bye packets.
	/// </summary>
	public class Bye
	{
		public static void HandleBye(Message theMessage, int sockNum)
		{
			try
			{
				//two bytes in little-endian format in the beginning
				byte[] bytesCode = new byte[2];
				//subtract 3 (2 for bytesCode and 1 for the null termination)
				byte[] bytesContents = new byte[theMessage.GetPayloadLength() - 3];

				Array.Copy(theMessage.GetPayload(), 0, bytesCode, 0, 2);
				Array.Copy(theMessage.GetPayload(), 2, bytesContents, 0, bytesContents.Length);

				ushort code = Endian.ToUInt16(bytesCode, 0, false);
				string contents = System.Text.Encoding.ASCII.GetString(bytesContents);

				//System.Diagnostics.Debug.WriteLine("bye: " + code.ToString() + " " + contents);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("HandleBye");
			}
		}
	}
}
