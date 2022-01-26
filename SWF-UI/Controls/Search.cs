// Search.cs
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
	/// The search page.
	/// </summary>
	public class Search : UserControl
	{
		public FileScope.ElTabControl tabControl1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button goButton;
		public System.Windows.Forms.TextBox textSearch;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Timer tmrConnection;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.ComponentModel.IContainer components;

		public Search()
		{
			InitializeComponent();
			this.pictureBox1.Image = StartApp.main.Icon.ToBitmap();
			toolTip1.SetToolTip(goButton, "Search for this item");
			DeserializeSearches();
			Control c = (Control)this;
			Themes.SetupTheme(c);
		}

		private void Search_Load(object sender, System.EventArgs e)
		{
			//
		}

		/// <summary>
		/// Serialize any current searches.
		/// </summary>
		public void SerializeSearches()
		{
			if(tabControl1.TabCount > 0)
				foreach(SearchTabPage stp in tabControl1.TabPages)
					stp.SerializeResults();
		}

		/// <summary>
		/// Load any possible searches.
		/// </summary>
		public void DeserializeSearches()
		{
			if(Directory.Exists(Utils.GetCurrentPath("searches")))
			{
				string[] files = Directory.GetFiles(Utils.GetCurrentPath("searches"));
				if(files.Length > 0)
					foreach(string fileName in files)
					{
						SearchTabPage page = new SearchTabPage(Path.GetFileName(fileName).Substring(1, Path.GetFileName(fileName).Length-2), true);
						tabControl1.TabPages.Add(page);
					}
			}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Search));
			this.tabControl1 = new FileScope.ElTabControl();
			this.label1 = new System.Windows.Forms.Label();
			this.textSearch = new System.Windows.Forms.TextBox();
			this.goButton = new System.Windows.Forms.Button();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.tmrConnection = new System.Windows.Forms.Timer(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabControl1.HotTrack = true;
			this.tabControl1.Location = new System.Drawing.Point(0, 45);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(352, 320);
			this.tabControl1.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.label1.Location = new System.Drawing.Point(39, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(113, 25);
			this.label1.TabIndex = 1;
			this.label1.Text = "Search for:";
			// 
			// textSearch
			// 
			this.textSearch.Enabled = false;
			this.textSearch.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textSearch.Location = new System.Drawing.Point(152, 10);
			this.textSearch.Multiline = true;
			this.textSearch.Name = "textSearch";
			this.textSearch.Size = new System.Drawing.Size(203, 25);
			this.textSearch.TabIndex = 2;
			this.textSearch.Text = "";
			this.textSearch.WordWrap = false;
			this.textSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textSearch_KeyPress);
			// 
			// goButton
			// 
			this.goButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.goButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.goButton.Image = ((System.Drawing.Bitmap)(resources.GetObject("goButton.Image")));
			this.goButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.goButton.Location = new System.Drawing.Point(361, 5);
			this.goButton.Name = "goButton";
			this.goButton.Size = new System.Drawing.Size(63, 33);
			this.goButton.TabIndex = 3;
			this.goButton.Text = "Go";
			this.goButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.goButton.Click += new System.EventHandler(this.goButton_Click);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(5, 5);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(32, 32);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			// 
			// tmrConnection
			// 
			this.tmrConnection.Enabled = true;
			this.tmrConnection.Interval = 600;
			this.tmrConnection.Tick += new System.EventHandler(this.tmrConnection_Tick);
			// 
			// Search
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.pictureBox1,
																		  this.goButton,
																		  this.textSearch,
																		  this.label1,
																		  this.tabControl1});
			this.Name = "Search";
			this.Size = new System.Drawing.Size(520, 456);
			this.Resize += new System.EventHandler(this.Search_Resize);
			this.Load += new System.EventHandler(this.Search_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void Search_Resize(object sender, System.EventArgs e)
		{
			tabControl1.Width = this.ClientSize.Width;
			tabControl1.Height = this.ClientSize.Height - tabControl1.Top;
		}

		private void goButton_Click(object sender, System.EventArgs e)
		{
			NewSearch();
		}

		/// <summary>
		/// Spawn a search.
		/// </summary>
		public void NewSearch()
		{
			//assert we have some search string
			if(textSearch.Text.Length == 0)
				return;

			//check to see if any search pages are already searching this item
			for(int x = 0; x < tabControl1.TabCount; x++)
				if(tabControl1.TabPages[x].Tag.ToString().ToLower() == textSearch.Text.ToLower())
				{
					tabControl1.SelectedIndex = x;
					textSearch.Text = "";
					return;
				}

			//create and add a search page
			SearchTabPage page = new SearchTabPage(textSearch.Text, false);
			tabControl1.TabPages.Add(page);

			//select the newest search page and clear the search textbox
			tabControl1.SelectedIndex = tabControl1.TabCount - 1;
			textSearch.Text = "";
			textSearch.Focus();

			//update HomePage
			StartApp.main.homepage.ResetSearchesText();
		}

		/// <summary>
		/// All browse host locations should really just call tbis function.
		/// </summary>
		public void BrowseHost(string host)
		{
			string curr = textSearch.Text;
			textSearch.Text = "browse: " + host;
			NewSearch();
			textSearch.Text = curr;
			StartApp.main.tabControl1.SelectedTab = StartApp.main.tabPage3;
		}

		public delegate void removeSearch(string name);

		/// <summary>
		/// Remove a search by name.
		/// </summary>
		public void RemoveSearch(string name)
		{
			for(int tbp = 0; tbp < tabControl1.TabPages.Count; tbp++)
			{
				if(tabControl1.TabPages[tbp].Tag.ToString() == name)
				{
					tabControl1.TabPages.RemoveAt(tbp);
					if(tabControl1.TabCount == 0)
						if(textSearch.Enabled)
							textSearch.Focus();
						else
						{
							textSearch.Enabled = true;
							textSearch.Focus();
							textSearch.Enabled = false;
						}
					//update HomePage
					StartApp.main.homepage.ResetSearchesText();
					return;
				}
			}
		}

		private void textSearch_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//make sure key is enter
			if(e.KeyChar == (char)13)
			{
				e.Handled = true;
				NewSearch();
			}
		}

		private void tmrConnection_Tick(object sender, System.EventArgs e)
		{
			//we have to make the search box enabled when we're connected to a network
			if(Stats.Updated.Gnutella.lastConnectionCount == 0 && Stats.Updated.OpenNap.lastConnectionCount == 0 && Stats.Updated.EDonkey.lastConnectionCount == 0 && Stats.Updated.Gnutella2.lastConnectionCount == 0)
				textSearch.Enabled = false;
			else
			{
				//if the search textbox isn't enabled, then enable it
				if(textSearch.Enabled == false)
					textSearch.Enabled = true;
			}
		}
	}
}
