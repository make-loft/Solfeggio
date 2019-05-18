using Ace;
using Rainbow;
using Solfeggio.Api;
using Solfeggio.Processors;
using System;
using System.ComponentModel;
using System.Linq;
using static Rainbow.Windowing;

namespace Solfeggio.Models
{
	[DataContract]
	public class ProcessingProfile : ContextObject, IAudioInputDevice, IExposable, IDisposable
	{
		public double[] SampleRates { get; } = AudioInputDevice.StandardSampleRates;

		[DataMember]
		public double SampleRate
		{
			get => Get(() => SampleRate, AudioInputDevice.DefaultSampleRate);
			set => Set(() => SampleRate, value);
		}

		[DataMember]
		public int SampleSize
		{
			get => Get(() => SampleSize, 4096);
			set => Set(() => SampleSize, value);
		}

		protected IProcessor inputProcessor;
		protected IProcessor outputProcessor;

		public SmartSet<Wave.In.DeviceInfo> InputDevices { get; set; }
		public SmartSet<Wave.Out.DeviceInfo> OutputDevices { get; set; }

		public Wave.In.DeviceInfo ActiveInputDevice
		{
			get => Get(() => ActiveInputDevice);
			set => Set(() => ActiveInputDevice, value);
		}

		public Wave.Out.DeviceInfo ActiveOutputDevice
		{
			get => Get(() => ActiveOutputDevice);
			set => Set(() => ActiveOutputDevice, value);
		}

		[DataMember]
		public int BuffersCount
		{
			get => Get(() => BuffersCount, 4);
			set => Set(() => BuffersCount, value);
		}

		[DataMember]
		public ApodizationFunc ActiveWindow
		{
			get => Get(() => ActiveWindow, Rectangle);
			set => Set(() => ActiveWindow, value);
		}

		public ApodizationFunc[] Windows { get; set; } =
		{
			BlackmanHarris,
			DOGWavelet,
			Gausse,
			Hamming,
			Hann,
			Rectangle,
		};

		[DataMember]
		public int FramePow
		{
			get => Get(() => FramePow, 11);
			set => Set(() => FramePow, value);
		}

		[DataMember] public bool UseSpectralInterpolation { get; set; } = true;
		[DataMember] public double ShiftsPerFrame { get; set; } = 0;
		public int ShiftSize => ShiftsPerFrame.Is(0d) ? 0 : (int)(FrameSize / ShiftsPerFrame);
		public int FrameSize => (int)Math.Pow(2.0d, FramePow);

		public TimeSpan FrameDuration => TimeSpan.FromSeconds(FrameSize / SampleRate);
		public TimeSpan SampleDuration => TimeSpan.FromSeconds(SampleSize / SampleRate);

		public ProcessingProfile()
		{
			InputDevices = Wave.In.EnumerateDevices().ToSet();
			OutputDevices = Wave.Out.EnumerateDevices().ToSet();
			InputDevices.Add(new SoftwareGenerator());

			ActiveInputDevice = ActiveInputDevice ?? InputDevices.FirstOrDefault();
			ActiveOutputDevice = ActiveOutputDevice ?? OutputDevices.FirstOrDefault();

			this[() => FramePow].PropertyChanged += (sender, args) =>
			{
				SampleSize = FrameSize + ShiftSize;
				EvokePropertyChanged(nameof(FrameDuration));
				EvokePropertyChanged(nameof(FrameSize));
			};
		}

		public void Expose()
		{
			if (inputProcessor.Is() || SampleSize.Is(default) || SampleRate.Is(default))
				return;

			var waveFormat = new WaveFormat((int)SampleRate, 16, 1); ;
			inputProcessor = ActiveInputDevice.CreateProcessor(waveFormat, SampleSize, BuffersCount);
			outputProcessor = ActiveOutputDevice.CreateProcessor(waveFormat, SampleSize, BuffersCount, inputProcessor);

			inputProcessor.DataAvailable += OnInputDataAvailable;

			inputProcessor.Wake();
			outputProcessor.Wake();

			PropertyChanged += OnPropertyChanged;
		}

		private readonly string[] ActivePropertyNames =
		{
			nameof(SampleRate),
			nameof(SampleSize),
			nameof(BuffersCount),
			nameof(ActiveWindow),
			nameof(ActiveInputDevice),
			nameof(ActiveOutputDevice),
		};

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (ActivePropertyNames.Contains(e.PropertyName).Not()) return;
			Restart();
			EvokePropertyChanged(nameof(FrameDuration));
			EvokePropertyChanged(nameof(SampleDuration));
		}

		public void Dispose()
		{
			if (inputProcessor.IsNot())
				return;

			inputProcessor.Free();
			outputProcessor.Free();

			inputProcessor.DataAvailable -= OnInputDataAvailable;

			outputProcessor = default;
			inputProcessor = default;

			PropertyChanged -= OnPropertyChanged;
		}

		public void Restart()
		{
			Dispose();
			Expose();
		}

		private void OnInputDataAvailable(object sender, ProcessingEventArgs args)
		{
			var sampleSize = args.Bins.Length;
			var frame = new Complex[sampleSize];
			for (var i = 0; i < sampleSize; i++)
			{
				frame[i] = (args.Bins[i] + 0.5d) / short.MaxValue;
			}

			SampleReady?.Invoke(this, new AudioInputEventArgs(this, frame, SampleRate));
		}

		public event EventHandler<AudioInputEventArgs> SampleReady;

		public void Start() => Expose();
		public void Stop() => Dispose();
	}
}
