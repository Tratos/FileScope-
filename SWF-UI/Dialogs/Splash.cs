// Splash.cs
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

namespace FileScope
{
	/// <summary>
	/// Splash screen.
	/// </summary>
	public class Splash : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox logo;
		private System.Windows.Forms.Timer timer1;
		private System.ComponentModel.IContainer components;

		public Splash()
		{
			InitializeComponent();
			if(Stats.settings.alwaysOnTop)
				this.TopMost = true;
			//load all our stuff asynchronously while the splash screen is up
			AsyncCallback j = new AsyncCallback(AsyncLoadOp);
			j.BeginInvoke(null, null, null);
		}

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Splash));
			this.logo = new System.Windows.Forms.PictureBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// logo
			// 
			this.logo.Image = ((System.Drawing.Bitmap)(resources.GetObject("logo.Image")));
			this.logo.Name = "logo";
			this.logo.Size = new System.Drawing.Size(434, 141);
			this.logo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.logo.TabIndex = 0;
			this.logo.TabStop = false;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 40;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// Splash
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(440, 144);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.logo});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "Splash";
			this.Opacity = 0;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Splash";
			this.TopMost = true;
			this.Load += new System.EventHandler(this.Splash_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void Splash_Load(object sender, System.EventArgs e)
		{
			this.Width = logo.Width;
			this.Height = logo.Height;
		}

		bool finished = false;

		void AsyncLoadOp(IAsyncResult ar)
		{
			//initialize stuff, load settings, etc.
			Stats.LoadSave.LoadSettings();
			Stats.InitializeVariables();
			Stats.LoadSave.LoadShares();
			Stats.LoadSave.LoadHosts();
			Stats.LoadSave.LoadWebCache();
			Stats.LoadSave.LoadLastFileSet();
			finished = true;
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			try
			{
				if(this.Opacity > 0.95)
				{
					while(true)
					{
						if(finished)
							break;
						System.Threading.Thread.Sleep(50);
					}
					//we don't need the timer anymore
					timer1.Stop();
					//create the main window
					StartApp.CreateMainWindow();
					this.Close();
					return;
				}
				this.Opacity += 0.08;
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Splash timer1_Tick");
			}
		}
	}
}
