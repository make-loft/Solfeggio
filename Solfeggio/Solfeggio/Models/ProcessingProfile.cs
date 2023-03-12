using Ace;

using Rainbow;

using Solfeggio.Api;
using Solfeggio.Processors;

using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using static Rainbow.Windowing;

namespace Solfeggio.Models
{
	[DataContract]
	public class ProcessingProfile : AProfile, IAudioInputDevice, IExposable, IDisposable
	{
		public double[] SampleRates { get; } = AudioInputDevice.StandardSampleRates;

		[DataMember]
		public double SampleRate
		{
			get => Get(() => SampleRate, AudioInputDevice.DefaultSampleRate);
			set => Set(() => SampleRate, value, true);
		}

		[DataMember]
		public int SampleSize
		{
			get => Get(() => SampleSize, 4096);
			set => Set(() => SampleSize, value, true);
		}

		protected IProcessor inputProcessor;
		protected IProcessor outputProcessor;

		public SmartSet<Wave.In.DeviceInfo> InputDevices { get; set; }
		public SmartSet<Wave.Out.DeviceInfo> OutputDevices { get; set; }

		public Wave.In.DeviceInfo ActiveInputDevice
		{
			get => Get(() => ActiveInputDevice);
			set => Set(() => ActiveInputDevice, value, true);
		}

		public Wave.Out.DeviceInfo ActiveOutputDevice
		{
			get => Get(() => ActiveOutputDevice);
			set => Set(() => ActiveOutputDevice, value, true);
		}

		[DataMember]
		public int BuffersCount
		{
			get => Get(() => BuffersCount, 16);
			set => Set(() => BuffersCount, value, true);
		}

		[DataMember]
		public ApodizationFunc ActiveWindow
		{
			get => Get(() => ActiveWindow, Rectangle);
			set => Set(() => ActiveWindow, value, true);
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
			set => Set(() => FramePow, value, true);
		}

		[DataMember] public int ActiveInputDeviceIndex { get; set; }
		[DataMember] public int ActiveOutputDeviceIndex { get; set; }
		[DataMember] public bool UseSpectralInterpolation { get; set; } = true;
		public double ShiftsPerFrame { get; set; } = 0;
		public int ShiftSize => ShiftsPerFrame.Is(0d) ? 0 : (int)(FrameSize / ShiftsPerFrame);
		public int FrameSize => (int)Math.Pow(2.0d, FramePow);

		public double FrameStep => SampleRate / FrameSize;
		public double Accuracy => 0.02 * FrameStep;

		public TimeSpan FrameDuration => TimeSpan.FromSeconds(FrameSize / SampleRate);
		public TimeSpan SampleDuration => TimeSpan.FromSeconds(SampleSize / SampleRate);

		public ProcessingProfile()
		{
			InputDevices = Wave.In.EnumerateDevices().ToSet();
			OutputDevices = Wave.Out.EnumerateDevices().ToSet();

			InputDevices.Add(new SoftwareSignalGenerator.DeviceInfo());
#if !NETSTANDARD
			//InputDevices.Add(new PcmBinDecoder.DeviceInfo());
			//InputDevices.Add(new StreamPcmTxtDecoder.DeviceInfo());
			//InputDevices.Add(new PcmReader.DeviceInfo());

			//OutputDevices.Add(new EncodeProcessor.DeviceInfo());
#endif

			this[() => ActiveInputDevice].Changed += (o, e) =>
				ActiveInputDeviceIndex = InputDevices.IndexOf(ActiveInputDevice);

			this[() => ActiveOutputDevice].Changed += (o, e) =>
				ActiveOutputDeviceIndex = OutputDevices.IndexOf(ActiveOutputDevice);

			this[() => FramePow].Changed += (o, e) =>
			{
				SampleSize = FrameSize + ShiftSize;
				EvokeFramePropertiesChanged();
			};

			this[() => InputLevel].Changed += (o, e) =>
				inputProcessor.To(out var p)?.With(p.Level = InputLevel);
			this[() => OutputLevel].Changed += (o, e) =>
				outputProcessor.To(out var p)?.With(p.Level = OutputLevel);
		}

		private void EvokeFramePropertiesChanged()
		{
			EvokePropertyChanged(nameof(FrameDuration));
			EvokePropertyChanged(nameof(FrameSize));
			EvokePropertyChanged(nameof(FrameStep));
			EvokePropertyChanged(nameof(Accuracy));
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

			var waveFormat = new WaveFormat((int)SampleRate, 16, 1);

			var activeInputDevice = ActiveInputDevice;
			if (activeInputDevice.Is())
			{
				inputProcessor = activeInputDevice.CreateProcessor(waveFormat, SampleSize, BuffersCount);
				inputProcessor.DataAvailable += OnInputDataAvailable;
				inputProcessor.Level = InputLevel;
				inputProcessor.Wake();
			}

			var activeOutputDevice = ActiveOutputDevice;
			if (activeOutputDevice.Is())
			{
				outputProcessor = activeOutputDevice.CreateProcessor(waveFormat, SampleSize, BuffersCount, inputProcessor);
				outputProcessor.Level = OutputLevel;
				outputProcessor.Wake();
			}

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

		bool _resetRequested;

		private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (LifecyclePropertyNames.Contains(e.PropertyName).Not())
				return;

			if (_resetRequested.Is(false))
			{
				_resetRequested = true;
				await Task.Delay(32);
			}
			else return;

			_resetRequested = false;

			EvokeFramePropertiesChanged();
			Dispose();
			Expose();
		}

		public void Dispose()
		{
			if (outputProcessor.Is())
			{
				outputProcessor.Free();
				outputProcessor = default;
			}

			if (inputProcessor.Is())
			{
				inputProcessor?.Free();
				inputProcessor.DataAvailable -= OnInputDataAvailable;
				inputProcessor = default;
			}

			PropertyChanged -= OnPropertyChanged;
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
			get => Get(() => OutputLevel, 0.01f);
			set => Set(() => OutputLevel, value);
		}

		[DataMember]
		public bool AdaptationState { get; set; } = true;

		private void AdaptLevels(Complex[] sample)
		{
			var scale = 0.8f;
			var thresholdLevel = 0.9f;
			var optimalInputLevel = 1.0f;
			var optimalOutputLevel = 0.1f;

			if (sample.Max(c => c.Real).To(out float maxLevel) <= thresholdLevel)
				return;

			if (InputLevel > optimalInputLevel)
			{
				var scaledLevel = InputLevel * scale;
				InputLevel = scaledLevel < optimalInputLevel
					? optimalInputLevel
					: scaledLevel;
			}
			
			if (OutputLevel > optimalOutputLevel)
			{
				OutputLevel *= scale;
				var scaledLevel = InputLevel * scale;
				InputLevel = scaledLevel < optimalOutputLevel
					? optimalOutputLevel 
					: scaledLevel;
			}
		}

		private void OnInputDataAvailable(object sender, ProcessingEventArgs args)
		{
			var sampleSize = args.Sample.Length;
			var sample = new Complex[sampleSize];
			for (var i = 0; i < sampleSize; i++)
			{
				sample[i] = args.Sample[i];
			}

			if (AdaptationState.Is(true))
				AdaptLevels(sample);

			SampleReady?.Invoke(this, new(this, sample, SampleRate));
		}

		public event EventHandler<AudioInputEventArgs> SampleReady;
	}
}
