// Connection.cs
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
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace FileScope
{
	/// <summary>
	/// Connection tab.
	/// </summary>
	public class Connection : System.Windows.Forms.UserControl
	{
		private FileScope.ElTabControl tabControl1;
		public delegate void oJustConnected(int sockNum);
		public delegate void oJustDisconnected(int sockNum);
		public delegate void oUpdateStats(int sockNum, string users, string files, string gigs);
		public delegate void oMessage(string serverNum, string message);
		public delegate void oConnectRandomOpenNap();
		public delegate void eGetRandomEDonkey(IPandPort ipap);
		public delegate void eJustConnected(int sockNum);
		public delegate void eJustDisconnected(int sockNum);
		public delegate void eDonkeyAdd(string server, string serverName, string serverDesc);
		public delegate void eMessage(string serverNum, string message);
		public delegate void eUpdateStats(int sockNum);
		public delegate void updateUltrapeerStatus();
		public delegate void gJustDisconnected(int sckNum);
		public delegate void gJustConnected(int sckNum, string type, string vendor);
		public delegate void gBandwidth(int sckNum, string bIn, string bOut);
		public delegate void gNewConnection(string host, string route, int sckNum);
		public delegate void gQuery(string query);
		public delegate void g2Query(string query);
		public delegate void g2NewConnection(string addr, int sockNum);
		public delegate void g2JustConnected(int sockNum);
		public delegate void g2Update(int sockNum, string bwInOut);
		public delegate void g2JustDisconnected(int sockNum);
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown updownConnections1;
		private System.Windows.Forms.Label labelStatus1;
		public System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private ElMenuItem menuItem1;
		public System.Windows.Forms.ListView listView2;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ToolBarButton toolBarButton4;
		private System.Windows.Forms.ToolBarButton toolBarButton3;
		private System.Windows.Forms.ToolBarButton toolBarButton5;
		private System.Windows.Forms.ToolBarButton toolBarButton6;
		private System.Windows.Forms.ToolBarButton toolBarButton7;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.ColumnHeader columnHeader12;
		private System.Windows.Forms.ColumnHeader columnHeader13;
		private System.Windows.Forms.ContextMenu contextMenu2;
		private ElMenuItem menuItem2;
		private ElMenuItem menuItem3;
		private ElMenuItem menuItem4;
		private ElMenuItem menuItem5;
		private ElMenuItem menuItem6;
		private ElMenuItem menuItem7;
		private ElMenuItem menuItem8;
		private ElMenuItem menuItem9;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.RichTextBox richMessages;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Label lblPushes;
		private System.Windows.Forms.Label lblQueryHits;
		private System.Windows.Forms.Label lblQueries;
		private System.Windows.Forms.Label lblPongs;
		private System.Windows.Forms.Label lblPings;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ListBox listQueries;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.ToolBarButton toolBarButton8;
		private System.Windows.Forms.ToolBarButton toolBarButton9;
		private System.Windows.Forms.ToolBarButton toolBarButton10;
		private System.Windows.Forms.ToolBarButton toolBarButton11;
		private System.Windows.Forms.ToolBarButton toolBarButton12;
		private System.Windows.Forms.ToolBarButton toolBarButton13;
		private System.Windows.Forms.ToolBarButton toolBarButton14;
		private System.Windows.Forms.ColumnHeader columnHeader14;
		private System.Windows.Forms.ColumnHeader columnHeader15;
		private System.Windows.Forms.ColumnHeader columnHeader16;
		private System.Windows.Forms.ColumnHeader columnHeader17;
		private System.Windows.Forms.ColumnHeader columnHeader18;
		private System.Windows.Forms.ColumnHeader columnHeader19;
		private System.Windows.Forms.ToolBar toolBar2;
		private System.Windows.Forms.ToolBarButton toolBarButton15;
		private System.Windows.Forms.ToolBarButton toolBarButton16;
		private System.Windows.Forms.ToolBarButton toolBarButton17;
		private System.Windows.Forms.ToolBarButton toolBarButton18;
		private System.Windows.Forms.ToolBarButton toolBarButton19;
		private System.Windows.Forms.ToolBarButton toolBarButton20;
		private System.Windows.Forms.ToolBarButton toolBarButton21;
		public System.Windows.Forms.ListView listView3;
		private System.Windows.Forms.ColumnHeader columnHeader20;
		private System.Windows.Forms.ColumnHeader columnHeader21;
		private System.Windows.Forms.ColumnHeader columnHeader22;
		private System.Windows.Forms.ColumnHeader columnHeader23;
		private System.Windows.Forms.ColumnHeader columnHeader25;
		private System.Windows.Forms.RichTextBox richMessages2;
		private System.Windows.Forms.ColumnHeader columnHeader26;
		private System.Windows.Forms.ColumnHeader columnHeader27;
		private FileScope.ElMenuItem elMenuItem1;
		private FileScope.ElMenuItem elMenuItem2;
		private System.Windows.Forms.ContextMenu contextMenu3;
		private FileScope.ElMenuItem menuItem10;
		private FileScope.ElMenuItem menuItem11;
		private FileScope.ElMenuItem menuItem12;
		private FileScope.ElMenuItem menuItem13;
		private FileScope.ElMenuItem menuItem14;
		private FileScope.ElMenuItem menuItem15;
		private FileScope.ElMenuItem menuItem16;
		private System.Windows.Forms.ColumnHeader columnHeader24;
		private System.Windows.Forms.ColumnHeader columnHeader28;
		private System.Windows.Forms.ColumnHeader columnHeader29;
		private System.Windows.Forms.ColumnHeader columnHeader30;
		private System.Windows.Forms.ColumnHeader columnHeader31;
		private System.Windows.Forms.ColumnHeader columnHeader32;
		private System.Windows.Forms.ColumnHeader columnHeader33;
		public System.Windows.Forms.ListView listView4;
		private System.Windows.Forms.ColumnHeader columnHeader34;
		private System.Windows.Forms.ColumnHeader columnHeader35;
		private System.Windows.Forms.ColumnHeader columnHeader36;
		private System.Windows.Forms.ColumnHeader columnHeader37;
		private System.Windows.Forms.ColumnHeader columnHeader38;
		private System.Windows.Forms.ColumnHeader columnHeader39;
		private System.Windows.Forms.ColumnHeader columnHeader40;
		private System.Windows.Forms.ColumnHeader columnHeader41;
		private System.Windows.Forms.ColumnHeader columnHeader42;
		private FileScope.ElMenuItem menuItem17;
		private FileScope.ElMenuItem menuItem18;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ContextMenu contextMenu4;
		private FileScope.ElMenuItem menuItem19;
		private FileScope.ElMenuItem menuItem20;
		private FileScope.ElMenuItem menuItem21;
		private FileScope.ElMenuItem menuItem22;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.ListBox listQueries2;
		private FileScope.ElMenuItem menuItem23;
		private System.Windows.Forms.Label lblg2qka;
		private System.Windows.Forms.Label lblg2qkr;
		private System.Windows.Forms.Label lblg2qht;
		private System.Windows.Forms.Label lblg2khl;
		private System.Windows.Forms.Label lblg2lni;
		private System.Windows.Forms.Label lblg2qa;
		private System.Windows.Forms.Label lblg2push;
		private System.Windows.Forms.Label lblg2qh2;
		private System.Windows.Forms.Label lblg2q2;
		private System.Windows.Forms.Label lblg2po;
		private System.Windows.Forms.Label lblg2pi;
		public ContextMenu[] cmArray;

		public Connection()
		{
			InitializeComponent();
			cmArray = new ContextMenu[]{this.contextMenu1, this.contextMenu2, this.contextMenu3, this.contextMenu4};
			UpdateUltrapeerStatus();
			updownConnections1.Value = Stats.settings.gConnectionsToKeep;
			LoadOpenNapList();
			LoadEDonkeyList();
			Control c = (Control)this;
			Themes.SetupTheme(c);
			tabControl1.SelectedIndex = 1;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Connection));
			this.tabControl1 = new FileScope.ElTabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.listQueries = new System.Windows.Forms.ListBox();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lblPushes = new System.Windows.Forms.Label();
			this.lblQueryHits = new System.Windows.Forms.Label();
			this.lblQueries = new System.Windows.Forms.Label();
			this.lblPongs = new System.Windows.Forms.Label();
			this.lblPings = new System.Windows.Forms.Label();
			this.labelStatus1 = new System.Windows.Forms.Label();
			this.updownConnections1 = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.listQueries2 = new System.Windows.Forms.ListBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.lblg2qka = new System.Windows.Forms.Label();
			this.lblg2qkr = new System.Windows.Forms.Label();
			this.lblg2qht = new System.Windows.Forms.Label();
			this.lblg2khl = new System.Windows.Forms.Label();
			this.lblg2lni = new System.Windows.Forms.Label();
			this.lblg2qa = new System.Windows.Forms.Label();
			this.lblg2push = new System.Windows.Forms.Label();
			this.lblg2qh2 = new System.Windows.Forms.Label();
			this.lblg2q2 = new System.Windows.Forms.Label();
			this.lblg2po = new System.Windows.Forms.Label();
			this.lblg2pi = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.listView4 = new System.Windows.Forms.ListView();
			this.columnHeader34 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader35 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader36 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader37 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader41 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader38 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader42 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader39 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader40 = new System.Windows.Forms.ColumnHeader();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.richMessages2 = new System.Windows.Forms.RichTextBox();
			this.toolBar2 = new System.Windows.Forms.ToolBar();
			this.toolBarButton15 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton16 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton17 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton18 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton19 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton20 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton21 = new System.Windows.Forms.ToolBarButton();
			this.listView3 = new System.Windows.Forms.ListView();
			this.columnHeader20 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader26 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader27 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader21 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader22 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader23 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader25 = new System.Windows.Forms.ColumnHeader();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.richMessages = new System.Windows.Forms.RichTextBox();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton4 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton5 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton6 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton7 = new System.Windows.Forms.ToolBarButton();
			this.listView2 = new System.Windows.Forms.ListView();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuItem9 = new FileScope.ElMenuItem();
			this.menuItem18 = new FileScope.ElMenuItem();
			this.menuItem17 = new FileScope.ElMenuItem();
			this.menuItem1 = new FileScope.ElMenuItem();
			this.contextMenu2 = new System.Windows.Forms.ContextMenu();
			this.menuItem2 = new FileScope.ElMenuItem();
			this.menuItem3 = new FileScope.ElMenuItem();
			this.menuItem4 = new FileScope.ElMenuItem();
			this.menuItem5 = new FileScope.ElMenuItem();
			this.menuItem6 = new FileScope.ElMenuItem();
			this.menuItem7 = new FileScope.ElMenuItem();
			this.menuItem8 = new FileScope.ElMenuItem();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.toolBarButton8 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton9 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton10 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton11 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton12 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton13 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton14 = new System.Windows.Forms.ToolBarButton();
			this.columnHeader14 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader15 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader16 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader17 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader18 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader19 = new System.Windows.Forms.ColumnHeader();
			this.elMenuItem1 = new FileScope.ElMenuItem();
			this.elMenuItem2 = new FileScope.ElMenuItem();
			this.contextMenu3 = new System.Windows.Forms.ContextMenu();
			this.menuItem10 = new FileScope.ElMenuItem();
			this.menuItem11 = new FileScope.ElMenuItem();
			this.menuItem12 = new FileScope.ElMenuItem();
			this.menuItem13 = new FileScope.ElMenuItem();
			this.menuItem14 = new FileScope.ElMenuItem();
			this.menuItem15 = new FileScope.ElMenuItem();
			this.menuItem16 = new FileScope.ElMenuItem();
			this.columnHeader24 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader28 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader29 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader30 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader31 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader32 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader33 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu4 = new System.Windows.Forms.ContextMenu();
			this.menuItem23 = new FileScope.ElMenuItem();
			this.menuItem19 = new FileScope.ElMenuItem();
			this.menuItem20 = new FileScope.ElMenuItem();
			this.menuItem21 = new FileScope.ElMenuItem();
			this.menuItem22 = new FileScope.ElMenuItem();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.updownConnections1)).BeginInit();
			this.tabPage3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.tabPage4.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.tabPage1,
																					  this.tabPage3,
																					  this.tabPage4,
																					  this.tabPage2});
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabControl1.HotTrack = true;
			this.tabControl1.ImageList = this.imageList1;
			this.tabControl1.Multiline = true;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(880, 440);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.groupBox2,
																				   this.listView1,
																				   this.groupBox1});
			this.tabPage1.ImageIndex = 0;
			this.tabPage1.Location = new System.Drawing.Point(4, 39);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(872, 397);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Gnutella1";
			this.tabPage1.Resize += new System.EventHandler(this.tabPage1_Resize);
			this.tabPage1.Paint += new System.Windows.Forms.PaintEventHandler(this.tabPage1_Paint);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.listQueries});
			this.groupBox2.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.groupBox2.Location = new System.Drawing.Point(420, 217);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(276, 160);
			this.groupBox2.TabIndex = 6;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Incoming Queries";
			// 
			// listQueries
			// 
			this.listQueries.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.listQueries.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listQueries.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.listQueries.ItemHeight = 14;
			this.listQueries.Location = new System.Drawing.Point(6, 28);
			this.listQueries.Name = "listQueries";
			this.listQueries.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.listQueries.Size = new System.Drawing.Size(264, 126);
			this.listQueries.TabIndex = 0;
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1,
																						this.columnHeader2,
																						this.columnHeader3,
																						this.columnHeader4,
																						this.columnHeader5,
																						this.columnHeader6,
																						this.columnHeader7});
			this.listView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listView1.FullRowSelect = true;
			this.listView1.GridLines = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.HideSelection = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(736, 186);
			this.listView1.TabIndex = 5;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Host";
			this.columnHeader1.Width = 120;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Status";
			this.columnHeader2.Width = 70;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Route";
			this.columnHeader3.Width = 42;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Bandwidth (I/O) KB/s";
			this.columnHeader4.Width = 124;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Ultrapeer";
			this.columnHeader5.Width = 55;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Vendor";
			this.columnHeader6.Width = 110;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "C#";
			this.columnHeader7.Width = 26;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.lblPushes,
																					this.lblQueryHits,
																					this.lblQueries,
																					this.lblPongs,
																					this.lblPings,
																					this.labelStatus1,
																					this.updownConnections1,
																					this.label1});
			this.groupBox1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(110, 217);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(276, 160);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Stats";
			// 
			// lblPushes
			// 
			this.lblPushes.AutoSize = true;
			this.lblPushes.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblPushes.Location = new System.Drawing.Point(134, 113);
			this.lblPushes.Name = "lblPushes";
			this.lblPushes.Size = new System.Drawing.Size(98, 18);
			this.lblPushes.TabIndex = 13;
			this.lblPushes.Text = "Pushes / sec:";
			// 
			// lblQueryHits
			// 
			this.lblQueryHits.AutoSize = true;
			this.lblQueryHits.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblQueryHits.Location = new System.Drawing.Point(134, 92);
			this.lblQueryHits.Name = "lblQueryHits";
			this.lblQueryHits.Size = new System.Drawing.Size(115, 18);
			this.lblQueryHits.TabIndex = 12;
			this.lblQueryHits.Text = "QueryHits / sec:";
			// 
			// lblQueries
			// 
			this.lblQueries.AutoSize = true;
			this.lblQueries.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblQueries.Location = new System.Drawing.Point(17, 134);
			this.lblQueries.Name = "lblQueries";
			this.lblQueries.Size = new System.Drawing.Size(101, 18);
			this.lblQueries.TabIndex = 11;
			this.lblQueries.Text = "Queries / sec:";
			// 
			// lblPongs
			// 
			this.lblPongs.AutoSize = true;
			this.lblPongs.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblPongs.Location = new System.Drawing.Point(17, 113);
			this.lblPongs.Name = "lblPongs";
			this.lblPongs.Size = new System.Drawing.Size(91, 18);
			this.lblPongs.TabIndex = 10;
			this.lblPongs.Text = "Pongs / sec:";
			// 
			// lblPings
			// 
			this.lblPings.AutoSize = true;
			this.lblPings.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblPings.Location = new System.Drawing.Point(17, 92);
			this.lblPings.Name = "lblPings";
			this.lblPings.Size = new System.Drawing.Size(85, 18);
			this.lblPings.TabIndex = 9;
			this.lblPings.Text = "Pings / sec:";
			// 
			// labelStatus1
			// 
			this.labelStatus1.AutoSize = true;
			this.labelStatus1.Font = new System.Drawing.Font("Arial", 14.5F);
			this.labelStatus1.Location = new System.Drawing.Point(82, 21);
			this.labelStatus1.Name = "labelStatus1";
			this.labelStatus1.Size = new System.Drawing.Size(110, 23);
			this.labelStatus1.TabIndex = 6;
			this.labelStatus1.Text = "[Leaf Node]";
			// 
			// updownConnections1
			// 
			this.updownConnections1.Enabled = false;
			this.updownConnections1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.updownConnections1.Location = new System.Drawing.Point(72, 56);
			this.updownConnections1.Maximum = new System.Decimal(new int[] {
																			   800,
																			   0,
																			   0,
																			   0});
			this.updownConnections1.Minimum = new System.Decimal(new int[] {
																			   20,
																			   0,
																			   0,
																			   0});
			this.updownConnections1.Name = "updownConnections1";
			this.updownConnections1.Size = new System.Drawing.Size(48, 26);
			this.updownConnections1.TabIndex = 5;
			this.updownConnections1.Value = new System.Decimal(new int[] {
																			 100,
																			 0,
																			 0,
																			 0});
			this.updownConnections1.ValueChanged += new System.EventHandler(this.updownConnections1_ValueChanged);
			this.updownConnections1.Leave += new System.EventHandler(this.updownConnections1_Leave);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Enabled = false;
			this.label1.Font = new System.Drawing.Font("Arial", 14.5F);
			this.label1.Location = new System.Drawing.Point(23, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(245, 23);
			this.label1.TabIndex = 4;
			this.label1.Text = "keep           connections up";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.groupBox4,
																				   this.groupBox3,
																				   this.listView4});
			this.tabPage3.ImageIndex = 2;
			this.tabPage3.Location = new System.Drawing.Point(4, 39);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(872, 397);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Gnutella2";
			this.tabPage3.Resize += new System.EventHandler(this.tabPage3_Resize);
			this.tabPage3.Paint += new System.Windows.Forms.PaintEventHandler(this.tabPage3_Paint);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.listQueries2});
			this.groupBox4.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.groupBox4.Location = new System.Drawing.Point(432, 216);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(276, 160);
			this.groupBox4.TabIndex = 8;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Incoming Queries";
			// 
			// listQueries2
			// 
			this.listQueries2.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.listQueries2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listQueries2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.listQueries2.ItemHeight = 14;
			this.listQueries2.Location = new System.Drawing.Point(6, 28);
			this.listQueries2.Name = "listQueries2";
			this.listQueries2.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.listQueries2.Size = new System.Drawing.Size(264, 126);
			this.listQueries2.TabIndex = 0;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.lblg2qka,
																					this.lblg2qkr,
																					this.lblg2qht,
																					this.lblg2khl,
																					this.lblg2lni,
																					this.lblg2qa,
																					this.lblg2push,
																					this.lblg2qh2,
																					this.lblg2q2,
																					this.lblg2po,
																					this.lblg2pi,
																					this.label7,
																					this.numericUpDown1,
																					this.label8});
			this.groupBox3.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.groupBox3.Location = new System.Drawing.Point(120, 216);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(276, 160);
			this.groupBox3.TabIndex = 7;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Stats";
			// 
			// lblg2qka
			// 
			this.lblg2qka.AutoSize = true;
			this.lblg2qka.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2qka.Location = new System.Drawing.Point(176, 136);
			this.lblg2qka.Name = "lblg2qka";
			this.lblg2qka.Size = new System.Drawing.Size(64, 14);
			this.lblg2qka.TabIndex = 19;
			this.lblg2qka.Text = "QKA / sec:";
			// 
			// lblg2qkr
			// 
			this.lblg2qkr.AutoSize = true;
			this.lblg2qkr.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2qkr.Location = new System.Drawing.Point(176, 120);
			this.lblg2qkr.Name = "lblg2qkr";
			this.lblg2qkr.Size = new System.Drawing.Size(64, 14);
			this.lblg2qkr.TabIndex = 18;
			this.lblg2qkr.Text = "QKR / sec:";
			// 
			// lblg2qht
			// 
			this.lblg2qht.AutoSize = true;
			this.lblg2qht.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2qht.Location = new System.Drawing.Point(176, 104);
			this.lblg2qht.Name = "lblg2qht";
			this.lblg2qht.Size = new System.Drawing.Size(64, 14);
			this.lblg2qht.TabIndex = 17;
			this.lblg2qht.Text = "QHT / sec:";
			// 
			// lblg2khl
			// 
			this.lblg2khl.AutoSize = true;
			this.lblg2khl.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2khl.Location = new System.Drawing.Point(88, 120);
			this.lblg2khl.Name = "lblg2khl";
			this.lblg2khl.Size = new System.Drawing.Size(62, 14);
			this.lblg2khl.TabIndex = 16;
			this.lblg2khl.Text = "KHL / sec:";
			// 
			// lblg2lni
			// 
			this.lblg2lni.AutoSize = true;
			this.lblg2lni.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2lni.Location = new System.Drawing.Point(88, 104);
			this.lblg2lni.Name = "lblg2lni";
			this.lblg2lni.Size = new System.Drawing.Size(57, 14);
			this.lblg2lni.TabIndex = 15;
			this.lblg2lni.Text = "LNI / sec:";
			// 
			// lblg2qa
			// 
			this.lblg2qa.AutoSize = true;
			this.lblg2qa.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2qa.Location = new System.Drawing.Point(8, 136);
			this.lblg2qa.Name = "lblg2qa";
			this.lblg2qa.Size = new System.Drawing.Size(55, 14);
			this.lblg2qa.TabIndex = 14;
			this.lblg2qa.Text = "QA / sec:";
			// 
			// lblg2push
			// 
			this.lblg2push.AutoSize = true;
			this.lblg2push.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2push.Location = new System.Drawing.Point(88, 88);
			this.lblg2push.Name = "lblg2push";
			this.lblg2push.Size = new System.Drawing.Size(72, 14);
			this.lblg2push.TabIndex = 13;
			this.lblg2push.Text = "PUSH / sec:";
			// 
			// lblg2qh2
			// 
			this.lblg2qh2.AutoSize = true;
			this.lblg2qh2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2qh2.Location = new System.Drawing.Point(88, 136);
			this.lblg2qh2.Name = "lblg2qh2";
			this.lblg2qh2.Size = new System.Drawing.Size(63, 14);
			this.lblg2qh2.TabIndex = 12;
			this.lblg2qh2.Text = "QH2 / sec:";
			// 
			// lblg2q2
			// 
			this.lblg2q2.AutoSize = true;
			this.lblg2q2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2q2.Location = new System.Drawing.Point(8, 120);
			this.lblg2q2.Name = "lblg2q2";
			this.lblg2q2.Size = new System.Drawing.Size(54, 14);
			this.lblg2q2.TabIndex = 11;
			this.lblg2q2.Text = "Q2 / sec:";
			// 
			// lblg2po
			// 
			this.lblg2po.AutoSize = true;
			this.lblg2po.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2po.Location = new System.Drawing.Point(8, 104);
			this.lblg2po.Name = "lblg2po";
			this.lblg2po.Size = new System.Drawing.Size(55, 14);
			this.lblg2po.TabIndex = 10;
			this.lblg2po.Text = "PO / sec:";
			// 
			// lblg2pi
			// 
			this.lblg2pi.AutoSize = true;
			this.lblg2pi.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblg2pi.Location = new System.Drawing.Point(8, 88);
			this.lblg2pi.Name = "lblg2pi";
			this.lblg2pi.Size = new System.Drawing.Size(49, 14);
			this.lblg2pi.TabIndex = 9;
			this.lblg2pi.Text = "PI / sec:";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Arial", 14.5F);
			this.label7.Location = new System.Drawing.Point(82, 21);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(110, 23);
			this.label7.TabIndex = 6;
			this.label7.Text = "[Leaf Node]";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Enabled = false;
			this.numericUpDown1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.numericUpDown1.Location = new System.Drawing.Point(72, 56);
			this.numericUpDown1.Maximum = new System.Decimal(new int[] {
																		   800,
																		   0,
																		   0,
																		   0});
			this.numericUpDown1.Minimum = new System.Decimal(new int[] {
																		   20,
																		   0,
																		   0,
																		   0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(48, 26);
			this.numericUpDown1.TabIndex = 5;
			this.numericUpDown1.Value = new System.Decimal(new int[] {
																		 100,
																		 0,
																		 0,
																		 0});
			this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
			this.numericUpDown1.Leave += new System.EventHandler(this.numericUpDown1_Leave);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Enabled = false;
			this.label8.Font = new System.Drawing.Font("Arial", 14.5F);
			this.label8.Location = new System.Drawing.Point(23, 56);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(245, 23);
			this.label8.TabIndex = 4;
			this.label8.Text = "keep           connections up";
			// 
			// listView4
			// 
			this.listView4.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader34,
																						this.columnHeader35,
																						this.columnHeader36,
																						this.columnHeader37,
																						this.columnHeader41,
																						this.columnHeader38,
																						this.columnHeader42,
																						this.columnHeader39,
																						this.columnHeader40});
			this.listView4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listView4.FullRowSelect = true;
			this.listView4.GridLines = true;
			this.listView4.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView4.HideSelection = false;
			this.listView4.Name = "listView4";
			this.listView4.Size = new System.Drawing.Size(736, 192);
			this.listView4.TabIndex = 6;
			this.listView4.View = System.Windows.Forms.View.Details;
			this.listView4.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView4_MouseUp);
			// 
			// columnHeader34
			// 
			this.columnHeader34.Text = "Host";
			this.columnHeader34.Width = 120;
			// 
			// columnHeader35
			// 
			this.columnHeader35.Text = "Status";
			this.columnHeader35.Width = 70;
			// 
			// columnHeader36
			// 
			this.columnHeader36.Text = "Route";
			this.columnHeader36.Width = 42;
			// 
			// columnHeader37
			// 
			this.columnHeader37.Text = "Bandwidth (I/O) KB/s";
			this.columnHeader37.Width = 124;
			// 
			// columnHeader41
			// 
			this.columnHeader41.Text = "Name";
			this.columnHeader41.Width = 70;
			// 
			// columnHeader38
			// 
			this.columnHeader38.Text = "Mode";
			this.columnHeader38.Width = 55;
			// 
			// columnHeader42
			// 
			this.columnHeader42.Text = "Leaves";
			this.columnHeader42.Width = 50;
			// 
			// columnHeader39
			// 
			this.columnHeader39.Text = "Vendor";
			this.columnHeader39.Width = 110;
			// 
			// columnHeader40
			// 
			this.columnHeader40.Text = "C#";
			this.columnHeader40.Width = 26;
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.richMessages2,
																				   this.toolBar2,
																				   this.listView3});
			this.tabPage4.ImageIndex = 3;
			this.tabPage4.Location = new System.Drawing.Point(4, 39);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(872, 397);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "eDonkey";
			this.tabPage4.Resize += new System.EventHandler(this.tabPage4_Resize);
			// 
			// richMessages2
			// 
			this.richMessages2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.richMessages2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.richMessages2.Location = new System.Drawing.Point(96, 245);
			this.richMessages2.Name = "richMessages2";
			this.richMessages2.ReadOnly = true;
			this.richMessages2.Size = new System.Drawing.Size(776, 152);
			this.richMessages2.TabIndex = 5;
			this.richMessages2.Text = "";
			// 
			// toolBar2
			// 
			this.toolBar2.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar2.AutoSize = false;
			this.toolBar2.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarButton15,
																						this.toolBarButton16,
																						this.toolBarButton17,
																						this.toolBarButton18,
																						this.toolBarButton19,
																						this.toolBarButton20,
																						this.toolBarButton21});
			this.toolBar2.ButtonSize = new System.Drawing.Size(100, 100);
			this.toolBar2.Divider = false;
			this.toolBar2.Dock = System.Windows.Forms.DockStyle.Left;
			this.toolBar2.DropDownArrows = true;
			this.toolBar2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.toolBar2.Name = "toolBar2";
			this.toolBar2.ShowToolTips = true;
			this.toolBar2.Size = new System.Drawing.Size(96, 397);
			this.toolBar2.TabIndex = 4;
			this.toolBar2.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar2_ButtonClick);
			// 
			// toolBarButton15
			// 
			this.toolBarButton15.Enabled = false;
			this.toolBarButton15.Text = "Connect";
			this.toolBarButton15.ToolTipText = "Connect to selected server(s)";
			// 
			// toolBarButton16
			// 
			this.toolBarButton16.Enabled = false;
			this.toolBarButton16.Text = "Disconnect";
			this.toolBarButton16.ToolTipText = "Disconnect from selected server(s)";
			// 
			// toolBarButton17
			// 
			this.toolBarButton17.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// toolBarButton18
			// 
			this.toolBarButton18.Text = "Add";
			this.toolBarButton18.ToolTipText = "Add a new server";
			// 
			// toolBarButton19
			// 
			this.toolBarButton19.Enabled = false;
			this.toolBarButton19.Text = "Remove";
			this.toolBarButton19.ToolTipText = "Remove the selected server(s)";
			// 
			// toolBarButton20
			// 
			this.toolBarButton20.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// toolBarButton21
			// 
			this.toolBarButton21.Text = "Open .met";
			this.toolBarButton21.ToolTipText = "Import/Add a list of eDonkey servers";
			// 
			// listView3
			// 
			this.listView3.AutoArrange = false;
			this.listView3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listView3.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader20,
																						this.columnHeader26,
																						this.columnHeader27,
																						this.columnHeader21,
																						this.columnHeader22,
																						this.columnHeader23,
																						this.columnHeader25});
			this.listView3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.listView3.FullRowSelect = true;
			this.listView3.GridLines = true;
			this.listView3.HideSelection = false;
			this.listView3.Location = new System.Drawing.Point(136, 0);
			this.listView3.Name = "listView3";
			this.listView3.Size = new System.Drawing.Size(720, 296);
			this.listView3.TabIndex = 3;
			this.listView3.View = System.Windows.Forms.View.Details;
			this.listView3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView3_MouseUp);
			this.listView3.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView3_ColumnClick);
			// 
			// columnHeader20
			// 
			this.columnHeader20.Text = "Name";
			this.columnHeader20.Width = 205;
			// 
			// columnHeader26
			// 
			this.columnHeader26.Text = "Server";
			this.columnHeader26.Width = 90;
			// 
			// columnHeader27
			// 
			this.columnHeader27.Text = "Description";
			this.columnHeader27.Width = 90;
			// 
			// columnHeader21
			// 
			this.columnHeader21.Text = "Status";
			this.columnHeader21.Width = 80;
			// 
			// columnHeader22
			// 
			this.columnHeader22.Text = "Users/Max";
			this.columnHeader22.Width = 64;
			// 
			// columnHeader23
			// 
			this.columnHeader23.Text = "Files";
			this.columnHeader23.Width = 68;
			// 
			// columnHeader25
			// 
			this.columnHeader25.Text = "C#";
			this.columnHeader25.Width = 26;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.AddRange(new System.Windows.Forms.Control[] {
																				   this.richMessages,
																				   this.toolBar1,
																				   this.listView2});
			this.tabPage2.ImageIndex = 1;
			this.tabPage2.Location = new System.Drawing.Point(4, 39);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(872, 397);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "OpenNap";
			this.tabPage2.Resize += new System.EventHandler(this.tabPage2_Resize);
			// 
			// richMessages
			// 
			this.richMessages.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.richMessages.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.richMessages.Location = new System.Drawing.Point(96, 245);
			this.richMessages.Name = "richMessages";
			this.richMessages.ReadOnly = true;
			this.richMessages.Size = new System.Drawing.Size(776, 152);
			this.richMessages.TabIndex = 2;
			this.richMessages.Text = "";
			this.richMessages.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richMessages_KeyPress);
			// 
			// toolBar1
			// 
			this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar1.AutoSize = false;
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarButton1,
																						this.toolBarButton2,
																						this.toolBarButton3,
																						this.toolBarButton4,
																						this.toolBarButton5,
																						this.toolBarButton6,
																						this.toolBarButton7});
			this.toolBar1.ButtonSize = new System.Drawing.Size(100, 100);
			this.toolBar1.Divider = false;
			this.toolBar1.Dock = System.Windows.Forms.DockStyle.Left;
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(96, 397);
			this.toolBar1.TabIndex = 1;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.Enabled = false;
			this.toolBarButton1.Text = "Connect";
			this.toolBarButton1.ToolTipText = "Connect to selected server(s)";
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.Enabled = false;
			this.toolBarButton2.Text = "Disconnect";
			this.toolBarButton2.ToolTipText = "Disconnect from selected server(s)";
			// 
			// toolBarButton3
			// 
			this.toolBarButton3.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// toolBarButton4
			// 
			this.toolBarButton4.Text = "Add";
			this.toolBarButton4.ToolTipText = "Add a new server";
			// 
			// toolBarButton5
			// 
			this.toolBarButton5.Enabled = false;
			this.toolBarButton5.Text = "Remove";
			this.toolBarButton5.ToolTipText = "Remove the selected server(s)";
			// 
			// toolBarButton6
			// 
			this.toolBarButton6.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// toolBarButton7
			// 
			this.toolBarButton7.Text = "Open .wsx";
			this.toolBarButton7.ToolTipText = "Import/Add a list of OpenNap servers";
			// 
			// listView2
			// 
			this.listView2.AutoArrange = false;
			this.listView2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader8,
																						this.columnHeader9,
																						this.columnHeader10,
																						this.columnHeader11,
																						this.columnHeader12,
																						this.columnHeader13});
			this.listView2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.listView2.FullRowSelect = true;
			this.listView2.GridLines = true;
			this.listView2.HideSelection = false;
			this.listView2.Location = new System.Drawing.Point(152, 0);
			this.listView2.Name = "listView2";
			this.listView2.Size = new System.Drawing.Size(592, 296);
			this.listView2.TabIndex = 0;
			this.listView2.View = System.Windows.Forms.View.Details;
			this.listView2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView2_MouseUp);
			this.listView2.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView2_ColumnClick);
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "Server";
			this.columnHeader8.Width = 205;
			// 
			// columnHeader9
			// 
			this.columnHeader9.Text = "Status";
			this.columnHeader9.Width = 80;
			// 
			// columnHeader10
			// 
			this.columnHeader10.Text = "Users";
			this.columnHeader10.Width = 48;
			// 
			// columnHeader11
			// 
			this.columnHeader11.Text = "Files";
			this.columnHeader11.Width = 68;
			// 
			// columnHeader12
			// 
			this.columnHeader12.Text = "Gigabytes";
			this.columnHeader12.Width = 72;
			// 
			// columnHeader13
			// 
			this.columnHeader13.Text = "C#";
			this.columnHeader13.Width = 26;
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(32, 32);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItem9,
																						 this.menuItem18,
																						 this.menuItem17,
																						 this.menuItem1});
			// 
			// menuItem9
			// 
			this.menuItem9.Enabled = false;
			this.menuItem9.Index = 0;
			this.menuItem9.Text = "Chat";
			this.menuItem9.Click += new System.EventHandler(this.menuItem9_Click);
			// 
			// menuItem18
			// 
			this.menuItem18.Index = 1;
			this.menuItem18.Text = "-";
			// 
			// menuItem17
			// 
			this.menuItem17.Index = 2;
			this.menuItem17.Text = "Add";
			this.menuItem17.Click += new System.EventHandler(this.menuItem17_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Enabled = false;
			this.menuItem1.Index = 3;
			this.menuItem1.Text = "Remove";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// contextMenu2
			// 
			this.contextMenu2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItem2,
																						 this.menuItem3,
																						 this.menuItem4,
																						 this.menuItem5,
																						 this.menuItem6,
																						 this.menuItem7,
																						 this.menuItem8});
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 0;
			this.menuItem2.Text = "Connect";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 1;
			this.menuItem3.Text = "Disconnect";
			this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 2;
			this.menuItem4.Text = "-";
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 3;
			this.menuItem5.Text = "Add";
			this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 4;
			this.menuItem6.Text = "Remove";
			this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 5;
			this.menuItem7.Text = "-";
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 6;
			this.menuItem8.Text = "Open .wsx";
			this.menuItem8.Click += new System.EventHandler(this.menuItem8_Click);
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// toolBarButton8
			// 
			this.toolBarButton8.Enabled = false;
			this.toolBarButton8.Text = "Connect";
			this.toolBarButton8.ToolTipText = "Connect to selected server(s)";
			// 
			// toolBarButton9
			// 
			this.toolBarButton9.Enabled = false;
			this.toolBarButton9.Text = "Disconnect";
			this.toolBarButton9.ToolTipText = "Disconnect from selected server(s)";
			// 
			// toolBarButton10
			// 
			this.toolBarButton10.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// toolBarButton11
			// 
			this.toolBarButton11.Text = "Add";
			this.toolBarButton11.ToolTipText = "Add a new server";
			// 
			// toolBarButton12
			// 
			this.toolBarButton12.Enabled = false;
			this.toolBarButton12.Text = "Remove";
			this.toolBarButton12.ToolTipText = "Remove the selected server(s)";
			// 
			// toolBarButton13
			// 
			this.toolBarButton13.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// toolBarButton14
			// 
			this.toolBarButton14.Text = "Open .wsx";
			this.toolBarButton14.ToolTipText = "Import a list of OpenNap servers";
			// 
			// columnHeader14
			// 
			this.columnHeader14.Text = "Server";
			this.columnHeader14.Width = 205;
			// 
			// columnHeader15
			// 
			this.columnHeader15.Text = "Status";
			this.columnHeader15.Width = 80;
			// 
			// columnHeader16
			// 
			this.columnHeader16.Text = "Users";
			this.columnHeader16.Width = 48;
			// 
			// columnHeader17
			// 
			this.columnHeader17.Text = "Files";
			this.columnHeader17.Width = 68;
			// 
			// columnHeader18
			// 
			this.columnHeader18.Text = "Gigabytes";
			this.columnHeader18.Width = 72;
			// 
			// columnHeader19
			// 
			this.columnHeader19.Text = "C#";
			this.columnHeader19.Width = 26;
			// 
			// elMenuItem1
			// 
			this.elMenuItem1.Index = -1;
			this.elMenuItem1.Text = "Connect";
			// 
			// elMenuItem2
			// 
			this.elMenuItem2.Index = -1;
			this.elMenuItem2.Text = "Connect";
			// 
			// contextMenu3
			// 
			this.contextMenu3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItem10,
																						 this.menuItem11,
																						 this.menuItem12,
																						 this.menuItem13,
																						 this.menuItem14,
																						 this.menuItem15,
																						 this.menuItem16});
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 0;
			this.menuItem10.Text = "Connect";
			this.menuItem10.Click += new System.EventHandler(this.menuItem10_Click);
			// 
			// menuItem11
			// 
			this.menuItem11.Index = 1;
			this.menuItem11.Text = "Disconnect";
			this.menuItem11.Click += new System.EventHandler(this.menuItem11_Click);
			// 
			// menuItem12
			// 
			this.menuItem12.Index = 2;
			this.menuItem12.Text = "-";
			// 
			// menuItem13
			// 
			this.menuItem13.Index = 3;
			this.menuItem13.Text = "Add";
			this.menuItem13.Click += new System.EventHandler(this.menuItem13_Click);
			// 
			// menuItem14
			// 
			this.menuItem14.Index = 4;
			this.menuItem14.Text = "Remove";
			this.menuItem14.Click += new System.EventHandler(this.menuItem14_Click);
			// 
			// menuItem15
			// 
			this.menuItem15.Index = 5;
			this.menuItem15.Text = "-";
			// 
			// menuItem16
			// 
			this.menuItem16.Index = 6;
			this.menuItem16.Text = "Open .met";
			this.menuItem16.Click += new System.EventHandler(this.menuItem16_Click);
			// 
			// columnHeader24
			// 
			this.columnHeader24.Text = "Host";
			this.columnHeader24.Width = 120;
			// 
			// columnHeader28
			// 
			this.columnHeader28.Text = "Status";
			this.columnHeader28.Width = 70;
			// 
			// columnHeader29
			// 
			this.columnHeader29.Text = "Route";
			this.columnHeader29.Width = 42;
			// 
			// columnHeader30
			// 
			this.columnHeader30.Text = "Bandwidth (I/O) KB/s";
			this.columnHeader30.Width = 124;
			// 
			// columnHeader31
			// 
			this.columnHeader31.Text = "Ultrapeer";
			this.columnHeader31.Width = 55;
			// 
			// columnHeader32
			// 
			this.columnHeader32.Text = "Vendor";
			this.columnHeader32.Width = 110;
			// 
			// columnHeader33
			// 
			this.columnHeader33.Text = "C#";
			this.columnHeader33.Width = 26;
			// 
			// contextMenu4
			// 
			this.contextMenu4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItem23,
																						 this.menuItem19,
																						 this.menuItem20,
																						 this.menuItem21,
																						 this.menuItem22});
			// 
			// menuItem23
			// 
			this.menuItem23.Enabled = false;
			this.menuItem23.Index = 0;
			this.menuItem23.Text = "Browse";
			this.menuItem23.Click += new System.EventHandler(this.menuItem23_Click);
			// 
			// menuItem19
			// 
			this.menuItem19.Enabled = false;
			this.menuItem19.Index = 1;
			this.menuItem19.Text = "Chat";
			this.menuItem19.Click += new System.EventHandler(this.menuItem19_Click);
			// 
			// menuItem20
			// 
			this.menuItem20.Index = 2;
			this.menuItem20.Text = "-";
			// 
			// menuItem21
			// 
			this.menuItem21.Index = 3;
			this.menuItem21.Text = "Add";
			this.menuItem21.Click += new System.EventHandler(this.menuItem21_Click);
			// 
			// menuItem22
			// 
			this.menuItem22.Enabled = false;
			this.menuItem22.Index = 4;
			this.menuItem22.Text = "Remove";
			this.menuItem22.Click += new System.EventHandler(this.menuItem22_Click);
			// 
			// Connection
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.tabControl1});
			this.Name = "Connection";
			this.Size = new System.Drawing.Size(880, 440);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.updownConnections1)).EndInit();
			this.tabPage3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.tabPage4.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void tabPage1_Resize(object sender, System.EventArgs e)
		{
			groupBox1.Left = (tabPage1.ClientSize.Width / 4) - (groupBox1.Width / 2);
			groupBox2.Left = tabPage1.ClientSize.Width - groupBox1.Left - groupBox2.Width;
			groupBox1.Top = tabPage1.ClientSize.Height - groupBox1.Height;
			groupBox2.Top = groupBox1.Top;
			listView1.Height = groupBox1.Top - 25;
			listView1.Width = tabPage1.ClientSize.Width;
		}

		private void tabPage1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			HatchBrush hbtr = new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.Transparent, Color.FromArgb(40, Stats.settings.clHomeBR));
			Pen ptr = new Pen(hbtr, 4);
			e.Graphics.DrawLine(ptr, 0, groupBox1.Top - 10, tabPage1.ClientSize.Width, groupBox1.Top - 10);
			e.Graphics.DrawLine(ptr, tabPage1.ClientSize.Width / 2, groupBox1.Top - 10, tabPage1.ClientSize.Width / 2, tabPage1.ClientSize.Height);
			hbtr.Dispose();
			ptr.Dispose();
			e.Graphics.Dispose();
		}

		private void tabPage3_Resize(object sender, System.EventArgs e)
		{
			groupBox3.Left = (tabPage3.ClientSize.Width / 4) - (groupBox3.Width / 2);
			groupBox4.Left = tabPage3.ClientSize.Width - groupBox3.Left - groupBox4.Width;
			groupBox3.Top = tabPage3.ClientSize.Height - groupBox3.Height;
			groupBox4.Top = groupBox3.Top;
			listView4.Height = groupBox3.Top - 25;
			listView4.Width = tabPage3.ClientSize.Width;
		}

		private void tabPage3_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			HatchBrush hbtr = new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.Transparent, Color.FromArgb(40, Stats.settings.clHomeBR));
			Pen ptr = new Pen(hbtr, 4);
			e.Graphics.DrawLine(ptr, 0, groupBox3.Top - 10, tabPage3.ClientSize.Width, groupBox3.Top - 10);
			e.Graphics.DrawLine(ptr, tabPage3.ClientSize.Width / 2, groupBox3.Top - 10, tabPage3.ClientSize.Width / 2, tabPage3.ClientSize.Height);
			hbtr.Dispose();
			ptr.Dispose();
			e.Graphics.Dispose();
		}

		private void tabPage2_Resize(object sender, System.EventArgs e)
		{
			listView2.Left = toolBar1.Width;
			listView2.Width = tabPage2.ClientSize.Width - listView2.Left;
			listView2.Height = tabPage2.ClientSize.Height - richMessages.Height;
		}

		private void tabPage4_Resize(object sender, System.EventArgs e)
		{
			listView3.Left = toolBar2.Width;
			listView3.Width = tabPage4.ClientSize.Width - listView3.Left;
			listView3.Height = tabPage4.ClientSize.Height - richMessages2.Height;
		}

		public void GBandwidth(int sckNum, string bIn, string bOut)
		{
			try
			{
				if(tabControl1.SelectedIndex != 0)
					return;
				foreach(ListViewItem lvi in listView1.Items)
					if(lvi.SubItems[6].Text == sckNum.ToString())
					{
						lvi.SubItems[3].Text = bIn + @" / " + bOut;
						return;
					}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection GBandwidth");
			}
		}

		public void GNewConnection(string host, string route, int sckNum)
		{
			try
			{
				string[] subitems = new string[7];
				subitems[0] = host;
				subitems[1] = "Connecting";
				subitems[2] = route;
				subitems[3] = "0.00 / 0.00";
				subitems[4] = "?";
				subitems[5] = "?";
				subitems[6] = sckNum.ToString();
				ListViewItem lvi = new ListViewItem(subitems);
				lvi.ForeColor = Stats.settings.clHighlight3;
				listView1.Items.Add(lvi);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("GNewConnection");
			}
		}

		public void GJustDisconnected(int sckNum)
		{
			try
			{
				ListViewItem lvi;
				for(int x = 0; x < listView1.Items.Count; x++)
				{
					lvi = (ListViewItem)listView1.Items[x];
					if(lvi.SubItems[6].Text == sckNum.ToString())
					{
						listView1.Items.RemoveAt(x);
						return;
					}
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection GJustDisconnected");
			}
		}

		public void GJustConnected(int sckNum, string type, string vendor)
		{
			try
			{
				foreach(ListViewItem lvi in listView1.Items)
					if(lvi.SubItems[6].Text == sckNum.ToString())
					{
						if(Gnutella.Sck.scks[sckNum].incoming)
							lvi.SubItems[0].Text = Gnutella.Sck.scks[sckNum].RemoteIP();
						lvi.SubItems[1].Text = "Connected";
						lvi.SubItems[4].Text = type;
						lvi.SubItems[5].Text = vendor;
						if(Gnutella.Sck.scks[sckNum].ultrapeer)
							lvi.ForeColor = Stats.settings.clHighlight1;
						else
							lvi.ForeColor = Stats.settings.clHighlight2;
						return;
					}

				//if it gets this far, we'll add the item from scratch
				//System.Diagnostics.Debug.WriteLine("GJustConnected ^ " + sckNum.ToString() + "\r\n\r\n");
				string[] subitems = new string[7];
				if(Gnutella.Sck.scks[sckNum].incoming)
					subitems[0] = Gnutella.Sck.scks[sckNum].RemoteIP();
				else
					subitems[0] = Gnutella.Sck.scks[sckNum].address + ":" + Gnutella.Sck.scks[sckNum].port.ToString();
				subitems[1] = "Connected";
				subitems[2] = Gnutella.Sck.scks[sckNum].incoming ? "In" : "Out";
				subitems[3] = "0.00 / 0.00";
				subitems[4] = Gnutella.Sck.scks[sckNum].ultrapeer.ToString();
				subitems[5] = vendor;
				subitems[6] = sckNum.ToString();
				ListViewItem ldvi = new ListViewItem(subitems);
				if(Gnutella.Sck.scks[sckNum].ultrapeer)
					ldvi.ForeColor = Stats.settings.clHighlight1;
				else
					ldvi.ForeColor = Stats.settings.clHighlight2;
				listView1.Items.Add(ldvi);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection GJustConnected");
			}
		}

		public void G2NewConnection(string addr, int sockNum)
		{
			try
			{
				string[] subitems = new string[9];
				subitems[0] = addr;
				subitems[1] = "Connecting";
				if(Gnutella2.Sck.scks[sockNum].incoming)
					subitems[2] = "In";
				else
					subitems[2] = "Out";
				subitems[3] = "0.00 / 0.00";
				subitems[4] = "?";
				subitems[5] = "?";
				subitems[6] = "?";
				subitems[7] = "?";
				subitems[8] = sockNum.ToString();
				ListViewItem lvi = new ListViewItem(subitems);
				lvi.ForeColor = Stats.settings.clHighlight3;
				listView4.Items.Add(lvi);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection G2NewConnection");
			}
		}

		public void G2JustConnected(int sockNum)
		{
			try
			{
				foreach(ListViewItem lvi in listView4.Items)
					if(lvi.SubItems[8].Text == sockNum.ToString())
					{
						if(Gnutella2.Sck.scks[sockNum].incoming)
							lvi.SubItems[0].Text = Gnutella2.Sck.scks[sockNum].RemoteIP();
						lvi.SubItems[1].Text = "Connected";
						lvi.SubItems[5].Text = Gnutella2.Sck.scks[sockNum].mode.ToString();
						lvi.SubItems[7].Text = Gnutella2.Sck.scks[sockNum].vendor;
						lvi.SubItems[8].Text = sockNum.ToString();
						if(Gnutella2.Sck.scks[sockNum].mode == Gnutella2.G2Mode.Ultrapeer)
							lvi.ForeColor = Stats.settings.clHighlight1;
						else
							lvi.ForeColor = Stats.settings.clHighlight2;
						return;
					}
				System.Diagnostics.Debug.WriteLine("Connection G2JustConnected shouldn't have gotten this far");
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection G2JustConnected");
			}
		}

		public void G2Update(int sockNum, string bwInOut)
		{
			try
			{
				if(tabControl1.SelectedIndex != 1)
					return;
				foreach(ListViewItem lvi in listView4.Items)
					if(lvi.SubItems[8].Text == sockNum.ToString())
					{
						if(lvi.SubItems[1].Text == "Connecting")
							lvi.SubItems[1].Text = "Handshaking";
						if(lvi.SubItems[3].Text != bwInOut)
							lvi.SubItems[3].Text = bwInOut;
						string leavesData = Gnutella2.Sck.scks[sockNum].leaves.ToString() + @" / " + Gnutella2.Sck.scks[sockNum].maxleaves.ToString();
						if(lvi.SubItems[6].Text != leavesData)
							lvi.SubItems[6].Text = leavesData;
						return;
					}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection G2Update");
			}
		}

		public void G2JustDisconnected(int sockNum)
		{
			try
			{
				ListViewItem lvi;
				for(int x = 0; x < listView4.Items.Count; x++)
				{
					lvi = (ListViewItem)listView4.Items[x];
					if(lvi.SubItems[8].Text == sockNum.ToString())
					{
						listView4.Items.RemoveAt(x);
						return;
					}
				}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection G2JustDisconnected");
			}
		}

		public void OJustConnected(int sockNum)
		{
			try
			{
				foreach(ListViewItem lvi in listView2.Items)
					if(lvi.SubItems[5].Text == sockNum.ToString())
					{
						lvi.SubItems[1].Text = "Connected";
						lvi.ForeColor = Stats.settings.clHighlight3;
						return;
					}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection OJustConnected");
			}
		}

		public void OJustDisconnected(int sockNum)
		{
			try
			{
				foreach(ListViewItem lvi in listView2.Items)
					if(lvi.SubItems[5].Text == sockNum.ToString())
					{
						lvi.SubItems[1].Text = "Disconnected";
						lvi.SubItems[2].Text = "?";
						lvi.SubItems[3].Text = "?";
						lvi.SubItems[4].Text = "?";
						lvi.SubItems[5].Text = "?";
						lvi.ForeColor = Stats.settings.clHighlight4;
						return;
					}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection OJustDisconnected");
			}
		}

		public void OUpdateStats(int sockNum, string users, string files, string gigs)
		{
			try
			{
				foreach(ListViewItem lvi in listView2.Items)
					if(lvi.SubItems[5].Text == sockNum.ToString())
					{
						lvi.SubItems[2].Text = users;
						int totalUsers = 0;
						try{totalUsers = Convert.ToInt32(users);}
						catch{System.Diagnostics.Debug.WriteLine("Connection OUpdateStats 1");}
						if(totalUsers >= 2500)
							lvi.ForeColor = Stats.settings.clHighlight1;
						else if(totalUsers >= 800)
							lvi.ForeColor = Stats.settings.clHighlight2;
						else
							lvi.ForeColor = Stats.settings.clHighlight3;
						lvi.SubItems[3].Text = files;
						lvi.SubItems[4].Text = gigs;
						return;
					}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection OUpdateStats 2");
			}
		}

		public void OMessage(string serverNum, string message)
		{
			try
			{
				if(serverNum.Length < 1 || message.Length < 1)
					return;
				richMessages.SelectionColor = Stats.settings.clChatHeader;
				richMessages.SelectionFont = new Font("Arial", 8, FontStyle.Bold);
				richMessages.AppendText("<C#"+serverNum+"> ");
				richMessages.SelectionColor = Stats.settings.clChatPeer;
				richMessages.SelectionFont = new Font("Arial", 8, FontStyle.Regular);
				richMessages.AppendText(message + "\r\n");
				richMessages.Select();
				listView2.Select();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection OMessage");
			}
		}

		/// <summary>
		/// Connect to a random OpenNap server.
		/// </summary>
		public void ConnectRandomOpenNap()
		{
			if(listView2.Items.Count == 0)
				return;
			//pick a random listviewitem
			ListViewItem lvi = listView2.Items[GUID.rand.Next(0, listView2.Items.Count)];

			int sck = OpenNap.Sck.GetSck();
			if(sck != -1)
			{
				if(lvi.SubItems[1].Text == "Disconnected")
				{
					lvi.SubItems[1].Text = "Connecting";
					lvi.SubItems[5].Text = sck.ToString();
					lvi.ForeColor = Stats.settings.clHighlight3;
					OpenNap.Sck.scks[sck].Reset(lvi.Text);
				}
			}
			else
				return;
		}

		/// <summary>
		/// Get a random eDonkey server from our list.
		/// </summary>
		public void GetRandomEDonkey(IPandPort ipap)
		{
			if(listView3.Items.Count == 0)
				return;
			ListViewItem lvi = listView3.Items[GUID.rand.Next(0, listView3.Items.Count)];
			Utils.AddrParse(lvi.SubItems[1].Text, out ipap.ip, out ipap.port, 8888);
			return;
		}

		/// <summary>
		/// Incoming gnutella query.
		/// </summary>
		public void GQuery(string query)
		{
			if(tabControl1.SelectedIndex == 0)
			{
				listQueries.Items.Insert(0, query);
				if(listQueries.Items.Count > 50)
					listQueries.Items.RemoveAt(listQueries.Items.Count-1);
			}
		}

		/// <summary>
		/// Incoming gnutella 2 query.
		/// </summary>
		public void G2Query(string query)
		{
			if(tabControl1.SelectedIndex == 1)
			{
				listQueries2.Items.Insert(0, query);
				if(listQueries2.Items.Count > 50)
					listQueries2.Items.RemoveAt(listQueries2.Items.Count-1);
			}
		}

		private void menuItem1_Click(object sender, System.EventArgs e)
		{
			//g1 remove menu item
			GDisconnectEm();
		}

		/// <summary>
		/// Disconnect the selected gnutella connections.
		/// </summary>
		void GDisconnectEm()
		{
			try
			{
				foreach(ListViewItem lvi in listView1.SelectedItems)
					Gnutella.Sck.scks[Convert.ToInt32(lvi.SubItems[6].Text)].Disconnect("user disconnect");
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection GDisconnectEm");
			}
		}

		private void listView1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(listView1.SelectedItems.Count == 0)
				listView1.HideSelection = true;
			else
				listView1.HideSelection = false;
			this.contextMenu1.MenuItems[0].Enabled = (listView1.SelectedItems.Count == 1);
			this.contextMenu1.MenuItems[3].Enabled = (listView1.SelectedItems.Count > 0);

			//popup menu on right click
			if(e.Button == MouseButtons.Right)
			{
				System.Drawing.Point pos = new System.Drawing.Point(e.X, e.Y);
				this.contextMenu1.Show(listView1, pos);
			}
		}

		/// <summary>
		/// Loads the list of OpenNap servers from Stats.opennapHosts into our listview.
		/// </summary>
		public void LoadOpenNapList()
		{
			foreach(object obj in Stats.opennapHosts.Keys)
				AddOpenNapItem(obj.ToString());
		}

		public void AddOpenNapItem(string item)
		{
			string[] subitems = new string[6];
			subitems[0] = item;
			subitems[1] = "Disconnected";
			subitems[2] = "?";
			subitems[3] = "?";
			subitems[4] = "?";
			subitems[5] = "?";
			ListViewItem lvi = new ListViewItem(subitems);
			listView2.Items.Add(lvi);
		}

		/// <summary>
		/// Update the controls appropriately.
		/// </summary>
		public void UpdateUltrapeerStatus()
		{
			/*
			if(Stats.Updated.Gnutella.ultrapeer)
			{
				this.labelStatus1.Text = "[Ultrapeer]";
				this.label1.Enabled = true;
				this.updownConnections1.Enabled = true;
			}
			else
			{
				this.labelStatus1.Text = "[Leaf Node]";
				this.label1.Enabled = false;
				this.updownConnections1.Enabled = false;
			}
			*/
			if(Stats.Updated.Gnutella2.ultrapeer)
			{
				this.label7.Text = "[Ultrapeer]";
				this.label8.Enabled = true;
				this.numericUpDown1.Enabled = true;
			}
			else
			{
				this.label7.Text = "[Leaf Node]";
				this.label8.Enabled = false;
				this.numericUpDown1.Enabled = false;
			}
		}

		private void updownConnections1_ValueChanged(object sender, System.EventArgs e)
		{
			updownStuff();
		}

		private void updownConnections1_Leave(object sender, System.EventArgs e)
		{
			updownStuff();
		}

		void updownStuff()
		{
			if(Convert.ToInt32(numericUpDown1.Value) > 800)
				numericUpDown1.Value = 800;
			else if(Convert.ToInt32(numericUpDown1.Value) < 40)
				numericUpDown1.Value = 40;
			Stats.settings.gConnectionsToKeep = (int)numericUpDown1.Value;
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(e.Button.Text)
			{
				case "Connect":
					OpenNapConnect();
					break;
				case "Disconnect":
					OpenNapDisconnect();
					break;
				case "Add":
					OpenNapAdd();
					break;
				case "Remove":
					OpenNapRemove();
					break;
				case "Open .wsx":
					OpenNapWSX();
					break;
			}
		}

		private void listView2_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			//create new sorter
			ListViewSorter sorter = new ListViewSorter(e.Column, listView2.Columns[e.Column].Text.ToLower());
			//assign sorter
			listView2.ListViewItemSorter = sorter;
			//stop sorting
			listView2.ListViewItemSorter = null;
		}

		private void listView2_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(listView2.SelectedItems.Count == 0)
				listView2.HideSelection = true;
			else
				listView2.HideSelection = false;
			SetupToolbarAndMenus();

			//popup menu on right click
			if(e.Button == MouseButtons.Right)
			{
				System.Drawing.Point pos = new System.Drawing.Point(e.X, e.Y);
				this.contextMenu2.Show(listView2, pos);
			}
		}

		void SetupToolbarAndMenus()
		{
			this.contextMenu2.MenuItems[0].Enabled = (listView2.SelectedItems.Count > 0);
			this.contextMenu2.MenuItems[1].Enabled = (listView2.SelectedItems.Count > 0);
			this.contextMenu2.MenuItems[4].Enabled = (listView2.SelectedItems.Count > 0);
			this.toolBar1.Buttons[0].Enabled = (listView2.SelectedItems.Count > 0);
			this.toolBar1.Buttons[1].Enabled = (listView2.SelectedItems.Count > 0);
			this.toolBar1.Buttons[4].Enabled = (listView2.SelectedItems.Count > 0);
		}

		void OpenNapConnect()
		{
			try
			{
				for(int x = 0; x < listView2.SelectedItems.Count; x++)
				{
					int sck = OpenNap.Sck.GetSck();
					if(sck != -1)
					{
						listView2.SelectedItems[x].SubItems[1].Text = "Connecting";
						listView2.SelectedItems[x].SubItems[5].Text = sck.ToString();
						listView2.SelectedItems[x].ForeColor = Stats.settings.clHighlight3;
						OpenNap.Sck.scks[sck].Reset(listView2.SelectedItems[x].Text);
					}
					else
						return;
				}
				SetupToolbarAndMenus();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection OpenNapConnect");
			}
		}

		void OpenNapDisconnect()
		{
			try
			{
				for(int x = 0; x < listView2.SelectedItems.Count; x++)
				{
					if(listView2.SelectedItems[x].SubItems[5].Text != "?")
						OpenNap.Sck.scks[Convert.ToInt32(listView2.SelectedItems[x].SubItems[5].Text)].Disconnect();
				}
				SetupToolbarAndMenus();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection OpenNapDisconnect");
			}
		}

		void OpenNapAdd()
		{
			try
			{
				string server = InputBox.Show("What is the server address?");
				if(server != "")
				{
					string addr1; int port1;
					Utils.AddrParse(server, out addr1, out port1, 8888);
					server = addr1.ToLower()+":"+port1.ToString();
					if(!Stats.opennapHosts.ContainsKey(server))
					{
						AddOpenNapItem(server);
						Stats.opennapHosts.Add(server, null);
					}
				}
				SetupToolbarAndMenus();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection OpenNapAdd");
			}
		}

		void OpenNapRemove()
		{
			try
			{
				for(int x = listView2.SelectedItems.Count-1; x >= 0; x--)
				{
					Stats.opennapHosts.Remove(listView2.SelectedItems[x].Text);
					listView2.Items.Remove(listView2.SelectedItems[x]);
				}
				SetupToolbarAndMenus();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection OpenNapRemove");
			}
		}

		void OpenNapWSX()
		{
			try
			{
				OpenFileDialog ofd = new OpenFileDialog();
				ofd.Filter = "WSX Files (*.wsx)|*.wsx";
				ofd.ShowDialog();
				if(ofd.FileName != "")
				{
					listView2.Items.Clear();
					Stats.LoadSave.LoadOpenNapWSXFile(ofd.FileName);
				}
				else
					return;
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("OpenNapWSX: " + e.Message);
			}
			LoadOpenNapList();
			SetupToolbarAndMenus();
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			OpenNapConnect();
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			OpenNapDisconnect();
		}

		private void menuItem5_Click(object sender, System.EventArgs e)
		{
			OpenNapAdd();
		}

		private void menuItem6_Click(object sender, System.EventArgs e)
		{
			OpenNapRemove();
		}

		private void menuItem8_Click(object sender, System.EventArgs e)
		{
			OpenNapWSX();
		}

		private void menuItem9_Click(object sender, System.EventArgs e)
		{
			//g1 chat
			if(listView1.SelectedItems.Count != 1)
				return;
			string elIP = listView1.SelectedItems[0].SubItems[0].Text;
			ChatManager.Outgoing(ref elIP);
		}

		private void richMessages_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			e.Handled = true;
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			try
			{
				//g1
				lblPings.Text = "Pings / sec: " + Stats.Updated.Gnutella.numPings.ToString();
				Stats.Updated.Gnutella.numPings = 0;
				lblPongs.Text = "Pongs / sec: " + Stats.Updated.Gnutella.numPongs.ToString();
				Stats.Updated.Gnutella.numPongs = 0;
				lblQueries.Text = "Queries / sec: " + Stats.Updated.Gnutella.numQueries.ToString();
				Stats.Updated.Gnutella.numQueries = 0;
				lblQueryHits.Text = "QueryHits / sec: " + Stats.Updated.Gnutella.numQueryHits.ToString();
				Stats.Updated.Gnutella.numQueryHits = 0;
				lblPushes.Text = "Pushes / sec: " + Stats.Updated.Gnutella.numPushes.ToString();
				Stats.Updated.Gnutella.numPushes = 0;

				//g2
				lblg2pi.Text = "PI / sec: " + Stats.Updated.Gnutella2.numPI.ToString();
				Stats.Updated.Gnutella2.numPI = 0;
				lblg2po.Text = "PO / sec: " + Stats.Updated.Gnutella2.numPO.ToString();
				Stats.Updated.Gnutella2.numPO = 0;
				lblg2lni.Text = "LNI / sec: " + Stats.Updated.Gnutella2.numLNI.ToString();
				Stats.Updated.Gnutella2.numLNI = 0;
				lblg2khl.Text = "KHL / sec: " + Stats.Updated.Gnutella2.numKHL.ToString();
				Stats.Updated.Gnutella2.numKHL = 0;
				lblg2q2.Text = "Q2 / sec: " + Stats.Updated.Gnutella2.numQ2.ToString();
				Stats.Updated.Gnutella2.numQ2 = 0;
				lblg2qa.Text = "QA / sec: " + Stats.Updated.Gnutella2.numQA.ToString();
				Stats.Updated.Gnutella2.numQA = 0;
				lblg2qh2.Text = "QH2 / sec: " + Stats.Updated.Gnutella2.numQH2.ToString();
				Stats.Updated.Gnutella2.numQH2 = 0;
				lblg2push.Text = "PUSH / sec: " + Stats.Updated.Gnutella2.numPUSH.ToString();
				Stats.Updated.Gnutella2.numPUSH = 0;
				lblg2qht.Text = "QHT / sec: " + Stats.Updated.Gnutella2.numQHT.ToString();
				Stats.Updated.Gnutella2.numQHT = 0;
				lblg2qkr.Text = "QKR / sec: " + Stats.Updated.Gnutella2.numQKR.ToString();
				Stats.Updated.Gnutella2.numQKR = 0;
				lblg2qka.Text = "QKA / sec: " + Stats.Updated.Gnutella2.numQKA.ToString();
				Stats.Updated.Gnutella2.numQKA = 0;
			}
			catch(Exception e2)
			{
				System.Diagnostics.Debug.WriteLine("Connection timer1_Tick: " + e2.Message);
			}
		}

		public void EJustConnected(int sockNum)
		{
			try
			{
				foreach(ListViewItem lvi in listView3.Items)
					if(lvi.SubItems[6].Text == sockNum.ToString())
					{
						lvi.SubItems[3].Text = "Connected";
						lvi.ForeColor = Stats.settings.clHighlight3;
						return;
					}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection EJustConnected");
			}
		}

		public void EJustDisconnected(int sockNum)
		{
			try
			{
				foreach(ListViewItem lvi in listView3.Items)
					if(lvi.SubItems[6].Text == sockNum.ToString())
					{
						lvi.SubItems[3].Text = "Disconnected";
						lvi.SubItems[6].Text = "?";
						lvi.ForeColor = Stats.settings.clHighlight4;
						return;
					}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection EJustDisconnected");
			}
		}

		public void EMessage(string serverNum, string message)
		{
			try
			{
				if(serverNum.Length < 1 || message.Length < 1)
					return;
				richMessages2.SelectionColor = Stats.settings.clChatHeader;
				richMessages2.SelectionFont = new Font("Arial", 8, FontStyle.Bold);
				richMessages2.AppendText("<C#"+serverNum+"> ");
				richMessages2.SelectionColor = Stats.settings.clChatPeer;
				richMessages2.SelectionFont = new Font("Arial", 8, FontStyle.Regular);
				richMessages2.AppendText(message + "\r\n");
				richMessages2.Select();
				listView3.Select();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection EMessage");
			}
		}

		public void EUpdateStats(int sockNum)
		{
			try
			{
				foreach(ListViewItem lvi in listView3.Items)
					if(lvi.SubItems[6].Text == sockNum.ToString())
					{
						//find info
						Stats.EDonkeyServerInfo edsi = (Stats.EDonkeyServerInfo)Stats.edonkeyHosts[lvi.SubItems[1].Text];
						lvi.SubItems[0].Text = edsi.servName;
						lvi.SubItems[2].Text = edsi.servDesc;
						lvi.SubItems[4].Text = edsi.curUsers.ToString() + " / " + edsi.maxUsers.ToString();
						if(edsi.curUsers >= 4000)
							lvi.ForeColor = Stats.settings.clHighlight1;
						else if(edsi.curUsers >= 1000)
							lvi.ForeColor = Stats.settings.clHighlight2;
						else
							lvi.ForeColor = Stats.settings.clHighlight3;
						lvi.SubItems[5].Text = edsi.curFiles.ToString();
						return;
					}
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection EUpdateStats 2");
			}
		}

		private void toolBar2_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(e.Button.Text)
			{
				case "Connect":
					EDonkeyConnect();
					break;
				case "Disconnect":
					EDonkeyDisconnect();
					break;
				case "Add":
					EDonkeyAdd();
					break;
				case "Remove":
					EDonkeyRemove();
					break;
				case "Open .met":
					EDonkeyMET();
					break;
			}
		}

		void EDonkeyConnect()
		{
			try
			{
				for(int x = 0; x < listView3.SelectedItems.Count; x++)
				{
					int sck = EDonkey.Sck.GetSck();
					if(sck != -1)
					{
						listView3.SelectedItems[x].SubItems[3].Text = "Connecting";
						listView3.SelectedItems[x].SubItems[6].Text = sck.ToString();
						listView3.SelectedItems[x].ForeColor = Stats.settings.clHighlight3;
						EDonkey.Sck.scks[sck].Reset(listView3.SelectedItems[x].SubItems[1].Text, true);
					}
					else
						return;
				}
				SetupToolbarAndMenus2();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection EDonkeyConnect");
			}
		}

		void EDonkeyDisconnect()
		{
			try
			{
				for(int x = 0; x < listView3.SelectedItems.Count; x++)
				{
					if(listView3.SelectedItems[x].SubItems[6].Text != "?")
						EDonkey.Sck.scks[Convert.ToInt32(listView3.SelectedItems[x].SubItems[6].Text)].Disconnect("from gui");
				}
				SetupToolbarAndMenus2();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection EDonkeyDisconnect");
			}
		}

		void EDonkeyAdd()
		{
			try
			{
				string server = InputBox.Show("What is the server address?");
				if(server == "")
					return;
				//check if valid
				try
				{
					string checkip = server;
					if(checkip.LastIndexOf(":") != -1)
						checkip = checkip.Substring(0, checkip.LastIndexOf(":"));
					byte[] bytesIP = Endian.BigEndianIP(checkip);
				}
				catch
				{
					MessageBox.Show("Invalid IP");
					return;
				}
				string addr1; int port1;
				Utils.AddrParse(server, out addr1, out port1, 4661);
				server = addr1.ToLower()+":"+port1.ToString();
				EDonkeyAdd(server, " Added Server", "New Server");
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection EDonkeyAdd");
			}
		}

		public void EDonkeyAdd(string server, string serverName, string serverDesc)
		{
			try
			{
				if(!Stats.edonkeyHosts.ContainsKey(server))
				{
					Stats.EDonkeyServerInfo edsi = new Stats.EDonkeyServerInfo();
					edsi.curFiles = 0;
					edsi.curUsers = 0;
					edsi.maxUsers = 0;
					edsi.servDesc = serverDesc;
					edsi.servName = serverName;
					Stats.edonkeyHosts.Add(server, edsi);
					string[] subitems = new string[7];
					subitems[0] = serverName;
					subitems[1] = server;
					subitems[2] = serverDesc;
					subitems[3] = "Disconnected";
					subitems[4] = "? / ?";
					subitems[5] = "?";
					subitems[6] = "?";
					ListViewItem lvi = new ListViewItem(subitems);
					listView3.Items.Add(lvi);
				}
				SetupToolbarAndMenus2();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection EDonkeyAdd2");
			}
		}

		void EDonkeyRemove()
		{
			try
			{
				for(int x = listView3.SelectedItems.Count-1; x >= 0; x--)
				{
					Stats.edonkeyHosts.Remove(listView3.SelectedItems[x].SubItems[1].Text);
					listView3.Items.Remove(listView3.SelectedItems[x]);
				}
				SetupToolbarAndMenus2();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection EDonkeyRemove");
			}
		}

		void EDonkeyMET()
		{
			try
			{
				OpenFileDialog ofd = new OpenFileDialog();
				ofd.Filter = "MET Files (*.met)|*.met";
				ofd.ShowDialog();
				if(ofd.FileName != "")
				{
					listView3.Items.Clear();
					Stats.LoadSave.LoadEDonkeyMETFile(ofd.FileName);
				}
				else
					return;
				
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("EDonkeyMET: " + e.Message);
			}
			LoadEDonkeyList();
			SetupToolbarAndMenus2();
		}

		void SetupToolbarAndMenus2()
		{
			this.contextMenu3.MenuItems[0].Enabled = (listView3.SelectedItems.Count > 0);
			this.contextMenu3.MenuItems[1].Enabled = (listView3.SelectedItems.Count > 0);
			this.contextMenu3.MenuItems[4].Enabled = (listView3.SelectedItems.Count > 0);
			this.toolBar2.Buttons[0].Enabled = (listView3.SelectedItems.Count > 0);
			this.toolBar2.Buttons[1].Enabled = (listView3.SelectedItems.Count > 0);
			this.toolBar2.Buttons[4].Enabled = (listView3.SelectedItems.Count > 0);
		}

		/// <summary>
		/// Load all of the stuff from Stats.edonkeyHosts into our listview.
		/// </summary>
		void LoadEDonkeyList()
		{
			for(int x = 0; x < Stats.edonkeyHosts.Count; x++)
			{
				string[] subitems = new string[7];
				subitems[1] = (string)Stats.edonkeyHosts.GetKeyList()[x];
				Stats.EDonkeyServerInfo edsi = (Stats.EDonkeyServerInfo)Stats.edonkeyHosts.GetValueList()[x];
				subitems[0] = edsi.servName;
				subitems[2] = edsi.servDesc;
				subitems[3] = "Disconnected";
				subitems[4] = (edsi.curUsers == 0) ? "?" : edsi.curUsers.ToString();
				subitems[4] += " / " + ((edsi.maxUsers == 0) ? "?" : edsi.maxUsers.ToString());
				subitems[5] = (edsi.curFiles == 0) ? "?" : edsi.curFiles.ToString();
				subitems[6] = "?";
				ListViewItem lvi = new ListViewItem(subitems);
				listView3.Items.Add(lvi);
			}
		}

		private void menuItem10_Click(object sender, System.EventArgs e)
		{
			EDonkeyConnect();
		}

		private void menuItem11_Click(object sender, System.EventArgs e)
		{
			EDonkeyDisconnect();
		}

		private void menuItem13_Click(object sender, System.EventArgs e)
		{
			EDonkeyAdd();
		}

		private void menuItem14_Click(object sender, System.EventArgs e)
		{
			EDonkeyRemove();
		}

		private void menuItem16_Click(object sender, System.EventArgs e)
		{
			EDonkeyMET();
		}

		private void listView3_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			//create new sorter
			ListViewSorter sorter = new ListViewSorter(e.Column, listView3.Columns[e.Column].Text.ToLower());
			//assign sorter
			listView3.ListViewItemSorter = sorter;
			//stop sorting
			listView3.ListViewItemSorter = null;
		}

		private void listView3_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(listView3.SelectedItems.Count == 0)
				listView3.HideSelection = true;
			else
				listView3.HideSelection = false;
			SetupToolbarAndMenus2();

			//popup menu on right click
			if(e.Button == MouseButtons.Right)
			{
				System.Drawing.Point pos = new System.Drawing.Point(e.X, e.Y);
				this.contextMenu3.Show(listView3, pos);
			}
		}

		private void listView4_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(listView4.SelectedItems.Count == 0)
				listView4.HideSelection = true;
			else
				listView4.HideSelection = false;
			this.contextMenu4.MenuItems[0].Enabled = (listView4.SelectedItems.Count == 1);
			this.contextMenu4.MenuItems[1].Enabled = (listView4.SelectedItems.Count == 1);
			this.contextMenu4.MenuItems[4].Enabled = (listView4.SelectedItems.Count > 0);

			//popup menu on right click
			if(e.Button == MouseButtons.Right)
			{
				System.Drawing.Point pos = new System.Drawing.Point(e.X, e.Y);
				this.contextMenu4.Show(listView4, pos);
			}
		}

		private void numericUpDown1_Leave(object sender, System.EventArgs e)
		{
			this.updownStuff();
		}

		private void numericUpDown1_ValueChanged(object sender, System.EventArgs e)
		{
			this.updownStuff();
		}

		private void menuItem19_Click(object sender, System.EventArgs e)
		{
			//g2 chat
			if(listView4.SelectedItems.Count != 1)
				return;
			Gnutella2.Sck g2sck = Gnutella2.Sck.scks[Convert.ToInt32(listView4.SelectedItems[0].SubItems[8].Text)];
			string chatip = g2sck.address + ":" + g2sck.port.ToString();
			ChatManager.Outgoing(ref chatip);
		}

		private void menuItem23_Click(object sender, System.EventArgs e)
		{
			//g2 browse
			if(listView4.SelectedItems.Count != 1)
				return;
			Gnutella2.Sck g2sck = Gnutella2.Sck.scks[Convert.ToInt32(listView4.SelectedItems[0].SubItems[8].Text)];
			Gnutella2.Sck.OutgoingBrowseHost(g2sck.address + ":" + g2sck.port.ToString());
		}

		private void menuItem17_Click(object sender, System.EventArgs e)
		{
			//g1 add
			string newHost = InputBox.Show("What is the IP address of the host you wish to connect to?");
			if(newHost.Length > 0)
			{
				if(newHost.ToLower().IndexOf(@"http://") != -1)
					Gnutella.Sck.OutgoingWebCache(newHost);
				else
					Gnutella.Sck.Outgoing(newHost);
			}
		}

		private void menuItem21_Click(object sender, System.EventArgs e)
		{
			//g2 add
			string newHost = InputBox.Show("What is the IP address of the host you wish to connect to?");
			if(newHost.Length > 0)
			{
				if(newHost.ToLower().IndexOf(@"http://") != -1)
					Gnutella2.Sck.OutgoingWebCache(newHost);
				else
					Gnutella2.Sck.Outgoing(newHost);
			}
		}

		private void menuItem22_Click(object sender, System.EventArgs e)
		{
			//g2 remove
			try
			{
				foreach(ListViewItem lvi in listView4.SelectedItems)
					Gnutella2.Sck.scks[Convert.ToInt32(lvi.SubItems[8].Text)].Disconnect("user disconnect");
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Connection g2 remove");
			}
		}

		public void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			listQueries.Items.Clear();
			listQueries2.Items.Clear();
		}
	}
}
