using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Threading
{
	class DispatcherTimer
	{
		public TimeSpan Interval { get; set; }
		private bool IsEnabled { get; set; } = true;
		public async void Start()
		{
			while (true)
			{
				await Task.Delay(Interval);
				if (IsEnabled)
					Tick?.Invoke(this, EventArgs.Empty);
			}
		}
		public void Stop() => IsEnabled = false;

		public event EventHandler Tick;
	}
}
