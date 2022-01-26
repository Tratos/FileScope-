// Themes.cs
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
using System.Windows.Forms;
using System.Drawing;

namespace FileScope
{
	/// <summary>
	/// Responsible for dealing with different color schemes in FileScope.
	/// </summary>
	public class Themes
	{
		/// <summary>
		/// This will take any object like a form or control and theme it accordingly.
		/// </summary>
		public static void SetupTheme(Control container)
		{
			//context menus
			ContextMenu[] cmArray = null;
			if(container.GetType() == typeof(Connection))
				cmArray = ((Connection)container).cmArray;
			else if(container.GetType() == typeof(Library))
				cmArray = ((Library)container).cmArray;
			else if(container.GetType() == typeof(SearchTabPage))
				cmArray = ((SearchTabPage)container).cmArray;
			else if(container.GetType() == typeof(Transfers))
				cmArray = ((Transfers)container).cmArray;
			if(cmArray != null)
				foreach(ContextMenu cm in cmArray)
					foreach(ElMenuItem emi in cm.MenuItems)
					{
						if(Stats.settings.theme != "Default Colors")
							emi.SwitchOwner();
						else if(Stats.settings.theme == "Default Colors")
							emi.SwitchDefault();
					}
			//main menus
			if(container.GetType() == typeof(MainDlg))
			{
				MainDlg md = (MainDlg)container;
				foreach(ElMenuItem emi in md.Menu.MenuItems)
				{
					if(Stats.settings.theme != "Default Colors")
						emi.SwitchOwner();
					else if(Stats.settings.theme == "Default Colors")
						emi.SwitchDefault();
					foreach(ElMenuItem emi2 in emi.MenuItems)
					{
						if(Stats.settings.theme != "Default Colors")
							emi2.SwitchOwner();
						else if(Stats.settings.theme == "Default Colors")
							emi2.SwitchDefault();
					}
				}
			}
			container.BackColor = Stats.settings.clFormsBack;
			foreach(Control c in container.Controls)
			{
				if(c.GetType() == typeof(Label) || c.GetType() == typeof(LinkLabel))
				{
					c.ForeColor = Stats.settings.clLabelFore;
				}
				else if(c.GetType() == typeof(Button))
				{
					c.BackColor = Stats.settings.clButtonBack;
					c.ForeColor = Stats.settings.clButtonFore;
				}
				else if(c.GetType() == typeof(TextBox) || c.GetType() == typeof(ComboBox) || c.GetType() == typeof(NumericUpDown))
				{
					c.BackColor = Stats.settings.clTextBoxBack;
					c.ForeColor = Stats.settings.clTextBoxFore;
				}
				else if(c.GetType() == typeof(CheckBox) || c.GetType() == typeof(RadioButton))
				{
					c.ForeColor = Stats.settings.clCheckBoxFore;
				}
				else if(c.GetType() == typeof(Splitter))
				{
					c.BackColor = Stats.settings.clLabelFore2;
				}
				else if(c.GetType() == typeof(GroupBox))
				{
					Control c4 = (Control)c;
					SetupTheme(c4);
					c.BackColor = Stats.settings.clGroupBoxBack;
					c.ForeColor = Stats.settings.clGroupBoxFore;
				}
				else if(c.GetType() == typeof(ListBox) || c.GetType() == typeof(TreeView) || c.GetType() == typeof(ListView))
				{
					//exception
					if(c.GetType() == typeof(ListBox))
						if(((ListBox)c).Name == "listQueries" || ((ListBox)c).Name == "listQueries2")
						{
							c.BackColor = Stats.settings.clGroupBoxBack;
							c.ForeColor = Stats.settings.clLabelFore;
							continue;
						}
					c.BackColor = Stats.settings.clListBoxBack;
					c.ForeColor = Stats.settings.clListBoxFore;
					if(c.GetType() == typeof(ListView))
					{
						ListView lv = (ListView)c;
						lv.GridLines = Stats.settings.clGridLines;
					}
				}
				else if(c.GetType() == typeof(RichTextBox))
				{
					c.BackColor = Stats.settings.clRichTextBoxBack;
				}
				else if(c.GetType() == typeof(Panel))
				{
					Control c3 = (Control)c;
					SetupTheme(c3);
				}
				else if(c.GetType() == typeof(ElTabControl))
				{
					if(Stats.settings.theme != "Default Colors")
						((ElTabControl)c).SwitchOwner(true);
					else if(Stats.settings.theme == "Default Colors")
						((ElTabControl)c).SwitchDefault(true);
					TabControl c2 = (TabControl)c;
					foreach(TabPage tp in c2.TabPages)
					{
						Control elTp = (Control)tp;
						SetupTheme(elTp);
					}
				}
			}
		}

