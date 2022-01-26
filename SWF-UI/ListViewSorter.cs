// ListViewSorter.cs
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
using System.Windows.Forms;

namespace FileScope
{
	/// <summary>
	/// Used for sorting our listview controls by column clicks.
	/// </summary>
	public class ListViewSorter : IComparer
	{
		//switches from descending to ascending
		public static int[] columns = new int[12]{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1};

		int column;
		string col;

		public ListViewSorter(int column, string columnName)
		{
			this.column = column;
			this.col = columnName;
			//flip from one mode to another
			columns[column] *= -1;
		}

		public int Compare(object x, object y)
		{
			ListViewItem xItem = (ListViewItem)x;
			ListViewItem yItem = (ListViewItem)y;

			//figure out what we're sorting
			if(col == "#")
			{
				int xVal, yVal;
				if(xItem.SubItems[column].Text.IndexOf("*") == -1)
					xVal = Convert.ToInt32(xItem.SubItems[column].Text);
				else
					xVal = Convert.ToInt32(xItem.SubItems[column].Text.Replace("*", ""));
				if(yItem.SubItems[column].Text.IndexOf("*") == -1)
					yVal = Convert.ToInt32(yItem.SubItems[column].Text);
				else
					yVal = Convert.ToInt32(yItem.SubItems[column].Text.Replace("*", ""));
				int res = xVal.CompareTo(yVal);
				return (res * columns[column]);
			}
			else if(col == "filesize")
			{
				uint int1 = Utils.Strip(xItem.SubItems[column].Text, " KB");
				uint int2 = Utils.Strip(yItem.SubItems[column].Text, " KB");
				int res = int1.CompareTo(int2);
				return (res * columns[column]);
			}
			else if(col == "speed")
			{
				uint int1 = Utils.Strip(xItem.SubItems[column].Text, " KB/s");
				uint int2 = Utils.Strip(yItem.SubItems[column].Text, " KB/s");
				int res = int1.CompareTo(int2);
				return (res * columns[column]);
			}
			else if(col == "c#" || col == "gigabytes" || col == "files" || col == "users")
			{
				uint int1 = (xItem.SubItems[column].Text == "?" ? 0 : Convert.ToUInt32(xItem.SubItems[column].Text));
				uint int2 = (yItem.SubItems[column].Text == "?" ? 0 : Convert.ToUInt32(yItem.SubItems[column].Text));
				int res = int1.CompareTo(int2);
				return (res * columns[column]);
			}
			else if(col == "users/max")
			{
				string part1 = xItem.SubItems[column].Text.Substring(0, xItem.SubItems[column].Text.IndexOf(" "));
				string part2 = yItem.SubItems[column].Text.Substring(0, yItem.SubItems[column].Text.IndexOf(" "));
				uint int1 = (part1 == "?" ? 0 : Convert.ToUInt32(part1));
				uint int2 = (part2 == "?" ? 0 : Convert.ToUInt32(part2));
				int res = int1.CompareTo(int2);
				return (res * columns[column]);
			}
			else
			{
				int res = xItem.SubItems[column].Text.CompareTo(yItem.SubItems[column].Text);
				return (res * columns[column]);				
			}
		}
	}
}
