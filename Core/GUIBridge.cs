// GUIBridge.cs
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
using System.Text;
using System.Windows.Forms;

namespace FileScope
{
	/// <summary>
	/// Class used as a bridge between the backend core and the frontend gui.
	/// Almost everything is done via Invoke so that the GUI thread takes care of everything on its own.
	/// This class is the ONLY way the core can even touch the gui.
	/// </summary>
	public class GUIBridge
	{
		/// <summary>
		/// Inform GUI of a new incoming QueryHit.
		/// </summary>
		public static void AddQueryHit(QueryHitObject qho, QueryHitTable qht, ref string searchName)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				//we're trying to find the tabpage for this corresponding search
				foreach(SearchTabPage tabPage in StartApp.main.search.tabControl1.TabPages)
				{
					if(searchName.Length >= 8 && searchName.Substring(0, 8) == "browse: ")
					{
						if(tabPage.Tag.ToString().IndexOf(searchName) != -1)
						{
							tabPage.BeginInvoke(new SearchTabPage.addNewItem(tabPage.AddNewItem), new object[] {qho, qht});
							return;
						}
					}
					else if(tabPage.Tag.ToString() == searchName)
					{
						tabPage.BeginInvoke(new SearchTabPage.addNewItem(tabPage.AddNewItem), new object[] {qho, qht});
						return;
					}
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge AddQueryHitTable");
			}
		}

