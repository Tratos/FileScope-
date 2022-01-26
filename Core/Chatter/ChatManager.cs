// ChatManager.cs
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
	/// Manage active chats.
	/// </summary>
	public class ChatManager
	{
		//80 active chats allowed
		public static Chat[] chats = new Chat[80];

		/// <summary>
		/// Find an inactive chat object.
		/// </summary>
		public static int GetChat()
		{
			try
			{
				for(int x = 0; x < chats.Length; x++)
				{
					if(chats[x] == null)
					{
						//System.Diagnostics.Debug.WriteLine("GetChat: " + x.ToString());
						chats[x] = new Chat(x);
						return x;
					}
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("GetChat error");
			}
			return -1;
		}

		public static void DisconnectAll()
		{
			try
			{
				for(int x = 0; x < chats.Length; x++)
				{
					if(chats[x] == null)
						continue;
					if(chats[x].state != ChatState.Closed)
						chats[x].Disconnect();
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("ChatManager DisconnectAll");
			}
		}

		/// <summary>
		/// Spawn an outgoing chat request to a peer.
		/// </summary>
		public static void Outgoing(ref string peer)
		{
			int chatNum = GetChat();
			if(chatNum == -1)
				return;

			chats[chatNum].Outgoing(ref peer);
		}

		/// <summary>
		/// Accept an incoming chat.
		/// </summary>
		public static void Incoming(Socket elSock)
		{
			int chatNum = GetChat();
			if(chatNum == -1 || !Stats.settings.allowChats)
			{
				try
				{
					if(elSock != null)
					{
						if(elSock.Connected)
							elSock.Shutdown(SocketShutdown.Both);
						elSock.Close();
					}
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("ChatManager Incoming");
				}
				return;
			}
			chats[chatNum].Incoming(elSock);
		}

		public delegate void nullifyMe(int chatNum);

		public static void NullifyMe(int chatNum)
		{
			if(chats[chatNum] != null)
			{
				chats[chatNum].CleanUp();
				chats[chatNum] = null;
			}
		}
	}
}
