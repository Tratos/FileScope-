// Handshake.cs
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
using System.IO;
using System.Text.RegularExpressions;

namespace FileScope.Gnutella2
{
	/// <summary>
	/// Take care of G2 handshakes.
	/// </summary>
	public class Handshake
	{
		static bool ipSet = false;

		public static void Incoming(Sck sck, HandshakeMsg msg)
		{
			//System.Diagnostics.Debug.WriteLine(msg.handshake);
			bool isG1 = false;
			//check if valid handshake
			if(msg.handshake.Substring(0, 8).ToLower() != "gnutella" || msg.handshake.ToLower().IndexOf("x-gnutella2") == -1)
			{
				sck.Disconnect("invalid handshake or G1 only");
				//System.Diagnostics.Debug.WriteLine("bad g2 handshake: " + msg.handshake);
				isG1 = true;
			}
			StringReader sr = new StringReader(msg.handshake);
			string first = sr.ReadLine().ToLower();
			bool is503 = false;
			if(first.IndexOf("gnutella connect") == -1 && first.IndexOf("200 ok") == -1)
			{
				if(first.IndexOf("503") == -1)
					System.Diagnostics.Debug.WriteLine("strange G2 first line handshake: " + first);
				is503 = true;
			}
			//parse piece by piece
			while(true)
			{
				string line = sr.ReadLine();
				if(line.Length == 0)
					break;
				string lineLower = line.ToLower();
				if(lineLower.IndexOf("listen-ip:") != -1)
				{
					if(sck.incoming)
					{
						int portLoc = line.LastIndexOf(":");
						if(portLoc != -1)
						{
							portLoc++;
							sck.port = Convert.ToInt32(line.Substring(portLoc, line.Length-portLoc).Trim());
						}
					}
				}
				else if(lineLower.IndexOf("remote-ip:") != -1)
				{
					if(!Handshake.ipSet)
					{
						string ip = GetValue(line);
						string patternIP = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$";
						Match matchIP = Regex.Match(ip, patternIP);
						if(matchIP.Success)
							if(!IPfilter.Private2(matchIP.ToString()))
							{
								Stats.settings.ipAddress = matchIP.ToString();
								Stats.Updated.myIPA = System.Net.IPAddress.Parse(Stats.settings.ipAddress);
								Handshake.ipSet = true;
							}
					}
				}
				else if(lineLower.IndexOf("user-agent:") != -1)
					sck.vendor = GetValue(line);
				else if(lineLower.IndexOf("application/x-gnutella2") != -1)
					line = "";
				else if(lineLower.IndexOf("x-try-ultrapeers:") != -1)
				{
					if(!isG1)
					{
						string[] ips = line.Substring(17, line.Length-17).Split(',');
						for(int str = 0; str < ips.Length; str++)
						{
							if(ips[str].Length > 4 && ips[str][0] == ' ')
								ips[str] = ips[str].Substring(1, ips[str].Length-1);
							if(ips[str].IndexOf(' ') != -1)
								ips[str] = ips[str].Substring(0, ips[str].IndexOf(' '));
							string nhAddr;
							int nhPort;
							Utils.AddrParse(ips[str], out nhAddr, out nhPort, 6346);
							HubInfo hi = new HubInfo();
							hi.port = (ushort)nhPort;
							hi.timeKnown = 0;
							HostCache.AddRecentAndCache(System.Net.IPAddress.Parse(nhAddr), hi);
						}
					}
				}
				else if(lineLower.IndexOf("x-ultrapeer-loaded:") != -1)
					line = "";
				else if(lineLower.IndexOf("ggep:") != -1)
					line = "";
				else if(lineLower.IndexOf("x-query-routing:") != -1)
					line = "";
				else if(lineLower.IndexOf("pong-caching:") != -1)
					line = "";
				else if(lineLower.IndexOf("accept-encoding: deflate") != -1)
				{
					//specs say that leaf to hub compression isn't worth it
					if(Stats.Updated.Gnutella2.ultrapeer)
						if(GUID.rand.Next(0, 3) == 0)
							if(Stats.Updated.Gnutella2.enableDeflate)
								sck.deflateOut = true;
				}
				else if(lineLower.IndexOf("content-encoding: deflate") != -1)
					sck.deflateIn = true;
				else if(lineLower.IndexOf("x-ultrapeer:") != -1)
				{
					string boolRes = GetValue(lineLower);
					if(boolRes == "true")
						sck.mode = G2Mode.Ultrapeer;
					else
						sck.mode = G2Mode.Leaf;
				}
				else if(lineLower.IndexOf("x-ultrapeer-needed:") != -1)
				{
					string boolRes = GetValue(lineLower);
					if(boolRes == "true")
						sck.ultrapeerNeeded = true;
					else
						sck.ultrapeerNeeded = false;
				}
#if	DEBUG
				else if(!isG1)
					System.Diagnostics.Debug.WriteLine("G2 new handshake line: " + line);
#endif
			}
			if(is503 || isG1)
			{
				sck.Disconnect("server rejected");
				return;
			}
			//here a lock on sck will have a nested lock on recentHubs
			lock(sck)
			{
				IncrementHndShkState(sck);
				//see if we have to send a response
				if(sck.state != Condition.hndshk3)
				{
					bool busy = false;

					if(sck.state == Condition.hndshk)
					{
						if(!Stats.Updated.Gnutella2.ultrapeer)
						{
							sck.Disconnect("we're a leaf");
							return;
						}
						if(sck.mode == G2Mode.Ultrapeer && ConnectionManager.ultrapeers.Count >= ConnectionManager.numHubsWhenHubMax)
							busy = true;
						if(sck.mode == G2Mode.Leaf && ConnectionManager.leaves.Count >= Stats.settings.gConnectionsToKeep)
							busy = true;
					}
					if(sck.state == Condition.hndshk2 && Stats.Updated.Gnutella2.ultrapeer && !sck.ultrapeerNeeded)
					{
						if(ConnectionManager.leaves.Count >= 5)
						{
							sck.Disconnect("ultrapeer not needed");
							return;
						}
						else
							ConnectionManager.SwitchMode(sck.sockNum);
					}

					OMessage pckt = Outgoing(sck, busy);
					//allow this one more packet to be sent without using NetworkStream
					sck.allow1withoutStream = true;
					sck.SendPacket(pckt);
					if(busy)
					{
						System.Threading.Thread.Sleep(700);
						sck.Disconnect("busy");
						return;
					}
				}
			}
			if(sck.state == Condition.hndshk3)
				sck.JustFinishedHandshake();
		}

