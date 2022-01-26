// GoodTimer.cs
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
using System.Timers;

namespace FileScope
{
	/// <summary>
	/// This timer just assures that multiple delegates aren't passed to the Elapsed event.
	/// </summary>
	public class GoodTimer : Timer
	{
		public bool hasEvent = false;

		public GoodTimer() : base()
		{
			hasEvent = false;
		}

		public GoodTimer(double interval) : base(interval)
		{
			hasEvent = false;
		}

		public void AddEvent(ElapsedEventHandler eeh)
		{
			if(!hasEvent)
			{
				this.Elapsed += eeh;
				hasEvent = true;
			}
		}

		public void RemoveEvent(ElapsedEventHandler eeh)
		{
			if(hasEvent)
			{
				this.Elapsed -= eeh;
				hasEvent = false;
			}
		}
	}
}
