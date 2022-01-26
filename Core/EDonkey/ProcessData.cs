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

namespace FileScope.EDonkey
{
	class ProcessData
	{
		public static void Check1(int sockNum)
		{
			int dmNum = Sck.scks[sockNum].dmNum;
			int dlNum = Sck.scks[sockNum].dlNum;
			if(dlNum == -1)
				return;
			if(DownloadManager.dms[dmNum] != null)
				if(DownloadManager.dms[dmNum].active)
					if(DownloadManager.dms[dmNum].downloaders[dlNum] != null)
						if(((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).edsck != null)
							if(((Downloader)DownloadManager.dms[dmNum].downloaders[dlNum]).edsck != Sck.scks[sockNum])
								System.Diagnostics.Debug.WriteLine("big ed2k problem");
		}

		/// <summary>
		/// Process whatever eDonkey data we just received.
		/// </summary>
		public static void NewData(Packet pckt, int sockNum)
		{
//Check1(sockNum);
			switch(pckt.payload[0])
			{
				case 0x40://id change
					Messages.IDChange(pckt, sockNum);
					break;
				case 0x38://server message
					Messages.ServerMessage(pckt, sockNum);
					break;
				case 0x32://server list
					Messages.ServerList(pckt, sockNum);
					break;
				case 0x41://server info data
					Messages.ServerInfoData(pckt, sockNum);
					break;
				case 0x34://server status
					Messages.ServerStatus(pckt, sockNum);
					break;
				case 0x33://search results
					Messages.SearchResults(pckt, sockNum);
					break;
				case 0x42://found sources
					Messages.FoundSources(pckt.payload, 1, sockNum);
					break;
				case 0x35://callback request
					Messages.CallbackRequest(pckt, sockNum);
					break;
				case 0x36://callback failed
					Messages.CallbackRequestFailed(pckt, sockNum);
					break;
				case 0x01://client hello
					Messages.ClientHello(pckt, sockNum);
					break;
				case 0x4C://client hello response
					Messages.ClientHelloResponse(pckt, sockNum);
					break;
				case 0x58://file request
					Messages.FileRequest(pckt, sockNum);
					break;
				case 0x59://file request answer good
					Messages.FileRequestAnswer(pckt, sockNum, true);
					break;
				case 0x48://file request answer no such file
					Messages.FileRequestAnswer(pckt, sockNum, false);
					break;
				case 0x4F://file status request
					Messages.FileStatusRequest(pckt, sockNum);
					break;
				case 0x50://file status
					Messages.FileStatusResponse(pckt, sockNum);
					break;
				case 0x51://hash set request
					Messages.HashSetRequest(pckt, sockNum);
					break;
				case 0x52://hash set response
					Messages.HashSetResponse(pckt, sockNum);
					break;
				case 0x54://slot request
					Messages.SlotRequest(pckt, sockNum);
					break;
				case 0x55://slot given
					Messages.SlotResponse(pckt, sockNum);
					break;
				case 0x57://slot taken
					break;
				case 0x56://slot release
					Messages.SlotRelease(pckt, sockNum);
					break;
				case 0x46://sending part
					Messages.SendPartFinished(sockNum);
					break;
				case 0x47://request parts
					Messages.RequestParts(pckt, sockNum);
					break;
				case 0x49://end of download
					Messages.EndOfDownload(pckt, sockNum);
					break;
				case 0x4A://view files
					Messages.ViewFiles(sockNum);
					break;
				default:
					System.Diagnostics.Debug.WriteLine("Unknown eDonkey message: " + pckt.payload[0].ToString());
					break;
			}
//Check1(sockNum);
		}
	}
}
