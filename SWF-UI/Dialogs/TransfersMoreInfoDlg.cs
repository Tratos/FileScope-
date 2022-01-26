// TransfersMoreInfoDlg.cs
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
	/// More info window for both downloads and uploads.
	/// </summary>
	public class TransfersMoreInfoDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ListView listView2;
		private System.Windows.Forms.Timer timer1;
		private System.ComponentModel.IContainer components;
		bool download;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label label6;
		int transferIndex;

		public TransfersMoreInfoDlg()
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
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.label5 = new System.Windows.Forms.Label();
			this.listView2 = new System.Windows.Forms.ListView();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label6 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label1.Location = new System.Drawing.Point(72, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(41, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Name: ";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label2.Location = new System.Drawing.Point(72, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(32, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "Size: ";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label3.Location = new System.Drawing.Point(72, 40);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(69, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "SHA1 Hash: ";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(0, 72);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(86, 19);
			this.label4.TabIndex = 3;
			this.label4.Text = "Transfers:";
			// 
			// listView1
			// 
			this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1,
																						this.columnHeader2,
																						this.columnHeader3});
			this.listView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listView1.FullRowSelect = true;
			this.listView1.GridLines = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.Location = new System.Drawing.Point(0, 96);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(360, 280);
			this.listView1.TabIndex = 4;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Host";
			this.columnHeader1.Width = 120;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Vendor";
			this.columnHeader2.Width = 110;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Status";
			this.columnHeader3.Width = 120;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(360, 72);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(68, 19);
			this.label5.TabIndex = 5;
			this.label5.Text = "Offsets:";
			// 
			// listView2
			// 
			this.listView2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader4,
																						this.columnHeader5});
			this.listView2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listView2.FullRowSelect = true;
			this.listView2.GridLines = true;
			this.listView2.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView2.Location = new System.Drawing.Point(360, 96);
			this.listView2.MultiSelect = false;
			this.listView2.Name = "listView2";
			this.listView2.Size = new System.Drawing.Size(152, 280);
			this.listView2.TabIndex = 6;
			this.listView2.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Start";
			this.columnHeader4.Width = 70;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "End";
			this.columnHeader5.Width = 70;
			// 
			// timer1
			// 
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(60, 60);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 7;
			this.pictureBox1.TabStop = false;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label6.Location = new System.Drawing.Point(72, 56);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(64, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "MD4 Hash: ";
			// 
			// TransfersMoreInfoDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(552, 390);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label6,
																		  this.label5,
																		  this.label4,
																		  this.label2,
																		  this.label1,
																		  this.label3,
																		  this.pictureBox1,
																		  this.listView2,
																		  this.listView1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "TransfersMoreInfoDlg";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "More Info";
			this.Resize += new System.EventHandler(this.TransfersMoreInfoDlg_Resize);
			this.Load += new System.EventHandler(this.TransfersMoreInfoDlg_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Create this more info window for a download.
		/// </summary>
		public void SetupViewDownload(int dlNum)
		{
			transferIndex = dlNum;
			download = true;
			timer1.Interval = 50;
			timer1.Start();

			if(DownloadManager.dms[transferIndex] == null)
				return;
			if(!DownloadManager.dms[transferIndex].active)
				return;

			//constant values
			Downloader elDLer = (Downloader)DownloadManager.dms[dlNum].downloaders[0];
			label1.Text += elDLer.qho.fileName;
			label2.Text += Utils.Assemble(elDLer.qho.fileSize, " bytes");
			label3.Text += DownloadManager.dms[dlNum].sha1.Substring(DownloadManager.dms[dlNum].sha1.LastIndexOf(":")+1);
			if(DownloadManager.dms[dlNum].md4sum.Length > 0)
				label6.Text += DownloadManager.dms[dlNum].md4sum;
		}

		/// <summary>
		/// Create this more info window for an upload.
		/// </summary>
		public void SetupViewUpload(int upNum)
		{
			transferIndex = upNum;
			download = false;
			timer1.Interval = 50;
			timer1.Start();
		}

		void UpdateUploadInfo(int fIndex)
		{
			if(fIndex >= 0 && label1.Text == "Name: ")
			{
				lock(Stats.fileList)
				{
					label1.Text += System.IO.Path.GetFileName(((FileObject)Stats.fileList[fIndex]).location);
					label2.Text += Utils.Assemble(((FileObject)Stats.fileList[fIndex]).b, " bytes");
					label3.Text += ((FileObject)Stats.fileList[fIndex]).sha1;
					if(((FileObject)Stats.fileList[fIndex]).md4 != null)
						label6.Text += Utils.HexGuid(((FileObject)Stats.fileList[fIndex]).md4);
				}
			}
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			try
			{
				listView1.BeginUpdate();
				listView2.BeginUpdate();
				timer1.Interval = 1000;
				if(download)
				{
					if(DownloadManager.dms[transferIndex] == null)
					{
						for(int sdf = 0; sdf < listView1.Items.Count; sdf++)
							listView1.Items[sdf].SubItems[2].Text = "Done";
						listView1.EndUpdate();
						listView2.EndUpdate();
						return;
					}
					if(!DownloadManager.dms[transferIndex].active)
					{
						for(int sdf = 0; sdf < listView1.Items.Count; sdf++)
							listView1.Items[sdf].SubItems[2].Text = "Done";
						listView1.EndUpdate();
						listView2.EndUpdate();
						return;
					}
					int lastIndex = -1;
					foreach(Downloader d in DownloadManager.dms[transferIndex].downloaders)
					{
						string[] items = new string[3];
						if(d.qho.networkType == NetworkType.Gnutella1)
							items[0] = d.qho.ip + ":" + d.qho.port.ToString();
						else
							items[0] = d.qho.ip;
						items[1] = d.qho.vendor;
						if(d.state == DLState.Connected)
						{
							if(d.lastMessage == "Queued")
								items[2] = "Queued at #" + (d.queueState > 0 ? d.queueState.ToString() : "?");
							else
								items[2] = "Negotiating";
						}
						else if(d.state == DLState.Connecting)
							items[2] = "Connecting";
						else if(d.state == DLState.CouldNotConnect)
						{
							if(d.lastMessage != "")
								items[2] = d.lastMessage;
							else
								items[2] = "Could Not Connect";
						}
						else if(d.state == DLState.Downloading)
							items[2] = "Downloading";
						else if(d.state == DLState.SentPush)
							items[2] = "Sent Push";
						else if(d.state == DLState.Waiting)
						{
							if(d.qho.networkType == NetworkType.OpenNap)
							{
								items[2] = "Server Busy";
							}
							else
							{
								if(d.lastMessage != "")
									items[2] = d.lastMessage + " " + d.count.ToString() + "s";
								else
								{
									if(d.queueState != 0)
										items[2] = "Queued at #" + (d.queueState > 0 ? d.queueState.ToString() : "?");
									else if(d.reConnect.Interval == 1000)
										items[2] = "Waiting " + d.count.ToString() + "s";
									else
										items[2] = "Waiting";
								}
							}
						}
						lastIndex = d.dlNum;
						if(listView1.Items.Count <= lastIndex)
						{
							ListViewItem lvi = new ListViewItem(items);
							listView1.Items.Add(lvi);
						}
						else if(!LVICompare.Compare(listView1.Items[lastIndex], items))
						{
							listView1.Items[lastIndex].SubItems[0].Text = items[0];
							listView1.Items[lastIndex].SubItems[1].Text = items[1];
							listView1.Items[lastIndex].SubItems[2].Text = items[2];
						}
					}
					//remove trailing items
					while(listView1.Items.Count > lastIndex + 1)
						listView1.Items.RemoveAt(listView1.Items.Count - 1);

					lastIndex = -1;
					if(DownloadManager.dms[transferIndex].endpoints.Count > 0)
						for(int x = 0; x < DownloadManager.dms[transferIndex].endpoints.Count; x++)
						{
							uint start = (uint)DownloadManager.dms[transferIndex].endpoints.GetKey(x);
							EndPoint ep = (EndPoint)DownloadManager.dms[transferIndex].endpoints.GetByIndex(x);
							uint stop = ep.endOffset;
							string[] items = new string[2];
							items[0] = start.ToString();
							items[1] = stop.ToString();
							lastIndex = x;
							if(listView2.Items.Count <= lastIndex)
							{
								ListViewItem lvi = new ListViewItem(items);
								listView2.Items.Add(lvi);
							}
							else if(!LVICompare.Compare(listView2.Items[lastIndex], items))
							{
								listView2.Items[lastIndex].SubItems[0].Text = items[0];
								listView2.Items[lastIndex].SubItems[1].Text = items[1];
							}
						}
					//remove trailing items
					while(listView2.Items.Count > lastIndex + 1)
						listView2.Items.RemoveAt(listView2.Items.Count - 1);
				}
				else
				{
					if(UploadManager.ups[transferIndex] == null)
					{
						if(listView1.Items.Count > 0)
							listView1.Items[0].SubItems[2].Text = "Done";
						listView1.EndUpdate();
						listView2.EndUpdate();
						return;
					}
					if(!UploadManager.ups[transferIndex].active)
					{
						if(listView1.Items.Count > 0)
							listView1.Items[0].SubItems[2].Text = "Done";
						listView1.EndUpdate();
						listView2.EndUpdate();
						return;
					}
					listView1.Items.Clear();
					listView2.Items.Clear();

					UpdateUploadInfo(UploadManager.ups[transferIndex].fileIndex);

					string[] items = new string[3];
					items[0] = UploadManager.ups[transferIndex].RemoteIP();
					items[1] = UploadManager.ups[transferIndex].userAgent;
					items[2] = UploadManager.ups[transferIndex].ready ? "Uploading" : "Negotiating";
					ListViewItem lvi = new ListViewItem(items);
					listView1.Items.Add(lvi);

					string[] items2 = new string[2];
					items2[0] = UploadManager.ups[transferIndex].start.ToString();
					items2[1] = Convert.ToString(UploadManager.ups[transferIndex].stop + 1 - UploadManager.ups[transferIndex].bytes_to_send);
					ListViewItem lvi2 = new ListViewItem(items2);
					listView2.Items.Add(lvi2);
				}
				listView1.EndUpdate();
				listView2.EndUpdate();
			}
			catch(Exception excep)
			{
				System.Diagnostics.Debug.WriteLine("TransfersMoreInfoDlg timer: " + excep.Message);
				listView1.Clear();
				listView2.Clear();
				listView1.EndUpdate();
				listView2.EndUpdate();
			}
		}

		public void CleanUp()
		{
			this.timer1.Stop();
			this.timer1.Tick -= new System.EventHandler(this.timer1_Tick);
		}

		private void TransfersMoreInfoDlg_Load(object sender, System.EventArgs e)
		{
			pictureBox1.Image = StartApp.main.pictureMoreInfo.Image;
		}

		private void TransfersMoreInfoDlg_Resize(object sender, System.EventArgs e)
		{
			listView2.Left = listView1.Width = label5.Left = this.ClientSize.Width - listView2.Width;
			listView2.Height = listView1.Height = this.ClientSize.Height - listView1.Top;
		}
	}
}
