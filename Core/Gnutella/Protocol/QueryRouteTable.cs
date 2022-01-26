// QueryRouteTable.cs
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
using System.IO;

namespace FileScope.Gnutella
{
	/// <summary>
	/// Class dedicated to query routing.
	/// QueryRouteTable will create and handle all 0x30 packets.
	/// </summary>
	public class QueryRouteTable
	{
		//reset variant
		public static byte reset = (byte)0x0;
		//patch variant
		public static byte patch = (byte)0x1;
		//maximum QRT size is 16 KB
		public static int tableSize = 1<<14;
		//maximum TTL value is 2
		public static byte infinity = (byte)2;
		//the largest possible packet size for each patch is 1 KB
		public static int maxPatchSize = 1024;
		//qrTable[] values store ttl counts and the indices represent query hash values
		public static byte[] qrTable;
		//count the number of query routing table entries
		public static int qrEntries;
		//size of the entire sequence
		public static int seqSize;
		//actual patch array... ready to send
		public static ArrayList patchArray;

		//current sequence number during the actual sending or receiving of the QRT
		public int numSequence;
		//current uncompressed table we're receiving
		public ArrayList rawTable = new ArrayList();
		//final table we've received
		public byte[] recTable;
		//infinity value for this node sending us his QRT
		public int nodeInfinity;

		public static void HandleQueryRouteTable(Message theMessage, int sockNum)
		{
			//check first
			if(Sck.scks[sockNum].state != Condition.hndshk3 || Sck.scks[sockNum].ultrapeer || !Stats.Updated.Gnutella.ultrapeer)
			{
				System.Diagnostics.Debug.WriteLine("strange receive qrt error");
				return;
			}

			//entire payload
			byte[] payload = theMessage.GetPayload();

			try
			{
				switch(payload[0])
				{
					//reset
					case 0x0:
						//System.Diagnostics.Debug.WriteLine("reset");
						Sck.scks[sockNum].recQRT = new QueryRouteTable();
						Sck.scks[sockNum].recQRT.ProcessData(payload);
						break;
					//sequence
					case 0x1:
						//System.Diagnostics.Debug.WriteLine("sequence");
						Sck.scks[sockNum].recQRT.ProcessData(payload);
						break;
				}
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("HandleQueryRouteTable: " + e.Message);
			}
		}

		public QueryRouteTable()
		{
			numSequence = 0;
		}

		/// <summary>
		/// Process whatever the next packet is in the sequence.
		/// We're receiving a QRT at this point.
		/// </summary>
		public void ProcessData(byte[] payload)
		{
			switch(payload[0])
			{
				//reset
				case 0x0:
					int intlength = Endian.ToInt32(payload, 1, false);
					this.nodeInfinity = (int)payload[5];
					this.recTable = new byte[intlength];
					break;
				//sequence
				case 0x1:
					numSequence = (int)payload[1];
					int inSeqCount = (int)payload[2];
					byte inCompressor = payload[3];
					int inEntryBits = (int)payload[4];
					//copy the data from this payload
					byte[] temp = new byte[payload.Length - 5];
					Array.Copy(payload, 5, temp, 0, temp.Length);
					//add to table
					rawTable.AddRange(temp);
					//check if this is the last packet in the sequence
					if(numSequence == inSeqCount)
					{
						//this is a tag to know that the sequence is finished
						numSequence = 9999;
						byte[] totalData = new byte[rawTable.Count];
						rawTable.CopyTo(totalData);
						byte[] totalData2;
						if(inCompressor == 0x1)
							//zlib saves the day
							totalData2 = DeCompress(totalData, this.recTable.Length);
						else
							totalData2 = totalData;
						if(inEntryBits == 4)
							this.recTable = UnHalve(totalData2);
						else
							this.recTable = totalData2;
						//test
						/*
						for(int x = 0; x < this.recTable.Length; x++)
						{
							if(this.recTable[x] != 0)
							{
								sbyte idiot = (sbyte)((int)this.recTable[x] + this.nodeInfinity);
								System.Diagnostics.Debug.WriteLine(idiot.ToString() + " " + x.ToString());
							}
						}
						*/
					}
					break;
			}
		}

