using System;
using System.Linq;
using System.Windows.Threading;
using Ace;
using Rainbow;
using Solfeggio.Api;
using Solfeggio.ViewModels;

namespace Solfeggio.Processors
{
	class GenerationProcessor : IProcessor
	{
		public int BufferMilliseconds
		{
			get => (int)_timer.Interval.TotalMilliseconds;
			set => _timer.Interval = TimeSpan.FromMilliseconds(value);
		}

		public event EventHandler<ProcessingEventArgs> DataAvailable;

		private DispatcherTimer _timer = new DispatcherTimer();
		private HarmonicManager _manager = Store.Get<HarmonicManager>();
		private Generator _generator;
		public GenerationProcessor(Generator generator)
		{
			_generator = generator;
			_timer.Interval = TimeSpan.FromSeconds(0.5 * generator.SampleSize / generator.SampleRate);
			_timer.Tick += OnTimerTick;
			_bins = Next();
		}

		public void Tick()
		{
			DataAvailable?.Invoke(this, new ProcessingEventArgs(_bins, _bins.Length));
			_bins = Next();
		}

		public short[] Next()
		{
			var signal = _manager.ActiveProfile.GenerateSignalSample(_generator.SampleSize, _generator.SampleRate, false);
			var bins = signal.Select(d => (short)(d * short.MaxValue / 2d)).ToArray();
			return bins;
		}

		short[] _bins;

		public bool IsTimerEnabled { get; set; }

		private void OnTimerTick(object sender, EventArgs e)
		{
			if (IsTimerEnabled) Tick();
		}

		public void Free() => _timer.Stop();

		public void Lull() => _timer.Stop();

		public void Wake() => _timer.Start();
	}

	class Generator : SignalProcessor
	{
		public int BuffersCount { get; set; } = 4;

		protected override IProcessor CreateInputProcessor() =>
			new GenerationProcessor(this);

		protected override IProcessor CreateOutputProcessor() =>
			new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), SampleSize, BuffersCount, inputProcessor);

		public override string ToString() => "Ideal";
	}

	class Microphone : SignalProcessor
	{
		public int BuffersCount { get; set; } = 4;

		protected override IProcessor CreateInputProcessor() =>
			new Wave.In.Processor(Wave.In.DefaultDevice.CreateSession(WaveFormat), SampleSize, BuffersCount);

		protected override IProcessor CreateOutputProcessor() =>
			new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), SampleSize, BuffersCount, inputProcessor);

		public override string ToString() => "Real";
	}
}