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

namespace FileScope.Gnutella
{
	/// <summary>
	/// Gnutella handshake.
	/// We connect to 0.6 clients and above.
	/// We accept 0.4 and 0.6 and above clients.
	/// </summary>
	public class Handshake
	{
		//handshake start strings
		string ourVer;
		const string oldVer = "GNUTELLA CONNECT/0.4\n\n";

		//responses
		string resp2;
		const string resp3 = "GNUTELLA/0.6 200 OK\r\n\r\n";
		const string respOld = "GNUTELLA OK\n\n";
		string respOverloaded;
		string leafNode;

		int sockIndex; //hold the index of the Sck class we're working with
		string tempHnd;//hold the handshake we received for reference

		//leaf guidance?
		bool ultrapeerNeeded = true;

		/// <summary>
		/// Start the handshake.
		/// </summary>
		public Handshake(int sockNum)
		{
			SetupHandshakes();
			sockIndex = sockNum;
			//we're at Handshake phase one
			Sck.scks[sockNum].state = Condition.hndshk;
		}

		/// <summary>
		/// Send the handshake.
		/// </summary>
		public void Start()
		{
			//send our handshake
			Message pckt = new Message(System.Text.Encoding.ASCII.GetBytes(ourVer));
			Sck.scks[sockIndex].SendPacket(pckt);
			//wait for that response
			Sck.scks[sockIndex].responseYet.Start();
		}

		/// <summary>
		/// Respond to the handshake.
		/// </summary>
		public Handshake(string hndshk, int sockNum)
		{
			this.tempHnd = hndshk;
			sockIndex = sockNum;
			if(!IsHandshake())
				return;
			ParseInfo(ref hndshk);
			SetupHandshakes();
			if(hndshk.IndexOf("OK") != -1)
			//this is either the second or final handshake stage
			{
				if(Sck.scks[sockNum].state == Condition.hndshk)
				{
					//this is a response to your initial handshake (outgoing)
					Sck.scks[sockNum].state = Condition.hndshk2;
					Sck.scks[sockNum].responseYet.Stop();
				}
				else
				{
					if(hndshk.ToLower().IndexOf("x-ultrapeer: false") != -1)
					{
						//System.Diagnostics.Debug.WriteLine(sockNum.ToString() + " switched to Leaf");
						Sck.scks[sockNum].ultrapeer = false;
					}
					//this is the final handshake (incoming)
					Sck.scks[sockNum].state = Condition.hndshk3;
					Sck.scks[sockNum].inResponseYet2.Stop();
				}
			}
			else
			{
				//check to make sure we're not overloaded
				if(Stats.Updated.Gnutella.lastConnectionCount >= Stats.settings.gConnectionsToKeep)
				{
					Message pcket = new Message(System.Text.Encoding.ASCII.GetBytes(respOverloaded));
					Sck.scks[sockNum].SendPacket(pcket);
					Sck.scks[sockNum].inResponseYet1.Interval = 100;
					//Sck.scks[sockNum].inResponseYet1 will fire and finish the connection
					return;
				}

				//check to make sure we're not a leaf node
				if(!Stats.Updated.Gnutella.ultrapeer)
				{
					Message pcket = new Message(System.Text.Encoding.ASCII.GetBytes(leafNode));
					Sck.scks[sockNum].SendPacket(pcket);
					Sck.scks[sockNum].inResponseYet1.Interval = 100;
					//Sck.scks[sockNum].inResponseYet1 will fire and finish the connection
					return;
				}
				//check to make sure we can accept any more peers or ultrapeers
				else if(!Sck.scks[sockNum].newHost || Sck.scks[sockNum].ultrapeer)
				{
					int numPeersUltrapeers = 0;
					foreach(Sck d in Sck.scks)
						if(d != null)
							if(d.state == Condition.hndshk3)
								if(d.ultrapeer || !d.newHost)
									numPeersUltrapeers++;
					if(numPeersUltrapeers >= ConnectionManager.numNonLeafs)
					{
						Message pcket = new Message(System.Text.Encoding.ASCII.GetBytes(respOverloaded));
						Sck.scks[sockNum].SendPacket(pcket);
						Sck.scks[sockNum].inResponseYet1.Interval = 100;
						//Sck.scks[sockNum].inResponseYet1 will fire and finish the connection
						return;
					}
					else
					{
						//we need more ultrapeers
						this.ultrapeerNeeded = true;
						//setup handshakes again
						SetupHandshakes();
					}
				}

				//this guy must be requesting a connection
				//we must turn off that timer waiting for incoming handshake
				Sck.scks[sockNum].inResponseYet1.Stop();
				Sck.scks[sockNum].state = Condition.hndshk;

				bool recog = false; //if we recognize the version
				if(hndshk.IndexOf("GNUTELLA CONNECT/0.6\r\n") != -1)
				{
					Sck.scks[sockNum].protocolVer = 6;
					recog = true;
				}
				if(hndshk.IndexOf(oldVer) != -1)
				{
					Sck.scks[sockNum].protocolVer = 4;
					recog = true;
				}
				//if we don't recognize the version
				if(recog == false)
					//respond with our version; his/her version probably 0.7
					Sck.scks[sockNum].protocolVer = 6;
			}
		}

