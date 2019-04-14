using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Ace;
using Rainbow;

namespace Solfeggio
{
	public class Harmonic
	{
		public Func<double, double> Signal { get; set; } = v => Math.Sin(v);
		public double Magnitude { get; set; } = 0.3d;
		public double Frequency { get; set; } = 440d;
		public double Phase { get; set; } = 0d;

		private double value;

		public IEnumerable<double> EnumerateBins(double sampleRate)
		{
			var step = Frequency * Pi.Double / sampleRate;
			for (; ; value += step)
			{
				yield return Magnitude * Signal(value + Phase);
			}
		}
	}

	public class Generator : ContextObject, IAudioInputDevice
	{
		public double[] SampleRates { get; } = { 44100 };

		public double SampleRate { get; set; } = 44100;

		public int SampleSize { get; set; } = 1024;

		public event EventHandler<AudioInputEventArgs> DataReady;

		public SmartSet<Harmonic> Harmonics { get; } = new SmartSet<Harmonic>
		{
			new Harmonic {Frequency = 220d},
			new Harmonic {Frequency = 440d}
		};

		public Func<double, double> Signal { get; set; } = v => Math.Sin(v);

		private readonly DispatcherTimer _timer = new DispatcherTimer();

		public void Start()
		{
			var durationInSeconds = SampleSize / SampleRate;
			_timer.Interval = TimeSpan.FromSeconds(durationInSeconds);
			_timer.Start();
			_timer.Tick += (o, e) =>
			{
				var harmonics = Harmonics.Select(h => h.EnumerateBins(SampleRate).Take(SampleSize).ToArray()).ToArray();
				var signal = new Complex[SampleSize];
				for (var i = 0; i < SampleSize; i++)
				{
					foreach (var harmonic in harmonics)
					{
						signal[i] += harmonic[i];
					}
				}

				var args = new AudioInputEventArgs()
				{
					Frame = signal,
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

		public void Stop() => _timer.Stop();
	}
}
