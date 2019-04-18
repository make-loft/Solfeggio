using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Ace;
using Rainbow;
using Solfeggio.Api;

namespace Solfeggio
{
	public class Harmonic
	{
		[DataMember] public Func<double, double> Signal { get; set; } = v => Math.Sin(v);
		[DataMember] public double Magnitude { get; set; } = 0.3d;
		[DataMember] public double Frequency { get; set; } = 440d;
		[DataMember] public double Phase { get; set; } = 0d;
		[DataMember] public bool IsStatic { get; set; }

		private double offset;

		public IEnumerable<double> EnumerateBins(double sampleRate, bool isStatic)
		{
			var step = Frequency * Pi.Double / sampleRate;
			for (offset = IsStatic || isStatic ? 0d : offset; ; offset += step)
			{
				yield return 2d * Magnitude * Signal(offset + Phase);
			}
		}
	}
	
	[DataContract]
	public class Generator : ContextObject, IAudioInputDevice, IExposable, IWaveProvider
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

		[DataMember] public bool IsStatic { get; set; }

		[DataMember]
		public SmartSet<Harmonic> Harmonics { get; } = new SmartSet<Harmonic>
		{
			new Harmonic {Frequency = 220d},
			new Harmonic {Frequency = 440d}
		};

		public Func<double, double> Signal { get; set; } = v => Math.Sin(v);

		private readonly DispatcherTimer _timer = new DispatcherTimer();

		public void Start()
		{
			waveOut.Play();

			var durationInSeconds = SampleSize / SampleRate;
			_timer.Interval = TimeSpan.FromSeconds(durationInSeconds);
			//_timer.Start();
			_timer.Tick += (o, e) =>
			{
				if (SoundOn)
				{
					if (waveOut.PlaybackState.IsNot(PlaybackState.Playing))  waveOut.Play();
					return;
				}
				else
				{
					if (waveOut.PlaybackState.Is(PlaybackState.Playing)) waveOut.Stop();
				}

				durationInSeconds = SampleSize / SampleRate;
				_timer.Interval = TimeSpan.FromSeconds(durationInSeconds);

				var signal = GenerateSignal();
				var args = new AudioInputEventArgs()
				{
					Frame = signal,
					Source = this
				};

				DataReady?.Invoke(this, args);
			};
		}

		WaveOut waveOut;

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

			this[() => SampleSize].PropertyChanged += (o, e) =>	waveOut.Init(this, SampleSize * 4);

			waveOut = new WaveOut { Volume = 1f };
		}

		private Complex[] GenerateSignal()
		{
			var signal = new Complex[SampleSize];
			var harmonics = Harmonics.
				Select(h => h.EnumerateBins(SampleRate, IsStatic).Take(SampleSize).ToArray()).
				ToArray();

			foreach (var harmonic in harmonics)
			{
				for (var i = 0; i < SampleSize; i++)
				{
					signal[i] += harmonic[i];
				}
			}

			return signal;
		}

		public int Read(byte[] buffer, int offset, int count)
		{
			var signal = GenerateSignal();
			var args = new AudioInputEventArgs()
			{
				Frame = signal,
				Source = this
			};

			DataReady?.Invoke(this, args);

			for (var i = 0; i < signal.Length; i++)
			{
				//var bytes = BitConverter.GetBytes((short)(256d * signal[i].Real));
				var bytes = BitConverter.GetBytes((float)signal[i].Real);

				var j = 4 * (offset + i);
				buffer[j + 0] = bytes[0];
				buffer[j + 1] = bytes[1];
				buffer[j + 2] = bytes[2];
				buffer[j + 3] = bytes[3];
			}

			return signal.Length * sizeof(float);
		}

		public WaveFormat WaveFormat => new WaveFormat((int)SampleRate, 32, 1);
	}
}
