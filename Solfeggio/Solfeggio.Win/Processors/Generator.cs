using Ace;
using Rainbow;
using Solfeggio.Api;
using Solfeggio.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace Solfeggio.Processors
{
	[DataContract]
	public class Generator : ContextObject, IAudioInputDevice, IExposable, IDataSource<short>
	{
		public double[] SampleRates { get; } = AudioInputDevice.StandardSampleRates;
		public double SampleRate
		{
			get => Get(() => SampleRate, AudioInputDevice.DefaultSampleRate);
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
		public SmartSet<Harmonic.Profile> Profiles { get; set; } = new SmartSet<Harmonic.Profile>
		{
			Create(), Create()
		};

		[DataMember]
		public Harmonic.Profile ActiveProfile
		{
			get => Get(() => ActiveProfile, Profiles.LastOrDefault());
			set => Set(() => ActiveProfile, value);
		}

		public Func<double, double> Signal { get; set; } = v => Math.Sin(v);

		private DispatcherTimer _timer;

		private Wave.Out.Processor outputProcessor;

		public void StartWith(double sampleRate = 0, int desiredFrameSize = 0)
		{
			SampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
			SampleSize = desiredFrameSize.Is(default) ? SampleSize : desiredFrameSize;

			if (outputProcessor.Is() && outputProcessor.State.IsNot(ProcessingState.Hibernation)) outputProcessor.Free();
			outputProcessor = new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), this);
			var durationInSeconds = SampleSize / SampleRate;
			_timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(durationInSeconds)
			};

			_timer.Tick += (o, e) =>
			{
				var signal = ActiveProfile.GenerateSignalSample(SampleSize, SampleRate, IsStatic);
				EvokeDataReady(signal);
			};

			Start();
		}

		public void Start()
		{
			_timer.Start();
			outputProcessor.Wake();
			Debug.WriteLine($"Started {this}");
		}

		public void Stop()
		{
			_timer.Stop();
			outputProcessor.Free();
			Debug.WriteLine($"Stopped {this}");
		}

		public void Expose()
		{
			this[Context.Set.Create].Executed += (o, e) => Create().Use(Profiles.Add);
			this[Context.Set.Delete].Executed += (o, e) => e.Parameter.To<Harmonic.Profile>().Use(Profiles.Remove);

			Profiles.CollectionChanged += (o, e) => ActiveProfile = Profiles.LastOrDefault();
		}

		private static Harmonic.Profile Create() => new Harmonic.Profile { Title = DateTime.Now.ToShortDateString() };

		public void EvokeDataReady(Complex[] frame) => DataReady?.Invoke(this, new AudioInputEventArgs()
		{
			Frame = frame,
			Source = this
		});

		//private readonly int _binSize = sizeof(short);

		public short[] Fill(in short[] buffer, int offset, int count)
		{
			if (ActiveProfile.IsNot()) return new short[0];
			var signal = ActiveProfile.GenerateSignalSample(SampleSize, SampleRate, IsStatic);
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
