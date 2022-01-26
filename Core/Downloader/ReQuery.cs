// ReQuery.cs
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
using System.Collections;
using System.Text;
using System.Net;

namespace FileScope
{
	/// <summary>
	/// Take care of any re-searching for copies of files.
	/// This is a very useful algorithm when downloading anything.
	/// </summary>
	public class ReQuery
	{
		//delegate for a Gnutella requery response
		public delegate void gnutellaReQueryResponse(QueryHitObject elQHO, string elHash);

		public static void FindMoreSourcesRequested(int dmNum)
		{
			try
			{
				Downloader dler = (Downloader)DownloadManager.dms[dmNum].downloaders[0];
				if(dler.qho.networkType == NetworkType.EDonkey)
				{
					EDonkeyRequery(dler.qho.md4sum);
				}
				else if(dler.qho.networkType == NetworkType.Gnutella1)
				{
					GnutellaRequery(dler.qho.fileName, DownloadManager.dms[dmNum].sha1);
				}
				else if(dler.qho.networkType == NetworkType.OpenNap)
				{
					if(dler.state == DLState.CouldNotConnect || dler.state == DLState.Waiting)
						OpenNapRequery(dler.qho.fileName);
				}
				else
				{
					Gnutella2Requery(dler.qho);
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("ReQuery FindMoreSourcesRequested");
			}
		}

		/// <summary>
		/// Find another instance of an OpenNap file.
		/// We'll requery on ALL OpenNap networks.
		/// </summary>
		public static void OpenNapRequery(string filename)
		{
			string requerystring = Utils.FilterChars(filename);
			//System.Diagnostics.Debug.WriteLine("OpenNap RE-SEARCH: " + requerystring);
			OpenNap.Messages.Search(requerystring);
		}

		/// <summary>
		/// Find another instance of a Gnutella file.
		/// We're going to requery on each Gnutella connection.
		/// We match files by hash/filesize when possible, and by filename/filesize when not.
		/// </summary>
		public static void GnutellaRequery(string filename, string hash)
		{
			string requerystring = Utils.FilterChars(filename);
			if(requerystring.IndexOf(" ") != -1 && hash != "")
				requerystring = requerystring.Substring(0, requerystring.IndexOf(" "));
			//System.Diagnostics.Debug.WriteLine("Gnutella RE-SEARCH: " + requerystring + " " + hash);
			//take out the "urn:" part because the BroadcastReQuery function will add it back
			if(hash != "")
				hash = hash.Substring(4, hash.Length - 4);
			Gnutella.Query.BroadcastReQuery(requerystring, hash);
		}

		//general purpose flag used when receiving QKA packets
		public static bool g2requeried = false;

		/// <summary>
		/// Find another instance of the Gnutella2 file:
		/// 1. We'll pick a random hub
		/// 2. If we have a key, we'll send a requery
		/// 3. If we don't, we'll send out a query key request
		/// As query key answers come in, if q2requeried is set, some will be directed to downloads for requeries.
		/// </summary>
		public static void Gnutella2Requery(QueryHitObject qho)
		{
			if(qho.sha1sum.Length == 0)
			{
				System.Diagnostics.Debug.WriteLine("g2 no sha1 for requery");
				return;
			}
			if(Gnutella2.HostCache.recentHubs.Count == 0)
				return;
			g2requeried = true;
			IPAddress ipa;
			Gnutella2.HubInfo hi;
			lock(Gnutella2.HostCache.recentHubs)
			{
				int which = GUID.rand.Next(0, Gnutella2.HostCache.recentHubs.Count);
				ipa = (IPAddress)Gnutella2.HostCache.recentHubs.GetKey(which);
				hi = (Gnutella2.HubInfo)Gnutella2.HostCache.recentHubs.GetByIndex(which);
				if(hi.mhi == null)
					return;
			}
		again:
			if(!hi.mhi.qk)
				Gnutella2.UDPSR.SendOutgoingPacket(new Gnutella2.OQKR(), new IPEndPoint(ipa, hi.port));
			else
			{
				//check first
				if(!Stats.Updated.udpIncoming)
				{
					if(Gnutella2.ConnectionManager.SckNumForRouting(hi.mhi.queryKeyedHub) == -1)
					{
						hi.mhi.qk = false;
						hi.mhi.queryKeyedHub = null;
						goto again;
					}
				}
				else
				{
					if(hi.mhi.queryKeyedHub != null)
					{
						hi.mhi.qk = false;
						hi.mhi.queryKeyedHub = null;
						goto again;
					}
				}
				//check next
				if(hi.mhi.retryCountDown > 0)
					return;
				//it's ready
				Gnutella2.OQ2 oq2 = new Gnutella2.OQ2();
				oq2.urn = new byte[25];
				Encoding.ASCII.GetBytes("sha1", 0, 4, oq2.urn, 0);
				oq2.urn[4] = (byte)'\0';
				Array.Copy(Base32.Decode(qho.sha1sum, 9, 32), 0, oq2.urn, 5, 20);
				oq2.guid = new byte[16];
				GUID.rand.NextBytes(oq2.guid);
				oq2.hi = hi;
				//we don't need all metadata and other stuff
				oq2.interest = 1;
				Gnutella2.Search.SearchHub(ipa, hi, oq2);
			}
		}

		/// <summary>
		/// Just received a QKA for some hub.
		/// </summary>
		public static void Gnutella2NewHub(IPAddress ipa, Gnutella2.HubInfo hi)
		{
			if(hi.mhi.retryCountDown > 0)
				return;
			ArrayList alG2s = new ArrayList();
			foreach(DownloadManager dMEr in DownloadManager.dms)
				if(dMEr != null)
					if(dMEr.active)
						if(((Downloader)dMEr.downloaders[0]).qho.networkType == NetworkType.Gnutella2)
							alG2s.Add(((Downloader)dMEr.downloaders[0]).qho);
			if(alG2s.Count == 0)
				return;
			int rand = GUID.rand.Next(0, alG2s.Count);
			//send query
			Gnutella2.OQ2 oq2 = new Gnutella2.OQ2();
			oq2.urn = new byte[25];
			Encoding.ASCII.GetBytes("sha1", 0, 4, oq2.urn, 0);
			oq2.urn[4] = (byte)'\0';
			Array.Copy(Base32.Decode(((QueryHitObject)alG2s[rand]).sha1sum, 9, 32), 0, oq2.urn, 5, 20);
			oq2.guid = new byte[16];
			GUID.rand.NextBytes(oq2.guid);
			oq2.hi = hi;
			oq2.interest = 1;
			Gnutella2.Search.SearchHub(ipa, hi, oq2);
		}

		/// <summary>
		/// Find more sources for the md4 hash we have.
		/// We'll requery all connected eDonkey servers.
		/// </summary>
		public static void EDonkeyRequery(byte[] md4hash)
		{
			if(md4hash == null)
			{
				System.Diagnostics.Debug.WriteLine("EDonkeyRequery empty md4 digest");
				return;
			}
			//System.Diagnostics.Debug.WriteLine("eDonkey RE-SEARCH: " + Utils.HexGuid(md4hash));
			EDonkey.Messages.FindSources(md4hash, false);
		}

		/// <summary>
		/// Find more sources using the global udp request.
		/// </summary>
		public static void EDonkeyRequeryGlobalUdp(byte[] md4hash)
		{
			if(md4hash == null)
			{
				System.Diagnostics.Debug.WriteLine("EDonkeyRequeryGlobalUdp empty md4 digest");
				return;
			}
			EDonkey.Messages.FindSources(md4hash, true);
		}

		/*
		 * all OpenNap related ReQuery responses are in OpenNap/Protocol/Messages.cs
		 */

		/// <summary>
		/// This is probably a response to one of our Gnutella requeries.
		/// If all goes well, we'll have another Downloader instance to swarm download from.
		/// We match files by hash/filesize when possible, and by filename/filesize when not.
		/// </summary>
		public static void GnutellaReQueryResponse(QueryHitObject elQHO, string elHash)
		{
			foreach(DownloadManager dMEr in DownloadManager.dms)
				if(dMEr != null)
					if(dMEr.active)
						if(dMEr.sha1 != "")
						{
							//if the DownloadManager has a hash, the qho hash must be the same
							if(dMEr.sha1.ToLower() == elHash.ToLower() && ((Downloader)dMEr.downloaders[0]).qho.fileSize == elQHO.fileSize)
							{
								AddGnutellaReQueryFoundDownloader(elQHO, dMEr);
								return;
							}
						}
						else
						{
							//no hash; we can only do it by filename and filesize
							if(QHOStuff.Match2(((Downloader)dMEr.downloaders[0]).qho.fileName, elQHO.fileName) && ((Downloader)dMEr.downloaders[0]).qho.fileSize == elQHO.fileSize)
							{
								AddGnutellaReQueryFoundDownloader(elQHO, dMEr);
								return;
							}
						}
		}

		public static void AddGnutellaReQueryFoundDownloader(QueryHitObject elQHO, DownloadManager dMEr)
		{
			bool hostKnown = false;
			//check to see if the host isn't already known
			foreach(Downloader downer in dMEr.downloaders)
				if(downer.qho.ip == elQHO.ip)
					hostKnown = true;
			try
			{
				if(!hostKnown)
				{
					string ext = System.IO.Path.GetExtension(elQHO.fileName);
					if(elQHO.fileName.Length > 150)
					{
						elQHO.fileName = elQHO.fileName.Substring(0, 140);
						if(ext.Length > 0)
							elQHO.fileName += ext;
					}
					elQHO.fileName = elQHO.fileName.Replace(@"\", "-");
					elQHO.fileName = elQHO.fileName.Replace(@"/", "-");
					//create a downloader for this host
					Downloader elDLer = new Downloader(elQHO, dMEr.downloaders.Count, dMEr.dmNum, 12);
					dMEr.downloaders.Add(elDLer);
				}
			}
			catch(Exception ee)
			{
				System.Diagnostics.Debug.WriteLine("AddReQueryFoundDownloader: " + ee.Message);
			}
		}

		/// <summary>
		/// A new eDonkey source was discovered for a particular md4sum.
		/// </summary>
		public static void EDonkeyFoundSource(ref string hexmd4sum, ref string ip, ushort port, int numServers, int sockNum)
		{
			foreach(DownloadManager dMEr in DownloadManager.dms)
				if(dMEr != null)
					if(dMEr.active)
						if(dMEr.md4sum == hexmd4sum)
						{
							lock(dMEr.downloaders)
							{
								bool hostKnown = false;
								//check to see if the host isn't already known
								foreach(Downloader downer in dMEr.downloaders)
									if(downer.qho.ip == ip)
										hostKnown = true;
								try
								{
									if(!hostKnown)
									{
										QueryHitObject qhoFirst = ((Downloader)dMEr.downloaders[0]).qho;
										QueryHitObject elQHO = new QueryHitObject();
										elQHO.extensions = qhoFirst.extensions;
										elQHO.fileIndex = qhoFirst.fileIndex;
										elQHO.fileName = qhoFirst.fileName;
										elQHO.fileSize = qhoFirst.fileSize;
										elQHO.ip = ip;
										elQHO.md4sum = qhoFirst.md4sum;
										elQHO.networkType = qhoFirst.networkType;
										elQHO.port = port;
										elQHO.sockWhereFrom = sockNum;
										elQHO.speed = qhoFirst.speed;
										elQHO.unseenHosts = numServers;
										elQHO.vendor = qhoFirst.vendor;
										//create a downloader for this host
										Downloader elDLer = new Downloader(elQHO, dMEr.downloaders.Count, dMEr.dmNum, numServers);
										dMEr.downloaders.Add(elDLer);
									}
								}
								catch(Exception ee)
								{
									System.Diagnostics.Debug.WriteLine("EDonkeyFoundSource: " + ee.Message);
								}
								return;
							}
						}
		}
	}
}
