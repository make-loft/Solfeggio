using Ace;
using Solfeggio.Api;
using Solfeggio.ViewModels;
using System;
using System.Linq;
using System.Windows.Threading;

namespace Solfeggio.Processors
{
	public class SoftwareGenerator : Wave.In.DeviceInfo
	{
		public SoftwareGenerator() : base(int.MinValue) { }

		public override string ProductName => "Software Signal Generator";

		public override WaveInCapabilities GetCapabilities() => throw new NotImplementedException();

		public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
			new SoftwareProcessor(waveFormat.SampleRate, sampleSize, buffersCount);
	}

	class SoftwareProcessor : IProcessor
	{
		public int BufferMilliseconds
		{
			get => (int)_timer.Interval.TotalMilliseconds;
			set => _timer.Interval = TimeSpan.FromMilliseconds(value);
		}

		public double Level { get; set; } = 1d;
		public double Boost { get; set; } = 1d;

		public event EventHandler<ProcessingEventArgs> DataAvailable;

		private readonly DispatcherTimer _timer = new();
		private readonly HarmonicManager _manager = Store.Get<HarmonicManager>();
		private readonly int _sampleRate;
		private readonly int _sampleSize;

		public SoftwareProcessor(int sampleRate, int sampleSize, int buffersCount)
		{
			_sampleRate = sampleRate;
			_sampleSize = sampleSize;
			_timer.Interval = TimeSpan.FromSeconds(0.5 * sampleSize / sampleRate);
			_timer.Tick += OnTimerTick;
			_bins = Next();
		}

		public void Tick()
		{
			DataAvailable?.Invoke(this, new ProcessingEventArgs(this, _bins, _bins.Length));
			_bins = Next();
		}

		public short[] Next()
		{
			var signal = _manager.ActiveProfile.GenerateSignalSample(_sampleSize, _sampleRate, false);
			var bins = signal.Stretch(Level).Select(d => (short)(d * short.MaxValue / 2d)).ToArray();
			return bins.Scale(Boost);
		}

		short[] _bins;

		public bool IsTimerEnabled { get; set; }
#if NETSTANDARD
		= true;
#endif

		private void OnTimerTick(object sender, EventArgs e)
		{
			if (IsTimerEnabled) Tick();
		}

		public void Free() => _timer.Stop();
		public void Lull() => _timer.Stop();
		public void Wake() => _timer.Start();
	}
}
