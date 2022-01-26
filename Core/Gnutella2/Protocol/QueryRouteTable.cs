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
using System.Timers;
using System.Collections;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace FileScope.Gnutella2
{
	/// <summary>
	/// Class dedicated to G2 query hash/route tables.
	/// In G2 each bit represents a word hash value, 0 for filled, 1 for empty.
	/// </summary>
	public class QueryRouteTable
	{
		//our local query hash table
		public static BitArray ourQHT = null;
		//all leafs and I
		public static BitArray aggregateQHT = null;
		//send table every time qhtUpdate is true and/or send aggregate table once in a while, when hub
		public static bool qhtUpdate = false;
		public static GoodTimer qhtSend = new GoodTimer(20000);

		public static void PrepareTable()
		{
			if(ourQHT != null)
			{
				lock(ourQHT)
					ourQHT = new BitArray(1048576, true);
			}
			else
			{
				ourQHT = new BitArray(1048576, true);
				qhtSend.AddEvent(new ElapsedEventHandler(qhtSend_Tick));
				qhtSend.Start();
			}
		}

		public static void AddWords(string filename)
		{
			lock(ourQHT)
			{
				string[] words = Keywords.GetKeywords(ref filename);
				//set these hashes in the table to "not empty"
				for(int x = 0; x < words.Length; x++)
				{
					ourQHT[Hashing.Hash(ref words[x], words[x].Length, 20)] = false;
					if(words[x].Length >= 5)
					{
						ourQHT[Hashing.Hash(ref words[x], words[x].Length-1, 20)] = false;
						ourQHT[Hashing.Hash(ref words[x], words[x].Length-2, 20)] = false;
					}
				}
			}
		}

		public static void AddHash(ref string hash)
		{
			lock(ourQHT)
			{
#if DEBUG
//				System.Diagnostics.Debug.WriteLine("g2 qht AddHash: " + hash);
#endif
				string urnSha1 = "urn:sha1:" + hash;
				ourQHT[Hashing.Hash(ref urnSha1, urnSha1.Length, 20)] = false;
			}
		}

		public static void SendQRT()
		{
			lock(ourQHT)
				qhtUpdate = true;
		}

		static void qhtSend_Tick(object sender, ElapsedEventArgs e)
		{
			lock(ourQHT)
			{
				if(Stats.Updated.Gnutella2.ultrapeer)
				{
					/*
					 * right now we are just taking tables our size n=20; later we'll move on to this:
					 * 
					 * qhts of n > 30 or n < 5 should be blocked
					 * making the aggregate table:
					 * let ndiff = difference in n bits
					 * 1. if we're adding some large table, foreach value: shift to the right by ndiff and set that value in table to true
					 * 2. if we're adding some small table, foreach value: shift to left by ndiff and set pos equal to that; then for(int count=0; count < (2 to power of ndiff); count++, pos++) and set those pos values in table to true
					 */

					//if we're ultrapeer, we'll send aggregate tables every once in a while, regardless of qhtUpdate
					if(GUID.rand.Next(0, 3) == 0)
					{
						//System.Diagnostics.Debug.WriteLine("start qht combine");
						aggregateQHT = (BitArray)ourQHT.Clone();
						foreach(Sck sck in Sck.scks)
							if(sck != null)
								if(sck.mode == G2Mode.Leaf && sck.inQHT != null)
								{
									if(sck.inQHT.Length == aggregateQHT.Length)
									{
										//sck.inQHT is unaffected, only aggregateQHT changes in this operation
										aggregateQHT.And(sck.inQHT);
									}
									else
										System.Diagnostics.Debug.WriteLine("g2 leaf's qht size: " + sck.inQHT.Length.ToString());
								}
						//System.Diagnostics.Debug.WriteLine("end qht combine");
						foreach(Sck sck in Sck.scks)
							if(sck != null)
								if(sck.state == Condition.hndshk3 && sck.mode == G2Mode.Ultrapeer && !sck.browseHost && !sck.webCache)
									FillWithPatches(sck);
					}
				}
				else
				{
					//as a leaf node, we'll just send our table on every update
					if(qhtUpdate)
					{
						qhtUpdate = false;
						foreach(Sck sck in Sck.scks)
							if(sck != null)
								if(sck.state == Condition.hndshk3 && !sck.browseHost && !sck.webCache)
									FillWithPatches(sck);
					}
				}
				commonOldQHT = null;
			}
		}

		public static int maxPatchSize = 1024;
		//common/reoccuring patch related variables
		static BitArray commonOldQHT = null;
		static int commonDataSizeDeflate;
		static int commonDataSizeNonDeflate;
		static byte[] commonDataDeflate = null;
		static byte[] commonDataNonDeflate = null;

		/// <summary>
		/// Update sck with our new QHT.
		/// </summary>
		public static void FillWithPatches(Sck sck)
		{
			lock(ourQHT)
			{
				bool withReset;
				bool deflate;
			    int dataSize;
				byte[] data;

				//latest qht that we want the host to have
				BitArray baToSend;
				if(Stats.Updated.Gnutella2.ultrapeer)
				{
					if(aggregateQHT == null)
						aggregateQHT = (BitArray)ourQHT.Clone();
					baToSend = aggregateQHT;
				}
				else
					baToSend = ourQHT;

				/*
				 * check to see if we already have a patch set ready to update this host
				 * this happens if we're updating several ultrapeers in a row and we can use a common patch set for all of them
				 */
				if(sck.lastQHT == commonOldQHT && commonOldQHT != null)
				{
					if(sck.deflateOut && commonDataDeflate != null)
					{
						withReset = false;
						deflate = true;
						dataSize = commonDataSizeDeflate;
						data = commonDataDeflate;
						sck.lastQHT = baToSend;
						goto begin;
					}
					if(!sck.deflateOut && commonDataNonDeflate != null)
					{
						withReset = false;
						deflate = false;
						dataSize = commonDataSizeNonDeflate;
						data = commonDataNonDeflate;
						sck.lastQHT = baToSend;
						goto begin;
					}
				}
				else if(sck.lastQHT != null)
				{
					//we have to clear these because a new commonOldQHT is gonna be set
					commonDataDeflate = null;
					commonDataNonDeflate = null;
				}

				//the "update" data to be sent in patches
				BitArray patches = new BitArray(baToSend);
				if(sck.lastQHT == null)
				{
					withReset = true;
					patches.Not();
					sck.lastQHT = baToSend;
				}
				else
				{
					withReset = false;
					commonOldQHT = sck.lastQHT;
					patches.Xor(sck.lastQHT);
					sck.lastQHT = baToSend;
				}

				deflate = false;
				dataSize = patches.Length/8;
				data = new byte[dataSize];
				patches.CopyTo(data, 0);
				//if we're not already sending deflate, we'll compress this data
				if(!sck.deflateOut)
				{
					byte[] dataCompressed = new byte[dataSize];
					Deflater dfer = new Deflater();
					dfer.Reset();
					dfer.SetInput(data);
					dfer.Finish();
					int num = dfer.Deflate(dataCompressed);
					//if the compression actually did something...
					if(num < data.Length)
					{
						data = dataCompressed;
						dataSize = num;
						deflate = true;
					}
				}

				//if we're sending an update in patches, perhaps all other updates will use these patches
				if(!withReset)
				{
					if(deflate)
					{
						commonDataSizeDeflate = dataSize;
						commonDataDeflate = data;
					}
					else
					{
						commonDataSizeNonDeflate = dataSize;
						commonDataNonDeflate = data;
					}
				}

			begin:
				//make a reset packet first, if necessary
				if(withReset)
				{
					OQHT oqhtreset = new OQHT();
					oqhtreset.reset = true;
					sck.SendPacket(oqhtreset);
				}

				//patches
				int fragNum = 1;
				int fragCount;
				//fragments are sent 1kb each
				if(dataSize % maxPatchSize == 0)
					fragCount = (int)(dataSize / maxPatchSize);
				else
					fragCount = (int)(Math.Ceiling(Convert.ToDouble(dataSize) / maxPatchSize + 1));
				for(int x = 0; x < dataSize; x += maxPatchSize)
				{
					int len = Math.Min(maxPatchSize, dataSize-x);
					OQHT oqhtpatch = new OQHT();
					oqhtpatch.reset = false;
					oqhtpatch.deflate = 0x00;
					if(deflate)
						oqhtpatch.deflate = 0x01;
					oqhtpatch.fragNum = (byte)fragNum;
					oqhtpatch.fragCount = (byte)fragCount;
					oqhtpatch.indexindata = x;
					oqhtpatch.lenofdata = len;
					oqhtpatch.data = data;
					sck.SendPacket(oqhtpatch);
					fragNum++;
				}
			}
		}
	}
}
