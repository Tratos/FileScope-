// Keywords.cs
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

namespace FileScope
{
	public class G2Word
	{
		public bool negative;
		public string word;
		public string[] subwords = null;
		public G2Word next = null;
	}

	/// <summary>
	/// This class is used for determining keywords for shared files.
	/// Each file has associated keyword(s).
	/// This class will also check if a query matches a file.
	/// </summary>
	public class Keywords
	{
		//a bunch of delimeters to split up filenames into keywords
		public static char[] delimeters = new char[]{' ', '-', '+', '.', ',', '_', '&'};

		/// <summary>
		/// Split up a filename into several keywords.
		/// </summary>
		public static string[] GetKeywords(ref string query)
		{
			return query.Split(delimeters);
		}

		public static string[] GetKeywords(string query)
		{
			return query.Split(delimeters);
		}

		/// <summary>
		/// Split up a file name into several lower case keywords; complying with G2 query language definition.
		/// </summary>
		public static G2Word GetG2Keywords(ref string query)
		{
			//we need at least one keyword
			G2Word first = null;
			G2Word last = null;

			try
			{
				int current = 0;
				int start = 0;
				bool started = false;
				bool negative = false;
				bool quoted = false;
				for(;;)
				{
					if(started)
					{
						if(current >= query.Length)
						{
							started = false;
							goto wordend;
						}
						else if((quoted && query[current] == '\"') || (!quoted && query[current] == ' '))
						{
							goto wordend;
						}
						current++;
						continue;
					wordend:
						if(first == null)
						{
							first = new G2Word();
							first.negative = negative;
							first.word = query.Substring(start, current-start).ToLower();;
							last = first;
						}
						else
						{
							last.next = new G2Word();
							last = last.next;
							last.negative = negative;
							last.word = query.Substring(start, current-start).ToLower();
						}
						if(!started)
							break;
						started = false;
						negative = false;
						quoted = false;
					}
					else
					{
						if(current >= query.Length)
							break;
						if(query[current] == '-' && current + 1 < query.Length)
						{
							if(query[current+1] != ' ')
							{
								negative = true;
								started = true;
								current++;
								if(query[current] == '\"')
								{
									quoted = true;
									start = current + 1;
								}
								else
									start = current;
							}
						}
						else if(query[current] == '\"')
						{
							quoted = true;
							start = current + 1;
							started = true;
						}
						else if(query[current] != ' ')
						{
							start = current;
							started = true;
						}
					}
					current++;
				}
				return first;
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine("GetG2Keywords: " + e.Message);
				return null;
			}
		}

		/// <summary>
		/// Our style of getting prefixes from all keywords.
		/// </summary>
		public static string[] GetPrefixes(string[] words)
		{
			ArrayList l = new ArrayList();
			for(int i = 0; i < words.Length; i++)
			{
				//add the keyword itself
				l.Add(words[i]);
				int len = words[i].Length;
				if(len > 4 && len < 8)
					l.Add(words[i].Substring(0, len-2));
			}
			//we are finished... simply convert from ArrayList to string
			return (string[])l.ToArray(typeof(string));
		}
	}
}