		/// <summary>
		/// Setup a color scheme for FileScope.
		/// </summary>
		public static void SetColors(string kind)
		{
			Stats.settings.theme = kind;
			switch(kind)
			{
				case "Default Colors":
					Stats.settings.clButtonBack = Color.FromKnownColor(KnownColor.Control);
					Stats.settings.clButtonFore = Color.FromKnownColor(KnownColor.ControlText);
					Stats.settings.clChatHeader = Color.FromArgb(255,145,55);
					Stats.settings.clChatPeer = Color.FromArgb(0,90,255);
					Stats.settings.clChatYou = Color.FromArgb(255,40,40);
					Stats.settings.clCheckBoxFore = Color.FromKnownColor(KnownColor.ControlText);
					Stats.settings.clFormsBack = Color.FromKnownColor(KnownColor.Control);
					Stats.settings.clMenuBox = Color.Green;
					Stats.settings.clMenuHighlight1 = Color.Green;
					Stats.settings.clMenuHighlight2 = Color.Green;
					Stats.settings.clMenuBorder = Color.Green;
					Stats.settings.clGroupBoxBack = Color.FromKnownColor(KnownColor.Control);
					Stats.settings.clGroupBoxFore = Color.FromKnownColor(KnownColor.ControlText);
					Stats.settings.clHighlight1 = Color.FromArgb(0,0,255);
					Stats.settings.clHighlight2 = Color.FromArgb(0,255,0);
					Stats.settings.clHighlight3 = Color.FromArgb(255,155,55);
					Stats.settings.clHighlight4 = Color.FromArgb(255,0,0);
					Stats.settings.clLabelFore = Color.FromKnownColor(KnownColor.ControlText);
					Stats.settings.clLabelFore2 = Color.FromKnownColor(KnownColor.ControlText);
					Stats.settings.clListBoxBack = Color.FromKnownColor(KnownColor.Window);
					Stats.settings.clListBoxFore = Color.FromKnownColor(KnownColor.ControlText);
					Stats.settings.clRichTextBoxBack = Color.FromArgb(84,103,117);
					Stats.settings.clTextBoxBack = Color.FromKnownColor(KnownColor.Window);
					Stats.settings.clTextBoxFore = Color.FromKnownColor(KnownColor.ControlText);
					Stats.settings.clHomeBR = Color.FromArgb(80, Color.Gray);
					Stats.settings.clHomeTL = Color.Orange;
					Stats.settings.clGridLines = true;
					break;
				case "Blueish":
					Stats.settings.clButtonBack = Color.LightBlue;
					Stats.settings.clButtonFore = Color.Black;
					Stats.settings.clChatHeader = Color.Black;
					Stats.settings.clChatPeer = Color.FromArgb(0,90,255);
					Stats.settings.clChatYou = Color.FromArgb(255,40,40);
					Stats.settings.clCheckBoxFore = Color.MidnightBlue;
					Stats.settings.clFormsBack = Color.SteelBlue;
					Stats.settings.clMenuBox = Color.MidnightBlue;
					Stats.settings.clMenuHighlight1 = Color.FromArgb(40, Color.Blue);
					Stats.settings.clMenuHighlight2 = Color.CornflowerBlue;
					Stats.settings.clMenuBorder = Color.White;
					Stats.settings.clGroupBoxBack = Color.DeepSkyBlue;
					Stats.settings.clGroupBoxFore = Color.MidnightBlue;
					Stats.settings.clHighlight1 = Color.FromArgb(255,255,255);
					Stats.settings.clHighlight2 = Color.FromArgb(0,0,200);
					Stats.settings.clHighlight3 = Color.FromArgb(120,0,0);
					Stats.settings.clHighlight4 = Color.FromArgb(0,0,0);
					Stats.settings.clLabelFore = Color.MidnightBlue;
					Stats.settings.clLabelFore2 = Color.WhiteSmoke;
					Stats.settings.clListBoxBack = Color.DodgerBlue;
					Stats.settings.clListBoxFore = Color.Black;
					Stats.settings.clRichTextBoxBack = Color.LightSteelBlue;
					Stats.settings.clTextBoxBack = Color.MediumBlue;
					Stats.settings.clTextBoxFore = Color.LightSkyBlue;
					Stats.settings.clHomeBR = Color.MidnightBlue;
					Stats.settings.clHomeTL = Color.PowderBlue;
					Stats.settings.clGridLines = false;
					break;
				case "Cherry":
					Stats.settings.clButtonBack = Color.FromArgb(252,83,83);
					Stats.settings.clButtonFore = Color.FromArgb(0,0,0);
					Stats.settings.clChatHeader = Color.FromArgb(255,255,0);
					Stats.settings.clChatPeer = Color.FromArgb(0,90,255);
					Stats.settings.clChatYou = Color.FromArgb(255,40,40);
					Stats.settings.clCheckBoxFore = Color.FromArgb(25,255,0);
					Stats.settings.clFormsBack = Color.FromArgb(170,100,100);
					Stats.settings.clMenuBox = Color.FromArgb(220,104,130);
					Stats.settings.clMenuHighlight1 = Color.Pink;
					Stats.settings.clMenuHighlight2 = Color.FromArgb(220,104,130);
					Stats.settings.clMenuBorder = Color.Purple;
					Stats.settings.clGroupBoxBack = Color.FromArgb(160,52,89);
					Stats.settings.clGroupBoxFore = Color.FromArgb(255,250,0);
					Stats.settings.clHighlight1 = Color.FromArgb(0,0,255);
					Stats.settings.clHighlight2 = Color.FromArgb(0,255,0);
					Stats.settings.clHighlight3 = Color.FromArgb(255,255,0);
					Stats.settings.clHighlight4 = Color.FromArgb(0,0,0);
					Stats.settings.clLabelFore = Color.FromArgb(64,128,255);
					Stats.settings.clLabelFore2 = Color.DarkGray;
					Stats.settings.clListBoxBack = Color.FromArgb(220,104,130);
					Stats.settings.clListBoxFore = Color.FromArgb(255,255,255);
					Stats.settings.clRichTextBoxBack = Color.FromArgb(220,104,130);
					Stats.settings.clTextBoxBack = Color.FromArgb(188,255,188);
					Stats.settings.clTextBoxFore = Color.FromArgb(165,24,24);
					Stats.settings.clHomeBR = Color.FromArgb(200,130,130);
					Stats.settings.clHomeTL = Color.FromArgb(160,52,89);
					Stats.settings.clGridLines = true;
					break;
				case "Dungeon":
					Stats.settings.clButtonBack = Color.FromArgb(0,0,0);
					Stats.settings.clButtonFore = Color.FromArgb(233,220,107);
					Stats.settings.clChatHeader = Color.FromArgb(127,255,252);
					Stats.settings.clChatPeer = Color.FromArgb(40,140,255);
					Stats.settings.clChatYou = Color.FromArgb(255,80,80);
					Stats.settings.clCheckBoxFore = Color.FromArgb(120,171,255);
					Stats.settings.clFormsBack = Color.FromArgb(52,62,51);
					Stats.settings.clMenuBox = Color.FromArgb(65,89,65);
					Stats.settings.clMenuHighlight1 = Color.FromArgb(20, Color.White);
					Stats.settings.clMenuHighlight2 = Color.FromArgb(50, Color.White);
					Stats.settings.clMenuBorder = Color.White;
					Stats.settings.clGroupBoxBack = Color.FromArgb(0,11,21);
					Stats.settings.clGroupBoxFore = Color.FromArgb(248,128,0);
					Stats.settings.clHighlight1 = Color.FromArgb(0,255,72);
					Stats.settings.clHighlight2 = Color.FromArgb(0,200,56);
					Stats.settings.clHighlight3 = Color.FromArgb(0,138,39);
					Stats.settings.clHighlight4 = Color.FromArgb(0,0,16);
					Stats.settings.clLabelFore = Color.Silver;
					Stats.settings.clLabelFore2 = Color.White;
					Stats.settings.clListBoxBack = Color.FromArgb(53,89,137);
					Stats.settings.clListBoxFore = Color.FromArgb(114,253,124);
					Stats.settings.clRichTextBoxBack = Color.FromArgb(84,103,117);
					Stats.settings.clTextBoxBack = Color.FromArgb(53,89,137);
					Stats.settings.clTextBoxFore = Color.FromArgb(114,253,124);
					Stats.settings.clHomeBR = Color.FromArgb(60, Color.Yellow);
					Stats.settings.clHomeTL = Color.WhiteSmoke;
					Stats.settings.clGridLines = false;
					break;
				case "FileScope":
					Stats.settings.clButtonBack = Color.FromArgb(0,0,0);
					Stats.settings.clButtonFore = Color.Orange;
					Stats.settings.clChatHeader = Color.Green;
					Stats.settings.clChatPeer = Color.FromArgb(0,90,255);
					Stats.settings.clChatYou = Color.FromArgb(255,40,40);
					Stats.settings.clCheckBoxFore = Color.Orange;
					Stats.settings.clFormsBack = Color.FromArgb(80,80,80);
					Stats.settings.clMenuBox = Color.FromArgb(66,66,66);
					Stats.settings.clMenuHighlight1 = Color.Orange;
					Stats.settings.clMenuHighlight2 = Color.FromArgb(120, Color.Orange);
					Stats.settings.clMenuBorder = Color.White;
					Stats.settings.clGroupBoxBack = Color.FromArgb(70,70,70);
					Stats.settings.clGroupBoxFore = Color.Orange;
					Stats.settings.clHighlight1 = Color.FromArgb(250, 180, 40);
					Stats.settings.clHighlight2 = Color.FromArgb(220, 120, 40);
					Stats.settings.clHighlight3 = Color.FromArgb(160, 80, 40);
					Stats.settings.clHighlight4 = Color.FromArgb(120, 40, 40);
					Stats.settings.clLabelFore = Color.Black;
					Stats.settings.clLabelFore2 = Color.LightGreen;
					Stats.settings.clListBoxBack = Color.FromArgb(0,0,0);
					Stats.settings.clListBoxFore = Color.Gray;
					Stats.settings.clRichTextBoxBack = Color.FromArgb(30,30,30);
					Stats.settings.clTextBoxBack = Color.FromArgb(0,0,0);
					Stats.settings.clTextBoxFore = Color.Orange;
					Stats.settings.clHomeBR = Color.FromArgb(120, Color.Orange);
					Stats.settings.clHomeTL = Color.Black;
					Stats.settings.clGridLines = false;
					break;
				case "Forest":
					Stats.settings.clButtonBack = Color.FromArgb(37,147,60);
					Stats.settings.clButtonFore = Color.FromArgb(0,0,0);
					Stats.settings.clChatHeader = Color.FromArgb(255,145,55);
					Stats.settings.clChatPeer = Color.FromArgb(0,90,255);
					Stats.settings.clChatYou = Color.FromArgb(255,40,40);
					Stats.settings.clCheckBoxFore = Color.FromArgb(0,0,0);
					Stats.settings.clFormsBack = Color.FromArgb(37,147,60);
					Stats.settings.clMenuBox = Color.FromArgb(57,167,80);
					Stats.settings.clMenuHighlight1 = Color.ForestGreen;
					Stats.settings.clMenuHighlight2 = Color.DarkGreen;
					Stats.settings.clMenuBorder = Color.White;
					Stats.settings.clGroupBoxBack = Color.FromArgb(37,123,60);
					Stats.settings.clGroupBoxFore = Color.FromArgb(0,0,0);
					Stats.settings.clHighlight1 = Color.FromArgb(0,0,255);
					Stats.settings.clHighlight2 = Color.FromArgb(0,255,0);
					Stats.settings.clHighlight3 = Color.FromArgb(255,155,55);
					Stats.settings.clHighlight4 = Color.FromArgb(255,0,0);
					Stats.settings.clLabelFore = Color.FromArgb(255,255,23);
					Stats.settings.clLabelFore2 = Color.LightBlue;
					Stats.settings.clListBoxBack = Color.FromArgb(85,54,0);
					Stats.settings.clListBoxFore = Color.FromArgb(127,101,94);
					Stats.settings.clRichTextBoxBack = Color.FromArgb(37,189,60);
					Stats.settings.clTextBoxBack = Color.FromArgb(85,54,0);
					Stats.settings.clTextBoxFore = Color.FromArgb(227,201,14);
					Stats.settings.clHomeBR = Color.Black;
					Stats.settings.clHomeTL = Color.Black;
					Stats.settings.clGridLines = false;
					break;
				case "Pleasant":
					Stats.settings.clButtonBack = Color.FromArgb(45,205,30);
					Stats.settings.clButtonFore = Color.FromArgb(0,0,0);
					Stats.settings.clChatHeader = Color.FromArgb(255,145,55);
					Stats.settings.clChatPeer = Color.FromArgb(0,90,255);
					Stats.settings.clChatYou = Color.FromArgb(255,40,40);
					Stats.settings.clCheckBoxFore = Color.FromArgb(0,0,0);
					Stats.settings.clFormsBack = Color.FromArgb(110,110,110);
					Stats.settings.clMenuBox = Color.FromArgb(140,140,140);
					Stats.settings.clMenuHighlight1 = Color.Yellow;
					Stats.settings.clMenuHighlight2 = Color.Orange;
					Stats.settings.clMenuBorder = Color.Green;
					Stats.settings.clGroupBoxBack = Color.FromArgb(100,100,100);
					Stats.settings.clGroupBoxFore = Color.FromArgb(255,250,0);
					Stats.settings.clHighlight1 = Color.FromArgb(0,0,255);
					Stats.settings.clHighlight2 = Color.FromArgb(0,255,0);
					Stats.settings.clHighlight3 = Color.FromArgb(255,155,55);
					Stats.settings.clHighlight4 = Color.FromArgb(255,0,0);
					Stats.settings.clLabelFore = Color.FromArgb(160,200,240);
					Stats.settings.clLabelFore2 = Color.Black;
					Stats.settings.clListBoxBack = Color.FromArgb(100,195,250);
					Stats.settings.clListBoxFore = Color.FromArgb(10,40,10);
					Stats.settings.clRichTextBoxBack = Color.FromArgb(84,103,117);
					Stats.settings.clTextBoxBack = Color.FromArgb(45,205,30);
					Stats.settings.clTextBoxFore = Color.FromArgb(10,40,10);
					Stats.settings.clHomeBR = Color.FromArgb(140,100,195,250);
					Stats.settings.clHomeTL = Color.FromArgb(100,195,250);
					Stats.settings.clGridLines = false;
					break;
				case "Starburst":
					Stats.settings.clFormsBack = Color.FromArgb(84,131,82);
					Stats.settings.clMenuBox = Color.FromArgb(104,145,117);
					Stats.settings.clMenuHighlight1 = Color.FromArgb(120, Color.Yellow);
					Stats.settings.clMenuHighlight2 = Color.CadetBlue;
					Stats.settings.clMenuBorder = Color.Orange;
					Stats.settings.clListBoxBack = Color.FromArgb(84,131,193);
					Stats.settings.clListBoxFore = Color.WhiteSmoke;
					Stats.settings.clGroupBoxBack = Color.FromArgb(64,111,62);
					Stats.settings.clGroupBoxFore = Color.Yellow;
					Stats.settings.clTextBoxBack = Color.FromArgb(84,131,193);
					Stats.settings.clTextBoxFore = Color.Black;
					Stats.settings.clButtonBack = Color.FromArgb(44,140,207);
					Stats.settings.clButtonFore = Color.FromArgb(250,220,0);
					Stats.settings.clLabelFore = Color.WhiteSmoke;
					Stats.settings.clLabelFore2 = Color.Black;
					Stats.settings.clCheckBoxFore = Color.WhiteSmoke;
					Stats.settings.clRichTextBoxBack = Color.FromArgb(94,90,94);
					Stats.settings.clChatHeader = Color.FromArgb(255,145,55);
					Stats.settings.clChatPeer = Color.FromArgb(0,90,255);
					Stats.settings.clChatYou = Color.FromArgb(255,40,40);
					Stats.settings.clHighlight1 = Color.FromArgb(255,255,180);
					Stats.settings.clHighlight2 = Color.FromArgb(255,255,0);
					Stats.settings.clHighlight3 = Color.FromArgb(200,200,0);
					Stats.settings.clHighlight4 = Color.FromArgb(50,50,0);
					Stats.settings.clHomeBR = Color.FromArgb(84,131,193);
					Stats.settings.clHomeTL = Color.Yellow;
					Stats.settings.clGridLines = true;
					break;
				case "UniGray":
					Stats.settings.clFormsBack = Color.FromArgb(120,120,120);
					Stats.settings.clMenuBox = Color.FromArgb(130,150,150);
					Stats.settings.clMenuHighlight1 = Color.Silver;
					Stats.settings.clMenuHighlight2 = Color.DarkGray;
					Stats.settings.clMenuBorder = Color.WhiteSmoke;
					Stats.settings.clListBoxBack = Color.FromArgb(85,85,85);
					Stats.settings.clListBoxFore = Color.FromArgb(205,205,205);
					Stats.settings.clGroupBoxBack = Color.FromArgb(85,85,85);
					Stats.settings.clGroupBoxFore = Color.FromArgb(205,205,205);
					Stats.settings.clTextBoxBack = Color.FromArgb(85,85,85);
					Stats.settings.clTextBoxFore = Color.FromArgb(205,205,205);
					Stats.settings.clButtonBack = Color.FromArgb(85,85,85);
					Stats.settings.clButtonFore = Color.FromArgb(205,205,205);
					Stats.settings.clLabelFore = Color.FromArgb(205,205,205);
					Stats.settings.clLabelFore2 = Color.FromArgb(185,185,185);
					Stats.settings.clCheckBoxFore = Color.FromArgb(205,205,205);
					Stats.settings.clRichTextBoxBack = Color.FromArgb(85,85,85);
					Stats.settings.clChatHeader = Color.FromArgb(255,145,55);
					Stats.settings.clChatPeer = Color.FromArgb(0,90,255);
					Stats.settings.clChatYou = Color.FromArgb(255,40,40);
					Stats.settings.clHighlight1 = Color.White;
					Stats.settings.clHighlight2 = Color.LightGray;
					Stats.settings.clHighlight3 = Color.FromArgb(97,149,88);
					Stats.settings.clHighlight4 = Color.FromArgb(0,0,0);
					Stats.settings.clHomeBR = Color.WhiteSmoke;
					Stats.settings.clHomeTL = Color.WhiteSmoke;
					Stats.settings.clGridLines = true;
					break;
				case "WinterFresh":
					Stats.settings.clButtonBack = Color.LightGreen;
					Stats.settings.clButtonFore = Color.Black;
					Stats.settings.clChatHeader = Color.Green;
					Stats.settings.clChatPeer = Color.FromArgb(0,90,255);
					Stats.settings.clChatYou = Color.FromArgb(255,40,40);
					Stats.settings.clCheckBoxFore = Color.FromArgb(3,22,67);
					Stats.settings.clFormsBack = Color.WhiteSmoke;
					Stats.settings.clMenuBox = Color.ForestGreen;
					Stats.settings.clMenuHighlight1 = Color.LightGreen;
					Stats.settings.clMenuHighlight2 = Color.DarkGreen;
					Stats.settings.clMenuBorder = Color.White;
					Stats.settings.clGroupBoxBack = Color.LightGray;
					Stats.settings.clGroupBoxFore = Color.FromArgb(3,22,67);
					Stats.settings.clHighlight1 = Color.FromArgb(0,250,0);
					Stats.settings.clHighlight2 = Color.FromArgb(0,180,0);
					Stats.settings.clHighlight3 = Color.FromArgb(0,110,0);
					Stats.settings.clHighlight4 = Color.FromArgb(0,40,0);
					Stats.settings.clLabelFore = Color.FromArgb(3,22,67);
					Stats.settings.clLabelFore2 = Color.Black;
					Stats.settings.clListBoxBack = Color.White;
					Stats.settings.clListBoxFore = Color.DarkGreen;
					Stats.settings.clRichTextBoxBack = Color.White;
					Stats.settings.clTextBoxBack = Color.White;
					Stats.settings.clTextBoxFore = Color.Green;
					Stats.settings.clHomeBR = Color.Green;
					Stats.settings.clHomeTL = Color.Green;
					Stats.settings.clGridLines = true;
					break;
			}
		}
	}
}
