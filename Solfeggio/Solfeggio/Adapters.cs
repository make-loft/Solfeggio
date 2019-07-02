using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows.Threading
{
	class DispatcherTimer
	{
		public TimeSpan Interval { get; set; }

		public void Start() { }
		public void Stop() { }
		public event EventHandler Tick;
	}
}
