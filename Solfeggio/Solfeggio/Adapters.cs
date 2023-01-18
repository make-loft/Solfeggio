using System.Threading.Tasks;

namespace System.Windows.Threading
{
	class DispatcherTimer
	{
		public event EventHandler Tick;
		public TimeSpan Interval { get; set; }
		private bool IsEnabled { get; set; } = true;
		public void Stop() => IsEnabled = false;
		public async void Start()
		{
			while (true)
			{
				await Task.Delay(Interval);
				if (IsEnabled)
					Tick?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}
