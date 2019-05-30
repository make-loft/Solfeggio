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
		[DataMember]
		public string Title
		{
			get => Get(() => Title, DateTime.Now.Millisecond.ToString());
			set => Set(() => Title, value);
		}

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

		[DataMember] public int ActiveInputDeviceIndex { get; set; }
		[DataMember] public int ActiveOutputDeviceIndex { get; set; }
		[DataMember] public bool UseSpectralInterpolation { get; set; } = true;
		public double ShiftsPerFrame { get; set; } = 0;
		public int ShiftSize => ShiftsPerFrame.Is(0d) ? 0 : (int)(FrameSize / ShiftsPerFrame);
		public int FrameSize => (int)Math.Pow(2.0d, FramePow);

		public TimeSpan FrameDuration => TimeSpan.FromSeconds(FrameSize / SampleRate);
		public TimeSpan SampleDuration => TimeSpan.FromSeconds(SampleSize / SampleRate);

		public ProcessingProfile()
		{
			InputDevices = Wave.In.EnumerateDevices().ToSet();
			OutputDevices = Wave.Out.EnumerateDevices().ToSet();
			InputDevices.Add(new SoftwareGenerator());

			this[() => ActiveInputDevice].PropertyChanged += (o, e) =>
				ActiveInputDeviceIndex = InputDevices.IndexOf(ActiveInputDevice);

			this[() => ActiveOutputDevice].PropertyChanged += (o, e) =>
				ActiveOutputDeviceIndex = OutputDevices.IndexOf(ActiveOutputDevice);

			this[() => FramePow].PropertyChanged += (o, e) =>
			{
				SampleSize = FrameSize + ShiftSize;
				EvokePropertyChanged(nameof(FrameDuration));
				EvokePropertyChanged(nameof(FrameSize));
			};

			this[() => InputBoost].PropertyChanged += (o, e) =>
				inputProcessor.To(out var p)?.With(p.Boost = InputBoost);
			this[() => InputLevel].PropertyChanged += (o, e) =>
				inputProcessor.To(out var p)?.With(p.Level = InputLevel);
			this[() => OutputBoost].PropertyChanged += (o, e) =>
				outputProcessor.To(out var p)?.With(p.Boost = OutputBoost);
			this[() => OutputLevel].PropertyChanged += (o, e) =>
				outputProcessor.To(out var p)?.With(p.Level = OutputLevel);
		}

		private bool CheckIndex(int index, int count) => 0 <= index && index < count;

		public void Expose()
		{
			ActiveInputDevice = CheckIndex(ActiveInputDeviceIndex, InputDevices.Count)
				? InputDevices[ActiveInputDeviceIndex]
				: default;

			ActiveOutputDevice = CheckIndex(ActiveOutputDeviceIndex, OutputDevices.Count)
				? OutputDevices[ActiveOutputDeviceIndex]
				: default;

			if (inputProcessor.Is() || SampleSize.Is(default) || SampleRate.Is(default))
				return;

			var waveFormat = new WaveFormat((int)SampleRate, 16, 1); ;
			inputProcessor = ActiveInputDevice.CreateProcessor(waveFormat, SampleSize, BuffersCount);
			outputProcessor = ActiveOutputDevice.CreateProcessor(waveFormat, SampleSize, BuffersCount, inputProcessor);
			inputProcessor.Boost = InputBoost;
			outputProcessor.Boost = OutputBoost;
			inputProcessor.Level = InputLevel;
			outputProcessor.Level = OutputLevel;

			inputProcessor.DataAvailable += OnInputDataAvailable;

			inputProcessor.Wake();
			outputProcessor.Wake();

			PropertyChanged += OnPropertyChanged;
		}

		private readonly string[] LifecyclePropertyNames =
		{
			nameof(SampleRate),
			nameof(SampleSize),
			nameof(BuffersCount),
			nameof(ActiveInputDevice),
			nameof(ActiveOutputDevice),
		};

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (LifecyclePropertyNames.Contains(e.PropertyName).Not()) return;
			EvokePropertyChanged(nameof(SampleDuration));
			EvokePropertyChanged(nameof(FrameDuration));
			Dispose();
			Expose();
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

		[DataMember]
		public double InputBoost
		{
			get => Get(() => InputBoost, 1.0d);
			set => Set(() => InputBoost, value);
		}

		[DataMember]
		public double OutputBoost
		{
			get => Get(() => OutputBoost, 1.0d);
			set => Set(() => OutputBoost, value);
		}

		[DataMember]
		public float InputLevel
		{
			get => Get(() => InputLevel, 1.0f);
			set => Set(() => InputLevel, value);
		}

		[DataMember]
		public float OutputLevel
		{
			get => Get(() => OutputLevel, 0.05f);
			set => Set(() => OutputLevel, value);
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
	}
}
