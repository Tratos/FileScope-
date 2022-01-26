// UniQueryHit.cs
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
using System.Net;
using System.Collections;
using System.Text;

namespace FileScope
{
	/// <summary>
	/// Class for storing the info from a single queryhit.
	/// This is shared among networks.
	/// </summary>
	[Serializable]
	public class QueryHitObject
	{
		public int port;
		public string ip = "";
		public long longIP;
		public int speed;
		public int fileIndex;
		public uint fileSize;
		public string fileName = "";
		public string filePath = "";
		public string[] extensions;
		public byte[] md4sum;
		public string sha1sum = "";		//store in urn:sha1:*****
		public string vendor = "";
		public string nick = "";
		public int unseenHosts;
		public bool push;
		public bool busy;
		public bool uploaded;
		public bool trueSpeed;
		public bool chat;
		public bool browse;
		public string xml = "";
		public string guid = "";
		public byte[] servIdent;
		public int sockWhereFrom;
		public int hops;
		public NetworkType networkType;
		public IPEndPoint ipepNH;
	}

	/// <summary>
	/// NetworkType in QueryHitObject to determine the kind of network we're dealing with.
	/// </summary>
	public enum NetworkType
	{
		Gnutella1, Gnutella2, OpenNap, EDonkey
	}

	/// <summary>
	/// Table of SIMILAR query hits used for multi-source downloading.
	/// </summary>
	[Serializable]
	public class QueryHitTable
	{
		/*
		 * values that appear in listview
		 */
		//the type of file this is (audio, documents, etc.)
		public string type;
		//the whole ip including port in format *.*.*.*:* or just "multiple"
		public string address;
		//the total number of souces we don't actually know about; this happens in some networks (eDonkey)
		public int unseenHosts;
		//g2 hosts that support browsing
		public bool browse = false;
		//some hosts don't support chat; this value is true if any of them support chat
		public bool chat;
		//the total speed of combined hosts
		public int speed;
		//since some query hits don't contain mp3 info, this is the overall mp3 info
		public string mp3info;
		//if any queryhit doesn't have push set, this value is false
		public bool push;
		//if any queryhit doesn't have busy set, this value is false
		public bool busy;
		//other values that appear in listview are just taken from one of the queryhitobjects

		/*
		 * values that don't appear in listview
		 * these values can appear in a "more info" window or something like that
		 * values that do appear in the listview gui are:
		 * filename, #, filesize, mp3info, flags(vendor, busy, chat, firewall), speed, address
		 */
		//sha1 sum in urn base32 format (urn:sha1: ...)
		public string sha1;
		//store the md4 digest in base32 format
		public string md4sum;
		//whether or not the user attempted to download this item already
		public bool downloaded = false;

		//store one or more query hits
		public ArrayList queryHitObjects = new ArrayList();
	}

	/// <summary>
	/// Link a guid in hex format with a string query.
	/// </summary>
	public class ActiveSearch
	{
		//an arraylist of ActiveSearch instances
		public static ArrayList searches = new ArrayList();

		public string query;
		public string guid;
	}

	/// <summary>
	/// Some routines for analyzing QueryHitObjects.
	/// </summary>
	public class QHOStuff
	{
		/// <summary>
		/// Find the mp3 info area in extensions.
		/// </summary>
		public static string GetMp3Info(QueryHitObject qhObj)
		{
			try
			{
				if(qhObj.extensions == null)
					return "";
				//find kbps entry
				for(int y = 0; y < qhObj.extensions.Length; y++)
					if(qhObj.extensions[y].ToLower().IndexOf("kbps") != -1)
						return qhObj.extensions[y];
				return "";
			}
			catch(Exception exc)
			{
				System.Diagnostics.Debug.WriteLine("GetMp3Info: " + exc.Message);
				return "";
			}
		}

		/// <summary>
		/// Find the urn hash value in extensions.
		/// </summary>
		public static string GetSha1Ext(QueryHitObject qhObj)
		{
			try
			{
				if(qhObj.extensions == null)
					return "";
				//find sha1
				for(int y = 0; y < qhObj.extensions.Length; y++)
					if(qhObj.extensions[y].ToLower().IndexOf("sha1") != -1)
						return qhObj.extensions[y];
				return "";
			}
			catch(Exception exc)
			{
				System.Diagnostics.Debug.WriteLine("GetSha1Ext: " + exc.Message);
				return "";
			}
		}

		/// <summary>
		/// Return the type of file this is (ex. audio, video, etc.).
		/// </summary>
		public static string GetType(QueryHitObject qhObj)
		{
			return GetType(qhObj.fileName);
		}

		/// <summary>
		/// Return the type of file this is (ex. audio, video, etc.).
		/// </summary>
		public static string GetType(string filename)
		{
			try
			{
				string ext = System.IO.Path.GetExtension(filename);
				if(ext.Length <= 1)
					return "Unknown";
				ext = ext.Substring(1);
				return FileType.GetType(ext);
			}
			catch(Exception exc)
			{
				System.Diagnostics.Debug.WriteLine("GetType: " + exc.Message + "\n" + filename);
				return "Unknown";
			}
		}

