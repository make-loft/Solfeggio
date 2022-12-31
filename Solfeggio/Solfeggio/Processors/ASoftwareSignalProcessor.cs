
using Solfeggio.Api;

using System;
using System.Windows.Threading;

namespace Solfeggio.Processors
{
	abstract class ASoftwareSignalProcessor : IProcessor
	{
		public IProcessor Source { get; set; }
		public int BufferMilliseconds
		{
			get => (int)_timer.Interval.TotalMilliseconds;
			set => _timer.Interval = TimeSpan.FromMilliseconds(value);
		}

		public double Level { get; set; } = 1d;
		public double Boost { get; set; } = 1d;

		public event EventHandler<ProcessingEventArgs> DataAvailable;

		private readonly DispatcherTimer _timer = new();
		protected readonly int _sampleRate;
		protected readonly int _sampleSize;

		public ASoftwareSignalProcessor(int sampleRate, int sampleSize, int buffersCount)
		{
			_sampleRate = sampleRate;
			_sampleSize = sampleSize;
			_timer.Interval = TimeSpan.FromSeconds((double)sampleSize / sampleRate);
			_timer.Tick += OnTimerTick;
			_bins = new short[0];
		}

		DateTime _timestamp = DateTime.Now;

		public void Tick()
		{
			DataAvailable?.Invoke(this, new(this, _bins));
			_bins = Next() ?? new short[0];

			_timestamp = DateTime.Now;
		}

		public abstract short[] Next();

		short[] _bins;

		public bool IsAutoTickEnabledOnce { get; set; }
		public bool IsAutoTickEnabled { get; set; }
#if NETSTANDARD
		= false;
#endif

		private void OnTimerTick(object sender, EventArgs e)
		{
			var now = DateTime.Now;
			if ((now - _timestamp).TotalMilliseconds > 4 * _timer.Interval.TotalMilliseconds)
				IsAutoTickEnabledOnce = true;

			if (IsAutoTickEnabled || IsAutoTickEnabledOnce) Tick();
			IsAutoTickEnabledOnce = false;
		}

		public void Free() => _timer.Stop();
		public void Lull() => _timer.Stop();
		public void Wake() => _timer.Start();
	}
}
