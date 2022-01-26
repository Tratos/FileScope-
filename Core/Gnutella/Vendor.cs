// Vendor.cs
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
	public class Vendor
	{
		/// <summary>
		/// Return a vendor name from a 4 letter code.
		/// </summary>
		public static string GetVendor(string code)
		{
			switch(code.ToLower())
			{
				case "fscp":
					return "FileScope";
				case "toad":
					return "ToadNode";
				case "gnot":
					return "Gnotella";
				case "mact":
					return "Mactella";
				case "lime":
					return "LimeWire";
				case "bear":
					return "Bearshare";
				case "mrph":
					return "Morpheus";
				case "cult":
					return "Cultiv8r";
				case "gnuc":
					return "Gnucleus";
				case "gtkg":
					return "GTK-Gnutella";
				case "hslg":
					return "Hagelslag";
				case "naps":
					return "NapShare";
				case "ocfg":
					return "OpenCola";
				case "xolo":
					return "XoloX";
				case "swap":
					return "Swapper";
				case "qtel":
					return "QTella";
				case "phex":
					return "Phex";
				case "mute":
					return "Mutella";
				case "gnew":
					return "Gnewtellium";
				case "gnut":
					return "Gnut";
				case "snut":
					return "SwapNut";
				case "mnap":
					return "MyNapster";
				case "raza":
					return "Shareaza";
				case "atom":
					return "AtomWire";
				default:
					//System.Diagnostics.Debug.WriteLine("UNKNOWN VENDOR CODE: " + code);
					return code;
			}
		}
	}
}