		/// <summary>
		/// Basically returns the extension.
		/// </summary>
		public static string GetFormat(string filename)
		{
			try
			{
				string ext = System.IO.Path.GetExtension(filename);
				if(ext.Length <= 1)
					return "";
				ext = ext.Substring(1);
				return ext;
			}
			catch(Exception exc)
			{
				System.Diagnostics.Debug.WriteLine("GetFormat: " + exc.Message);
				return "";
			}
		}

		/// <summary>
		/// Checks to see whether two QueryHitObjects are similar enough.
		/// All similar query hits can be used in multi-source (swarm) downloading.
		/// 
		/// All similar query hits share these properties:
		/// 
		/// 1. same exact file size
		/// 2. same exact file extension
		/// 3. similar file name or same hash verification
		/// </summary>
		public static bool Matching(QueryHitObject qhObj1, QueryHitTable qhtObj2)
		{
			try
			{
				//set qhObj2 to the initial QueryHitObject in qhtObj2
				QueryHitObject qhObj2 = (QueryHitObject)qhtObj2.queryHitObjects[0];

				//if file sizes are different, then forget about it
				if(qhObj1.fileSize != qhObj2.fileSize)
					return false;

				//extract file extensions and file names
				string f1, f2, f1ext, f2ext;
				int pos1 = qhObj1.fileName.LastIndexOf(".");
				if(pos1 == -1)
				{
					f1 = qhObj1.fileName;
					f1ext = "";
				}
				else
				{
					f1 = qhObj1.fileName.Substring(0, pos1);
					f1ext = qhObj1.fileName.Substring(pos1+1, qhObj1.fileName.Length-pos1-1);
				}
				int pos2 = qhObj2.fileName.LastIndexOf(".");
				if(pos2 == -1)
				{
					f2 = qhObj2.fileName;
					f2ext = "";
				}
				else
				{
					f2 = qhObj2.fileName.Substring(0, pos2);
					f2ext = qhObj2.fileName.Substring(pos2+1, qhObj2.fileName.Length-pos2-1);
				}

				//make sure file extensions match
				if(f1ext != f2ext)
					return false;

				//if this table is based on hash, we only add query hits with the same hash
				if(qhtObj2.md4sum != "")
				{
					if(qhtObj2.md4sum == Utils.HexGuid(qhObj1.md4sum))
						return true;
					else
						return false;
				}
				else if(qhtObj2.sha1 != "")
				{
					if(GetSha1Ext(qhObj1) == qhtObj2.sha1)
					{
Utils.Diag("this is it guys: " + qhObj1.fileName);
						return true;
					}
					else
						return false;
				}
				else
				{
					//if it has its own hash, forget about it
					if(GetSha1Ext(qhObj1) != "")
						return false;

					//make sure filenames are similar enough
					//this is a filter of characters we can ignore
					string[] filter = new string[]
					{
						"-", "_", "(", ")", "&", "0", "1", "2", "3",
						"4", "5", "6", "7", "8", "9", ",", ".", "[",
						"]", "{", "}", "?", " "
					};
					f1 = f1.ToLower();
					f2 = f2.ToLower();
					//take out ignored characters
					for(int x = 0; x < filter.Length; x++)
					{
						f1 = f1.Replace(filter[x], "");
						f2 = f2.Replace(filter[x], "");
					}
					if(f1.Length < 15 || f2.Length < 15)
					{
						if(f1 != f2)
							return false;
						else
							return true;
					}
					if(f1.Substring(0, 15) != f2.Substring(0, 15))
						return false;

					//everything is perfect
					return true;
				}
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("QHOStuff Matching: " + e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
				return false;
			}
		}

		/// <summary>
		/// Just compare filenames for similarity.
		/// </summary>
		public static bool Match2(string filename1, string filename2)
		{
			try
			{
				//extract file extensions and file names
				string f1, f2, f1ext, f2ext;
				int pos1 = filename1.LastIndexOf(".");
				if(pos1 == -1)
				{
					f1 = filename1;
					f1ext = "";
				}
				else
				{
					f1 = filename1.Substring(0, pos1);
					f1ext = filename1.Substring(pos1+1, filename1.Length-pos1-1);
				}
				int pos2 = filename2.LastIndexOf(".");
				if(pos2 == -1)
				{
					f2 = filename2;
					f2ext = "";
				}
				else
				{
					f2 = filename2.Substring(0, pos2);
					f2ext = filename2.Substring(pos2+1, filename2.Length-pos2-1);
				}

				//make sure file extensions match
				if(f1ext != f2ext)
					return false;

				//make sure filenames are similar enough
				//this is a filter of characters we can ignore
				string[] filter = new string[]
				{
					"-", "_", "(", ")", "&", "0", "1", "2", "3",
					"4", "5", "6", "7", "8", "9", ",", ".", "[",
					"]", "{", "}", "?", " "
				};
				f1 = f1.ToLower();
				f2 = f2.ToLower();
				//take out ignored characters
				for(int x = 0; x < filter.Length; x++)
				{
					f1 = f1.Replace(filter[x], "");
					f2 = f2.Replace(filter[x], "");
				}
				if(f1.Length < 32 || f2.Length < 32)
				{
					if(f1 != f2)
						return false;
					else
						return true;
				}
				if(f1.Substring(0, 32) != f2.Substring(0,32))
					return false;

				//everything looks good
				return true;
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Match2: " + e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
				return false;				
			}
		}
	}
}
