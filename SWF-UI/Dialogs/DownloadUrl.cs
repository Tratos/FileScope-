// DownloadUrl.cs
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
	/// Link handler for magnets and such.
	/// </summary>
	public class DownloadUrl : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox1;
		private System.ComponentModel.Container components = null;

		public DownloadUrl()
		{
			InitializeComponent();
			Control c = (Control)this;
			Themes.SetupTheme(c);
			if(Stats.settings.alwaysOnTop)
				this.TopMost = true;
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(57, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(220, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "FileScope understands magnet links";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(57, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(440, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "(ex. magnet:?xt=urn:sha1:QI557Q2GXEXK33QA744NUKUFAIFTZY1Y)";
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.Location = new System.Drawing.Point(448, 55);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(96, 24);
			this.button1.TabIndex = 2;
			this.button1.Text = "Download";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// textBox1
			// 
			this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBox1.Location = new System.Drawing.Point(8, 56);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(438, 22);
			this.textBox1.TabIndex = 3;
			this.textBox1.Text = "";
			this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
			// 
			// DownloadUrl
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(554, 88);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.textBox1,
																		  this.button1,
																		  this.label2,
																		  this.label1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "DownloadUrl";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Download Url";
			this.ResumeLayout(false);

		}
		#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			Download(textBox1.Text);
		}

		void Download(string link)
		{
			if(textBox1.Text.Length == 0)
				return;
			textBox1.Text = "";
			if(link.Length < 9 || link.Substring(0, 8) != "magnet:?")
				MessageBox.Show("Link not understood", "Invalid link", MessageBoxButtons.OK, MessageBoxIcon.Error);
			else
			{
				link = link.Substring(8, link.Length-8);
				string[] parts = link.Split(new char[]{'&'}, 10);
				string urn = "";
				string dn = "";
				string host = "";
				foreach(string part in parts)
				{
					if(part.Length < 4)
						continue;
					switch(part.Substring(0, 2))
					{
						case "xt":
							if(urn == "")
								urn = part.Substring(3, part.Length-3);
							break;
						case "dn":
							if(dn == "")
								dn = part.Substring(3, part.Length-3);
							break;
						case "xs":
							int start = part.IndexOf("http://");
							if(start != -1)
							{
								string newPart = part.Substring(10, part.Length-10);
								int stop = newPart.IndexOf("/");
								if(stop != -1)
									host = newPart.Substring(0, stop);
							}
							break;
						default:
							break;
					}
				}
				//setup a download for this item; we need the urn at least
				if(urn != "")
				{
					//we need to support the urn
					if(urn.IndexOf("urn:sha1:") != -1)
					{
						if(dn == "")
							dn = urn.Substring(9, urn.Length-9);
						QueryHitObject qho = new QueryHitObject();
						qho.fileIndex = 1;
						qho.fileName = dn;
						qho.filePath = "";
						qho.fileSize = 1;
						qho.guid = Utils.HexGuid(GUID.newGUID());
						qho.hops = 1;
						qho.ip = "0.0.0.0";
						qho.networkType = NetworkType.Gnutella2;
						qho.port = 1;
						qho.servIdent = GUID.newGUID();
						qho.sha1sum = urn;
						qho.sockWhereFrom = -1;
						qho.speed = 1;
						qho.unseenHosts = 0;
						qho.vendor = "magnet";
						QueryHitTable qht = new QueryHitTable();
						qht.address = qho.ip;
						qht.busy = false;
						qht.chat = false;
						qht.downloaded = false;
						qht.md4sum = Utils.HexGuid(qho.md4sum);
						qht.mp3info = "";
						qht.push = false;
						qht.queryHitObjects.Add(qho);
						qht.speed = 1;
						qht.type = "";
						qht.unseenHosts = 0;
						qht.sha1 = qho.sha1sum;
						DownloadManager.NewDownload(qht);
					}
				}
			}
			textBox1.Focus();
		}

		private void textBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//if enter was pressed
			if(e.KeyChar == (char)13)
			{
				e.Handled = true;
				Download(textBox1.Text);
			}
		}
	}
}
