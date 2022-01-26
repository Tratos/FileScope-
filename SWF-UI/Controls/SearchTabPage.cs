// SearchTabPage.cs
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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileScope
{
	/// <summary>
	/// Tabpage embedded into the Search control.
	/// </summary>
	public class SearchTabPage : TabPage
	//public class SearchTabPage : UserControl
	{
		public delegate void addNewItem(QueryHitObject elQho, QueryHitTable elQht);
		private System.Windows.Forms.ImageList barIcons;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ToolBarButton toolBarButton3;
		private System.Windows.Forms.ToolBarButton toolBarButton4;
		private System.Windows.Forms.ToolBarButton toolBarButton5;
		private System.Windows.Forms.ToolBar toolbar;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ToolBarButton toolBarButton6;
		private System.Windows.Forms.ImageList iconList;
		private System.Windows.Forms.ComboBox cmbType;
		private System.Windows.Forms.ComboBox cmbSpeed;
		private System.Windows.Forms.ListView listResults;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.ColumnHeader columnHeader7;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private ElMenuItem menuItem1;
		private ElMenuItem menuItem2;
		private ElMenuItem menuItem3;
		private ElMenuItem menuItemsep;
		private ElMenuItem menuItem4;
		private ElMenuItem menuItem5;
		private ElMenuItem menuItem6;
		private ElMenuItem menuItem7;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ToolTip toolTip1;
		//current hidden ListViewItems
		public ArrayList hiddenlvis = new ArrayList();
		//store the filter settings
		int cbtype = 0;
		int cbspeed = 0;
		private System.Windows.Forms.ToolBarButton toolBarButton7;
		private ElMenuItem menuItem8;
		public ContextMenu[] cmArray;

		public SearchTabPage(string search, bool deserialize)
		{
			//this will be modified as more results come
			this.Text = search;
			//this tag is permanent for the search
			this.Tag = search;
			InitializeComponent();
			cmArray = new ContextMenu[]{this.contextMenu1};
			Control c = (Control)this;
			Themes.SetupTheme(c);
			cmbType.SelectedIndex = 0;
			cmbSpeed.SelectedIndex = 0;
			toolTip1.SetToolTip(cmbType, "Filter for a certain file type");
			toolTip1.SetToolTip(cmbSpeed, "Filter for a minimum speed");
			toolTip1.SetToolTip(label3, "Search results under the # column containing an asterisk(*) signify that those files are hash verified");
			if(deserialize)
			{
				//let's restore everything
				try
				{
					ArrayList serObjs;
					string filePath = Path.Combine(Utils.GetCurrentPath("searches"), "-" + search + "-");
					FileStream fsSer = new FileStream(filePath, FileMode.Open, FileAccess.Read);
					BinaryFormatter bfer = new BinaryFormatter();
					serObjs = (ArrayList)bfer.Deserialize(fsSer);
					fsSer.Close();
					File.Delete(filePath);
					foreach(QueryHitTable qht in serObjs)
						this.AddNewItem((QueryHitObject)qht.queryHitObjects[0], qht);
				}
				catch(Exception e)
				{
					System.Diagnostics.Debug.WriteLine("SearchTabPage deserialization problem: " + e.Message);
				}
			}
			else
			{
				if(search.Length < 8 || search.Substring(0, 8) != "browse: ")
				{
					//send out the actual searches
					Gnutella.Query.BroadcastQuery(search);
					Gnutella2.Search.BeginSearch(search);
					OpenNap.Messages.Search(search);
					EDonkey.Messages.Search(search);
				}
				else
					Gnutella2.Sck.OutgoingBrowseHost(search.Substring(8, search.Length-8));
			}
		}

		/// <summary>
		/// Save all of our results into a file.
		/// </summary>
		public void SerializeResults()
		{
			try
			{
				if(!Directory.Exists(Utils.GetCurrentPath("searches")))
					Directory.CreateDirectory(Utils.GetCurrentPath("searches"));
				ArrayList serObjs = new ArrayList(listResults.Items.Count + hiddenlvis.Count);
				foreach(ListViewItem lvi in listResults.Items)
					serObjs.Add((QueryHitTable)lvi.Tag);
				foreach(ListViewItem lvi in hiddenlvis)
					serObjs.Add((QueryHitTable)lvi.Tag);
				FileStream fsSer = File.Create(Path.Combine(Utils.GetCurrentPath("searches"), "-" + this.Tag.ToString() + "-"));
				BinaryFormatter bfer = new BinaryFormatter();
				bfer.Serialize(fsSer, serObjs);
				fsSer.Close();
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("SearchTabPage SerializeResults: " + e.Message);
			}
		}

		protected override void Dispose( bool disposing )
		{
			toolTip1.RemoveAll();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SearchTabPage));
			this.toolbar = new System.Windows.Forms.ToolBar();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton7 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton4 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton5 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton6 = new System.Windows.Forms.ToolBarButton();
			this.barIcons = new System.Windows.Forms.ImageList(this.components);
			this.listResults = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
			this.iconList = new System.Windows.Forms.ImageList(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.cmbType = new System.Windows.Forms.ComboBox();
			this.cmbSpeed = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new FileScope.ElMenuItem();
			this.menuItem7 = new FileScope.ElMenuItem();
			this.menuItem2 = new FileScope.ElMenuItem();
			this.menuItem8 = new FileScope.ElMenuItem();
			this.menuItem3 = new FileScope.ElMenuItem();
			this.menuItemsep = new FileScope.ElMenuItem();
			this.menuItem4 = new FileScope.ElMenuItem();
			this.menuItem5 = new FileScope.ElMenuItem();
			this.menuItem6 = new FileScope.ElMenuItem();
			this.label3 = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// toolbar
			// 
			this.toolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																					   this.toolBarButton1,
																					   this.toolBarButton2,
																					   this.toolBarButton7,
																					   this.toolBarButton3,
																					   this.toolBarButton4,
																					   this.toolBarButton5,
																					   this.toolBarButton6});
			this.toolbar.DropDownArrows = true;
			this.toolbar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.toolbar.ImageList = this.barIcons;
			this.toolbar.Name = "toolbar";
			this.toolbar.ShowToolTips = true;
			this.toolbar.Size = new System.Drawing.Size(804, 55);
			this.toolbar.TabIndex = 0;
			this.toolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolbar_ButtonClick);
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.Enabled = false;
			this.toolBarButton1.ImageIndex = 0;
			this.toolBarButton1.Text = "Download";
			this.toolBarButton1.ToolTipText = "Download selected file(s)";
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.Enabled = false;
			this.toolBarButton2.ImageIndex = 1;
			this.toolBarButton2.Text = "Chat";
			this.toolBarButton2.ToolTipText = "Chat with hosts carrying this file (G1/G2 only)";
			// 
			// toolBarButton7
			// 
			this.toolBarButton7.Enabled = false;
			this.toolBarButton7.ImageIndex = 2;
			this.toolBarButton7.Text = "Browse";
			this.toolBarButton7.ToolTipText = "Browse the files in this host\'s library (G2 only)";
			// 
			// toolBarButton3
			// 
			this.toolBarButton3.Enabled = false;
			this.toolBarButton3.ImageIndex = 3;
			this.toolBarButton3.Text = "More Info";
			this.toolBarButton3.ToolTipText = "More info on selected file";
			// 
			// toolBarButton4
			// 
			this.toolBarButton4.ImageIndex = 4;
			this.toolBarButton4.Text = "Clear";
			this.toolBarButton4.ToolTipText = "Clear all results";
			// 
			// toolBarButton5
			// 
			this.toolBarButton5.ImageIndex = 5;
			this.toolBarButton5.Text = "Stop";
			this.toolBarButton5.ToolTipText = "Stop receiving results";
			// 
			// toolBarButton6
			// 
			this.toolBarButton6.ImageIndex = 6;
			this.toolBarButton6.Text = "Close";
			this.toolBarButton6.ToolTipText = "Terminate this search";
			// 
			// barIcons
			// 
			this.barIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.barIcons.ImageSize = new System.Drawing.Size(32, 32);
			this.barIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("barIcons.ImageStream")));
			this.barIcons.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// listResults
			// 
			this.listResults.AutoArrange = false;
			this.listResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.columnHeader1,
																						  this.columnHeader2,
																						  this.columnHeader3,
																						  this.columnHeader5,
																						  this.columnHeader6,
																						  this.columnHeader7,
																						  this.columnHeader8});
			this.listResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listResults.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.listResults.FullRowSelect = true;
			this.listResults.GridLines = true;
			this.listResults.HideSelection = false;
			this.listResults.Location = new System.Drawing.Point(0, 55);
			this.listResults.Name = "listResults";
			this.listResults.Size = new System.Drawing.Size(804, 369);
			this.listResults.SmallImageList = this.iconList;
			this.listResults.TabIndex = 1;
			this.listResults.View = System.Windows.Forms.View.Details;
			this.listResults.DoubleClick += new System.EventHandler(this.listResults_DoubleClick);
			this.listResults.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listResults_MouseUp);
			this.listResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listResults_ColumnClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "FileName";
			this.columnHeader1.Width = 280;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "#";
			this.columnHeader2.Width = 28;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "FileSize";
			this.columnHeader3.Width = 70;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Mp3 Info";
			this.columnHeader5.Width = 70;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Flags";
			this.columnHeader6.Width = 110;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Speed";
			this.columnHeader7.Width = 90;
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "Node";
			this.columnHeader8.Width = 70;
			// 
			// iconList
			// 
			this.iconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.iconList.ImageSize = new System.Drawing.Size(16, 16);
			this.iconList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iconList.ImageStream")));
			this.iconList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label1.Location = new System.Drawing.Point(339, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(33, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Type:";
			// 
			// cmbType
			// 
			this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbType.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cmbType.Items.AddRange(new object[] {
														 "Everything",
														 "Audio",
														 "Video",
														 "Images",
														 "Documents",
														 "Archives",
														 "Programs"});
			this.cmbType.Location = new System.Drawing.Point(338, 27);
			this.cmbType.Name = "cmbType";
			this.cmbType.Size = new System.Drawing.Size(98, 21);
			this.cmbType.TabIndex = 3;
			this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
			// 
			// cmbSpeed
			// 
			this.cmbSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.cmbSpeed.Items.AddRange(new object[] {
														  "0 KB/s",
														  "10 KB/s",
														  "30 KB/s",
														  "100 KB/s",
														  "300 KB/s",
														  "600 KB/s",
														  "1,000 KB/s",
														  "10,000 KB/s",
														  "50,000 KB/s",
														  "100,000 KB/s",
														  "500,000 KB/s",
														  "1,000,000 KB/s"});
			this.cmbSpeed.Location = new System.Drawing.Point(445, 27);
			this.cmbSpeed.Name = "cmbSpeed";
			this.cmbSpeed.Size = new System.Drawing.Size(98, 21);
			this.cmbSpeed.TabIndex = 5;
			this.cmbSpeed.SelectedIndexChanged += new System.EventHandler(this.cmbSpeed_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label2.Location = new System.Drawing.Point(446, 10);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(65, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Min. Speed:";
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItem1,
																						 this.menuItem7,
																						 this.menuItem2,
																						 this.menuItem8,
																						 this.menuItem3,
																						 this.menuItemsep,
																						 this.menuItem4,
																						 this.menuItem5,
																						 this.menuItem6});
			// 
			// menuItem1
			// 
			this.menuItem1.Enabled = false;
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "Download";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 1;
			this.menuItem7.Text = "-";
			// 
			// menuItem2
			// 
			this.menuItem2.Enabled = false;
			this.menuItem2.Index = 2;
			this.menuItem2.Text = "Chat";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 3;
			this.menuItem8.Text = "Browse";
			this.menuItem8.Click += new System.EventHandler(this.menuItem8_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Enabled = false;
			this.menuItem3.Index = 4;
			this.menuItem3.Text = "More Info";
			this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
			// 
			// menuItemsep
			// 
			this.menuItemsep.Index = 5;
			this.menuItemsep.Text = "-";
			this.menuItemsep.Click += new System.EventHandler(this.menuItem4_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 6;
			this.menuItem4.Text = "Clear";
			this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 7;
			this.menuItem5.Text = "Stop";
			this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 8;
			this.menuItem6.Text = "Close";
			this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label3.Location = new System.Drawing.Point(548, 6);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 40);
			this.label3.TabIndex = 6;
			this.label3.Text = "( * ) Hash Verified";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// SearchTabPage
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label3,
																		  this.cmbSpeed,
																		  this.label2,
																		  this.cmbType,
																		  this.label1,
																		  this.listResults,
																		  this.toolbar});
			this.Name = "SearchTabPage";
			this.Size = new System.Drawing.Size(804, 424);
			this.ResumeLayout(false);

		}
		#endregion

		private void toolbar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(e.Button.Text)
			{
				case "Close":
					Close();
					break;
				case "Clear":
					Clear();
					break;
				case "Stop":
					Stop();
					break;
				case "Download":
					Download();
					break;
				case "Chat":
					Chat();
					break;
				case "Browse":
					Browse();
					break;
				case "More Info":
					MoreInfo();
					break;
			}
		}

		/// <summary>
		/// Add a new search result to our listview.
		/// We accept either QueryHitObject or both qho and qht.
		/// </summary>
		public void AddNewItem(QueryHitObject elQho, QueryHitTable elQht)
		{
			string elType;
			if(elQht != null)
				elType = elQht.type;
			else
				elType = QHOStuff.GetType(elQho);
			//see if this object should be "visible" right now
			bool visqh = true;
			switch(cbtype)
			{
				case 0:
					break;
				case 1:
					if(elType != "Audio")
						visqh = false;
					break;
				case 2:
					if(elType != "Video")
						visqh = false;
					break;
				case 3:
					if(elType != "Image")
						visqh = false;
					break;
				case 4:
					if(elType != "Document")
						visqh = false;
					break;
				case 5:
					if(elType != "Archive")
						visqh = false;
					break;
				case 6:
					if(elType != "Program")
						visqh = false;
					break;
			}
			//for reference
			bool isFileTypeSame = visqh;
			//take care of minimum speed requirements
			if(cbspeed > elQho.speed)
				visqh = false;

			if(elQht == null)
			{
				//first we check for similar QueryHitObjects
				bool foundUpdate;
				//similar items can't have different file types, but can have different speeds
				if(isFileTypeSame)
				{
					foundUpdate = ScanUpdate(elQho, this.listResults.Items);
					if(foundUpdate)
						return;
					foundUpdate = ScanUpdate(elQho, this.hiddenlvis);
					if(foundUpdate)
						return;
				}
				else
				{
					foundUpdate = ScanUpdate(elQho, this.hiddenlvis);
					if(foundUpdate)
						return;
				}
			}

			//create the QueryHitTable we will embed into the ListViewItem's tag
			QueryHitTable qhTable = new QueryHitTable();
			if(elQht == null)
			{
				qhTable.address = elQho.ip.ToString() + ":" + elQho.port.ToString();
				qhTable.browse = elQho.browse;
				qhTable.busy = elQho.busy;
				qhTable.chat = elQho.chat;
				qhTable.mp3info = QHOStuff.GetMp3Info(elQho);
				qhTable.push = elQho.push;
				qhTable.speed = elQho.speed;
				qhTable.type = elType;
				qhTable.unseenHosts = elQho.unseenHosts;
				qhTable.sha1 = QHOStuff.GetSha1Ext(elQho);
				qhTable.md4sum = Utils.HexGuid(elQho.md4sum);
				//add this singular queryhit to the array of similar queryhits
				qhTable.queryHitObjects.Add(elQho);
			}
			else
				qhTable = elQht;
			int quickcount = listResults.Items.Count;
			if(visqh)
				quickcount++;
			this.Text = this.Tag + " (" + quickcount.ToString() + ")";
			//create listview subitems
			string[] items = new string[7];
			//filename
			items[0] = elQho.fileName;
			//# of hosts
			if(qhTable.sha1 == "")
				items[1] = qhTable.queryHitObjects.Count.ToString();
			else
				items[1] = qhTable.queryHitObjects.Count.ToString() + "*";
			//exception for eDonkey
			if(elQho.networkType == NetworkType.EDonkey)
				items[1] = qhTable.unseenHosts.ToString() + "*";
			//filesize
			items[2] = Utils.Assemble(Convert.ToUInt32(Math.Round((double)elQho.fileSize / 1024)), " KB");
			//mp3 info
			items[3] = qhTable.mp3info;
			//flags
			if(elQho.vendor != "")
				items[4] += elQho.vendor + ":";
			if(elQho.browse)
				items[4] += "-Browse";
			if(qhTable.chat)
				items[4] += "-Chat";
			if(qhTable.busy)
				items[4] += "-Busy";
			if(qhTable.push)
				items[4] += "-Firewall";
			//speed
			items[5] = Utils.Assemble(Convert.ToUInt32(qhTable.speed), " KB/s");
			//address
			items[6] = qhTable.address;

			//create listview item
			ListViewItem lvi = new ListViewItem(items);
			//add icon
			switch(qhTable.type)
			{
				case "Video":
					lvi.ImageIndex = 2;
					break;
				case "Audio":
					lvi.ImageIndex = 1;
					break;
				case "Image":
					lvi.ImageIndex = 4;
					break;
				case "Document":
					lvi.ImageIndex = 5;
					break;
				case "Archive":
					lvi.ImageIndex = 3;
					break;
				case "Program":
					lvi.ImageIndex = 6;
					break;
				default:
					lvi.ImageIndex = 0;
					break;
			}
			//tag stuff
			lvi.Tag = qhTable;
			//maybe we're already downloading it
			if(!qhTable.downloaded)
				foreach(DownloadManager dMEr in DownloadManager.dms)
					if(dMEr != null)
						if(dMEr.active)
						{
							QueryHitObject dlqho = ((Downloader)dMEr.downloaders[0]).qho;
							if(dlqho.fileSize == elQho.fileSize)
								if(Utils.SameArray(elQho.md4sum, dlqho.md4sum) || (elQho.sha1sum == dlqho.sha1sum && elQho.sha1sum != ""))
								{
									qhTable.downloaded = true;
									break;
								}
						}
			//highlight item
			HighlightItem(lvi);
			//add it
			if(visqh)
				listResults.Items.Add(lvi);
			else
				hiddenlvis.Add(lvi);

			//update the HomePage
			StartApp.main.homepage.ResetSearchesText();
		}

		/// <summary>
		/// Scan to see if we already have a similar QueryHitObject.
		/// If we do, we'll update the corresponding ListViewItem.
		/// If not, we return false.
		/// </summary>
		bool ScanUpdate(QueryHitObject elQho, IList ic)
		{
			QueryHitTable tbl;
			foreach(ListViewItem lvi in ic)
			{
				tbl = (QueryHitTable)lvi.Tag;
				//check for match
				if(QHOStuff.Matching(elQho, tbl))
				{
					//make sure the same host doesn't report multiple instances of the file
					if(!IPfilter.Private(elQho.ip))
						foreach(QueryHitObject qhObj in tbl.queryHitObjects)
							if(elQho.ip == qhObj.ip)
								return true;
					tbl.address = "Multiple";
					if(tbl.busy)
						tbl.busy = elQho.busy;
					if(!tbl.browse)
						tbl.browse = elQho.browse;
					if(!tbl.chat)
						tbl.chat = elQho.chat;
					if(tbl.mp3info == "")
						tbl.mp3info = QHOStuff.GetMp3Info(elQho);
					if(tbl.push)
						tbl.push = elQho.push;
					tbl.speed += elQho.speed;
					//add this singular queryhit to the array of similar queryhits
					tbl.queryHitObjects.Add(elQho);
					//count
					tbl.unseenHosts += elQho.unseenHosts;
					if(elQho.networkType == NetworkType.EDonkey)
						lvi.SubItems[1].Text = tbl.unseenHosts.ToString() + "*";
					else if(tbl.sha1 == "")
						lvi.SubItems[1].Text = tbl.queryHitObjects.Count.ToString();
					else
						lvi.SubItems[1].Text = tbl.queryHitObjects.Count.ToString() + "*";
					//mp3 info
					if(lvi.SubItems[3].Text == "")
						lvi.SubItems[3].Text = tbl.mp3info;
					//flags
					lvi.SubItems[4].Text = elQho.vendor + ":";
					if(tbl.browse)
						lvi.SubItems[4].Text += "-Browse";
					if(tbl.chat)
						lvi.SubItems[4].Text += "-Chat";
					if(tbl.busy)
						lvi.SubItems[4].Text += "-Busy";
					if(tbl.push)
						lvi.SubItems[4].Text += "-Firewall";
					//speed
					lvi.SubItems[5].Text = Utils.Assemble(Convert.ToUInt32(tbl.speed), " KB/s");
					//address
					lvi.SubItems[6].Text = tbl.address;
					//highlight item
					HighlightItem(lvi);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// [1] is the number of copies for the file
		/// [3] is mp3 info
		/// [4] is flags
		/// [5] is speed
		/// </summary>
		void HighlightItem(ListViewItem lvi)
		{
			QueryHitTable tbl = (QueryHitTable)lvi.Tag;

			//keep score
			int score = 0;

			int numCopies = tbl.queryHitObjects.Count;
			if(tbl.unseenHosts > 0)
				numCopies = tbl.unseenHosts;
			if(numCopies >= 10)
				score += 3;
			else if(numCopies >= 6)
				score += 2;
			else if(numCopies >= 3)
				score += 1;
			else
				score += 0;
			/*
			uint speed = Utils.Strip(lvi.SubItems[5].Text, " KB/s");
			if(speed >= 1200)
				score += 4;
			else if(speed >= 600)
				score += 3;
			else if(speed >= 100)
				score += 2;
			else
				score += 1;
			*/
			if(!tbl.busy && !tbl.push && tbl.chat)
				score += 3;
			else if(!tbl.busy && !tbl.push)
				score += 3;
			else if(!tbl.busy || !tbl.push)
				score += 1;
			else
				score -= 1;
			/*
			int mp3bitrate = 0;
			try{mp3bitrate = Convert.ToInt32(lvi.SubItems[3].Text.Substring(0, 3));}
			catch{}
			if(mp3bitrate > 192)
				score += 4;
			else if(mp3bitrate > 128)
				score += 3;
			else if(mp3bitrate > 64)
				score += 2;
			else
				score += 1;
			*/
			if(tbl.downloaded)
				lvi.ForeColor = Stats.settings.clListBoxFore;
			else if(score >= 4)
				lvi.ForeColor = Stats.settings.clHighlight1;
			else if(score >= 3)
				lvi.ForeColor = Stats.settings.clHighlight2;
			else if(score >= 1)
				lvi.ForeColor = Stats.settings.clHighlight3;
			else
				lvi.ForeColor = Stats.settings.clHighlight4;
		}

		private void listResults_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			//create new sorter
			ListViewSorter sorter = new ListViewSorter(e.Column, listResults.Columns[e.Column].Text.ToLower());
			//assign sorter
			listResults.ListViewItemSorter = sorter;
			//stop sorting
			listResults.ListViewItemSorter = null;
		}

		private void cmbType_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			this.cbtype = cmbType.SelectedIndex;
			cmbChange();
		}

		private void cmbSpeed_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			this.cbspeed = (int)Utils.Strip(cmbSpeed.Text, " KB/s");
			cmbChange();
		}

		/// <summary>
		/// Some combobox filter was just changed.
		/// </summary>
		void cmbChange()
		{
			//scan the listview
			listResults.BeginUpdate();
			ListViewItem lvi;
			for(int a = listResults.Items.Count - 1; a >= 0; a--)
			{
				lvi = listResults.Items[a];
				if(!AllowVisible(lvi))
				{
					/*
					 * the methodology is we check this item
					 * that way we know it has been analyzed
					 * when we scan the hiddenlvis, we ignore the checked ones
					 */
					lvi.Checked = true;
					hiddenlvis.Add(lvi);
					listResults.Items.Remove(lvi);
				}
			}

			//scan the hidden listviewitems
			for(int a = hiddenlvis.Count - 1; a >= 0; a--)
			{
				lvi = (ListViewItem)hiddenlvis[a];
				if(lvi.Checked)
				{
					//we ignore these items... see above
					lvi.Checked = false;
				}
				else if(AllowVisible(lvi))
				{
					listResults.Items.Add(lvi);
					hiddenlvis.Remove(lvi);
				}
			}
			listResults.EndUpdate();

			//update count
			this.Text = this.Tag.ToString() + " (" + listResults.Items.Count.ToString() + ")";
		}

		bool AllowVisible(ListViewItem lvi)
		{
			QueryHitTable elQht = (QueryHitTable)lvi.Tag;
			string elType = elQht.type;
			//see if this object should be "visible" right now
			bool visqh = true;
			switch(cbtype)
			{
				case 0:
					break;
				case 1:
					if(elType != "Audio")
						visqh = false;
					break;
				case 2:
					if(elType != "Video")
						visqh = false;
					break;
				case 3:
					if(elType != "Image")
						visqh = false;
					break;
				case 4:
					if(elType != "Document")
						visqh = false;
					break;
				case 5:
					if(elType != "Archive")
						visqh = false;
					break;
				case 6:
					if(elType != "Program")
						visqh = false;
					break;
			}
			//take care of minimum speed requirements
			if(cbspeed > elQht.speed)
				visqh = false;
			return visqh;
		}

		private void listResults_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			SetupToolbar();

			//popup menu on right click
			if(e.Button == MouseButtons.Right)
			{
				System.Drawing.Point pos = new System.Drawing.Point(e.X, e.Y);
				this.contextMenu1.Show(listResults, pos);
			}
		}

		void SetupToolbar()
		{
			if(listResults.SelectedItems.Count == 0)
				listResults.HideSelection = true;
			else
				listResults.HideSelection = false;
			//enable/disable stuff
			switch(listResults.SelectedItems.Count)
			{
				case 0:
					this.toolBarButton1.Enabled = false;
					this.toolBarButton2.Enabled = false;
					this.toolBarButton3.Enabled = false;
					this.toolBarButton7.Enabled = false;
					this.menuItem1.Enabled = false;
					this.menuItem2.Enabled = false;
					this.menuItem3.Enabled = false;
					this.menuItem8.Enabled = false;
					break;
				case 1:
					this.toolBarButton1.Enabled = true;
					this.toolBarButton2.Enabled = true;
					this.toolBarButton3.Enabled = true;
					this.toolBarButton7.Enabled = true;
					this.menuItem1.Enabled = true;
					this.menuItem2.Enabled = true;
					this.menuItem3.Enabled = true;
					this.menuItem8.Enabled = ((QueryHitObject)((QueryHitTable)listResults.SelectedItems[0].Tag).queryHitObjects[0]).browse;
					break;
				default:
					this.toolBarButton1.Enabled = true;
					this.toolBarButton2.Enabled = false;
					this.toolBarButton3.Enabled = false;
					this.toolBarButton7.Enabled = false;
					this.menuItem1.Enabled = true;
					this.menuItem2.Enabled = false;
					this.menuItem3.Enabled = false;
					this.menuItem8.Enabled = false;
					break;
			}
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			MoreInfo();
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			Chat();
		}

		private void menuItem1_Click(object sender, System.EventArgs e)
		{
			Download();
		}

		/// <summary>
		/// Find more info on the selected item.
		/// </summary>
		void MoreInfo()
		{
			if(listResults.SelectedItems.Count != 1)
				return;
			
			MoreSearchInfoDlg dlg = new MoreSearchInfoDlg();
			dlg.SetupView(((QueryHitTable)listResults.SelectedItems[0].Tag));
			dlg.ShowDialog();
		}

		/// <summary>
		/// Stop the current search.
		/// </summary>
		void Stop()
		{
			lock(Gnutella2.Search.activeSearches)
				foreach(Gnutella2.Search g2s in Gnutella2.Search.activeSearches)
					if(g2s.query == this.Tag.ToString())
					{
						g2s.TerminateSearch();
						return;
					}
			lock(ActiveSearch.searches)
				foreach(ActiveSearch searche in ActiveSearch.searches)
					if(searche.query == this.Tag.ToString())
					{
						searche.guid = "stopped";
						break;
					}
		}

		/// <summary>
		/// Clear current list of search responses.
		/// </summary>
		void Clear()
		{
			listResults.Items.Clear();
			hiddenlvis.Clear();
			this.Text = this.Tag.ToString();
		}

		/// <summary>
		/// Close this current search.
		/// </summary>
		void Close()
		{
			toolTip1.RemoveAll();
			this.contextMenu1.MenuItems.Clear();
			this.hiddenlvis.Clear();
			this.listResults.Items.Clear();
			this.toolbar.Buttons.Clear();
			//remove this search
			lock(Gnutella2.Search.activeSearches)
				for(int y = 0; y < Gnutella2.Search.activeSearches.Count; y++)
				{
					Gnutella2.Search g2s = (Gnutella2.Search)Gnutella2.Search.activeSearches[y];
					if(g2s.query == this.Tag.ToString())
					{
						g2s.TerminateSearch();
						Gnutella2.Search.activeSearches.RemoveAt(y);
						if(Gnutella2.Search.activeSearches.Count == 0)
							Gnutella2.Search.hubsWithoutKeys.Clear();
						break;
					}
				}
			lock(ActiveSearch.searches)
				for(int y = 0; y < ActiveSearch.searches.Count; y++)
				{
					ActiveSearch search = (ActiveSearch)ActiveSearch.searches[y];
					if(search.query == this.Tag.ToString())
					{
						ActiveSearch.searches.RemoveAt(y);
						break;
					}
				}
			//remove this search tabpage from the search area
			StartApp.main.search.BeginInvoke(new Search.removeSearch(StartApp.main.search.RemoveSearch), new object[]{this.Tag.ToString()});
		}

		/// <summary>
		/// Chat with the selected node.
		/// </summary>
		void Chat()
		{
			if(listResults.SelectedItems.Count != 1)
				return;

			ChattersDlg cd = new ChattersDlg();
			cd.SetupWindow(((QueryHitTable)listResults.SelectedItems[0].Tag));
			cd.ShowDialog();
		}

		/// <summary>
		/// Browse the file library of the selected node.
		/// </summary>
		void Browse()
		{
			if(listResults.SelectedItems.Count != 1)
				return;

			BrowseDlg bd = new BrowseDlg();
			bd.SetupWindow(((QueryHitTable)listResults.SelectedItems[0].Tag));
			bd.ShowDialog();
		}

		/// <summary>
		/// Download the selected item.
		/// </summary>
		void Download()
		{
			if(listResults.SelectedItems.Count == 0)
				return;

			if(listResults.SelectedItems.Count > 40)
			{
				MessageBox.Show("You cannot have more than 40 concurrent downloads!");
				return;
			}

			//loop through all selected items
			for(int y = 0; y < listResults.SelectedItems.Count; y++)
			{
				QueryHitTable tbl = ((QueryHitTable)listResults.SelectedItems[y].Tag);
				tbl.downloaded = true;
				QueryHitObject elqho = ((QueryHitObject)tbl.queryHitObjects[0]);
				//update the color on the ListViewItem
				HighlightItem(listResults.SelectedItems[y]);

				if(elqho.fileSize > 0)
				{
					string ext;
					try
					{
						ext = System.IO.Path.GetExtension(elqho.fileName);
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine("invalid file type");
						elqho.fileName = "invalid filename.unknown";
						ext = "unknown";
					}
					if(Stats.settings.fileAlert)
					{
						//provide a filter for dangerous file types
						if(ext.Length > 1)
						{
							ext = ext.Substring(1);
							if(FileType.GetType(ext) == "Program")
								if(!FileAlertDlg.Show())
									continue;
						}
					}
					//start the download
					DownloadManager.NewDownload(tbl);
				}
			}
		}

		private void listResults_DoubleClick(object sender, System.EventArgs e)
		{
			Download();
		}

		private void menuItem4_Click(object sender, System.EventArgs e)
		{
			Clear();
			SetupToolbar();
		}

		private void menuItem5_Click(object sender, System.EventArgs e)
		{
			Stop();
		}

		private void menuItem6_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void menuItem8_Click(object sender, System.EventArgs e)
		{
			Browse();
		}
	}
}
