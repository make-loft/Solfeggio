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

		private DateTime _timestamp = DateTime.Now;

		public void Tick()
		{
			_timestamp = DateTime.Now;
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

		private void OnTimerTick(object sender, EventArgs e)
		{
			if (DateTime.Now - _timestamp > _timer.Interval) Tick();
		}

		public void Free() => _timer.Stop();

		public void Lull() => _timer.Stop();

		public void Wake() => _timer.Start();
	}

	class Generator : SignalProcessor
	{
		protected override IProcessor CreateInputProcessor() =>
			new GenerationProcessor(this);

		protected override IProcessor CreateOutputProcessor() =>
			new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), SampleSize, inputProcessor);
	}

	class Microphone : SignalProcessor, IAudioInputDevice
	{
		protected override IProcessor CreateInputProcessor() =>
			new Wave.In.Processor(Wave.In.DefaultDevice.CreateSession(WaveFormat), SampleSize);

		protected override IProcessor CreateOutputProcessor() =>
			new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), SampleSize, inputProcessor);
	}
}