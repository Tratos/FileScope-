// ChattersDlg.cs
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
	/// Choose from several chatters to chat with.
	/// </summary>
	public class ChattersDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.ComponentModel.IContainer components;

		public ChattersDlg()
		{
			InitializeComponent();
			Control c = (Control)this;
			Themes.SetupTheme(c);
			if(Stats.settings.alwaysOnTop)
				this.TopMost = true;
			toolTip1.SetToolTip(button1, "Chat with selected host(s)");
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
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// listBox1
			// 
			this.listBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listBox1.Location = new System.Drawing.Point(8, 30);
			this.listBox1.Name = "listBox1";
			this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.listBox1.Size = new System.Drawing.Size(168, 199);
			this.listBox1.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.Location = new System.Drawing.Point(28, 235);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(129, 25);
			this.button1.TabIndex = 1;
			this.button1.Text = "Chat";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label1.Location = new System.Drawing.Point(32, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(124, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Chat Compatible Hosts:";
			// 
			// ChattersDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(185, 266);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label1,
																		  this.button1,
																		  this.listBox1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ChattersDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Available Chat Hosts";
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Add all of the chat compatible hosts from a QueryHitTable.
		/// </summary>
		public void SetupWindow(QueryHitTable qht)
		{
			foreach(QueryHitObject qho in qht.queryHitObjects)
				if(qho.chat)
					listBox1.Items.Add(qho.ip + ":" + qho.port.ToString());
			if(listBox1.Items.Count > 0)
				listBox1.SelectedIndex = 0;
		}

		/// <summary>
		/// Add all of the chat compatible hosts from a download.
		/// </summary>
		public void SetupWindow(DownloadManager dmer)
		{
			foreach(Downloader dler in dmer.downloaders)
			{
				if(dler.chatIP != "")
					listBox1.Items.Add(dler.chatIP);
				else
				{
					if(ChatCompatible(dler.qho.vendor))
						listBox1.Items.Add(dler.qho.ip + ":" + dler.qho.port.ToString());
				}
			}
			if(listBox1.Items.Count > 0)
				listBox1.SelectedIndex = 0;
		}

		/// <summary>
		/// Add all of the chat compatible hosts from an upload.
		/// </summary>
		public void SetupWindow(Uploader uper)
		{
			if(uper.chatIP != "")
				listBox1.Items.Add(uper.chatIP);
			else
			{
				if(ChatCompatible(uper.userAgent))
					listBox1.Items.Add(uper.RemoteIP());
			}
			if(listBox1.Items.Count > 0)
				listBox1.SelectedIndex = 0;
		}

		/// <summary>
		/// Check if a vendor supports chatting.
		/// </summary>
		bool ChatCompatible(string vendorUpper)
		{
			string vendor = vendorUpper.ToLower();
				if(vendor.IndexOf("limewire") != -1  || vendor.IndexOf("filescope") != -1 || vendor.IndexOf("shareaza") != -1)
					return true;
			return false;
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			foreach(object obj in listBox1.SelectedItems)
			{
				string peer = (string)obj;
				ChatManager.Outgoing(ref peer);
			}
			this.Close();
		}
	}
}
