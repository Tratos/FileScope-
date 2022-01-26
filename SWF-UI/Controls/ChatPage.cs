// ChatPage.cs
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
	/// Host the instant messaging here.
	/// </summary>
	public class ChatPage : System.Windows.Forms.UserControl
	{
		private FileScope.ElTabControl tabControl1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.Container components = null;
		public delegate void newChat(int chatNum);
		public delegate void connectedChat(int chatNum);
		public delegate void newChatData(int chatNum, string msg);
		public delegate void disconnectedChat(int chatNum);

		public ChatPage()
		{
			InitializeComponent();
			Control c = (Control)this;
			Themes.SetupTheme(c);
		}

		private void ChatPage_Load(object sender, System.EventArgs e)
		{
			//
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
			this.tabControl1 = new FileScope.ElTabControl();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.tabControl1.HotTrack = true;
			this.tabControl1.Location = new System.Drawing.Point(0, 40);
			this.tabControl1.Multiline = true;
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(408, 336);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tabControl1_MouseUp);
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label1,
																				 this.textBox1,
																				 this.button1});
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(408, 40);
			this.panel1.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(16, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(31, 22);
			this.label1.TabIndex = 2;
			this.label1.Text = "IP:";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(56, 12);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(104, 20);
			this.textBox1.TabIndex = 1;
			this.textBox1.Text = "";
			this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.Location = new System.Drawing.Point(168, 12);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(80, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Connect";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// ChatPage
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.panel1,
																		  this.tabControl1});
			this.Name = "ChatPage";
			this.Size = new System.Drawing.Size(408, 376);
			this.Resize += new System.EventHandler(this.ChatPage_Resize);
			this.Load += new System.EventHandler(this.ChatPage_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public void NewChat(int chatNum)
		{
			StartApp.main.tabControl1.SelectedTab = StartApp.main.tabPage7;
			ChatTabPage newChat = new ChatTabPage();
			tabControl1.TabPages.Add(newChat);
			newChat.SetupCrip(chatNum);
			tabControl1.SelectedIndex = tabControl1.TabCount - 1;
			if(!ChatManager.chats[chatNum].incoming)
				newChat.StartedConnecting();
		}

		public void ConnectedChat(int chatNum)
		{
			foreach(ChatTabPage ctp in tabControl1.TabPages)
				if(ctp.chatNum == chatNum)
				{
					ctp.JustFinishedHandshake();
					return;
				}
		}

		public void NewChatData(int chatNum, string msg)
		{
			foreach(ChatTabPage ctp in tabControl1.TabPages)
				if(ctp.chatNum == chatNum)
				{
					try
					{
						if(tabControl1.SelectedTab != ctp)
							if(!ctp.Text.EndsWith(" *"))
								ctp.Text += " *";
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine("NewChatData 1");
					}
					ctp.JustGotMessage(msg);
					return;
				}
		}

		public void DisconnectedChat(int chatNum)
		{
			foreach(ChatTabPage ctp in tabControl1.TabPages)
				if(ctp.chatNum == chatNum)
				{
					ctp.JustDisconnected();
					return;
				}
		}

		public delegate void removeChatTabPage();

		public void RemoveChatTabPage()
		{
			for(int tbp = 0; tbp < tabControl1.TabPages.Count; tbp++)
			{
				if(tabControl1.TabPages[tbp].Text == "dispose")
				{
					tabControl1.TabPages.RemoveAt(tbp);
					tabControl1.Focus();
					return;
				}
			}
		}

		private void tabControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			try
			{
				if(tabControl1.SelectedTab != null)
					if(tabControl1.SelectedTab.Text.EndsWith(" *"))
						tabControl1.SelectedTab.Text = tabControl1.SelectedTab.Text.Substring(0, tabControl1.SelectedTab.Text.Length-2);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("ChatPage tabControl1_MouseUp");
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			try
			{
				if(tabControl1.SelectedTab != null)
					if(tabControl1.SelectedTab.Text.EndsWith(" *"))
						tabControl1.SelectedTab.Text = tabControl1.SelectedTab.Text.Substring(0, tabControl1.SelectedTab.Text.Length-2);
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("ChatPage tabControl1_SelectedIndexChanged");
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			ChatIP();
		}

		private void textBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//if enter was pressed
			if(e.KeyChar == (char)13)
			{
				e.Handled = true;
				ChatIP();
			}
		}

		void ChatIP()
		{
			if(textBox1.Text.Length == 0)
				return;
			string peer = textBox1.Text;
			ChatManager.Outgoing(ref peer);
			textBox1.Text = "";
		}

		private void ChatPage_Resize(object sender, System.EventArgs e)
		{
			this.tabControl1.Height = this.ClientSize.Height - panel1.ClientSize.Height;
		}
	}
}
