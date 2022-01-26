// ChatHandShake.cs
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
	/// Class used for crafting handshakes.
	/// </summary>
	public class ChatHandShake
	{
		public static void SendHandshake(int chatNum)
		{
			//craft handshake
			string hndshk = "";
			hndshk += "CHAT CONNECT/0.1\r\n";
			hndshk += "User-Agent: FileScope " + Stats.version + "\r\n";
			hndshk += "\r\n";

			//send handshake
			ChatManager.chats[chatNum].SendString(ref hndshk);
		}

		public static void SendResponse(int chatNum)
		{
			string hndshk = "";
			hndshk += "CHAT/0.1 200 OK\r\n";
			hndshk += "User-Agent: FileScope " + Stats.version + "\r\n";
			hndshk += "\r\n";

			//send response
			ChatManager.chats[chatNum].SendString(ref hndshk);
		}

		public static void SendFinalResponse(int chatNum)
		{
			string hndshk = "CHAT/0.1 200 OK\r\n\r\n";

			//send response
			ChatManager.chats[chatNum].SendString(ref hndshk);
		}
	}
}
