// ElTabControl.cs
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
	/// Our special owner drawn tabcontrol compliant with the current theme.
	/// </summary>
	public class ElTabControl : TabControl
	{
		SolidBrush bgBrush;
		HatchBrush hb;
		HatchBrush hb2;
		SolidBrush strBrush;
		SolidBrush highlightBrush;
		Font elFont;
		Pen elPen;
		Pen borderPen;
		int activeTab = -1;

		public ElTabControl()
		{
			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		}

		/// <summary>
		/// This can be called when themes change.
		/// </summary>
		public void ResetBrushes()
		{
			if(bgBrush != null) bgBrush.Dispose();
			bgBrush = new SolidBrush(Stats.settings.clFormsBack);
			if(hb != null) hb.Dispose();
			hb = new HatchBrush(HatchStyle.ZigZag, Stats.settings.clLabelFore2, Color.Transparent);
			if(hb2 != null) hb2.Dispose();
			hb2 = new HatchBrush(HatchStyle.SolidDiamond, Color.FromArgb(20, Color.White), Color.Transparent);
			if(strBrush != null) strBrush.Dispose();
			strBrush = new SolidBrush(Stats.settings.clLabelFore);
			if(highlightBrush != null) highlightBrush.Dispose();
			highlightBrush = new SolidBrush(Stats.settings.clMenuHighlight1);
			if(elFont != null) elFont.Dispose();
			elFont = new Font("Arial", 10F, FontStyle.Regular);
			this.Font = new Font("Arial", 10F, FontStyle.Regular);
			if(this.ImageList != null)
				if(this.ImageList.ImageSize.Width > 16)
				{
					elFont.Dispose();
					elFont = new Font("Arial", 12F, FontStyle.Regular);
					this.Font = new Font("Arial", 12F, FontStyle.Regular);
				}
			if(elPen != null) elPen.Dispose();
			elPen = new Pen(hb, 6);
			if(borderPen != null) borderPen.Dispose();
			borderPen = new Pen(Stats.settings.clMenuBorder, 1);
		}

		/// <summary>
		/// This function is called after the control is initialized and loaded.
		/// Apparently the GetTabRect() function works better if we run this after the control loads.
		/// </summary>
		public void SwitchOwner(bool refresh)
		{
			ResetBrushes();
			this.SetStyle(ControlStyles.UserPaint, true);
			if(refresh)
				this.Refresh();
		}

		public void SwitchDefault(bool refresh)
		{
			ResetBrushes();
			this.SetStyle(ControlStyles.UserPaint, false);
			if(refresh)
				this.Refresh();
		}

		protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
		{
			e.Graphics.FillRectangle(bgBrush, e.ClipRectangle);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			for(int x = 0; x < this.TabCount; x++)
				if(e.ClipRectangle.Contains(this.GetTabRect(x).Left, this.GetTabRect(x).Top))
					PaintItem(this.GetTabRect(x).Left, e, x);
				else if(e.ClipRectangle.Contains(this.GetTabRect(x).Right, this.GetTabRect(x).Top))
					PaintItem(this.GetTabRect(x).Left, e, x);
				else if(e.ClipRectangle.Contains(this.GetTabRect(x).Left, this.GetTabRect(x).Bottom))
					PaintItem(this.GetTabRect(x).Left, e, x);
				else if(e.ClipRectangle.Contains(this.GetTabRect(x).Right, this.GetTabRect(x).Bottom))
					PaintItem(this.GetTabRect(x).Left, e, x);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if(!this.GetStyle(ControlStyles.UserPaint))
				return;
			for(int x = 0; x < this.TabCount; x++)
				if(e.X < this.GetTabRect(x).Right)
				{
					if(this.activeTab != x)
						this.activeTab = x;
					break;
				}
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			if(!this.GetStyle(ControlStyles.UserPaint))
				return;
			this.activeTab = -1;
		}

		void PaintItem(int x, PaintEventArgs e, int index)
		{
			int magic1 = 8;
			int magic2 = 6;
			int underlineMagic1 = 0;
			int underlineMagic2 = 8;
			int topmagic = 2;
			if(this.ImageList != null)
			{
				//we'll need space for the icon
				magic1 += 16;
				magic2 += 16;
				if(this.ImageList.ImageSize.Width > 16)
				{
					magic2 += 14;
					topmagic += 4;
					underlineMagic1 = 14;
					underlineMagic2 -= 5;
				}
			}
			//draw highlighted state
			if(index == activeTab)
			{
				e.Graphics.FillRectangle(highlightBrush, GetTabRect(index));
				e.Graphics.DrawRectangle(borderPen, GetTabRect(index));
			}
			//draw selected state
			if(this.SelectedIndex == index)
				e.Graphics.FillRectangle(hb2, GetTabRect(index));
			//text
			if(index == activeTab)
				strBrush = new SolidBrush(Stats.settings.clLabelFore);
			else
				strBrush = new SolidBrush(Color.FromArgb(220, Stats.settings.clLabelFore));
			e.Graphics.DrawString(this.TabPages[index].Text, elFont, strBrush, x + magic2, topmagic);
			//underline
			e.Graphics.DrawLine(elPen, new Point(GetTabRect(index).Left + magic1 + underlineMagic1, GetTabRect(index).Bottom - 3), new Point(GetTabRect(index).Right - underlineMagic2, GetTabRect(index).Bottom - 3));
			//icon
			if(this.ImageList != null)
				e.Graphics.DrawImage(this.ImageList.Images[this.TabPages[index].ImageIndex], GetTabRect(index).Left + 4, 4);
			//borderline if necessary
			if(this.Appearance != TabAppearance.Normal)
				e.Graphics.DrawLine(borderPen, new Point(GetTabRect(index).Right + 5, 2), new Point(GetTabRect(index).Right + 5, GetTabRect(index).Bottom - 2));
			strBrush.Dispose();
		}
	}
}