		public static OMessage Outgoing(Sck sck, bool busy)
		{
			//remote ip
			string remoteIP = sck.RemoteIP();

			OHandshake hm = new OHandshake();
			if(sck.state == Condition.Connected)
			{
				//we're initiating the handshake
				hm.handshake += "GNUTELLA CONNECT/0.6\r\n";
				hm.handshake += "Listen-IP: " + Stats.settings.ipAddress + ":" + Stats.settings.port.ToString() + "\r\n";
				hm.handshake += (remoteIP == "") ? "" : "Remote-IP: " + remoteIP + "\r\n";
				hm.handshake += "User-Agent: FileScope " + Stats.version + "\r\n";
				hm.handshake += "Accept: application/x-gnutella2\r\n";
				if(GUID.rand.Next(0, 3) == 0)
					if(Stats.Updated.Gnutella2.enableDeflate)
						hm.handshake += "Accept-Encoding: deflate\r\n";
				hm.handshake += "X-Ultrapeer: " + ((Stats.Updated.Gnutella2.ultrapeer == true) ? "True\r\n" : "False\r\n");
				hm.handshake += "\r\n";
			}
			else if(sck.state == Condition.hndshk)
			{
				//we're responding to the incoming handshake
				if(!busy)
				{
					hm.handshake += "GNUTELLA/0.6 200 OK\r\n";
					hm.handshake += "Listen-IP: " + Stats.settings.ipAddress + ":" + Stats.settings.port.ToString() + "\r\n";
					hm.handshake += (remoteIP == "") ? "" : "Remote-IP: " + remoteIP + "\r\n";
					hm.handshake += "User-Agent: FileScope " + Stats.version + "\r\n";
					hm.handshake += "Content-Type: application/x-gnutella2\r\n";
					hm.handshake += "Accept: application/x-gnutella2\r\n";
					if(GUID.rand.Next(0, 3) == 0)
						if(Stats.Updated.Gnutella2.enableDeflate)
							hm.handshake += "Accept-Encoding: deflate\r\n";
					if(sck.deflateOut)
						hm.handshake += "Content-Encoding: deflate\r\n";
					hm.handshake += "X-Ultrapeer: " + ((Stats.Updated.Gnutella2.ultrapeer == true) ? "True\r\n" : "False\r\n");
					hm.handshake += "X-Ultrapeer-Needed: " + ((ConnectionManager.ultrapeers.Count < ConnectionManager.numHubsWhenHubMax) ? "True\r\n" : "False\r\n");
					hm.handshake += GetXTryUltrapeers();
					hm.handshake += "\r\n";
				}
				else
				{
					hm.handshake += "GNUTELLA/0.6 503 Busy\r\n";
					hm.handshake += "Listen-IP: " + Stats.settings.ipAddress + ":" + Stats.settings.port.ToString() + "\r\n";
					hm.handshake += (remoteIP == "") ? "" : "Remote-IP: " + remoteIP + "\r\n";
					hm.handshake += "User-Agent: FileScope " + Stats.version + "\r\n";
					hm.handshake += "Content-Type: application/x-gnutella2\r\n";
					hm.handshake += "Accept: application/x-gnutella2\r\n";
					hm.handshake += "X-Ultrapeer: " + ((Stats.Updated.Gnutella2.ultrapeer == true) ? "True\r\n" : "False\r\n");
					hm.handshake += GetXTryUltrapeers();
					hm.handshake += "\r\n";
				}
			}
			else
			{
				//we're sending the last part of the handshake
				hm.handshake += "GNUTELLA/0.6 200 OK\r\n";
				hm.handshake += "Content-Type: application/x-gnutella2\r\n";
				if(sck.deflateOut)
					hm.handshake += "Content-Encoding: deflate\r\n";
				hm.handshake += "X-Ultrapeer: " + ((Stats.Updated.Gnutella2.ultrapeer == true) ? "True\r\n" : "False\r\n");
				hm.handshake += "\r\n";
			}
			//System.Diagnostics.Debug.WriteLine(hm.handshake);
			IncrementHndShkState(sck);
			return hm;
		}

