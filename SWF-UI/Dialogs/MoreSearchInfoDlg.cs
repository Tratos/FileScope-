// MoreSearchInfoDlg.cs
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
	/// Provides more information for a given search listviewitem.
	/// </summary>
	public class MoreSearchInfoDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.ComponentModel.Container components = null;

		public MoreSearchInfoDlg()
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
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
			this.label3 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label4 = new System.Windows.Forms.Label();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label1.Location = new System.Drawing.Point(74, 2);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Filename: ";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label2.Location = new System.Drawing.Point(74, 18);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(32, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Size: ";
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1,
																						this.columnHeader2,
																						this.columnHeader6,
																						this.columnHeader7,
																						this.columnHeader8,
																						this.columnHeader3,
																						this.columnHeader11});
			this.listView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listView1.FullRowSelect = true;
			this.listView1.GridLines = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.Location = new System.Drawing.Point(0, 65);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(601, 209);
			this.listView1.TabIndex = 2;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Filename";
			this.columnHeader1.Width = 200;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Address";
			this.columnHeader2.Width = 80;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Vendor";
			this.columnHeader6.Width = 68;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Firewalled";
			this.columnHeader7.Width = 65;
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "Busy";
			this.columnHeader8.Width = 40;
			// 
			// columnHeader11
			// 
			this.columnHeader11.Text = "Chat";
			this.columnHeader11.Width = 40;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label3.Location = new System.Drawing.Point(74, 34);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(69, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "SHA1 Hash: ";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(60, 60);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label4.Location = new System.Drawing.Point(74, 50);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "MD4 Hash: ";
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Browse";
			this.columnHeader3.Width = 55;
			// 
			// MoreSearchInfoDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(620, 359);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label4,
																		  this.pictureBox1,
																		  this.label3,
																		  this.label2,
																		  this.label1,
																		  this.listView1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimizeBox = false;
			this.Name = "MoreSearchInfoDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "More Info";
			this.Resize += new System.EventHandler(this.MoreSearchInfoDlg_Resize);
			this.Load += new System.EventHandler(this.MoreSearchInfoDlg_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void MoreSearchInfoDlg_Resize(object sender, System.EventArgs e)
		{
			listView1.Width = this.ClientSize.Width;
			listView1.Height = this.ClientSize.Height - listView1.Top;
		}

		private void MoreSearchInfoDlg_Load(object sender, System.EventArgs e)
		{
			listView1.Width = this.ClientSize.Width;
			listView1.Height = this.ClientSize.Height - listView1.Top;
			pictureBox1.Image = StartApp.main.pictureMoreInfo.Image;
		}

		/// <summary>
		/// Add a QueryHitTable to the listview.
		/// </summary>
		public void SetupView(QueryHitTable qht)
		{
foreach(QueryHitObject qholiops in qht.queryHitObjects)
{
	Utils.Diag("\nokie:");
	Utils.Diag(qholiops.sha1sum);
	Utils.Diag(Utils.HexGuid(qholiops.md4sum));
}
			//Filename, Address, Vendor, Firewalled, Busy, Browse, Chat

			QueryHitObject qho = (QueryHitObject)qht.queryHitObjects[0];
			this.label1.Text += qho.fileName;
			this.label2.Text += Utils.Assemble((qho.fileSize), " bytes");
			this.label3.Text += qho.sha1sum.Substring(qho.sha1sum.LastIndexOf(":")+1);
			if(qho.md4sum != null)
				this.label4.Text += Utils.HexGuid(qho.md4sum);

			//loop through each QueryHitObject
			for(int x = 0; x < qht.queryHitObjects.Count; x++)
			{
				qho = (QueryHitObject)qht.queryHitObjects[x];
				string[] subitems = new string[7];
				subitems[0] = qho.fileName;
				subitems[1] = qho.ip.ToString() + ":" + qho.port.ToString();
				subitems[2] = qho.vendor;
				if(qho.networkType == NetworkType.Gnutella2)
					subitems[3] = IPfilter.Private(qho.ip) ? "True" : "";
				else
					subitems[3] = qho.push ? "True" : "";
				subitems[4] = qho.busy ? "True" : "";
				subitems[5] = qho.browse ? "True" : "";
				subitems[6] = qho.chat ? "True" : "";
				ListViewItem lvi = new ListViewItem(subitems);
				//add listview item
				listView1.Items.Add(lvi);
			}
		}
	}
}
