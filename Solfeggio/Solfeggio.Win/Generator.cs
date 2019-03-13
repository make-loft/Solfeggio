using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Ace;
using Rainbow;

namespace Solfeggio
{
	public class Generator : ContextObject, IAudioInputDevice
	{
		public double[] SampleRates { get; } = { 44100 };

		public double SampleRate { get; set; } = 44100;

		public int SampleSize { get; set; } = 1024;

		public event EventHandler<AudioInputEventArgs> DataReady;

		public IEnumerable<double> EnumerateBins(Func<double, double> signal, double arg, double step)
		{
			for (var value = arg; ; value += step)
			{
				yield return signal(value);
			}
		}

		private IEnumerable<double> _source;

		public Func<double, double> Signal { get; set; } = v => Math.Sin(v);

		public double Frequancy
		{
			get => Get(() => Frequancy, 440d);
			set => Set(() => Frequancy, value);
		}

		private readonly DispatcherTimer _timer = new DispatcherTimer();

		public void Start()
		{
			this[() => Frequancy].PropertyChanged += (o, e) =>
			{
				var step = Frequancy * 2d * Math.PI / SampleRate;
				_source = EnumerateBins(Signal, 0d, step);
			};

			EvokePropertyChanged(() => Frequancy);

			var durationInSeconds = SampleSize / SampleRate;
			_timer.Interval = TimeSpan.FromSeconds(durationInSeconds);
			_timer.Start();
			_timer.Tick += (o, e) =>
			{
				var args = new AudioInputEventArgs()
				{
					Frame = _source.Take(SampleSize).Select(v => new Complex(v)).ToArray(),
					Source = this
				};

				DataReady?.Invoke(this, args);
			};
		}

		public void StartWith(double sampleRate = 0, int desiredFrameSize = 0)
		{
			SampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
			SampleSize = desiredFrameSize.Is(default) ? SampleSize : desiredFrameSize;
			Start();
		}

		public void Stop()
		{
			_timer.Stop();
		}
	}
}
