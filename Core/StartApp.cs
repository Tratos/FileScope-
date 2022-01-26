// StartApp.cs
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Threading;
using System.Reflection;

namespace FileScope
{
	/// <summary>
	/// This is where the application begins and ends.
	/// </summary>
	public class StartApp
	{
		//instance of the main window
		public static MainDlg main;
		public static Splash splash;
		//instance of the whole assembly
		public static Assembly assembly;

		[STAThread]
		public static void Main() 
		{
			assembly = Assembly.GetExecutingAssembly();
			Application.ThreadException += new ThreadExceptionEventHandler(AppError);
			//did we run this program before?
			if(File.Exists(Utils.GetCurrentPath("settings.fscp")) && File.Exists(Utils.GetCurrentPath("gnuthosts.fscp")) && File.Exists(Utils.GetCurrentPath("gnut2hosts.fscp")) && File.Exists(Utils.GetCurrentPath("gnut2cache.fscp")) && File.Exists(Utils.GetCurrentPath("gnut2keys.fscp")) && File.Exists(Utils.GetCurrentPath("shares.fscp")) && File.Exists(Utils.GetCurrentPath("webcache.fscp")) && File.Exists(Utils.GetCurrentPath("webcache2.fscp")) && File.Exists(Utils.GetCurrentPath("chatblocks.fscp")))
			{
				//splash screen first
				splash = new Splash();
				splash.Show();
				Application.Run();
			}
			else
			{
				//no settings; start the wizard
				WizardDlg wizard = new WizardDlg();
				wizard.Show();
				Application.Run();
			}
		}

		/// <summary>
		/// Error somewhere in our app.
		/// </summary>
		public static void AppError(object sender, ThreadExceptionEventArgs args)
		{
			string errmsg = args.Exception.Message + "\r\n" +
				args.Exception.Source + "\r\n" +
				args.Exception.StackTrace;

			if(Stats.Updated.closing)
			{
				try
				{
					FileStream fsClose = new FileStream(Utils.GetCurrentPath("Closing_Errors.txt"), FileMode.OpenOrCreate);
					byte[] elError = System.Text.Encoding.ASCII.GetBytes(errmsg);
					fsClose.Write(elError, 0, elError.Length);
					fsClose.Close();
				}
				catch
				{
					System.Diagnostics.Debug.WriteLine("AppError");
				}
			}

			System.Diagnostics.Debug.WriteLine(errmsg);
			MessageBox.Show(errmsg);
		}

		public static void CreateMainWindow()
		{
			main = new MainDlg();
			main.Show();
		}

		/// <summary>
		/// Exit FileScope.
		/// </summary>
		public static void ExitApp()
		{
			//let everyone know we're closing the app
			Stats.Updated.closing = true;
			Stats.StopFSWs();
			main.search.SerializeSearches();
			main.trayIcon.Visible = false;
			Listener.Abort();
			ChatManager.DisconnectAll();
			while(DownloadManager.countFinishes != 0)
				System.Threading.Thread.Sleep(50);
			DownloadManager.StopAllDownloads();
			UploadManager.DisconnectAll();
			Gnutella.StartStop.Abort();
			Gnutella2.StartStop.Stop();
			OpenNap.StartStop.Stop();
			EDonkey.StartStop.Stop();
			HashEngine.Stop();
			while(HashEngine.IsAlive())
				System.Threading.Thread.Sleep(20);
			Stats.LoadSave.SaveSettings();
			Stats.LoadSave.SaveShares();
			Stats.LoadSave.SaveHosts();
			Stats.LoadSave.SaveWebCache();
			Stats.LoadSave.SaveLastFileSet();
			System.Diagnostics.Debug.WriteLine("---exiting---");
			Application.Exit();
		}
	}
}
