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
			_timer.Interval = TimeSpan.FromSeconds(generator.SampleSize / generator.SampleRate);
			_timer.Tick += OnTimerTick;
			_generator = generator;
		}

		public void OnTimerTick(object sender, EventArgs e)
		{
			var signal = _manager.ActiveProfile.GenerateSignalSample(_generator.SampleSize, _generator.SampleRate, false);
			var bins = signal.Select(d => (short)(d * short.MaxValue / 2d)).ToArray();
			DataAvailable?.Invoke(this, new ProcessingEventArgs(bins, bins.Length));
		}

		public void Free() => _timer.Stop();

		public void Lull() => _timer.Stop();

		public void Wake() => _timer.Start();
	}

	class Generator : SignalProcessor, IDataSource<short>
	{
		protected override IProcessor CreateInputProcessor() =>
			new GenerationProcessor(this);

		protected override IProcessor CreateOutputProcessor() =>
			new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), SampleSize, this);
	}

	class Microphone : SignalProcessor, IAudioInputDevice, IDataSource<short>
	{
		protected override IProcessor CreateInputProcessor() =>
			new Wave.In.Processor(Wave.In.DefaultDevice.CreateSession(WaveFormat), SampleSize);

		protected override IProcessor CreateOutputProcessor() =>
			new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), SampleSize, this);
	}
}