		/// <summary>
		/// This will return true if one of the hash values from the query fits this node's hashes.
		/// Remember that this is only used for leaf nodes.
		/// We hash all of the keywords from the query.
		/// We then check to see if one of these hashes are present in the QRT for this node.
		/// </summary>
		public bool CheckQuery(string query)
		{
			//check if the sequence of patches is completed
			if(this.numSequence != 9999)
				return false;

			string[] words = Keywords.GetKeywords(query);
			for(int pos = 0; pos < words.Length; pos++)
			{
				int hash = Hashing.Hash(ref words[pos], words[pos].Length, (int)Math.Log(this.recTable.Length, 2));
				if(this.recTable[hash] != 0)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Renew whatever the current routing table is.
		/// </summary>
		public static void PrepareTable()
		{
			//make sure we're a leaf node
			if(Stats.Updated.Gnutella.ultrapeer)
				return;
			//initialize the query routing table to the right size
			qrTable = new byte[tableSize];
			for(int x = 0; x < qrTable.Length; x++)
				qrTable[x] = 0;
			qrEntries = 0;
			seqSize = 0;
		}

		/// <summary>
		/// Add a file to the table.
		/// </summary>
		public static void Add(string file)
		{
			//make sure we're a leaf node
			if(Stats.Updated.Gnutella.ultrapeer)
				return;
			//split file into keywords
			string[] words = Keywords.GetKeywords(ref file);
			string[] prefixes_keywords = Keywords.GetPrefixes(words);
			//loop through each keyword
			for(int pos = 0; pos < prefixes_keywords.Length; pos++)
			{
				int hash = Hashing.Hash(ref prefixes_keywords[pos], prefixes_keywords[pos].Length, (int)Math.Log(tableSize, 2));
				if(qrTable[hash] < infinity)
					qrEntries++;
				/*
				 * ttl = 1
				 * subtract the infinity of 2
				 * that's a -1
				 * but we're using byte... not signed byte
				 * so it's really 255
				 */
				qrTable[hash] = (byte)255;
			}
		}

		/// <summary>
		/// Create an entire array of the patch sequence.
		/// </summary>
		public static void CreatePatches()
		{
			//make sure we're a leaf node
			if(Stats.Updated.Gnutella.ultrapeer)
				return;
			//for now we don't need a full byte because ttl is only 1
			bool needFullByte = false;
			//is this compressed or not
			byte compressor;
			//make sure we have files
			if(Stats.fileList.Count == 0)
			{
				seqSize = 0;
				return;
			}

			//create patch
			byte[] data = qrTable;

			//try different methods of compression
			byte bits=8;
			if(!needFullByte) 
			{
				data = Halve(data);
				bits=4;
			}

			byte[] compressedPatch = Compress(data);
			if(compressedPatch.Length < data.Length)
			{
				//compression worked
				compressor = 0x1;
				//notice how it's put back into data
				data = compressedPatch;
			}
			else
				compressor = 0x0;

			//calculate size of the sequence
			if(data.Length % maxPatchSize == 0)
				seqSize = (int)(data.Length / maxPatchSize);
			else
				seqSize = (int)(Math.Ceiling(Convert.ToDouble(data.Length) / maxPatchSize + 1));

			//we must send the patches in 1kb chunks
			patchArray = new ArrayList();
			int seqNumber = 1;
			for(int x = 0; x < data.Length; x += maxPatchSize)
			{
				int start = x;
				int leNgth = Math.Min(maxPatchSize, data.Length-x);
				byte[] buf = new byte[5+leNgth];
				buf[0] = 0x1;				//variant
				buf[1] = (byte)seqNumber;	//sequence number
				buf[2] = (byte)seqSize;		//sequence size
				buf[3] = compressor;		//compression
				buf[4] = bits;				//entry bits
				Array.Copy(data, x, buf, 5, leNgth);
				patchArray.Add(buf);
				seqNumber++;
			}
		}

		/*
//raw way of deflate w/o streams:

byte[] dirt1 = new byte[24] {(byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d'};
byte[] dirt2 = new byte[24] {(byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d', (byte)'d'};

			Deflater dfer = new Deflater();
			dfer.Reset();
			dfer.SetInput(dirt1);
			dfer.Finish();
			int crip = dfer.Deflate(dirt2);

System.Diagnostics.Debug.WriteLine(dirt1.Length.ToString());
System.Diagnostics.Debug.WriteLine(dirt2.Length.ToString());
System.Diagnostics.Debug.WriteLine(((int)dirt1[1]).ToString());
System.Diagnostics.Debug.WriteLine(((int)dirt2[1]).ToString());
System.Diagnostics.Debug.WriteLine(crip.ToString());

			dirt1[1] = (byte)'f';
			Inflater ifer = new Inflater();
			ifer.SetInput(dirt2, 0, 10);
			ifer.Inflate(dirt1);

System.Diagnostics.Debug.WriteLine(dirt1.Length.ToString());
System.Diagnostics.Debug.WriteLine(dirt2.Length.ToString());
System.Diagnostics.Debug.WriteLine(((int)dirt1[1]).ToString());
System.Diagnostics.Debug.WriteLine(((int)dirt2[1]).ToString());
System.Diagnostics.Debug.WriteLine(crip.ToString());
		*/

		/// <summary>
		/// We are going to use our trusty zlib compression library.
		/// </summary>
		public static byte[] Compress(byte[] elData)
		{
			MemoryStream st = new MemoryStream();
			Stream s = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(st);
			s.Write(elData, 0, elData.Length);
			s.Close();
			return (byte[])st.ToArray();
		}

		/// <summary>
		/// We use zlib again; this time to decompress the table.
		/// </summary>
		public static byte[] DeCompress(byte[] elData, int len)
		{
			Stream s = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(new MemoryStream(elData));
			byte[] st = new byte[len];
			s.Read(st, 0, len);
			s.Close();
			return st;
		}

		/// <summary>
		/// This function will send your table to all connected ultrapeers.
		/// </summary>
		public static void SendQRTables()
		{
			//find fully connected ultrapeers
			foreach(Sck sck in Sck.scks)
				if(sck != null)
					if((sck.state == Condition.hndshk3) && sck.ultrapeer)
					{
						SendQRTables(sck.sockNum);
					}
		}

		/// <summary>
		/// Overloaded function; only sends the QRT to one host.
		/// </summary>
		public static void SendQRTables(int sckNum)
		{
			//make sure we're a leaf node
			if(Stats.Updated.Gnutella.ultrapeer)
				return;
			//make sure this node is a fully connected ultrapeer
			if((Sck.scks[sckNum].state == Condition.hndshk3) && Sck.scks[sckNum].ultrapeer)
			{
				QueryRouteTable qrt = new QueryRouteTable();
				Sck.scks[sckNum].SendQRT(qrt);
			}
		}

		/// <summary>
		/// Returns the entire reset packet.
		/// </summary>
		public byte[] GetResetPacket()
		{
			byte[] packet = new byte[6];
			packet[0] = reset;
			Array.Copy(Endian.GetBytes(tableSize, false), 0, packet, 1, 4);
			packet[5] = infinity;
			Message resetMessage = new Message(0x30, packet, 1);
			return resetMessage.GetWholePacket();
		}

		/// <summary>
		/// Returns the next packet in the sequence to send.
		/// </summary>
		public byte[] GetNextPacket()
		{
			if(numSequence == 0)
			{
				//we haven't sent a reset packet yet
				numSequence++;
				return GetResetPacket();
			}
			if(numSequence > seqSize)
				//attempt to send empty byte array so that we know the sequence is over
				return (new byte[0]);
			else
			{
				Message patchMessage = new Message(0x30, (byte[])patchArray[numSequence-1], 1);
				numSequence++;
				return patchMessage.GetWholePacket();
			}
		}

		private static byte[] Halve(byte[] array) 
		{
			byte[] ret = new byte[array.Length/2];
			for(int i = 0; i < ret.Length; i++)
				ret[i] = (byte)(((sbyte)array[2*i]<<4) | ((sbyte)array[2*i+1]&0xF));
			return ret;
		}

		private static byte[] UnHalve(byte[] array) 
		{
			byte[] ret = new byte[array.Length*2];
			for(int i = 0; i < array.Length; i++) 
			{
				ret[2*i] = (byte)((sbyte)array[i]>>4);
				ret[2*i+1] = (byte)ExtendNibble((sbyte)((sbyte)array[i]&0xF));
			}
			return ret;
		}

		private static sbyte ExtendNibble(sbyte b)
		{
			if((b&0x8) != 0)
				return (sbyte)(0xF0 | (byte)b);
			else
				return b;
		}
	}
}
