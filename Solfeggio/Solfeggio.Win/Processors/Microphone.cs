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
		public GenerationProcessor(Generator waveFormat)
		{
			_timer.Tick += OnTimerTick;
			_generator = waveFormat;
		}

		private void OnTimerTick(object sender, EventArgs e)
		{
			var signal = _manager.ActiveProfile.GenerateSignalSample(_generator.SampleSize, _generator.SampleRate, false);
			_generator._signal = signal.Select(d => (short)(d * short.MaxValue / 2d)).ToArray();
			_generator.EvokeDataReady(signal.Select(d => (Complex)d).ToArray());
		}

		public void Free()
		{
			//_timer.Reset();
		}

		public void Lull() => _timer.Stop();

		public void Wake() => _timer.Start();
	}

	class Generator : SignalProcessor, IDataSource<short>
	{
		public static readonly Microphone Default = new Microphone();

		public event EventHandler<ProcessingEventArgs> DataProcessed;

		protected override IProcessor CreateInputProcessor() =>
			new GenerationProcessor(this);

		protected override IProcessor CreateOutputProcessor() =>
			new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), this);
	}

	class Microphone : SignalProcessor, IAudioInputDevice, IDataSource<short>
	{
		public static readonly Microphone Default = new Microphone();


		public event EventHandler<AudioInputEventArgs> DataReady;
		public event EventHandler<ProcessingEventArgs> DataProcessed;

		protected override IProcessor CreateInputProcessor() =>
			new Wave.In.Processor(Wave.In.DefaultDevice.CreateSession(WaveFormat));

		protected override IProcessor CreateOutputProcessor() =>
			new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), this);
	}
}