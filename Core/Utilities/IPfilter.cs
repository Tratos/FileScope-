// IPfilter.cs
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
	/// Used as a scheme to prevent connecting to private IPs.
	/// </summary>
	public class IPfilter
	{
		/// <summary>
		/// Returns true if the ip is a private local ip.
		/// </summary>
		public static bool Private(string ip)
		{
			if(ip.IndexOf(Stats.settings.ipAddress) != -1)
				return true;
			if(ip.ToLower().IndexOf("localhost") != -1)
				return true;

			//take out the colon and port
			if(ip.IndexOf(":") != -1)
				ip = ip.Substring(0, ip.IndexOf(":"));

			try
			{
				byte[] bytesIP = Endian.BigEndianIP(ip);
				if(bytesIP[0]==(byte)10)
					return true;
				else if(bytesIP[0]==(byte)172 && bytesIP[1]>=(byte)16 && bytesIP[1]<=(byte)31)
					return true;
				else if(bytesIP[0]==(byte)192 && bytesIP[1]==(byte)168)
					return true;
				else if(bytesIP[0]==(byte)127 && bytesIP[1]==(byte)0)
					return true;
				else if(bytesIP[0]==(byte)0 && bytesIP[1]==(byte)0 && bytesIP[2]==(byte)0 && bytesIP[3]==(byte)0)
					return true;
				else
					return false;
			}
			catch
			{
				//probably a host name instead of an ip address
				System.Diagnostics.Debug.WriteLine("IPfilter Private ip: " + ip);
				return false;
			}
		}

		/// <summary>
		/// Returns true if the ip is a private local ip.
		/// However, unlike Private, Private2 will disregard the local public ip address.
		/// </summary>
		public static bool Private2(string ip)
		{
			//take out the colon and port
			if(ip.IndexOf(":") != -1)
				ip = ip.Substring(0, ip.IndexOf(":"));

			try
			{
				byte[] bytesIP = Endian.BigEndianIP(ip);
				if(bytesIP[0]==(byte)10)
					return true;
				else if(bytesIP[0]==(byte)172 && bytesIP[1]>=(byte)16 && bytesIP[1]<=(byte)31)
					return true;
				else if(bytesIP[0]==(byte)192 && bytesIP[1]==(byte)168)
					return true;
				else if(bytesIP[0]==(byte)127 && bytesIP[1]==(byte)0)
					return true;
				else if(bytesIP[0]==(byte)0 && bytesIP[1]==(byte)0 && bytesIP[2]==(byte)0 && bytesIP[3]==(byte)0)
					return true;
				else
					return false;
			}
			catch
			{
				//probably a host name instead of an ip address
				System.Diagnostics.Debug.WriteLine("IPfilter Private ip: " + ip);
				return false;
			}
		}
	}
}
