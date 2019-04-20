using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Ace;
using Rainbow;
using Solfeggio.Api;
using Solfeggio.Models;

namespace Solfeggio
{
	[DataContract]
	public class Generator : ContextObject, IAudioInputDevice, IExposable, IDataSource<short>
	{
		public double[] SampleRates { get; } = { 44100 };
		public double SampleRate
		{
			get => Get(() => SampleRate, 44100);
			set => Set(() => SampleRate, value);
		}

		public int SampleSize
		{
			get => Get(() => SampleSize, 1024);
			set => Set(() => SampleSize, value);
		}

		public bool SoundOn { get; set; } = true;

		public event EventHandler<AudioInputEventArgs> DataReady;
		public event EventHandler<ProcessingEventArgs> DataProcessed;

		[DataMember] public bool IsStatic { get; set; }

		[DataMember]
		public SmartSet<Harmonic> Harmonics { get; } = new SmartSet<Harmonic>
		{
			new Harmonic {Frequency = 220d},
			new Harmonic {Frequency = 440d}
		};

		public Func<double, double> Signal { get; set; } = v => Math.Sin(v);

		private readonly DispatcherTimer _timer = new DispatcherTimer();

		private Wave.Out.Processor processor;

		public void Start()
		{
			processor = new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(), this);

			var durationInSeconds = SampleSize / SampleRate;
			_timer.Interval = TimeSpan.FromSeconds(durationInSeconds);
			_timer.Start();
			_timer.Tick += (o, e) =>
			{
				if (SoundOn)
				{
					if (processor.State.IsNot(ProcessingState.Processing))  processor.Wake();
					return;
				}
				else
				{
					if (processor.State.Is(ProcessingState.Processing)) processor.Free();
				}

				durationInSeconds = SampleSize / SampleRate;
				_timer.Interval = TimeSpan.FromSeconds(durationInSeconds);

				var signal = GenerateSignalSample(Harmonics, SampleSize, SampleRate, IsStatic);
				EvokeDataReady(signal);
			};
		}

		public void StartWith(double sampleRate = 0, int desiredFrameSize = 0)
		{
			SampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
			SampleSize = desiredFrameSize.Is(default) ? SampleSize : desiredFrameSize;

			Start();
		}

		public void Stop() => _timer.Stop();

		public void Expose()
		{
			this[Context.Set.Add].Executed += (o, e) =>	new Harmonic().Use(Harmonics.Add);
			this[Context.Set.Remove].Executed += (o, e) => e.Parameter.To<Harmonic>().Use(Harmonics.Remove);

			this[() => SampleSize].PropertyChanged += (o, e) =>
			processor = new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(), this);
		}

		private static Complex[] GenerateSignalSample(IEnumerable<Harmonic> harmonics, int length, double rate, bool isStatic)
		{
			var signalSample = new Complex[length];
			var harmonicSamples = harmonics.
				Where(h => h.IsEnabled).
				Select(h => h.EnumerateBins(rate, isStatic).Take(length).ToArray()).
				ToArray();

			foreach (var harmonicSample in harmonicSamples)
			{
				for (var i = 0; i < length; i++)
				{
					signalSample[i] += harmonicSample[i];
				}
			}

			return signalSample;
		}

		public void EvokeDataReady(Complex[] frame) => DataReady?.Invoke(this, new AudioInputEventArgs()
		{
			Frame = frame,
			Source = this
		});

		//private readonly int _binSize = sizeof(short);

		public short[] Fill(in short[] buffer, int offset, int count)
		{
			var signal = GenerateSignalSample(Harmonics, SampleSize, SampleRate, IsStatic);
			EvokeDataReady(signal);

			for (var n = 0; n < offset; n++)
				buffer[n] = default;

			for (var i = offset; i < count; i++)
			{
				//var j = _binSize * (offset + i);
				buffer[i] = (short)(signal[i].Real * short.MaxValue / 2d);
			}

			for (var n = offset + count; n < buffer.Length; n++)
				buffer[n] = default;

			return buffer;
		}

		public WaveFormat WaveFormat => new WaveFormat((int)SampleRate, 16, 1);
	}
}
