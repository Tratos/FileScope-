// ProcessData.cs
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

namespace FileScope.OpenNap
{
	public class ProcessData
	{
		/// <summary>
		/// Whenever we get any new data, we have to process it.
		/// That's what this function is for.
		/// </summary>
		public static void NewData(int sockNum)
		{
			//move the entire buffer into 'packets' byte array
			byte[] packets;
			lock(Sck.scks[sockNum].buf)
			{
				packets = new byte[Sck.scks[sockNum].buf.Count];
				Sck.scks[sockNum].buf.CopyTo(packets);
				Sck.scks[sockNum].buf.Clear();
			}

			//this shouldn't happen, but we check anyway
			if(packets.Length == 0)
				return;

			/*
			 * it's time to start processing packets
			 * the 'packets' byte array stores one or more packets
			 * the last packet in the byte array could be fragmented
			 * if that's the case, we insert it back into Sck.scks[sockNum].buf
			 * buffIndex stores the location in the 'packets' array where we're processing
			 */
			int buffIndex = 0;

			//begin
			for(;;)
			{
				if(packets.Length - buffIndex == 0)
					//there is nothing left to process
					break;

				//process the first packet
				Packet packet = new Packet(packets, ref buffIndex);

				//check up on the status of this packet
				if(packet.type == 0)
				//good packet
				{
					//what message is this?
					switch(packet.cmd)
					{
						case 0://error on login
							Messages.Error(packet, sockNum);
							break;
						case 3://login ack
							Messages.LoginAck(packet, sockNum);
							break;
						case 201://search response
							Messages.SearchResponse(packet, sockNum);
							break;
						case 621://message of the day
							Messages.MOD(packet, sockNum);
							break;
						case 204://download ack
							Messages.DownloadAck(packet, sockNum);
							break;
						case 214://server stats response
							Messages.ServerStatsResponse(packet, sockNum);
							break;
						case 216://resume search response
							Messages.ResumeRequestResponse(packet, sockNum);
							break;
						case 404://error message
							Messages.Error404(packet, sockNum);
							break;
						case 607://upload request
							Messages.UploadRequest(packet, sockNum);
							break;
						case 620://queue limit reached
							Messages.QueueLimit(packet, sockNum);
							break;
						default://unknown
							System.Diagnostics.Debug.WriteLine("Unknown message: "
								+ packet.cmd.ToString()
								+ " : " + packet.payload);
							break;
					}
				}
				else if(packet.type == 1)
				//fragment packet; we need to receive the rest of it
				{
					byte[] frag = new byte[packets.Length - buffIndex];
					Array.Copy(packets, buffIndex, frag, 0, frag.Length);
					//put the fragment back into that buffer
					lock(Sck.scks[sockNum].buf)
						Sck.scks[sockNum].buf.InsertRange(0, frag);
					break;
				}
				else if(packet.type == 2)
				//corrupt packet
				{
					System.Diagnostics.Debug.WriteLine("opennap corrupto packeto");
					Sck.scks[sockNum].Disconnect();
					break;
				}
			}
		}
	}
}