		public static void RefreshHomePageNetworks()
		{
			if(StartApp.main == null)
				return;
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.homepage.BeginInvoke(new HomePage.resetNetworksText(StartApp.main.homepage.ResetNetworksText));
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge RefreshHomePageNetworks");
			}
		}

		public static void RefreshHomePageTransfers()
		{
			if(StartApp.main == null)
				return;
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.homepage.BeginInvoke(new HomePage.resetTransfersText(StartApp.main.homepage.ResetTransfersText));
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge RefreshHomePageTransfers");
			}
		}

		public static void RefreshHomePageLibrary()
		{
			if(StartApp.main == null)
				return;
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.homepage.BeginInvoke(new HomePage.resetLibraryText(StartApp.main.homepage.ResetLibraryText));
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge RefreshHomePageLibrary");
			}
		}

		/// <summary>
		/// We just initiated a new gnutella connection that's not connected yet.
		/// </summary>
		public static void GNewConnection(string host, string route, int sckNum)
		{
			if(Stats.Updated.closing)
				return;
			Gnutella.ConnectionManager.AddActiveHost(host, sckNum);
			try
			{
				StartApp.main.connection.Invoke(new Connection.gNewConnection(StartApp.main.connection.GNewConnection), new object[] {host, route, sckNum});
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("guibridge GNewConnection " + e.Message);
			}
		}

		/// <summary>
		/// We just connected to a gnutella host.
		/// </summary>
		public static void GJustConnected(int sckNum, string type, string vendor)
		{
			if(Stats.Updated.closing)
				return;
			Gnutella.ConnectionManager.AddActiveHost(Gnutella.Sck.scks[sckNum].RemoteIP(), sckNum);
			try
			{
				StartApp.main.connection.Invoke(new Connection.gJustConnected(StartApp.main.connection.GJustConnected), new object[] {sckNum, type, vendor});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge GJustConnected");
			}
		}

		/// <summary>
		/// Update incoming and outgoing bandwidth.
		/// </summary>
		public static void GBandwidth(int sckNum, string bIn, string bOut)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				if(FileScope.MainDlg.selectedIndexMain != 1)
					return;
				StartApp.main.connection.BeginInvoke(new Connection.gBandwidth(StartApp.main.connection.GBandwidth), new object[] {sckNum, bIn, bOut});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge GBandwidth");
			}
		}

		/// <summary>
		/// A given gnutella socket just lost its connection.
		/// </summary>
		public static void GJustDisconnected(int sckNum)
		{
			if(Stats.Updated.closing)
				return;
			Gnutella.ConnectionManager.RemoveActiveHost(sckNum);
			try
			{
				StartApp.main.connection.Invoke(new Connection.gJustDisconnected(StartApp.main.connection.GJustDisconnected), new object[] {sckNum});
			}
			catch(System.Threading.ThreadAbortException e)
			{e = e;}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge GJustDisconnected");
			}
		}

		/// <summary>
		/// Response to a Gnutella requery.
		/// </summary>
		public static void GReQueryResponse(QueryHitObject qho, string elHash)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.Invoke(new ReQuery.gnutellaReQueryResponse(ReQuery.GnutellaReQueryResponse), new object[] {qho, elHash});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge GReQueryResponse");
			}
		}

		/// <summary>
		/// We just initiated a connection to a g2 host.
		/// </summary>
		public static void G2NewConnection(string addr, int sockNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.g2NewConnection(StartApp.main.connection.G2NewConnection), new object[] {addr, sockNum});
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("guibridge G2NewConnection " + e.Message);
			}
		}

		/// <summary>
		/// Handshake is complete and we're now connected to the g2 host.
		/// </summary>
		public static void G2JustConnected(int sockNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.g2JustConnected(StartApp.main.connection.G2JustConnected), new object[] {sockNum});
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("guibridge G2JustConnected " + e.Message);
			}
		}

		/// <summary>
		/// Update bandwidth and other stuff.
		/// </summary>
		public static void G2Update(int sckNum, ref string bIn, ref string bOut)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				if(FileScope.MainDlg.selectedIndexMain != 1)
					return;
				StartApp.main.connection.BeginInvoke(new Connection.g2Update(StartApp.main.connection.G2Update), new object[] {sckNum, bIn + @" / " + bOut});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge G2Update");
			}
		}

		/// <summary>
		/// We just disconnected from a g2 host.
		/// </summary>
		public static void G2JustDisconnected(int sockNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.g2JustDisconnected(StartApp.main.connection.G2JustDisconnected), new object[] {sockNum});
			}
			catch(System.Threading.ThreadAbortException e)
			{e = e;}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge G2JustDisconnected");
			}
		}

		/// <summary>
		/// New download; update UI.
		/// </summary>
		public static void NewDownload(QueryHitTable qht, int dlNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.transfers.Invoke(new Transfers.newDownload(StartApp.main.transfers.NewDownload), new object[] {qht, dlNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge NewDownload");
			}
		}

		/// <summary>
		/// Update the download information.
		/// </summary>
		public static void UpdateDownload(ref string status, ref string percent, ref string speed, int dlNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.transfers.Invoke(new Transfers.updateDownload(StartApp.main.transfers.UpdateDownload), new object[] {status, percent, speed, dlNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge UpdateDownload");
			}
		}

		/// <summary>
		/// A download just ended; update UI.
		/// </summary>
		public static void RemoveDownload(int dlNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.transfers.Invoke(new Transfers.removeDownload(StartApp.main.transfers.RemoveDownload), new object[] {dlNum});
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("guibridge RemoveDownload: " + e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
		}

		/// <summary>
		/// New upload.
		/// </summary>
		public static void NewUpload(int upNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.transfers.Invoke(new Transfers.newUpload(StartApp.main.transfers.NewUpload), new object[] {upNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge NewUpload");
			}
		}

		/// <summary>
		/// Update the UI about this upload.
		/// </summary>
		public static void UpdateUpload(string filename, uint filesize, string status, string percent, string offsets, string speed, int upNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.transfers.Invoke(new Transfers.updateUpload(StartApp.main.transfers.UpdateUpload), new object[] {filename, filesize, status, percent, offsets, speed, upNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge UpdateUpload");
			}
		}

		/// <summary>
		/// Upload just finished.
		/// </summary>
		public static void RemoveUpload(int upNum, int type)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.transfers.Invoke(new Transfers.removeUpload(StartApp.main.transfers.RemoveUpload), new object[] {upNum, type});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge RemoveUpload");
			}
		}

		/// <summary>
		/// Either an incoming or outgoing chat was created.
		/// </summary>
		public static void NewChat(int chatNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.chatpage.Invoke(new ChatPage.newChat(StartApp.main.chatpage.NewChat), new object[] {chatNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge NewChat");
			}
		}

		/// <summary>
		/// An outgoing chat has just completed its handshake.
		/// </summary>
		public static void ConnectedChat(int chatNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.chatpage.Invoke(new ChatPage.connectedChat(StartApp.main.chatpage.ConnectedChat), new object[] {chatNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge ConnectedChat");
			}
		}

		/// <summary>
		/// We've received a chat message from our peer.
		/// </summary>
		public static void NewChatData(int chatNum, string msg)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.chatpage.Invoke(new ChatPage.newChatData(StartApp.main.chatpage.NewChatData), new object[] {chatNum, msg});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge NewChatData");
			}
		}

		/// <summary>
		/// Either an incoming or outgoing chat was finished.
		/// </summary>
		public static void DisconnectedChat(int chatNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.chatpage.Invoke(new ChatPage.disconnectedChat(StartApp.main.chatpage.DisconnectedChat), new object[] {chatNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge DisconnectedChat");
			}
		}

		/// <summary>
		/// Notify gui of incoming search for g1.
		/// </summary>
		public static void Query(ref string query)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				if(FileScope.MainDlg.selectedIndexMain == 1)
					StartApp.main.connection.BeginInvoke(new Connection.gQuery(StartApp.main.connection.GQuery), new object[] {query});
			}
			catch(System.Threading.ThreadAbortException e)
			{e = e;}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge Query");
			}
		}

		/// <summary>
		/// Notify gui of incoming G2 search.
		/// </summary>
		public static void QueryG2(ref string query)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				if(FileScope.MainDlg.selectedIndexMain == 1)
					StartApp.main.connection.BeginInvoke(new Connection.g2Query(StartApp.main.connection.G2Query), new object[] {query});
			}
			catch(System.Threading.ThreadAbortException e)
			{e = e;}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge QueryG2");
			}
		}

		/// <summary>
		/// Connect randomly to one of the OpenNap servers stored.
		/// </summary>
		public static void OConnectRandom()
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.oConnectRandomOpenNap(StartApp.main.connection.ConnectRandomOpenNap));
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge OConnectRandom");
			}
		}

		/// <summary>
		/// Socket # sockNum just connected to an OpenNap server.
		/// </summary>
		public static void OJustConnected(int sockNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.oJustConnected(StartApp.main.connection.OJustConnected), new object[] {sockNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge OJustConnected");
			}
		}

		/// <summary>
		/// Socket # sockNum just disconnected from an OpenNap server.
		/// </summary>
		public static void OJustDisconnected(int sockNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.oJustDisconnected(StartApp.main.connection.OJustDisconnected), new object[] {sockNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge OJustDisconnected");
			}
		}

		/// <summary>
		/// Update the stats for a given OpenNap connection.
		/// </summary>
		public static void OUpdateStats(int sockNum, string users, string files, string gigs)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.oUpdateStats(StartApp.main.connection.OUpdateStats), new object[] {sockNum, users, files, gigs});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge OUpdateStats");
			}
		}

		/// <summary>
		/// We just got a message from an OpenNap server... display it.
		/// </summary>
		public static void OMessage(int cNum, string message)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.oMessage(StartApp.main.connection.OMessage), new object[] {cNum.ToString(), message});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge OMessage");
			}
		}

		/// <summary>
		/// Socket # sockNum just connected to an eDonkey network.
		/// </summary>
		public static void EJustConnected(int sockNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.eJustConnected(StartApp.main.connection.EJustConnected), new object[] {sockNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge EJustConnected");
			}
		}

		/// <summary>
		/// Socket # sockNum just disconnected from an eDonkey network.
		/// </summary>
		public static void EJustDisconnected(int sockNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.eJustDisconnected(StartApp.main.connection.EJustDisconnected), new object[] {sockNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge EJustDisconnected");
			}
		}

		/// <summary>
		/// We just got a message from an eDonkey server... display it.
		/// </summary>
		public static void EMessage(int cNum, string message)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.eMessage(StartApp.main.connection.EMessage), new object[] {cNum.ToString(), message});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge EMessage");
			}
		}

		/// <summary>
		/// Update the stats for an eDonkey server.
		/// </summary>
		public static void EUpdateStats(int sockNum)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.eUpdateStats(StartApp.main.connection.EUpdateStats), new object[] {sockNum});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge EUpdateStats");
			}
		}

		/// <summary>
		/// Get a random eDonkey server and place it into ipap.
		/// </summary>
		public static void EGetRandomServer(IPandPort ipap)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.eGetRandomEDonkey(StartApp.main.connection.GetRandomEDonkey), new object[] {ipap});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge EGetRandomServer");
			}
		}

		/// <summary>
		/// One of our eDonkey servers just gave us a list of more servers.
		/// </summary>
		public static void ENewServer(ref string server)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				string servName = " ";
				string servDesc = "New Server";
				StartApp.main.connection.Invoke(new Connection.eDonkeyAdd(StartApp.main.connection.EDonkeyAdd), new object[] {server, servName, servDesc});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge ENewServer");
			}
		}

		/// <summary>
		/// There was a change in the shared content, modify library accordingly.
		/// </summary>
		public static void ChangeShared()
		{
			if(Stats.Updated.closing)
				return;
			if(StartApp.main == null)
				return;
			try
			{
				StartApp.main.library.Invoke(new Library.updateShares(StartApp.main.library.UpdateShares));
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge ChangeShared");
			}
		}

		/// <summary>
		/// New version of FileScope is available.
		/// </summary>
		public static void NewVersion(string version)
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				//check if there is a new version out
				if(version != Stats.version && Stats.settings.updateNotify)
					StartApp.main.Invoke(new MainDlg.newVersion(StartApp.main.NewVersion), new object[] {version});
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge NewVersion");
			}
		}

		/// <summary>
		/// Switched mode from ultrapeer to leafnode or vice-versa.
		/// </summary>
		public static void SwitchedMode()
		{
			if(Stats.Updated.closing)
				return;
			try
			{
				StartApp.main.connection.Invoke(new Connection.updateUltrapeerStatus(StartApp.main.connection.UpdateUltrapeerStatus));
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("guibridge SwitchedMode");
			}
		}
	}
}
