// QueryHit.cs
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

namespace FileScope.Gnutella
{
	/// <summary>
	/// Class dedicated to query hit packets.
	/// Search responses are created and handled here.
	/// </summary>
	public class QueryHit
	{
		/* ----------------------------------------------------------------
		 * EQHD UPDATE 8/17/01
		 * Byte 0-3 : Vendor Code
		 * Byte 4   : Public area size (either 2 or 4) if 4 then 7-8 is xml size
		 * Byte 5-6 : Public area
		 * Byte 7-8 : Size of XML + 1 (for a null), you need to count backward
		 * from the client GUID
		 * Byte 9   : private vendor flag
		 * Byte 10-X: GGEP area
		 * Byte X-beginning of xml : (new) private area
		 * Byte (payload.length - 16 - xmlSize (above)) - 
				(payload.length - 16 - 1) : XML
		 * Byte (payload.length - 16 - 1) : NULL
		 * Last 16 Bytes: client GUID
		 */

		private static byte PUSH_MASK = (byte)0x01;
		private static byte BUSY_MASK = (byte)0x04;
		private static byte UPLOADED_MASK = (byte)0x08;
		private static byte SPEED_MASK = (byte)0x10;

		public static void HandleQueryHit(Message theMessage, int sockNum)
		{
			Stats.Updated.Gnutella.numQueryHits++;

			try
			{
				//find out the route for this query hit
				int route = Router.GetQueryHitRoute(theMessage.gitem);
				//System.Diagnostics.Debug.WriteLine("query hit " + route.ToString());
				if(route == -2)
					return;

				//add route entry for push request routing
				Router.SeenYet(theMessage, sockNum);

				if(route != -1 && Stats.Updated.Gnutella.ultrapeer)
				{
					//route this query hit
					theMessage.RoutePacket(route);
					return;
				}

				byte[] payload = theMessage.GetPayload();
				byte byteNumHits = payload[0];	//results count

				//create search response instances
				QueryHitObject[] queryHitObj = new QueryHitObject[(int)byteNumHits];

				//set all to correct values before entering 
				for(int y = 0; y < queryHitObj.Length; y++)
				{
					queryHitObj[y] = new QueryHitObject();
					queryHitObj[y].port = (int)Endian.ToUInt16(payload, 1, false);
					queryHitObj[y].sockWhereFrom = sockNum;
					queryHitObj[y].ip = Endian.BigEndianIP(payload, 3);
					//speed of connection in KB/sec
					queryHitObj[y].speed = Endian.ToInt32(payload, 7, false);
					queryHitObj[y].extensions = new string[0];
					queryHitObj[y].hops = theMessage.GetHOPS();
					queryHitObj[y].networkType = NetworkType.Gnutella1;
					queryHitObj[y].unseenHosts = 0;
				}

				//process the result set
				int curByte = 11;
				for(int x = 0; x < (int)byteNumHits; x++)
				{
					queryHitObj[x].fileIndex = Endian.ToInt32(payload, curByte, false);
					curByte += 4;

					queryHitObj[x].fileSize = Endian.ToUInt32(payload, curByte, false);
					curByte += 4;

					//take care of the filename
					int jumpIndex = -1;
					for(int y = 0; y < 1000; y++)
						if(payload[curByte+y] == 0x0)
						{
							jumpIndex = y;
							break;
						}
					if(jumpIndex == -1)
						break;
					queryHitObj[x].fileName = System.Text.Encoding.ASCII.GetString(payload, curByte, jumpIndex);
					//jump over the entire filename and the first null
					curByte += jumpIndex+1;

					//take care of extension between nulls
					int jumpIndex2 = -1;
					for(int y = 0; y < 1000; y++)
						if(payload[curByte+y] == 0x0)
						{
							jumpIndex2 = y;
							break;
						}
					if(jumpIndex2 == -1)
						break;
					if(jumpIndex2 == 0)
						curByte += 1;
					else
					{
						string extension = System.Text.Encoding.ASCII.GetString(payload, curByte, jumpIndex2);
						char[] delimeter = new Char[1];
						delimeter[0] = (char)0x1C;
						queryHitObj[x].extensions = extension.Split(delimeter);
						curByte += jumpIndex2+1;
					}
					queryHitObj[x].sha1sum = QHOStuff.GetSha1Ext(queryHitObj[x]);
				}

				//check for extended query hit descriptor
				//some idiots incorrectly call this the QHD
				if(payload.Length - curByte > 16)
				{
					//EQHD
					string vendor = System.Text.Encoding.ASCII.GetString(payload, curByte, 4);
					curByte += 4;
					int openDataSize = (int)payload[curByte];
					curByte++;
					byte control = payload[curByte];curByte++;
					byte flags = payload[curByte];curByte++;
					bool pushFlag = false;
					bool busyFlag = false;
					bool uploadedFlag = false;
					bool speedFlag = false;
					if((control & PUSH_MASK) != 0)
						pushFlag = (flags&PUSH_MASK) != 0 ? true: false;
					if((control & BUSY_MASK) != 0)
						busyFlag = (flags&BUSY_MASK) != 0 ? true: false;
					if((control & UPLOADED_MASK) != 0)
						uploadedFlag = (flags&UPLOADED_MASK) != 0 ? true: false;
					if((control & SPEED_MASK) != 0)
						speedFlag = (flags&SPEED_MASK) != 0 ? true: false;
					int xmlSize = 0;
					string xml = "";
					if(openDataSize > 2)
					{
						//there is xml
						xmlSize = (int)Endian.ToUInt16(payload, curByte, false);
						curByte += 2;
						if(xmlSize > 0)
						{
							//we have to count backwards because of those Limewire people
							int xmlIndex = payload.Length - 16 - xmlSize;
							//the -1 is for the null at the end
							xml = System.Text.Encoding.ASCII.GetString(payload, xmlIndex, xmlSize-1);
						}
					}
					//is there still that private area left?
					bool chat = false;
					if(payload.Length - 16 > curByte+xmlSize)
					{
						//if((vendor.ToLower() == "fscp" || vendor.ToLower() == "lime" || vendor.ToLower() == "raza"))
							if(payload[curByte] == 0x1)
								chat = true;
					}
					//set values for all
					for(int y = 0; y < queryHitObj.Length; y++)
					{
						//wow read this one carefully
						queryHitObj[y].vendor = Vendor.GetVendor(vendor);
						//rest of booleans
						queryHitObj[y].busy = busyFlag;
						queryHitObj[y].chat = chat;
						queryHitObj[y].push = pushFlag;
						queryHitObj[y].trueSpeed = speedFlag;
						queryHitObj[y].uploaded = uploadedFlag;
						//xml if any
						queryHitObj[y].xml = xml;
					}
				}
				else
				{
					//no EQHD
					for(int y = 0; y < queryHitObj.Length; y++)
					{
						//we'll know there is no eqhd if vendor is empty
						queryHitObj[y].vendor = "";
					}
				}

				//servent identifier
				byte[] guid = new byte[16];
				Array.Copy(payload, payload.Length-16, guid, 0, 16);
				string strGuid = Utils.HexGuid(guid);
				for(int y = 0; y < queryHitObj.Length; y++)
				{
					queryHitObj[y].guid = strGuid;
					queryHitObj[y].servIdent = guid;
				}

				//we're done
				for(int y = 0; y < queryHitObj.Length; y++)
					AddToTables(queryHitObj[y], Utils.HexGuid(theMessage.GetGUID()));
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("HandleQueryHit: " + e.Message);
			}
		}

