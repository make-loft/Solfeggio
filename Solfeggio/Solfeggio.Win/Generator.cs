using System;
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

		private readonly DispatcherTimer _timer = new DispatcherTimer();

		private Wave.Out.Processor processor;

		public void Start()
		{
			processor = new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), this);

			var durationInSeconds = SampleSize / SampleRate;
			_timer.Interval = TimeSpan.FromSeconds(durationInSeconds);
			_timer.Start();
			_timer.Tick += (o, e) =>
			{
				if (SoundOn)
				{
					if (processor.State.IsNot(ProcessingState.Processing)) processor.Wake();
					return;
				}
				else
				{
					if (processor.State.Is(ProcessingState.Processing)) processor.Free();
				}

				durationInSeconds = SampleSize / SampleRate;
				_timer.Interval = TimeSpan.FromSeconds(durationInSeconds);

				var signal = ActiveProfile.GenerateSignalSample(SampleSize, SampleRate, IsStatic);
				EvokeDataReady(signal);
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
			if (processor.State.Is(ProcessingState.Processing)) processor.Free();
		}

		public void Expose()
		{
			this[Context.Set.Create].Executed += (o, e) => Create().Use(Profiles.Add);
			this[Context.Set.Delete].Executed += (o, e) => e.Parameter.To<Harmonic.Profile>().Use(Profiles.Remove);

			this[() => SampleSize].PropertyChanged += (o, e) =>
				processor = new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), this);

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
