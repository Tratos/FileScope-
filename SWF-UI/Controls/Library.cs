// Library.cs
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

namespace FileScope
{
	/// <summary>
	/// Library page for managing our files.
	/// </summary>
	public class Library : System.Windows.Forms.UserControl
	{
		public delegate void updateShares();
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.TreeView treeShares;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.ListView listViewFiles;
		private System.Windows.Forms.ImageList iconList;
		private System.Windows.Forms.ImageList toolbarList;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ToolBarButton toolBarButton3;
		private System.Windows.Forms.ToolBarButton toolBarButton4;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private ElMenuItem menuItem1;
		private ElMenuItem menuItem2;
		private ElMenuItem menuItem3;
		private ElMenuItem menuItem4;
		private ElMenuItem menuItem5;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ToolBarButton toolBarButton5;
		private System.Windows.Forms.Label label6;
		public ContextMenu[] cmArray;

		public Library()
		{
			InitializeComponent();
			cmArray = new ContextMenu[]{this.contextMenu1};
			Control c = (Control)this;
			Themes.SetupTheme(c);
			pictureBox1.Image = StartApp.main.pictureMoreInfo.Image;
			UpdateShares();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Library));
			this.treeShares = new System.Windows.Forms.TreeView();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton5 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton4 = new System.Windows.Forms.ToolBarButton();
			this.toolbarList = new System.Windows.Forms.ImageList(this.components);
			this.listViewFiles = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.iconList = new System.Windows.Forms.ImageList(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new FileScope.ElMenuItem();
			this.menuItem5 = new FileScope.ElMenuItem();
			this.menuItem2 = new FileScope.ElMenuItem();
			this.menuItem3 = new FileScope.ElMenuItem();
			this.menuItem4 = new FileScope.ElMenuItem();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.label6 = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// treeShares
			// 
			this.treeShares.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.treeShares.Dock = System.Windows.Forms.DockStyle.Left;
			this.treeShares.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.treeShares.HideSelection = false;
			this.treeShares.HotTracking = true;
			this.treeShares.ImageIndex = -1;
			this.treeShares.Name = "treeShares";
			this.treeShares.Scrollable = false;
			this.treeShares.SelectedImageIndex = -1;
			this.treeShares.Size = new System.Drawing.Size(104, 408);
			this.treeShares.TabIndex = 0;
			this.treeShares.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeShares_AfterSelect);
			this.treeShares.MouseMove += new System.Windows.Forms.MouseEventHandler(this.treeShares_MouseMove);
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.Color.Black;
			this.splitter1.Location = new System.Drawing.Point(104, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(6, 408);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			// 
			// toolBar1
			// 
			this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarButton1,
																						this.toolBarButton5,
																						this.toolBarButton2,
																						this.toolBarButton3,
																						this.toolBarButton4});
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.toolBar1.ImageList = this.toolbarList;
			this.toolBar1.Location = new System.Drawing.Point(110, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(482, 41);
			this.toolBar1.TabIndex = 2;
			this.toolBar1.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.Enabled = false;
			this.toolBarButton1.ImageIndex = 0;
			this.toolBarButton1.Text = "Launch";
			this.toolBarButton1.ToolTipText = "Open the selected file";
			// 
			// toolBarButton5
			// 
			this.toolBarButton5.ImageIndex = 1;
			this.toolBarButton5.Text = "Refresh";
			this.toolBarButton5.ToolTipText = "Refresh all shared files";
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.Enabled = false;
			this.toolBarButton2.ImageIndex = 3;
			this.toolBarButton2.Text = "Move";
			this.toolBarButton2.ToolTipText = "Move the selected file(s)";
			// 
			// toolBarButton3
			// 
			this.toolBarButton3.Enabled = false;
			this.toolBarButton3.ImageIndex = 2;
			this.toolBarButton3.Text = "Rename";
			this.toolBarButton3.ToolTipText = "Rename the selected file";
			// 
			// toolBarButton4
			// 
			this.toolBarButton4.Enabled = false;
			this.toolBarButton4.ImageIndex = 4;
			this.toolBarButton4.Text = "Delete";
			this.toolBarButton4.ToolTipText = "Delete the selected file(s)";
			// 
			// toolbarList
			// 
			this.toolbarList.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.toolbarList.ImageSize = new System.Drawing.Size(32, 32);
			this.toolbarList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("toolbarList.ImageStream")));
			this.toolbarList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// listViewFiles
			// 
			this.listViewFiles.AutoArrange = false;
			this.listViewFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnHeader1,
																							this.columnHeader2,
																							this.columnHeader3});
			this.listViewFiles.Dock = System.Windows.Forms.DockStyle.Top;
			this.listViewFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.listViewFiles.FullRowSelect = true;
			this.listViewFiles.GridLines = true;
			this.listViewFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewFiles.HideSelection = false;
			this.listViewFiles.Location = new System.Drawing.Point(110, 41);
			this.listViewFiles.Name = "listViewFiles";
			this.listViewFiles.Size = new System.Drawing.Size(482, 151);
			this.listViewFiles.SmallImageList = this.iconList;
			this.listViewFiles.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewFiles.TabIndex = 3;
			this.listViewFiles.View = System.Windows.Forms.View.Details;
			this.listViewFiles.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listViewFiles_MouseUp);
			this.listViewFiles.SelectedIndexChanged += new System.EventHandler(this.listViewFiles_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 260;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Size";
			this.columnHeader2.Width = 70;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Content Type";
			this.columnHeader3.Width = 120;
			// 
			// iconList
			// 
			this.iconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.iconList.ImageSize = new System.Drawing.Size(16, 16);
			this.iconList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("iconList.ImageStream")));
			this.iconList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label6,
																				 this.label4,
																				 this.label5,
																				 this.label3,
																				 this.label2,
																				 this.pictureBox1,
																				 this.label1});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(110, 256);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(482, 152);
			this.panel1.TabIndex = 4;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label4.Location = new System.Drawing.Point(24, 104);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(76, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Content-Type:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label5.Location = new System.Drawing.Point(24, 120);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(66, 13);
			this.label5.TabIndex = 6;
			this.label5.Text = "SHA1 Hash:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label3.Location = new System.Drawing.Point(24, 88);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(29, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Size:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label2.Location = new System.Drawing.Point(24, 72);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(31, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Path:";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(8, 8);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(60, 60);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 3;
			this.pictureBox1.TabStop = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(80, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(95, 22);
			this.label1.TabIndex = 2;
			this.label1.Text = "More Info";
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItem1,
																						 this.menuItem5,
																						 this.menuItem2,
																						 this.menuItem3,
																						 this.menuItem4});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "Open/Launch";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 1;
			this.menuItem5.Text = "Refresh";
			this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 2;
			this.menuItem2.Text = "Move";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 3;
			this.menuItem3.Text = "Rename";
			this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 4;
			this.menuItem4.Text = "Delete";
			this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.label6.Location = new System.Drawing.Point(24, 136);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(61, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "MD4 Hash:";
			// 
			// Library
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.panel1,
																		  this.listViewFiles,
																		  this.toolBar1,
																		  this.splitter1,
																		  this.treeShares});
			this.Name = "Library";
			this.Size = new System.Drawing.Size(592, 408);
			this.Resize += new System.EventHandler(this.Library_Resize);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// There was a change in the shared content, so we'll update the library.
		/// </summary>
		public void UpdateShares()
		{
			treeShares.BeginUpdate();
			try
			{
				toolBar1.Buttons[0].Enabled = false;
				toolBar1.Buttons[2].Enabled = false;
				toolBar1.Buttons[3].Enabled = false;
				toolBar1.Buttons[4].Enabled = false;
				contextMenu1.MenuItems[0].Enabled = false;
				contextMenu1.MenuItems[2].Enabled = false;
				contextMenu1.MenuItems[3].Enabled = false;
				contextMenu1.MenuItems[4].Enabled = false;
				string prevSelection = "";
				if(treeShares.SelectedNode != null)
					prevSelection = treeShares.SelectedNode.Tag.ToString();
				treeShares.Nodes.Clear();
				listViewFiles.Items.Clear();
				ClearMoreInfoArea();
				//create the shared lists
				lock(Stats.shareList)
				{
					foreach(string sharedDir in Stats.shareList)
					{
						string lastSubDir;
						int indexLastSep = sharedDir.LastIndexOf(Path.DirectorySeparatorChar) + 1;
						if(indexLastSep != 0)
							lastSubDir = sharedDir.Substring(indexLastSep);
						else
							lastSubDir = sharedDir;
						TreeNode tn = new TreeNode(lastSubDir);
						tn.Tag = sharedDir;
						treeShares.Nodes.Add(tn);
					}
				}
				//select one of the directories
				if(prevSelection != "")
					foreach(TreeNode tn in treeShares.Nodes)
						if(tn.Tag.ToString() == prevSelection)
							treeShares.SelectedNode = tn;
				if(treeShares.SelectedNode == null && treeShares.Nodes.Count > 0)
					treeShares.SelectedNode = treeShares.Nodes[0];
				//sometimes if i don't do this, the treeview gets messed up
				treeShares.ShowLines = false;
				treeShares.ShowLines = true;
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Library UpdateShares");
			}
			treeShares.EndUpdate();
		}

		private void treeShares_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			ClearMoreInfoArea();
			SeeDir(e.Node.Tag.ToString());
			listViewFiles.Height = this.Height - listViewFiles.Top - panel1.Height;
		}

		private void treeShares_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			TreeNode theNode = this.treeShares.GetNodeAt(e.X, e.Y);
			if(theNode != null)
				if(theNode.Tag != null)
				{
					if(theNode.Tag.ToString() != this.toolTip1.GetToolTip(this.treeShares))
						this.toolTip1.SetToolTip(this.treeShares, theNode.Tag.ToString());
					return;
				}
			this.toolTip1.SetToolTip(this.treeShares, "");
		}

		/// <summary>
		/// List files from a directory.
		/// </summary>
		void SeeDir(string path)
		{
			listViewFiles.BeginUpdate();
			try
			{
				toolBar1.Buttons[0].Enabled = false;
				toolBar1.Buttons[2].Enabled = false;
				toolBar1.Buttons[3].Enabled = false;
				toolBar1.Buttons[4].Enabled = false;
				contextMenu1.MenuItems[0].Enabled = false;
				contextMenu1.MenuItems[2].Enabled = false;
				contextMenu1.MenuItems[3].Enabled = false;
				contextMenu1.MenuItems[4].Enabled = false;
				//show all files shared from directory
				bool started = false;
				listViewFiles.Items.Clear();
				lock(Stats.fileList)
				{
					foreach(FileObject fo in Stats.fileList)
					{
						if(Path.GetDirectoryName(fo.location) == path)
						{
							started = true;
							string[] items = new string[3];
							items[0] = Path.GetFileName(fo.location);
							items[1] = Utils.Assemble(Convert.ToUInt32(Math.Round((double)fo.b / 1024)), " KB");
							items[2] = FileType.GetContentType(items[0]);
							ListViewItem lvi = new ListViewItem(items);
							lvi.Tag = fo.fileIndex;
							listViewFiles.Items.Add(lvi);
						}
						else
						{
							if(started == true)
							{
								listViewFiles.EndUpdate();
								return;
							}
						}
					}
				}
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Library SeeDir: " + e.Message);
			}
			listViewFiles.EndUpdate();
		}

		void ClearMoreInfoArea()
		{
			label2.Text = "Path: ";
			label3.Text = "Size: ";
			label4.Text = "Content-Type: ";
			label5.Text = "SHA1 Hash: ";
			label6.Text = "MD4 Hash: ";
		}

		private void listViewFiles_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(listViewFiles.SelectedItems.Count != 1)
			{
				ClearMoreInfoArea();
				return;
			}
			try
			{
				int fileID = (int)listViewFiles.SelectedItems[0].Tag;
				FileObject fo = (FileObject)Stats.fileList[fileID];
				label2.Text = "Path: " + fo.location;
				label3.Text = "Size: " + Utils.Assemble(fo.b, " bytes");
				label4.Text = "Content-Type: " + listViewFiles.SelectedItems[0].SubItems[2].Text;
				label5.Text = "SHA1 Hash: " + fo.sha1;
				label6.Text = "MD4 Hash: " + Utils.HexGuid(fo.md4);
			}
			catch(Exception eee)
			{
				System.Diagnostics.Debug.WriteLine("Library listViewFiles_SelectedIndexChanged: " + eee.Message);
			}
		}

		private void Library_Resize(object sender, System.EventArgs e)
		{
			listViewFiles.Height = this.Height - listViewFiles.Top - panel1.Height;
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(e.Button.Text)
			{
				case "Launch":
					Launch();
					break;
				case "Move":
					MoveFile();
					break;
				case "Rename":
					Rename();
					break;
				case "Delete":
					Delete();
					break;
				case "Refresh":
					Stats.LoadSave.UpdateShares();
					break;
			}
		}

		/// <summary>
		/// Open/launch the currently selected file.
		/// </summary>
		void Launch()
		{
			try
			{
				if(listViewFiles.SelectedItems.Count != 1)
					return;
				int fileID = (int)listViewFiles.SelectedItems[0].Tag;
				FileObject fo = (FileObject)Stats.fileList[fileID];
				System.Diagnostics.Process.Start(fo.location);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Library Launch");
			}
		}

		/// <summary>
		/// Move the currently selected files to a different directory.
		/// </summary>
		void MoveFile()
		{
			try
			{
				if(listViewFiles.SelectedItems.Count == 0)
					return;
				//browse for folder
				BrowseForFolder pickFolder = new BrowseForFolder();
				string strFolder = pickFolder.Show("Move to...");
				if(strFolder.Length == 0)
					return;
				Stats.LoadSave.suppressUpdate = true;
				foreach(ListViewItem lvi in listViewFiles.SelectedItems)
				{
					try
					{
						int fileID = (int)lvi.Tag;
						FileObject fo = (FileObject)Stats.fileList[fileID];
						File.Move(fo.location, Path.Combine(strFolder, Path.GetFileName(fo.location)));
					}
					catch
					{
						MessageBox.Show("Couldn't move this file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
				Stats.LoadSave.suppressUpdate = false;
				Stats.LoadSave.UpdateShares();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Library MoveFile");
			}
		}

		/// <summary>
		/// Rename the currently selected file.
		/// </summary>
		void Rename()
		{
			try
			{
				if(listViewFiles.SelectedItems.Count != 1)
					return;
				int fileID = (int)listViewFiles.SelectedItems[0].Tag;
				FileObject fo = (FileObject)Stats.fileList[fileID];
				string curDir = Path.GetDirectoryName(fo.location);
				string newFileName = InputBox.Show("What is the new name for this file?", Path.GetFileName(fo.location));
				if(newFileName == "")
					return;
				File.Move(fo.location, Path.Combine(curDir, newFileName));
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("Library Rename: " + e.Message);
			}
		}

		/// <summary>
		/// Delete the currently selected files.
		/// </summary>
		void Delete()
		{
			try
			{
				if(listViewFiles.SelectedItems.Count == 0)
					return;

				//ask the user if he/she's sure
				DialogResult dr = MessageBox.Show("Are you sure you want to permanently delete the selected files?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if(dr.ToString().ToLower() == "no")
					return;

				Stats.LoadSave.suppressUpdate = true;
				foreach(ListViewItem lvi in listViewFiles.SelectedItems)
				{
					try
					{
						int fileID = (int)lvi.Tag;
						FileObject fo = (FileObject)Stats.fileList[fileID];
						File.Delete(fo.location);
					}
					catch
					{
						MessageBox.Show("Couldn't delete this file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
				Stats.LoadSave.suppressUpdate = false;
				Stats.LoadSave.UpdateShares();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("Library Delete");
			}
		}

		private void menuItem1_Click(object sender, System.EventArgs e)
		{
			Launch();
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			MoveFile();
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			Rename();
		}

		private void menuItem4_Click(object sender, System.EventArgs e)
		{
			Delete();
		}

		private void menuItem5_Click(object sender, System.EventArgs e)
		{
			Stats.LoadSave.UpdateShares();
		}

		private void listViewFiles_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(listViewFiles.SelectedItems.Count == 0)
			{
				toolBar1.Buttons[0].Enabled = false;
				toolBar1.Buttons[2].Enabled = false;
				toolBar1.Buttons[3].Enabled = false;
				toolBar1.Buttons[4].Enabled = false;
				contextMenu1.MenuItems[0].Enabled = false;
				contextMenu1.MenuItems[2].Enabled = false;
				contextMenu1.MenuItems[3].Enabled = false;
				contextMenu1.MenuItems[4].Enabled = false;
			}
			else if(listViewFiles.SelectedItems.Count == 1)
			{
				toolBar1.Buttons[0].Enabled = true;
				toolBar1.Buttons[2].Enabled = true;
				toolBar1.Buttons[3].Enabled = true;
				toolBar1.Buttons[4].Enabled = true;
				contextMenu1.MenuItems[0].Enabled = true;
				contextMenu1.MenuItems[2].Enabled = true;
				contextMenu1.MenuItems[3].Enabled = true;
				contextMenu1.MenuItems[4].Enabled = true;
			}
			else
			{
				toolBar1.Buttons[0].Enabled = false;
				toolBar1.Buttons[2].Enabled = true;
				toolBar1.Buttons[3].Enabled = false;
				toolBar1.Buttons[4].Enabled = true;
				contextMenu1.MenuItems[0].Enabled = false;
				contextMenu1.MenuItems[2].Enabled = true;
				contextMenu1.MenuItems[3].Enabled = false;
				contextMenu1.MenuItems[4].Enabled = true;
			}

			//popup menu on right click
			if(e.Button == MouseButtons.Right)
			{
				System.Drawing.Point pos = new System.Drawing.Point(e.X, e.Y);
				contextMenu1.Show(listViewFiles, pos);
			}
		}
	}
}