		/// <summary>
		/// Determine our response.
		/// </summary>
		public void Respond()
		{
			//check if handshake
			if(!IsHandshake())
				return;
			//make sure we accepted the initial handshake
			if(Sck.scks[sockIndex].state == Condition.Connected)
				return;
			//make sure it's not finished; hndshk3 means it's all done
			if(Sck.scks[sockIndex].state == Condition.hndshk3)
			{
				//it's over
				Sck.scks[sockIndex].HandshakeComplete();
				return;
			}
			switch(Sck.scks[sockIndex].protocolVer)
			{
				case 4:
					//send reply; skip handshake stage two for this protocol
					Message pcket = new Message(System.Text.Encoding.ASCII.GetBytes(respOld));
					Sck.scks[sockIndex].SendPacket(pcket);
					Sck.scks[sockIndex].state = Condition.hndshk3;
					//it's over
					Sck.scks[sockIndex].HandshakeComplete();
					break;
				case 6:
					//check stage first
					if(Sck.scks[sockIndex].state == Condition.hndshk)
					{
						//send response to that incoming connection
						Message pckt = new Message(System.Text.Encoding.ASCII.GetBytes(resp2));
						Sck.scks[sockIndex].SendPacket(pckt);
						Sck.scks[sockIndex].state = Condition.hndshk2;
						//another timer that waits for the final response
						Sck.scks[sockIndex].inResponseYet2.Start();
					}
					else if(Sck.scks[sockIndex].state == Condition.hndshk2)
					{
						if(this.ultrapeerNeeded == true)
						{
							//send that last final response (outgoing connection)
							Message pckt = new Message(System.Text.Encoding.ASCII.GetBytes(resp3));
							Sck.scks[sockIndex].SendPacket(pckt);
							Sck.scks[sockIndex].state = Condition.hndshk3;
							//it's over
							Sck.scks[sockIndex].HandshakeComplete();
						}
						else
						//we have to switch from ultrapeer mode to leaf node mode
						{
							string newResp3 = "GNUTELLA/0.6 200 OK\r\n";
							newResp3 += "X-Ultrapeer: False\r\n";
							newResp3 += "\r\n";
							//disconnect from everything
							ConnectionManager.SwitchMode(sockIndex);
							//send that last final response (outgoing connection)
							Message pckt = new Message(System.Text.Encoding.ASCII.GetBytes(newResp3));
							Sck.scks[sockIndex].SendPacket(pckt);
							Sck.scks[sockIndex].state = Condition.hndshk3;
							//it's over
							Sck.scks[sockIndex].HandshakeComplete();
						}
					}
					break;
				default:
					//this shouldn't happen; so far it's either 4 or 6
					break;
			}
		}

		/// <summary>
		/// Return the position of the ending of the handshake in a string.
		/// </summary>
		public static int EndOfHandshake(ref string hndShake)
		{
			if(hndShake.LastIndexOf("\r\n") == -1)
				//this is a 0.4 protocol handshake
				return (hndShake.LastIndexOf("\n\n") + 2);
			else
				//this is a 0.6 or newer protocol handshake
				return (hndShake.LastIndexOf("\r\n") + 2);
		}

