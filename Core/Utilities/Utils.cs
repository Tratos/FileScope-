// Utils.cs
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
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace FileScope
{
	/// <summary>
	/// Small and useful procedures.
	/// </summary>
	public class Utils
	{
		/// <summary>
		/// Returns a full path in the current directory FileScope is running in for the given appendix.
		/// </summary>
		public static string GetCurrentPath(string appendix)
		{
			//return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(StartApp.assembly.Location), appendix);
			return System.IO.Path.Combine(Application.StartupPath, appendix);
		}

		/// <summary>
		/// Convert *.*.*.*:* into separate IP address and port.
		/// </summary>
		public static void AddrParse(string addr, out string address, out int port, int portDefault)
		{
			if(addr.IndexOf(":") != -1)
			{
				address = addr.Substring(0, addr.IndexOf(":"));
				try
				{
					port = (int)Convert.ToUInt16(addr.Substring(addr.IndexOf(":") + 1));
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("AddrParse invalid port");
					port = portDefault;
				}
			}
			else
			{
				address = addr;
				port = portDefault;
			}
		}

		/// <summary>
		/// Returns false if IP, true if host name.
		/// </summary>
		public static bool IsHostName(string addr)
		{
			for(int x = 0; x < addr.Length; x++)
				if(char.IsLetter(addr, x))
					return true;
			return false;
		}

		/// <summary>
		/// Launch web browser to go to a link.
		/// </summary>
		public static void SpawnLink(string url)
		{
			try
			{
				System.Diagnostics.Process.Start(url);
			}
			catch
			{
				try
				{
					System.Diagnostics.Process.Start("IExplore", url);
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("SpawnLink: " + e.Message);
				}
			}
		}

		/// <summary>
		/// Diagnostics shortcut function that will simply print a message in output window.
		/// </summary>
		public static void Diag(string what)
		{
#if DEBUG
			System.Diagnostics.Debug.WriteLine(what);
#endif
		}

		/// <summary>
		/// Strip a number from listview.
		/// </summary>
		public static uint Strip(string val, string appendix)
		{
			string good = val.Replace(",", "");
			good = good.Substring(0, good.Length - appendix.Length);
			return Convert.ToUInt32(good);
		}

		/// <summary>
		/// Assemble a number for listview.
		/// </summary>
		public static string Assemble(uint val, string appendix)
		{
			if(val == 0)
				return "0" + appendix;
			string good = string.Format("{0:#,###}", val);
			good += appendix;
			return good;
		}

		/// <summary>
		/// Convert something like 100s to 1:40.
		/// </summary>
		public static string TimeFormatFromSeconds(int seconds)
		{
			if(seconds < 60)
			{
				if(seconds.ToString().Length == 1)
					return "0:" + "0" + seconds.ToString();
				else
					return "0:" + seconds.ToString();
			}
			else if(seconds < 3600)
			{
				int mins = (int)Math.Floor((double)seconds / (double)60);
				int secs = seconds % 60;
				if(secs.ToString().Length == 1)
					return mins.ToString() + ":" + "0" + secs.ToString();
				else
					return mins.ToString() + ":" + secs.ToString();
			}
			else
			{
				int hours = (int)Math.Floor((double)seconds / (double)3600);
				seconds = seconds % 3600;
				int mins = (int)Math.Floor((double)seconds / (double)60);
				int secs = seconds % 60;
				if(secs.ToString().Length == 1)
					return hours.ToString() + ":" + mins.ToString() + ":" + "0" + secs.ToString();
				else
					return hours.ToString() + ":" + mins.ToString() + ":" + secs.ToString();
			}
		}

		public static int FindNull(byte[] data, int loc, int len)
		{
			for(int x = 1; x < len; x++)
				if(data[loc+x] == 0x00)
					return loc+x;
			return -1;
		}

		/// <summary>
		/// Used with G2 to get strings from byte[] data.
		/// </summary>
		public static string GetString(byte[] data, int loc, int len)
		{
			//take out the null ending
			if(data[loc+len-1] == 0x00)
				len--;
			return System.Text.Encoding.UTF8.GetString(data, loc, len);
		}

		/// <summary>
		/// Convert from byte[] to hex.
		/// </summary>
		public static string HexGuid(byte[] guid)
		{
			if(guid == null)
				return "";
			return BitConverter.ToString(guid).Replace("-", "");
		}

		public static string HexGuid(byte[] guid, int start, int length)
		{
			if(guid == null)
				return "";
			return BitConverter.ToString(guid, start, length).Replace("-", "");
		}

		/// <summary>
		/// Returns true if the two arrays are the same, false if otherwise.
		/// </summary>
		public static bool SameArray(byte[] byt1, byte[] byt2)
		{
			if(byt1 == null || byt2 == null)
				return false;
			if(byt1.Length != byt2.Length)
				return false;
			for(int x = 0; x < byt1.Length; x++)
				if(byt1[x] != byt2[x])
					return false;
			return true;
		}

		/// <summary>
		/// Returns true if the two arrays are the same, false if otherwise.
		/// </summary>
		public static bool SameArray(byte[] byt1, int loc1, int len1, byte[] byt2, int loc2, int len2)
		{
			if(len1 != len2)
				return false;
			for(int x = 0; x < len1; x++)
				if(byt1[x+loc1] != byt2[x+loc2])
					return false;
			return true;
		}

		/// <summary>
		/// Take out characters like (, ), -, etc.
		/// In the mean time... convert to lower case and take out extension.
		/// </summary>
		public static string FilterChars(string filename)
		{
			//chars to take out
			string[] filter = new string[]
			{
				"-", "_", "(", ")", "&", "0", "1", "2", "3",
				"4", "5", "6", "7", "8", "9", ",", ".", "[",
				"]", "{", "}", "?"
			};

			//convert to lower and take out extension
			filename = filename.ToLower();
			if(filename.LastIndexOf(".") != -1)
				filename = filename.Substring(0, filename.LastIndexOf("."));

			//take out ignored characters
			for(int x = 0; x < filter.Length; x++)
				filename = filename.Replace(filter[x], " ");

			return filename;
		}
	}

	/// <summary>
	/// Very simple class used infrequently.
	/// </summary>
	public class IPandPort
	{
		public string ip = "";
		public int port = -1;
	}
}
