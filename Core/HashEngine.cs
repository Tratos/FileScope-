// HashEngine.cs
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
using System.Threading;

namespace FileScope
{
	/// <summary>
	/// Engine responsible for generating hash values for all files we're sharing.
	/// </summary>
	public class HashEngine
	{
		//loops through all our files and generates sha1 values for each file
		static Thread hashThread;
		//the current fileList item we're working on
		static int fileListIndex;

		/// <summary>
		/// Start generating hashes.
		/// </summary>
		public static void Start()
		{
			hashThread = new Thread(new ThreadStart(FuncThread));
			hashThread.Priority = ThreadPriority.Lowest;
			fileListIndex = 0;
			hashThread.Start();
		}

		/// <summary>
		/// Stop generating hashes.
		/// </summary>
		public static void Stop()
		{
			try
			{
				if(IsAlive())
					hashThread.Abort();
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("HashEngine Stop: " + e.Message);
			}
		}

		public static bool IsAlive()
		{
			if(hashThread == null)
				return false;
			else
				return hashThread.IsAlive;
		}

		static void FuncThread()
		{
			while(true)
			{
				try
				{
					if(fileListIndex >= Stats.fileList.Count)
					{
						Thread.Sleep(1000);
						return;
					}
					//these two values verify we're talking about the SAME file
					string filePathName;
					uint bytes;
					//just for reference
					int fileIndex;
					lock(Stats.fileList)
					{
						FileObject fo = (FileObject)Stats.fileList[fileListIndex];
						filePathName = fo.location;
						bytes = fo.b;
						fileIndex = fo.fileIndex;
					}
					string hash = "";
					byte[] md4 = null;
					byte[] sha1bytes = null;
					//check to see if we already know the hash for this file
					if(Stats.lastFileSet.ContainsKey(filePathName))
					{
						FileObject2 fo2 = (FileObject2)Stats.lastFileSet[filePathName];
						//we know the file location and name are the same; check filesize
						if(bytes == fo2.bytes)
						{
							md4 = fo2.md4;
							hash = fo2.sha1;
							sha1bytes = fo2.sha1bytes;
						}
					}
					//if we couldn't locate an existing hash value for the file
					if(hash.Length == 0)
					{
						md4 = HashSums.CalcMD4(filePathName);
						hash = HashSums.CalcSha1(filePathName, ref sha1bytes);
					}
					//insert hashes into QHTs
					Gnutella2.QueryRouteTable.AddHash(ref hash);
					//insert back to the original file object
					lock(Stats.fileList)
					{
						FileObject fo = new FileObject();
						fo.b = bytes;
						fo.fileIndex = fileIndex;
						fo.location = filePathName;
						fo.lcaseFileName = System.IO.Path.GetFileName(filePathName).ToLower();
						fo.md4 = md4;
						fo.sha1 = hash;
						fo.sha1bytes = sha1bytes;
						Stats.fileList[fileListIndex] = fo;
					}
					fileListIndex++;
					Thread.Sleep(200);
				}
				catch(ThreadAbortException tae)
				{tae=tae;Stats.LoadSave.hashEngineAborted = true;}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("HashEngine: " + e.Message);
				}
			}
		}
	}
}
