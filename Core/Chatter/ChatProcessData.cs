// ChatProcessData.cs
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
	/// Used for processing data we just received from our peer.
	/// All messages end with a '\n'.
	/// </summary>
	public class ChatProcessData
	{
		/// <summary>
		/// Take care of the new data we received.
		/// Put incomplete data back into chats[chatNum].buf.
		/// </summary>
		public static void NewData(int chatNum)
		{
			byte[] msgs;
			lock(ChatManager.chats[chatNum].buf)
			{
				msgs = new byte[ChatManager.chats[chatNum].buf.Count];
				ChatManager.chats[chatNum].buf.CopyTo(msgs);
				ChatManager.chats[chatNum].buf.Clear();
			}
			string strMsgs = Encoding.ASCII.GetString(msgs);

			for(;;)
			{
				if(strMsgs.Length == 0)
					return;

				//are we waiting for a handshake or a chat message
				if(ChatManager.chats[chatNum].state == ChatState.Connected)
				{
					if(strMsgs.IndexOf("\r\n\r\n") == -1)
					{
						//we didn't finish getting the handshake
						lock(ChatManager.chats[chatNum].buf)
							ChatManager.chats[chatNum].buf.InsertRange(0, msgs);
						return;
					}
					else
					{
						if(strMsgs.ToLower().IndexOf("ok") != -1)
						{
							if(ChatManager.chats[chatNum].incoming)
							{
								GUIBridge.NewChat(chatNum);
								//we just received the rest of the handshake
								ChatManager.chats[chatNum].state = ChatState.HndShk;
								ChatManager.chats[chatNum].connectYet.Stop();
								return;
							}
							else
							{
								//we send the last part of the handshake
								ChatHandShake.SendFinalResponse(chatNum);
								ChatManager.chats[chatNum].state = ChatState.HndShk;
								ChatManager.chats[chatNum].connectYet.Stop();
								//notify gui that the handshake is done
								GUIBridge.ConnectedChat(chatNum);
								return;
							}
						}
						else
							return;
					}
				}
				else
				{
					if(strMsgs.IndexOf("\n") == -1)
					{
						//we didn't get the rest of the chat message
						msgs = Encoding.ASCII.GetBytes(strMsgs);
						lock(ChatManager.chats[chatNum].buf)
							ChatManager.chats[chatNum].buf.InsertRange(0, msgs);
						return;
					}
					else
					{
						//get the chat message
						string msg = strMsgs.Substring(0, strMsgs.IndexOf("\n"));
						if(msg[msg.Length-1] == '\r')
							msg = msg.Substring(0, msg.Length-1);
						GUIBridge.NewChatData(chatNum, msg);

						int loc = strMsgs.IndexOf("\n") + 1;
						strMsgs = strMsgs.Substring(loc, strMsgs.Length - loc);
					}
				}
			}
		}
	}
}