		/// <summary>
		/// Setup some of the handshakes.
		/// </summary>
		void SetupHandshakes()
		{
			//remote IP Address
			string remoteIP = Sck.scks[this.sockIndex].RemoteIP();

			//ourVer
			ourVer = "GNUTELLA CONNECT/0.6\r\n";
			ourVer += "User-Agent: FileScope " + Stats.version + "\r\n";
			ourVer += "X-Ultrapeer: " + ((Stats.Updated.Gnutella.ultrapeer == true) ? "True\r\n" : "False\r\n");
			ourVer += "X-Query-Routing: 0.1\r\n";
			ourVer += "Pong-Caching: 0.1\r\n";
			ourVer += "Hops-Flow: 1.0\r\n";
			ourVer += (remoteIP == "") ? "" : "Remote-IP: " + remoteIP + "\r\n";
			ourVer += "\r\n";

			//resp2
			resp2 = "GNUTELLA/0.6 200 OK\r\n";
			resp2 += "User-Agent: FileScope " + Stats.version + "\r\n";
			resp2 += "X-Ultrapeer: " + ((Stats.Updated.Gnutella.ultrapeer == true) ? "True\r\n" : "False\r\n");
			resp2 += "X-Ultrapeer-Needed: " + this.ultrapeerNeeded.ToString() + "\r\n";
			resp2 += "X-Query-Routing: 0.1\r\n";
			resp2 += "Pong-Caching: 0.1\r\n";
			resp2 += "Hops-Flow: 1.0\r\n";
			resp2 += (remoteIP == "") ? "" : "Remote-IP: " + remoteIP + "\r\n";
			resp2 += "\r\n";

			//respOverloaded
			respOverloaded = "GNUTELLA/0.6 503 Service unavailable\r\n";
			respOverloaded += "User-Agent: FileScope " + Stats.version + "\r\n";
			respOverloaded += "X-Ultrapeer: True\r\n";
			respOverloaded += "X-Query-Routing: 0.1\r\n";
			respOverloaded += "Pong-Caching: 0.1\r\n";
			respOverloaded += "Hops-Flow: 1.0\r\n";
			respOverloaded += (remoteIP == "") ? "" : "Remote-IP: " + remoteIP + "\r\n";
			respOverloaded += "\r\n";

			//leafNode
			leafNode = "GNUTELLA/0.6 503 I am a shielded leaf node\r\n";
			leafNode += "User-Agent: FileScope " + Stats.version + "\r\n";
			leafNode += "X-Ultrapeer: False\r\n";
			leafNode += "X-Query-Routing: 0.1\r\n";
			leafNode += "Pong-Caching: 0.1\r\n";
			leafNode += "Hops-Flow: 1.0\r\n";
			leafNode += (remoteIP == "") ? "" : "Remote-IP: " + remoteIP + "\r\n";
			leafNode += "\r\n";
		}

		/// <summary>
		/// Is this a handshake or some trash data?
		/// </summary>
		bool IsHandshake()
		{
			if(this.tempHnd.IndexOf("GNUTELLA/0.6 503") != -1)
			{
				//shielded leaf node
				Sck.scks[this.sockIndex].Disconnect("shielded leaf node");
				return false;
			}
			if(this.tempHnd.IndexOf("GNUTELLA") != -1)
				return true;
			return false;
		}

		/// <summary>
		/// "Remote-IP:" tag will tell us what our public IP address is.
		/// </summary>
		public void GetRemoteIP()
		{
			//let gnutella 2 take care of this
			return;
		}

		/// <summary>
		/// Parse any information from the handshake.
		/// ex. "Pong-Caching: 0.1"
		/// </summary>
		public void ParseInfo(ref string handshake)
		{
			//take care of x-ultrapeer-needed stuff

			//if we're connecting to the ultrapeer
			if(handshake.IndexOf("200 OK") != -1 && Stats.Updated.Gnutella.ultrapeer && handshake.ToLower().IndexOf("ultrapeer-needed: false") != -1 && Stats.Updated.Gnutella.lastConnectionCount < 3)
				this.ultrapeerNeeded = false;

			//if the ultrapeer is connecting to us
			if(handshake.IndexOf("GNUTELLA CONNECT") != -1 && handshake.ToLower().IndexOf("ultrapeer: true") != -1 && Stats.Updated.Gnutella.ultrapeer && Stats.settings.gConnectionsToKeep - Stats.Updated.Gnutella.lastConnectionCount > 5)
				this.ultrapeerNeeded = false;

			//find our IP
			GetRemoteIP();
			//is this a host supporting ultrapeers
			if(handshake.ToLower().IndexOf("ultrapeer") != -1)
				Sck.scks[sockIndex].newHost = true;
			//is this node ultrapeer?
			if(handshake.ToLower().IndexOf("ultrapeer: true") != -1 || handshake.ToLower().IndexOf("ultrapeer:true") != -1)
				Sck.scks[sockIndex].ultrapeer = true;
			//find node's vendor and version
			int vendorIndex = handshake.ToLower().IndexOf("user-agent:");
			if(vendorIndex != -1)
			{
				vendorIndex += 11;
				string vendor = handshake.Substring(vendorIndex, (handshake.IndexOf("\r\n", vendorIndex) - vendorIndex));
				if(vendor[0].ToString() == " ")
					vendor = vendor.Substring(1, vendor.Length-1);
				Sck.scks[this.sockIndex].vendor = vendor;
			}
		}
	}
}
