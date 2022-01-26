// UploadManager.cs
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
using System.Net.Sockets;

namespace FileScope
{
	/// <summary>
	/// Manage uploads.
	/// </summary>
	public class UploadManager
	{
		public static Uploader[] ups = new Uploader[100];

		/// <summary>
		/// Get a free Uploader instance.
		/// </summary>
		public static int GetUp()
		{
			try
			{
				for(int x = 0; x < ups.Length; x++)
				{
					if(ups[x] == null)
					{
						ups[x] = new Uploader();
						//Utils.Diag("Uploader " + x.ToString() + " available");
						return x;
					}
				//	if(!ups[x].active)
				//	{
				//		ups[x] = new Uploader();
				//		return x;
				//	}
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("GetUp error");
			}
			return -1;
		}

		/// <summary>
		/// Start a brand new upload.
		/// </summary>
		public static void Incoming(Socket tmpSock, ref string elMsg)
		{
			int elUp = GetUp();
			if(elUp == -1)
				return;

			ups[elUp].Incoming(elUp, tmpSock, ref elMsg);
		}

		public static void IncomingEdSck(byte[] md4hash, uint start, uint stop, int edsckNum)
		{
			int elUp = GetUp();
			if(elUp == -1)
				return;

			ups[elUp].IncomingEdSck(elUp, md4hash, start, stop, edsckNum);
		}

		/// <summary>
		/// Start a brand new pushed upload.
		/// </summary>
		public static void Outgoing(int fileIndex, string ip, int port)
		{
			//check for this fileIndex; -5 means G2 upload
			if(fileIndex >= Stats.fileList.Count || (fileIndex < 0 && fileIndex != -5))
				return;

			//check to make sure we're not already dealing with this guy and this file
			for(int x = 0; x < ups.Length; x++)
				if(ups[x] != null)
					if(ups[x].active)
						if(ups[x].RemoteIP().IndexOf(ip) != -1 && ups[x].fileIndex == fileIndex)
							return;

			int elUp = GetUp();
			if(elUp == -1)
				return;

			ups[elUp].Outgoing(elUp, fileIndex, ip, port);
		}

		public static void DisconnectAll()
		{
			for(int x = 0; x < ups.Length; x++)
			{
				if(ups[x] == null)
					continue;
				if(ups[x].active)
					ups[x].StopEverything("UploadManager DisconnectAll");
			}
		}

		public delegate void nullifyMe(int upNum);

		/// <summary>
		/// Set a non-active Uploader to null so that it can be collected.
		/// </summary>
		public static void NullifyMe(int upNum)
		{
			if(ups[upNum] != null)
			{
				try
				{if(ups[upNum].sendThread != null) if(ups[upNum].sendThread.IsAlive) ups[upNum].sendThread.Abort();}
				catch
				{System.Diagnostics.Debug.WriteLine("UploadManager NullifyMe");}

				ups[upNum] = null;
				GC.Collect();
			}
		}
	}
}
