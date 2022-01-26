// Transfers.cs
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace FileScope
{
	/// <summary>
	/// Transfers tab.
	/// </summary>
	public class Transfers : System.Windows.Forms.UserControl
	{
		public delegate void newDownload(QueryHitTable qht, int dlNum);
		public delegate void updateDownload(string status, string percent, string speed, int dlNum);
		public delegate void removeDownload(int dlNum);
		public delegate void newUpload(int upNum);
		public delegate void updateUpload(string filename, uint filesize, string status, string percent, string offsets, string speed, int upNum);
		public delegate void removeUpload(int upNum, int type);
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ToolBar toolBar2;
		private System.Windows.Forms.ListView listViewDowns;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ListView listViewUps;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.Timer tmr1;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader12;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ToolBarButton toolDowns1;
		private System.Windows.Forms.ToolBarButton toolDowns2;
		private System.Windows.Forms.ToolBarButton toolDowns3;
		private System.Windows.Forms.ToolBarButton toolDowns4;
		private System.Windows.Forms.ToolBarButton toolDowns5;
		private System.Windows.Forms.ToolBarButton toolUps1;
		private System.Windows.Forms.ToolBarButton toolUps2;
		private System.Windows.Forms.ToolBarButton toolUps3;
		private System.Windows.Forms.ToolBarButton toolUps4;
		private System.Windows.Forms.ToolBarButton toolUps5;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private ElMenuItem menuItem1;
		private ElMenuItem menuItem2;
		private ElMenuItem menuItem3;
		private ElMenuItem menuItem4;
		private ElMenuItem menuItem5;
		private ElMenuItem menuItem6;
		private ElMenuItem menuItem7;
		private ElMenuItem menuItem9;
		private ElMenuItem menuItem10;
		private ElMenuItem menuItem11;
		private System.Windows.Forms.ToolBarButton toolDowns6;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown updownUploads;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ToolBarButton toolDowns7;
		private System.Windows.Forms.ToolBarButton toolUps6;
		private ElMenuItem menuItem8;
		public ContextMenu[] cmArray;

		public Transfers()
		{
			InitializeComponent();
			cmArray = new ContextMenu[]{this.contextMenu1};
			Control c = (Control)this;
			Themes.SetupTheme(c);
			this.updownUploads.Value = Stats.settings.maxUploads;
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Transfers));
			this.panel1 = new System.Windows.Forms.Panel();
			this.listViewDowns = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.toolBar2 = new System.Windows.Forms.ToolBar();
			this.toolDowns1 = new System.Windows.Forms.ToolBarButton();
			this.toolDowns2 = new System.Windows.Forms.ToolBarButton();
			this.toolDowns3 = new System.Windows.Forms.ToolBarButton();
			this.toolDowns4 = new System.Windows.Forms.ToolBarButton();
			this.toolDowns5 = new System.Windows.Forms.ToolBarButton();
			this.toolDowns6 = new System.Windows.Forms.ToolBarButton();
			this.toolDowns7 = new System.Windows.Forms.ToolBarButton();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.panel2 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.updownUploads = new System.Windows.Forms.NumericUpDown();
			this.listViewUps = new System.Windows.Forms.ListView();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolUps1 = new System.Windows.Forms.ToolBarButton();
			this.toolUps2 = new System.Windows.Forms.ToolBarButton();
			this.toolUps3 = new System.Windows.Forms.ToolBarButton();
			this.toolUps4 = new System.Windows.Forms.ToolBarButton();
			this.toolUps5 = new System.Windows.Forms.ToolBarButton();
			this.toolUps6 = new System.Windows.Forms.ToolBarButton();
			this.tmr1 = new System.Windows.Forms.Timer(this.components);
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new FileScope.ElMenuItem();
			this.menuItem2 = new FileScope.ElMenuItem();
			this.menuItem3 = new FileScope.ElMenuItem();
			this.menuItem4 = new FileScope.ElMenuItem();
			this.menuItem5 = new FileScope.ElMenuItem();
			this.menuItem6 = new FileScope.ElMenuItem();
			this.menuItem7 = new FileScope.ElMenuItem();
			this.menuItem9 = new FileScope.ElMenuItem();
			this.menuItem8 = new FileScope.ElMenuItem();
			this.menuItem10 = new FileScope.ElMenuItem();
			this.menuItem11 = new FileScope.ElMenuItem();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.updownUploads)).BeginInit();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.listViewDowns,
																				 this.toolBar2});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(760, 208);
			this.panel1.TabIndex = 0;
			// 
			// listViewDowns
			// 
			this.listViewDowns.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listViewDowns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnHeader1,
																							this.columnHeader2,
																							this.columnHeader3,
																							this.columnHeader4,
																							this.columnHeader5,
																							this.columnHeader6});
			this.listViewDowns.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewDowns.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listViewDowns.FullRowSelect = true;
			this.listViewDowns.GridLines = true;
			this.listViewDowns.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewDowns.HideSelection = false;
			this.listViewDowns.Location = new System.Drawing.Point(0, 41);
			this.listViewDowns.Name = "listViewDowns";
			this.listViewDowns.Size = new System.Drawing.Size(758, 165);
			this.listViewDowns.TabIndex = 1;
			this.listViewDowns.View = System.Windows.Forms.View.Details;
			this.listViewDowns.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listViewDowns_MouseUp);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "File";
			this.columnHeader1.Width = 200;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "FileSize";
			this.columnHeader2.Width = 70;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Status";
			this.columnHeader3.Width = 215;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Percent";
			this.columnHeader4.Width = 54;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Speed";
			this.columnHeader5.Width = 65;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "C#";
			this.columnHeader6.Width = 26;
			// 
			// toolBar2
			// 
			this.toolBar2.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar2.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolDowns1,
																						this.toolDowns2,
																						this.toolDowns3,
																						this.toolDowns4,
																						this.toolDowns5,
																						this.toolDowns6,
																						this.toolDowns7});
			this.toolBar2.DropDownArrows = true;
			this.toolBar2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.toolBar2.ImageList = this.imageList1;
			this.toolBar2.Name = "toolBar2";
			this.toolBar2.ShowToolTips = true;
			this.toolBar2.Size = new System.Drawing.Size(758, 41);
			this.toolBar2.TabIndex = 0;
			this.toolBar2.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBar2.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar2_ButtonClick);
			// 
			// toolDowns1
			// 
			this.toolDowns1.Enabled = false;
			this.toolDowns1.Pushed = true;
			this.toolDowns1.Text = "Downloads (0)";
			// 
			// toolDowns2
			// 
			this.toolDowns2.Enabled = false;
			this.toolDowns2.ImageIndex = 0;
			this.toolDowns2.Text = "Retry";
			this.toolDowns2.ToolTipText = "Reconnect to any busy hosts";
			// 
			// toolDowns3
			// 
			this.toolDowns3.Enabled = false;
			this.toolDowns3.ImageIndex = 1;
			this.toolDowns3.Text = "Cancel";
			this.toolDowns3.ToolTipText = "Cancel selected download(s)";
			// 
			// toolDowns4
			// 
			this.toolDowns4.ImageIndex = 2;
			this.toolDowns4.Text = "Clear";
			this.toolDowns4.ToolTipText = "Clear all inactive downloads";
			// 
			// toolDowns5
			// 
			this.toolDowns5.Enabled = false;
			this.toolDowns5.ImageIndex = 3;
			this.toolDowns5.Text = "More Info";
			this.toolDowns5.ToolTipText = "More info on selected download";
			// 
			// toolDowns6
			// 
			this.toolDowns6.Enabled = false;
			this.toolDowns6.ImageIndex = 5;
			this.toolDowns6.Text = "Chat";
			this.toolDowns6.ToolTipText = "Chat with the hosts you\'re downloading from";
			// 
			// toolDowns7
			// 
			this.toolDowns7.Enabled = false;
			this.toolDowns7.ImageIndex = 6;
			this.toolDowns7.Text = "Browse";
			this.toolDowns7.ToolTipText = "Browse the file libraries of some of the hosts you\'re downloading from";
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(32, 32);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label1,
																				 this.updownUploads,
																				 this.listViewUps,
																				 this.toolBar1});
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 208);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(760, 240);
			this.panel2.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label1.Location = new System.Drawing.Point(520, 5);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Max. Uploads:";
			// 
			// updownUploads
			// 
			this.updownUploads.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.updownUploads.Location = new System.Drawing.Point(528, 20);
			this.updownUploads.Name = "updownUploads";
			this.updownUploads.Size = new System.Drawing.Size(48, 20);
			this.updownUploads.TabIndex = 3;
			this.updownUploads.ValueChanged += new System.EventHandler(this.updownUploads_ValueChanged);
			this.updownUploads.Leave += new System.EventHandler(this.updownUploads_Leave);
			// 
			// listViewUps
			// 
			this.listViewUps.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listViewUps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.columnHeader8,
																						  this.columnHeader9,
																						  this.columnHeader10,
																						  this.columnHeader11,
																						  this.columnHeader12,
																						  this.columnHeader13});
			this.listViewUps.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewUps.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listViewUps.FullRowSelect = true;
			this.listViewUps.GridLines = true;
			this.listViewUps.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewUps.HideSelection = false;
			this.listViewUps.Location = new System.Drawing.Point(0, 41);
			this.listViewUps.Name = "listViewUps";
			this.listViewUps.Size = new System.Drawing.Size(758, 197);
			this.listViewUps.TabIndex = 2;
			this.listViewUps.View = System.Windows.Forms.View.Details;
			this.listViewUps.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listViewUps_MouseUp);
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "File";
			this.columnHeader8.Width = 200;
			// 
			// columnHeader9
			// 
			this.columnHeader9.Text = "FileSize";
			this.columnHeader9.Width = 70;
			// 
			// columnHeader10
			// 
			this.columnHeader10.Text = "Status";
			this.columnHeader10.Width = 155;
			// 
			// columnHeader11
			// 
			this.columnHeader11.Text = "Offsets";
			this.columnHeader11.Width = 115;
			// 
			// columnHeader12
			// 
			this.columnHeader12.Text = "Speed";
			this.columnHeader12.Width = 65;
			// 
			// columnHeader13
			// 
			this.columnHeader13.Text = "C#";
			this.columnHeader13.Width = 26;
			// 
			// toolBar1
			// 
			this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolUps1,
																						this.toolUps2,
																						this.toolUps3,
																						this.toolUps4,
																						this.toolUps5,
																						this.toolUps6});
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.toolBar1.ImageList = this.imageList1;
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(758, 41);
			this.toolBar1.TabIndex = 0;
			this.toolBar1.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolUps1
			// 
			this.toolUps1.Enabled = false;
			this.toolUps1.Pushed = true;
			this.toolUps1.Text = "Uploads (0)";
			// 
			// toolUps2
			// 
			this.toolUps2.Enabled = false;
			this.toolUps2.ImageIndex = 1;
			this.toolUps2.Text = "Cancel";
			this.toolUps2.ToolTipText = "Cancel selected upload(s)";
			// 
			// toolUps3
			// 
			this.toolUps3.ImageIndex = 2;
			this.toolUps3.Text = "Clear";
			this.toolUps3.ToolTipText = "Clear inactive uploads";
			// 
			// toolUps4
			// 
			this.toolUps4.Enabled = false;
			this.toolUps4.ImageIndex = 3;
			this.toolUps4.Text = "More Info";
			this.toolUps4.ToolTipText = "More info on selected upload";
			// 
			// toolUps5
			// 
			this.toolUps5.Enabled = false;
			this.toolUps5.ImageIndex = 5;
			this.toolUps5.Text = "Chat";
			this.toolUps5.ToolTipText = "Chat with selected host";
			// 
			// toolUps6
			// 
			this.toolUps6.Enabled = false;
			this.toolUps6.ImageIndex = 6;
			this.toolUps6.Text = "Browse";
			this.toolUps6.ToolTipText = "Browse the file library of this host";
			// 
			// tmr1
			// 
			this.tmr1.Enabled = true;
			this.tmr1.Interval = 200;
			this.tmr1.Tick += new System.EventHandler(this.tmr1_Tick);
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItem1,
																						 this.menuItem2,
																						 this.menuItem3,
																						 this.menuItem4,
																						 this.menuItem5,
																						 this.menuItem6,
																						 this.menuItem7,
																						 this.menuItem9,
																						 this.menuItem8,
																						 this.menuItem10,
																						 this.menuItem11});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "Retry";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.Text = "Cancel";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 2;
			this.menuItem3.Text = "Cancel/Delete";
			this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 3;
			this.menuItem4.Text = "-";
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 4;
			this.menuItem5.Text = "Clear Inactive";
			this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 5;
			this.menuItem6.Text = "-";
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 6;
			this.menuItem7.Text = "More Info";
			this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 7;
			this.menuItem9.Text = "Chat";
			this.menuItem9.Click += new System.EventHandler(this.menuItem9_Click);
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 8;
			this.menuItem8.Text = "Browse host";
			this.menuItem8.Click += new System.EventHandler(this.menuItem8_Click);
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 9;
			this.menuItem10.Text = "-";
			// 
			// menuItem11
			// 
			this.menuItem11.Index = 10;
			this.menuItem11.Text = "Find More Sources";
			this.menuItem11.Click += new System.EventHandler(this.menuItem11_Click);
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.Color.Black;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = new System.Drawing.Point(0, 208);
			this.splitter1.MinExtra = 3;
			this.splitter1.MinSize = 3;
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(760, 3);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			this.splitter1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter1_SplitterMoved);
			// 
			// Transfers
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.splitter1,
																		  this.panel2,
																		  this.panel1});
			this.Name = "Transfers";
			this.Size = new System.Drawing.Size(760, 448);
			this.Resize += new System.EventHandler(this.Transfers_Resize);
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.updownUploads)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void tmr1_Tick(object sender, System.EventArgs e)
		{
			try
			{
				//update the download and upload counts
				toolBar2.Buttons[0].Text = "Downloads (" + Stats.Updated.downloadsNow.ToString() + ")";
				toolBar1.Buttons[0].Text = "Uploads (" + Stats.Updated.uploadsNow.ToString() + ")";
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Transfers tmr1_Tick");
			}
		}

		/// <summary>
		/// New download to add to the listview.
		/// </summary>
		public void NewDownload(QueryHitTable qht, int dlNum)
		{
			//create the item to add
			string[] items = new string[6];
			QueryHitObject qho = (QueryHitObject)qht.queryHitObjects[0];
			items[0] = qho.fileName;
			items[1] = Utils.Assemble(Convert.ToUInt32(Math.Round((double)qho.fileSize / 1024)), " KB");
			items[2] = "Connecting to " + qht.queryHitObjects.Count.ToString() + " hosts";
			items[3] = "0%";
			items[4] = "0 KB/s";
			items[5] = dlNum.ToString();
			ListViewItem lvi = new ListViewItem(items);
			//add the item
			lvi.ForeColor = Stats.settings.clHighlight4;
			listViewDowns.Items.Add(lvi);
			if(Stats.settings.switchTransfers && Stats.Updated.opened)
				StartApp.main.tabControl1.SelectedTab = StartApp.main.tabPage5;
		}

		/// <summary>
		/// Update the status of transfer, percentage of file done, and speed of transfer.
		/// </summary>
		public void UpdateDownload(string status, string percent, string speed, int dlNum)
		{
			foreach(ListViewItem lvi in listViewDowns.Items)
				if(lvi.SubItems[5].Text == dlNum.ToString())
				{
					lvi.SubItems[2].Text = status;
					lvi.SubItems[3].Text = percent;
					lvi.SubItems[4].Text = speed;
					string elPercentage = percent.Replace("%", "");
					double dblElPercentage = Convert.ToDouble(elPercentage);
					if(dblElPercentage >= 75)
						lvi.ForeColor = Stats.settings.clHighlight1;
					else if(dblElPercentage >= 50)
						lvi.ForeColor = Stats.settings.clHighlight2;
					else if(dblElPercentage >= 25)
						lvi.ForeColor = Stats.settings.clHighlight3;
					else
						lvi.ForeColor = Stats.settings.clHighlight4;
					return;
				}
		}

		/// <summary>
		/// Discontinued download.
		/// </summary>
		public void RemoveDownload(int dlNum)
		{
			foreach(ListViewItem lvi in listViewDowns.Items)
				if(lvi.SubItems[5].Text == dlNum.ToString())
				{
					if(Stats.settings.clearDl)
						listViewDowns.Items.Remove(lvi);
					else
					{
						lvi.SubItems[2].Text = "Completed";
						lvi.SubItems[3].Text = "100%";
						lvi.SubItems[4].Text = "";
						lvi.SubItems[5].Text = "";
					}
					SetupToolbarDowns();
					return;
				}
		}

		/// <summary>
		/// New upload to add to the listview.
		/// </summary>
		public void NewUpload(int upNum)
		{
			//create the ListViewItem to add
			string[] items = new string[6];
			items[0] = "";
			items[1] = "";
			items[2] = "Negotiating";
			items[3] = "-";
			items[4] = "0 KB/s";
			items[5] = upNum.ToString();
			ListViewItem lvi = new ListViewItem(items);
			//add the item
			lvi.ForeColor = Stats.settings.clHighlight4;
			listViewUps.Items.Add(lvi);
		}

		/// <summary>
		/// Update the status of the upload.
		/// </summary>
		public void UpdateUpload(string filename, uint filesize, string status, string percent, string offsets, string speed, int upNum)
		{
			foreach(ListViewItem lvi in listViewUps.Items)
				if(lvi.SubItems[5].Text == upNum.ToString())
				{
					if(filename != "")
						lvi.SubItems[0].Text = filename;
					lvi.SubItems[1].Text = Utils.Assemble(Convert.ToUInt32(Math.Round((double)filesize / 1024)), " KB");
					if(status != "")
						lvi.SubItems[2].Text = status;
					if(offsets != "")
						lvi.SubItems[3].Text = offsets;
					if(speed != "")
						lvi.SubItems[4].Text = speed;
					string elPercentage = percent.Replace("%", "");
					double dblElPercentage = Convert.ToDouble(elPercentage);
					if(dblElPercentage >= 75)
						lvi.ForeColor = Stats.settings.clHighlight1;
					else if(dblElPercentage >= 50)
						lvi.ForeColor = Stats.settings.clHighlight2;
					else if(dblElPercentage >= 25)
						lvi.ForeColor = Stats.settings.clHighlight3;
					else
						lvi.ForeColor = Stats.settings.clHighlight4;
					return;
				}
		}

		/// <summary>
		/// Discontinued upload.
		/// </summary>
		public void RemoveUpload(int upNum, int type)
		{
			foreach(ListViewItem lvi in listViewUps.Items)
				if(lvi.SubItems[5].Text == upNum.ToString())
				{
					if(type == 0 || Stats.settings.clearUp)
						listViewUps.Items.Remove(lvi);
					else
					{
						lvi.SubItems[2].Text = "Done";
						if(UploadManager.ups[upNum] != null && UploadManager.ups[upNum].ipStored.Length > 0)
							lvi.SubItems[2].Text += " (" + UploadManager.ups[upNum].ipStored + ")";
						lvi.SubItems[3].Text = "";
						lvi.SubItems[4].Text = "";
						lvi.SubItems[5].Text = "";
					}
					SetupToolbarUps();
					return;
				}
		}

		/// <summary>
		/// Store where the context menu corresponds to.
		/// 0 for listViewDowns.
		/// 1 for listViewUps.
		/// </summary>
		int curMenu = 0;

		private void listViewDowns_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			SetupToolbarDowns();

			//popup menu on right click
			if(e.Button == MouseButtons.Right)
			{
				curMenu = 0;
				System.Drawing.Point pos = new System.Drawing.Point(e.X, e.Y);
				this.contextMenu1.Show(listViewDowns, pos);
			}
		}

		void SetupToolbarDowns()
		{
			try
			{
				if(listViewDowns.SelectedItems.Count == 0)
					listViewDowns.HideSelection = true;
				else
					listViewDowns.HideSelection = false;
				//enable/disable stuff
				switch(listViewDowns.SelectedItems.Count)
				{
					case 0:
						toolDowns2.Enabled = false;
						toolDowns3.Enabled = false;
						toolDowns5.Enabled = false;
						toolDowns6.Enabled = false;
						toolDowns7.Enabled = false;
						menuItem1.Enabled = false;
						menuItem2.Enabled = false;
						menuItem3.Enabled = false;
						menuItem7.Enabled = false;
						menuItem8.Enabled = false;
						menuItem9.Enabled = false;
						menuItem11.Enabled = false;
						break;
					case 1:
						if(listViewDowns.SelectedItems[0].SubItems[5].Text != "")
						{
							toolDowns2.Enabled = true;
							toolDowns3.Enabled = true;
							toolDowns5.Enabled = true;
							toolDowns6.Enabled = true;
							toolDowns7.Enabled = true;
							menuItem1.Enabled = true;
							menuItem2.Enabled = true;
							menuItem3.Enabled = true;
							menuItem7.Enabled = true;
							menuItem8.Enabled = true;
							menuItem9.Enabled = true;
							menuItem11.Enabled = true;
						}
						else
						{
							toolDowns2.Enabled = false;
							toolDowns3.Enabled = false;
							toolDowns5.Enabled = false;
							toolDowns6.Enabled = false;
							toolDowns7.Enabled = false;
							menuItem1.Enabled = false;
							menuItem2.Enabled = false;
							menuItem3.Enabled = false;
							menuItem7.Enabled = false;
							menuItem8.Enabled = false;
							menuItem9.Enabled = false;
							menuItem11.Enabled = false;
						}
						break;
					default:
						bool activeItemsFound = false;
						foreach(ListViewItem lvi in listViewDowns.Items)
							if(lvi.SubItems[5].Text != "")
							{
								activeItemsFound = true;
								break;
							}
						if(activeItemsFound)
						{
							toolDowns2.Enabled = true;
							toolDowns3.Enabled = true;
							toolDowns5.Enabled = false;
							toolDowns6.Enabled = false;
							toolDowns7.Enabled = false;
							menuItem1.Enabled = true;
							menuItem2.Enabled = true;
							menuItem3.Enabled = true;
							menuItem7.Enabled = false;
							menuItem8.Enabled = false;
							menuItem9.Enabled = false;
							menuItem11.Enabled = true;
						}
						else
						{
							toolDowns2.Enabled = false;
							toolDowns3.Enabled = false;
							toolDowns5.Enabled = false;
							toolDowns6.Enabled = false;
							toolDowns7.Enabled = false;
							menuItem1.Enabled = false;
							menuItem2.Enabled = false;
							menuItem3.Enabled = false;
							menuItem7.Enabled = false;
							menuItem8.Enabled = false;
							menuItem9.Enabled = false;
							menuItem11.Enabled = false;
						}
						break;
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Transfers SetupToolbarDowns");
			}
		}

		private void listViewUps_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			SetupToolbarUps();

			//popup menu on right click
			if(e.Button == MouseButtons.Right)
			{
				curMenu = 1;
				System.Drawing.Point pos = new System.Drawing.Point(e.X, e.Y);
				this.contextMenu1.Show(listViewUps, pos);
			}
		}

		void SetupToolbarUps()
		{
			try
			{
				if(listViewUps.SelectedItems.Count == 0)
					listViewUps.HideSelection = true;
				else
					listViewUps.HideSelection = false;
				//enable/disable stuff
				switch(listViewUps.SelectedItems.Count)
				{
					case 0:
						toolUps2.Enabled = false;
						toolUps4.Enabled = false;
						toolUps5.Enabled = false;
						toolUps6.Enabled = false;
						menuItem1.Enabled = false;
						menuItem2.Enabled = false;
						menuItem3.Enabled = false;
						menuItem7.Enabled = false;
						menuItem8.Enabled = false;
						menuItem9.Enabled = false;
						menuItem11.Enabled = false;
						break;
					case 1:
						if(listViewUps.SelectedItems[0].SubItems[5].Text != "")
						{
							toolUps2.Enabled = true;
							toolUps4.Enabled = true;
							toolUps5.Enabled = true;
							toolUps6.Enabled = true;
							menuItem1.Enabled = false;
							menuItem2.Enabled = true;
							menuItem3.Enabled = false;
							menuItem7.Enabled = true;
							menuItem8.Enabled = true;
							menuItem9.Enabled = true;
							menuItem11.Enabled = false;
						}
						else
						{
							toolUps2.Enabled = false;
							toolUps4.Enabled = false;
							toolUps5.Enabled = false;
							toolUps6.Enabled = false;
							menuItem1.Enabled = false;
							menuItem2.Enabled = false;
							menuItem3.Enabled = false;
							menuItem7.Enabled = false;
							menuItem8.Enabled = false;
							menuItem9.Enabled = false;
							menuItem11.Enabled = false;
						}
						break;
					default:
						bool activeItemsFound = false;
						foreach(ListViewItem lvi in listViewUps.Items)
							if(lvi.SubItems[5].Text != "")
							{
								activeItemsFound = true;
								break;
							}
						if(activeItemsFound)
						{
							toolUps2.Enabled = true;
							toolUps4.Enabled = false;
							toolUps5.Enabled = false;
							toolUps6.Enabled = false;
							menuItem1.Enabled = false;
							menuItem2.Enabled = true;
							menuItem3.Enabled = false;
							menuItem7.Enabled = false;
							menuItem8.Enabled = false;
							menuItem9.Enabled = false;
							menuItem11.Enabled = false;
						}
						else
						{
							toolUps2.Enabled = false;
							toolUps4.Enabled = false;
							toolUps5.Enabled = false;
							toolUps6.Enabled = false;
							menuItem1.Enabled = false;
							menuItem2.Enabled = false;
							menuItem3.Enabled = false;
							menuItem7.Enabled = false;
							menuItem8.Enabled = false;
							menuItem9.Enabled = false;
							menuItem11.Enabled = false;
						}
						break;
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Transfers SetupToolbarUps");
			}
		}

		private void toolBar2_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(e.Button.Text)
			{
				case "Retry":
					DownRetry();
					break;
				case "Cancel":
					DownCancel(false);
					break;
				case "Clear":
					DownClear();
					break;
				case "More Info":
					DownMoreInfo();
					break;
				case "Chat":
					DownChat();
					break;
				case "Browse":
					DownBrowse();
					break;
			}
		}

		/// <summary>
		/// Force reconnect to all hosts not downloading for each DownloadManager instance.
		/// </summary>
		void DownRetry()
		{
			if(listViewDowns.SelectedItems.Count == 0)
				return;

			foreach(ListViewItem lvi in listViewDowns.SelectedItems)
				if(lvi.SubItems[5].Text != "")
				{
					int dlNum = Convert.ToInt32(lvi.SubItems[5].Text);
					if(!DownloadManager.dms[dlNum].active)
						return;
					DownloadManager.dms[dlNum].RetryAll();
				}
		}

		/// <summary>
		/// Cancel all selected downloads.
		/// </summary>
		void DownCancel(bool delete)
		{
			if(listViewDowns.SelectedItems.Count == 0)
				return;

			//show alert if enabled
			if(Stats.settings.cancelDLAlert)
			{
				DialogResult dr = MessageBox.Show("Are you sure you want to cancel the download(s)?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if(dr.ToString().ToLower() == "no")
					return;
			}

			foreach(ListViewItem lvi in listViewDowns.SelectedItems)
				if(lvi.SubItems[5].Text != "")
				{
					if(Stats.settings.clearDl)
					{
						int dlNum = Convert.ToInt32(lvi.SubItems[5].Text);
						DownloadManager.dms[dlNum].CancelDownload(delete);
						listViewDowns.Items.Remove(lvi);
					}
					else
					{
						int dlNum = Convert.ToInt32(lvi.SubItems[5].Text);
						DownloadManager.dms[dlNum].CancelDownload(delete);
						lvi.SubItems[2].Text = "Cancelled";
						lvi.SubItems[4].Text = "";
						lvi.SubItems[5].Text = "";
					}
				}
			SetupToolbarDowns();
		}

		/// <summary>
		/// Remove all discontinued downloads.
		/// </summary>
		void DownClear()
		{
			foreach(ListViewItem lvi in listViewDowns.Items)
				if(lvi.SubItems[5].Text == "")
					listViewDowns.Items.Remove(lvi);
			SetupToolbarDowns();
		}

		/// <summary>
		/// Bring up a "more info" dialog for the selected download.
		/// </summary>
		void DownMoreInfo()
		{
			if(listViewDowns.SelectedItems.Count == 0)
				return;

			if(listViewDowns.SelectedItems[0].SubItems[5].Text != "")
			{
				int dlNum = Convert.ToInt32(listViewDowns.SelectedItems[0].SubItems[5].Text);
				TransfersMoreInfoDlg tmid = new TransfersMoreInfoDlg();
				tmid.SetupViewDownload(dlNum);
				tmid.ShowDialog();
				tmid.CleanUp();
			}
		}

		/// <summary>
		/// Chat with the hosts you're downloading from.
		/// </summary>
		void DownChat()
		{
			if(listViewDowns.SelectedItems.Count == 0)
				return;

			if(listViewDowns.SelectedItems[0].SubItems[5].Text != "")
			{
				int dlNum = Convert.ToInt32(listViewDowns.SelectedItems[0].SubItems[5].Text);
				ChattersDlg cd = new ChattersDlg();
				cd.SetupWindow(DownloadManager.dms[dlNum]);
				cd.ShowDialog();
			}
		}

		/// <summary>
		/// Browse the hosts you're downloading from.
		/// </summary>
		void DownBrowse()
		{
			if(listViewDowns.SelectedItems.Count == 0)
				return;

			if(listViewDowns.SelectedItems[0].SubItems[5].Text != "")
			{
				int dlNum = Convert.ToInt32(listViewDowns.SelectedItems[0].SubItems[5].Text);
				BrowseDlg bd = new BrowseDlg();
				bd.SetupWindow(DownloadManager.dms[dlNum]);
				bd.ShowDialog();
			}
		}

		/// <summary>
		/// Send out a requery or at least increase requery interval.
		/// </summary>
		void DownMoreSources()
		{
			if(listViewDowns.SelectedItems.Count == 0)
				return;

			foreach(ListViewItem lvi in listViewDowns.SelectedItems)
				if(lvi.SubItems[5].Text != "")
				{
					int dlNum = Convert.ToInt32(lvi.SubItems[5].Text);
					if(!DownloadManager.dms[dlNum].active)
						return;
					ReQuery.FindMoreSourcesRequested(dlNum);
				}
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(e.Button.Text)
			{
				case "Cancel":
					UpCancel();
					break;
				case "Clear":
					UpClear();
					break;
				case "More Info":
					UpMoreInfo();
					break;
				case "Chat":
					UpChat();
					break;
				case "Browse":
					UpBrowse();
					break;
			}
		}

		/// <summary>
		/// Cancel all selected items.
		/// </summary>
		void UpCancel()
		{
			foreach(ListViewItem lvi in listViewUps.SelectedItems)
				if(lvi.SubItems[5].Text != "")
					UploadManager.ups[Convert.ToInt32(lvi.SubItems[5].Text)].StopEverything("UpCancel");
			SetupToolbarUps();
		}

		/// <summary>
		/// Remove all discontinued uploads.
		/// </summary>
		void UpClear()
		{
			foreach(ListViewItem lvi in listViewUps.Items)
				if(lvi.SubItems[5].Text == "")
					listViewUps.Items.Remove(lvi);
			SetupToolbarUps();
		}

		/// <summary>
		/// More info on the currently selected upload.
		/// </summary>
		void UpMoreInfo()
		{
			if(listViewUps.SelectedItems.Count == 0)
				return;

			if(listViewUps.SelectedItems[0].SubItems[5].Text != "")
			{
				int upNum = Convert.ToInt32(listViewUps.SelectedItems[0].SubItems[5].Text);
				TransfersMoreInfoDlg tmid = new TransfersMoreInfoDlg();
				tmid.SetupViewUpload(upNum);
				tmid.ShowDialog();
				tmid.CleanUp();
			}
		}

		/// <summary>
		/// Chat with the host you're uploading a file to.
		/// </summary>
		void UpChat()
		{
			if(listViewUps.SelectedItems.Count == 0)
				return;

			if(listViewUps.SelectedItems[0].SubItems[5].Text != "")
			{
				int upNum = Convert.ToInt32(listViewUps.SelectedItems[0].SubItems[5].Text);
				ChattersDlg cd = new ChattersDlg();
				cd.SetupWindow(UploadManager.ups[upNum]);
				cd.ShowDialog();
			}
		}

		/// <summary>
		/// Browse with the host you're uploading a file to.
		/// </summary>
		void UpBrowse()
		{
			if(listViewUps.SelectedItems.Count == 0)
				return;

			if(listViewUps.SelectedItems[0].SubItems[5].Text != "")
			{
				int upNum = Convert.ToInt32(listViewUps.SelectedItems[0].SubItems[5].Text);
				BrowseDlg bd = new BrowseDlg();
				bd.SetupWindow(UploadManager.ups[upNum]);
				bd.ShowDialog();
			}
		}

		private void menuItem1_Click(object sender, System.EventArgs e)
		{
			if(curMenu == 0)
				DownRetry();
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			if(curMenu == 0)
				DownCancel(false);
			else
				UpCancel();
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			if(curMenu == 0)
				DownCancel(true);
			else
				UpCancel();
		}

		private void menuItem5_Click(object sender, System.EventArgs e)
		{
			if(curMenu == 0)
				DownClear();
			else
				UpClear();
		}

		private void menuItem7_Click(object sender, System.EventArgs e)
		{
			if(curMenu == 0)
				DownMoreInfo();
			else
				UpMoreInfo();
		}

		private void menuItem8_Click(object sender, System.EventArgs e)
		{
			if(curMenu == 0)
				DownBrowse();
			else
				UpBrowse();
		}

		private void menuItem9_Click(object sender, System.EventArgs e)
		{
			if(curMenu == 0)
				DownChat();
			else
				UpChat();
		}

		private void menuItem11_Click(object sender, System.EventArgs e)
		{
			if(curMenu == 0)
				DownMoreSources();
		}

		private void updownUploads_Leave(object sender, System.EventArgs e)
		{
			updownStuff();
		}

		private void updownUploads_ValueChanged(object sender, System.EventArgs e)
		{
			updownStuff();
		}

		void updownStuff()
		{
			if(Convert.ToInt32(updownUploads.Value) > 100)
				updownUploads.Value = 100;
			else if(Convert.ToInt32(updownUploads.Value) < 0)
				updownUploads.Value = 0;
			Stats.settings.maxUploads = (int)updownUploads.Value;
		}

		private void Transfers_Resize(object sender, System.EventArgs e)
		{
			try
			{
				panel1.Height = (int)(this.ClientSize.Height * Stats.settings.transSplitPerc);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("SetupToolbarUps");
			}
		}

		private void splitter1_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
		{
			Stats.settings.transSplitPerc = (float)panel1.Height / this.ClientSize.Height;
		}
	}
}