		static string GetXTryUltrapeers()
		{
			lock(HostCache.recentHubs)
			{
				if(HostCache.recentHubs.Count < 20)
					return "";
				else
				{
					string xtrys = "X-Try-Ultrapeers: ";
					int chCount = 20;
					int pos = GUID.rand.Next(0, HostCache.recentHubs.Count);
					while(true)
					{
						xtrys += ((System.Net.IPAddress)HostCache.recentHubs.GetKey(pos)).ToString() + ":" + ((HubInfo)HostCache.recentHubs.GetByIndex(pos)).port.ToString();
						pos++;
						if(pos == HostCache.recentHubs.Count)
							pos = 0;
						chCount--;
						if(chCount > 0)
							xtrys += ",";
						else
							break;
					}
					xtrys += "\r\n";
					return xtrys;
				}
			}
		}

		/// <summary>
		/// Get the string value after the colon in a line.
		/// </summary>
		static string GetValue(string line)
		{
			int startIndex = line.IndexOf(":") + 1;
			return line.Substring(startIndex, line.Length - startIndex).TrimStart(new char[]{' '});
		}

		static void IncrementHndShkState(Sck sck)
		{
			switch(sck.state)
			{
				case Condition.Connected:
					sck.state = Condition.hndshk;
					break;
				case Condition.hndshk:
					sck.state = Condition.hndshk2;
					break;
				case Condition.hndshk2:
					sck.state = Condition.hndshk3;
					break;
				default:
					throw new Exception("G2 IncrementHndShkState " + sck.sockNum.ToString());
			}
		}
	}
}
