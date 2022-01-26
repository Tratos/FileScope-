// ElMenuItem.cs
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
using System.Drawing.Drawing2D;

namespace FileScope
{
	/// <summary>
	/// Our owner drawn menu compliant with FileScope themes.
	/// </summary>
	public class ElMenuItem : MenuItem
	{
		Font elFont = SystemInformation.MenuFont;
		SolidBrush hovBrush;
		SolidBrush selBrush;
		Pen borderPen;
		SolidBrush boxBrush;
		SolidBrush backBrush;
		HatchBrush hb;
		Pen sepPen;
		Brush textBrush;

		public void SwitchOwner()
		{
			SetupBrushes();
			this.OwnerDraw = true;
		}

		public void SwitchDefault()
		{
			this.OwnerDraw = false;
		}

		void SetupBrushes()
		{
			if(hovBrush != null) hovBrush.Dispose();
			hovBrush = new SolidBrush(Stats.settings.clMenuHighlight1);
			if(selBrush != null) selBrush.Dispose();
			selBrush = new SolidBrush(Stats.settings.clMenuHighlight2);
			if(borderPen != null) borderPen.Dispose();
			borderPen = new Pen(Stats.settings.clMenuBorder);
			if(boxBrush != null) boxBrush.Dispose();
			boxBrush = new SolidBrush(Stats.settings.clMenuBox);
			if(backBrush != null) backBrush.Dispose();
			backBrush = new SolidBrush(Stats.settings.clFormsBack);
			if(hb != null) hb.Dispose();
			hb = new HatchBrush(HatchStyle.DarkUpwardDiagonal, SystemColors.ControlDark, Color.Transparent);
			if(sepPen != null) sepPen.Dispose();
			sepPen = new Pen(hb, 4);
			if(textBrush != null) textBrush.Dispose();
			textBrush = new SolidBrush(SystemColors.ControlText);
		}

		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			if(Text == "-")
			{
				e.ItemHeight = 8;
				return;
			}
			bool topLevel = (this.Parent == this.GetMainMenu());
			int textwidth = (int)(e.Graphics.MeasureString(this.Text, this.elFont).Width);
			e.ItemHeight = (int)(e.Graphics.MeasureString(this.Text, this.elFont).Height) + 8;
			if(topLevel)
				e.ItemWidth = textwidth; 
			else
				e.ItemWidth = textwidth + 40;
			e.Graphics.Dispose();
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			bool selected = (e.State & DrawItemState.Selected) > 0;
			bool toplevel = (this.Parent == this.GetMainMenu());

			//draw background
			if((((e.State & DrawItemState.Selected) > 0) || ((e.State & DrawItemState.HotLight) > 0)) && this.Enabled)
			{
				if(toplevel && ((e.State & DrawItemState.Selected) > 0))
				{
					e.Graphics.FillRectangle(selBrush, e.Bounds);
					ControlPaint.DrawBorder3D(e.Graphics, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height, Border3DStyle.SunkenOuter, Border3DSide.Top | Border3DSide.Left | Border3DSide.Right);
				}
				else
				{
					e.Graphics.FillRectangle(hovBrush, e.Bounds);
					e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
				}
			}
			else
			{
				if(toplevel)
				{
					e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
				} 
				else
				{
					e.Graphics.FillRectangle(boxBrush, e.Bounds);
					e.Graphics.FillRectangle(backBrush, e.Bounds.X+25, e.Bounds.Y, e.Bounds.Width-25, e.Bounds.Height);
				}
			}

			//draw check mark if applicable
			if(Checked)
			{
				int checkTop = e.Bounds.Top + (e.Bounds.Height - 16)/2;
				int checkLeft = e.Bounds.Left + ( 25 - 16)/2;
				ControlPaint.DrawMenuGlyph(e.Graphics, new Rectangle(checkLeft, checkTop, 16, 16), MenuGlyph.Checkmark);
				e.Graphics.DrawRectangle(borderPen, checkLeft-1, checkTop-1, 16+1, 16+1);
			}

			//draw text
			if(Text == "-")
			{
				int y = e.Bounds.Y + e.Bounds.Height / 2;
				e.Graphics.DrawLine(sepPen, e.Bounds.X + 25, y, e.Bounds.X + e.Bounds.Width - 2, y);
			} 
			else
			{
				int textwidth = (int)(e.Graphics.MeasureString(this.Text, this.elFont).Width);
				int x = toplevel ? e.Bounds.Left + (e.Bounds.Width - textwidth) / 2: e.Bounds.Left + 30;
				int topGap = toplevel ? 2 : 4;
				int y = e.Bounds.Top + topGap;
				if(!this.Enabled)
				{
					textBrush.Dispose();
					textBrush = new SolidBrush(Stats.settings.clMenuBox);
					elFont = new Font(SystemInformation.MenuFont, FontStyle.Strikeout);
					e.Graphics.DrawString(this.Text, this.elFont, textBrush, x, y);
					elFont = SystemInformation.MenuFont;
					textBrush.Dispose();
					textBrush = new SolidBrush(SystemColors.ControlText);
				}
				else
					e.Graphics.DrawString(this.Text, this.elFont, textBrush, x, y);
			}
			e.Graphics.Dispose();
		}
	}
}