		/// <summary>
		/// Send a QueryHitObject to gui.
		/// </summary>
		static void AddToTables(QueryHitObject qhObj, string guid)
		{
			ActiveSearch search = null;

			//find active search with matching guid
			bool found = false;
			
			lock(ActiveSearch.searches)
				for(int y = 0; y < ActiveSearch.searches.Count; y++)
				{
					search = (ActiveSearch)ActiveSearch.searches[y];
					if(search.guid == guid)
					{
						found = true;
						break;
					}
				}

			//this isn't a response to our search... maybe a requery for a download
			if(found == false)
				GUIBridge.GReQueryResponse(qhObj, QHOStuff.GetSha1Ext(qhObj));
			else
				GUIBridge.AddQueryHit(qhObj, null, ref search.query);
		}

		/// <summary>
		/// Check if we have the resources defined in the query.
		/// Make an appropriate response.
		/// </summary>
		public static void RespondToQuery(Message theMessage, ref string query, int sockNum)
		{
			try
			{
				string[] keywords = Keywords.GetKeywords(query);

				//find matching files
				ArrayList fileMatches = new ArrayList();
				lock(Stats.fileList)
				{
					foreach(FileObject fi in Stats.fileList)
					{
						bool match = true;
						string fName = fi.lcaseFileName;
						foreach(string str in keywords)
							if(fName.IndexOf(str.ToLower()) == -1)
								match = false;

						if(match)
							fileMatches.Add(fi);
					}
				}

				if(fileMatches.Count == 0)
					return;

				//this arraylist holds the entire byte[] payload
				ArrayList payload = new ArrayList();

				byte[] buf1 = new byte[11];
				//number of hits
				buf1[0] = (byte)fileMatches.Count;
				//port
				Array.Copy(Endian.GetBytes((ushort)Stats.settings.port, false), 0, buf1, 1, 2);
				//ip address
				Array.Copy(Endian.BigEndianIP(Stats.settings.ipAddress), 0, buf1, 3, 4);
				//speed
				Array.Copy(Endian.GetBytes(Stats.GetSpeed(), false), 0, buf1, 7, 4);
				//add it all to the final payload
				payload.AddRange(buf1);

				//fabricate result set
				foreach(FileObject fi in fileMatches)
				{
					payload.AddRange(Endian.GetBytes(fi.fileIndex, false));
					payload.AddRange(Endian.GetBytes(fi.b, false));
					//file name terminated with two nulls
					byte[] filename = Encoding.ASCII.GetBytes(System.IO.Path.GetFileName(fi.location));
					byte[] twoNulls = new byte[2]{0x00, 0x00};
					payload.AddRange(filename);
					if(fi.sha1 == "")
						payload.AddRange(twoNulls);
					else
					{
						//embed the hash value between nulls
						byte[] oneNull = new byte[1]{0x00};
						payload.AddRange(oneNull);
						payload.AddRange(Encoding.ASCII.GetBytes("urn:sha1:" + fi.sha1));
						payload.AddRange(oneNull);
					}
				}

				//EQHD
				//vendor code
				byte[] vendorCode = new byte[4]{(byte)'F', (byte)'S', (byte)'C', (byte)'P'};
				payload.AddRange(vendorCode);
				//open data
				byte[] openData = new byte[3];
				openData[0] = (byte)2;//size
				openData[1] = (byte)(PUSH_MASK | BUSY_MASK | UPLOADED_MASK | SPEED_MASK);
				openData[2] = (byte)((Stats.settings.firewall ? PUSH_MASK : (byte)0x00)
					| ((Stats.Updated.uploadsNow >= Stats.settings.maxUploads) ? BUSY_MASK : (byte)0x00)
					| ((Stats.Updated.uploads > 0) ? UPLOADED_MASK : (byte)0x00)
					| ((Stats.Updated.trueSpeed != -1) ? SPEED_MASK : (byte)0x00));
				payload.AddRange(openData);
				//we skip xml crap; go straight to private data
				byte[] privData = new byte[1];
				//we support chatting
				privData[0] = 0x1;
				payload.AddRange(privData);

				//servent identifier
				payload.AddRange(Stats.settings.myGUID);

				byte[] bytesPayload = new byte[payload.Count];
				payload.CopyTo(bytesPayload);

				//create packet and send
				Message message = new Message(theMessage.GetGUID(), 0x81, bytesPayload, theMessage.GetHOPS());
				Sck.scks[sockNum].SendPacket(message);
			}
			catch(Exception exc)
			{
				System.Diagnostics.Debug.WriteLine("QueryHit RespondToQuery: " + exc.Message);
			}
		}
	}
}
