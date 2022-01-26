// ChatTabPage.cs
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
	/// Tabpage inserted into the chat area.
	/// </summary>
	public class ChatTabPage : System.Windows.Forms.TabPage
	//public class ChatTabPage : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ToolBarButton toolBarButton3;
		public int chatNum;

		public ChatTabPage()
		{
			InitializeComponent();
			Control c = (Control)this;
			Themes.SetupTheme(c);
			toolTip1.SetToolTip(button1, "Send message to peer");
		}

		public void SetupCrip(int chatNum)
		{
			this.chatNum = chatNum;

			if(!ChatManager.chats[chatNum].incoming)
			{
				button1.Enabled = false;
				textBox1.Enabled = false;
				this.Text = ChatManager.chats[chatNum].address;
				this.Tag = ChatManager.chats[chatNum].address + ":" + ChatManager.chats[chatNum].port.ToString();
			}
			else
			{
				this.Text = ChatManager.chats[chatNum].RemoteIP();
				this.Tag = this.Text;
			}
		}

		public void StartedConnecting()
		{
			try
			{
				richTextBox1.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
				richTextBox1.SelectionColor = System.Drawing.Color.Green;
				richTextBox1.AppendText("[-Connecting to " + this.Text + "-]\r\n");
				ResizeMe();
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("ChatTabPage StartedConnecting");
			}
		}

		public void JustFinishedHandshake()
		{
			try
			{
				richTextBox1.SelectionColor = System.Drawing.Color.Green;
				richTextBox1.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
				richTextBox1.AppendText("[-Connected-]\r\n");
				textBox1.Enabled = true;
				button1.Enabled = true;
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("ChatTabPage JustFinishedHandshake");
			}
		}

		public void JustGotMessage(string msg)
		{
			try
			{
				richTextBox1.SelectionColor = Stats.settings.clChatHeader;
				richTextBox1.SelectionFont = new Font("Arial", 14, FontStyle.Bold);
				richTextBox1.AppendText("<Peer> ");
				richTextBox1.SelectionColor = Stats.settings.clChatPeer;
				richTextBox1.SelectionFont = new Font("Arial", 14, FontStyle.Regular);
				richTextBox1.AppendText(msg.Substring(0, msg.Length) + "\r\n");
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("ChatTabPage JustGotMessage");
			}
		}

		public void JustDisconnected()
		{
			try
			{
				richTextBox1.SelectionColor = System.Drawing.Color.Green;
				richTextBox1.SelectionFont = new Font("Arial", 10, FontStyle.Regular);
				richTextBox1.AppendText("[-Disconnected-]\r\n");
				textBox1.Enabled = false;
				button1.Enabled = false;
				this.chatNum = -1;
			}
			catch
			{
				System.Diagnostics.Debug.WriteLine("ChatTabPage JustDisconnected");
			}
		}

		protected override void Dispose( bool disposing )
		{
			this.toolTip1.RemoveAll();
			this.richTextBox1.KeyPress -= new System.Windows.Forms.KeyPressEventHandler(this.richTextBox1_KeyPress);
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ChatTabPage));
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBox1.Location = new System.Drawing.Point(0, 413);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(481, 27);
			this.textBox1.TabIndex = 0;
			this.textBox1.Text = "";
			this.textBox1.WordWrap = false;
			this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.button1.Location = new System.Drawing.Point(482, 413);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(76, 28);
			this.button1.TabIndex = 1;
			this.button1.Text = "Send";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// richTextBox1
			// 
			this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.richTextBox1.Location = new System.Drawing.Point(0, 42);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(528, 322);
			this.richTextBox1.TabIndex = 2;
			this.richTextBox1.Text = "";
			this.richTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBox1_KeyPress);
			// 
			// toolBar1
			// 
			this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarButton1,
																						this.toolBarButton3,
																						this.toolBarButton2});
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.toolBar1.ImageList = this.imageList1;
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(562, 41);
			this.toolBar1.TabIndex = 3;
			this.toolBar1.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.ImageIndex = 0;
			this.toolBarButton1.Text = "Block Host";
			this.toolBarButton1.ToolTipText = "Prevent this host from chatting with you";
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.ImageIndex = 0;
			this.toolBarButton2.Text = "Close";
			this.toolBarButton2.ToolTipText = "End the chat";
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(32, 32);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// toolBarButton3
			// 
			this.toolBarButton3.ImageIndex = 1;
			this.toolBarButton3.Text = "Browse Host";
			this.toolBarButton3.ToolTipText = "Check the shared library of this host";
			// 
			// ChatTabPage
			// 
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.toolBar1,
																		  this.richTextBox1,
																		  this.button1,
																		  this.textBox1});
			this.Name = "ChatTabPage";
			this.Size = new System.Drawing.Size(562, 446);
			this.Resize += new System.EventHandler(this.ChatTabPage_Resize);
			this.ResumeLayout(false);

		}
		#endregion

		private void ChatTabPage_Resize(object sender, System.EventArgs e)
		{
			ResizeMe();
		}

		void ResizeMe()
		{
			button1.Height = textBox1.Height;
			textBox1.Top = this.ClientSize.Height - textBox1.Height;
			button1.Top = textBox1.Top;
			button1.Left = this.ClientSize.Width - button1.Width;
			textBox1.Width = button1.Left;
			richTextBox1.Height = this.ClientSize.Height - textBox1.Height - toolBar1.Height - 2;
			richTextBox1.Width = this.ClientSize.Width;
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			SendMessage();
		}

		private void textBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			//make sure key is enter
			if(e.KeyChar == (char)13)
			{
				e.Handled = true;
				SendMessage();
			}
		}

		/// <summary>
		/// Send whatever's in the text box to our remote host.
		/// </summary>
		void SendMessage()
		{
			try
			{
				//make sure we have something to send
				if(textBox1.Text == "" || !textBox1.Enabled)
					return;

				richTextBox1.SelectionColor = Stats.settings.clChatHeader;
				richTextBox1.SelectionFont = new Font("Arial", 14, FontStyle.Bold);
				richTextBox1.AppendText("<You> ");
				richTextBox1.SelectionColor = Stats.settings.clChatYou;
				richTextBox1.SelectionFont = new Font("Arial", 14, FontStyle.Regular);

				//deal with our message
				string msg = textBox1.Text;
				textBox1.Clear();
				richTextBox1.AppendText(msg + "\r\n");

				//send our message
				msg += "\n";
				ChatManager.chats[chatNum].SendString(ref msg);
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("ChatTabPage SendMessage: " + e.Message);
			}
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(e.Button.Text)
			{
				case "Close":
					try
					{if(chatNum != -1) if(ChatManager.chats[chatNum] != null) ChatManager.chats[chatNum].Disconnect();}
					catch{System.Diagnostics.Debug.WriteLine("ChatTabPage toolBar1_ButtonClick 1");}
					this.richTextBox1.Clear();
					this.toolBar1.Buttons.Clear();
					this.Text = "dispose";
					StartApp.main.chatpage.BeginInvoke(new ChatPage.removeChatTabPage(StartApp.main.chatpage.RemoveChatTabPage));
					break;
				case "Block Host":
					try
					{if(chatNum != -1) if(ChatManager.chats[chatNum] != null) ChatManager.chats[chatNum].Disconnect();}
					catch{System.Diagnostics.Debug.WriteLine("ChatTabPage toolBar1_ButtonClick 2");}
					if(this.Text.EndsWith(" *"))
						this.Text = this.Text.Substring(0, this.Text.Length-2);
					if(this.Text != "")
						Stats.blockedChatHosts.Add(this.Text);
					this.richTextBox1.Clear();
					this.toolBar1.Buttons.Clear();
					this.Text = "dispose";
					StartApp.main.chatpage.BeginInvoke(new ChatPage.removeChatTabPage(StartApp.main.chatpage.RemoveChatTabPage));
					break;
				case "Browse Host":
					if(this.Text.EndsWith(" *"))
						this.Text = this.Text.Substring(0, this.Text.Length-2);
					if(this.Text != "")
						StartApp.main.search.BrowseHost((string)this.Tag);
					break;
			}
		}

		private void richTextBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			e.Handled = true;
		}
	}
}
