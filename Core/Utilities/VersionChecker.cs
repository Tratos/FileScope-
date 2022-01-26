// VersionChecker.cs
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
using System.Net;
using System.Threading;

namespace FileScope
{
	/// <summary>
	/// Checks for a new version of FileScope.
	/// </summary>
	public class VersionChecker
	{
		//thread for connecting to FileScope.com/version.txt
		static Thread connect;

		public static void Start()
		{
			connect = new Thread(new ThreadStart(FuncThread));
			connect.Start();
		}

		static void FuncThread()
		{
			try
			{
				//connect to server; initiate request
				HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("http://www.filescope.com/version.txt");
				httpRequest.UserAgent = "FileScope";
				//begin asynchronous get request
				httpRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), httpRequest);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("VersionChecker FuncThread");
			}
		}

		static void OnGetResponse(IAsyncResult ar)
		{
			try
			{
				HttpWebRequest tmpReq = (HttpWebRequest)ar.AsyncState;
				//get the response from the server
				HttpWebResponse resp = (HttpWebResponse)tmpReq.EndGetResponse(ar);
				//create stream to read from
				StreamReader stream = new StreamReader(resp.GetResponseStream());

				//read
				string line = stream.ReadLine();
				while(line != null)
				{
					if(line.IndexOf("FileScope") != -1)
						if(line.Length > 14 && line.Length < 18)
						{
							int pos = line.IndexOf("FileScope")+10;
							GUIBridge.NewVersion(line.Substring(pos, line.Length - pos));
							break;
						}
					line = stream.ReadLine();
				}
				stream.Close();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("VersionChecker OnGetResponse");
			}
		}
	}
}
