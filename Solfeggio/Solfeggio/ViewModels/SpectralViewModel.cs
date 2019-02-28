using System;
using System.Collections.Generic;
using System.Linq;
using Ace;
using Rainbow;
using static Rainbow.Windowing;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class SpectralViewModel : ContextObject, IExposable
	{
		public IAudioInputDevice ActiveDevice
		{
			get => Get(() => ActiveDevice, Devices.FirstOrDefault());
			set => Set(() => ActiveDevice, value);
		}

		public IAudioInputDevice[] Devices { get; } = { Store.Get<IAudioInputDevice>() };

		public delegate double ApodizationFunc(double binIndex, double frameSize);

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
			get => Get(() => FramePow, 10);
			set => Set(() => FramePow, value);
		}

		[DataMember]
		public bool ShowSettings
		{
			get => Get(() => ShowSettings);
			set => Set(() => ShowSettings, value);
		}

		public bool IsPaused
		{
			get => Get(() => IsPaused);
			set => Set(() => IsPaused, value);
		}

		public int MinFramePow { get; set; }
		public int FrameSize => (int)Math.Pow(2.0d, FramePow);
		public TimeSpan FrameDuration => TimeSpan.FromSeconds(FrameSize / SampleRate);
		public Dictionary<double, double> CurrentSpectrum { get; set; }
		public Dictionary<double, double> WaveInData { get; set; }
		public Dictionary<double, double> WaveOutData { get; set; }

		public double SampleRate
		{
			get => ActiveDevice?.SampleRate ?? default;
			set
			{
				if (value <= 0) return;
				SampleRates.Remove(value);
				ActiveDevice.StartWith(value, FrameSize);
				SampleRates.Insert(0, value);
				EvokePropertyChanged(() => SampleRate);
			}
		}

		public SmartSet<double> SampleRates { get; private set; }

		public double MinSampleRate => ActiveDevice?.SampleRates.Min() ?? default;
		public double MaxSampleRate => ActiveDevice?.SampleRates.Max() ?? default;

		public bool UseAliasing { get; set; } = true;
		public double ShiftsPerFrame { get; set; } = 16;
		private int ShiftSize => (int)(FrameSize / ShiftsPerFrame);

		public SmartSet<double> Pitches { get; } = new SmartSet<double>();

		public void Expose()
		{
			var tracker = new Pitch.PitchTracker();

			this[() => FramePow].PropertyChanged += (sender, args) =>
			{
				ActiveDevice.StartWith(ActiveDevice.SampleRate, (int)(FrameSize * (1d + 1d / ShiftsPerFrame)));
				EvokePropertyChanged(nameof(FrameDuration));
				EvokePropertyChanged(nameof(FrameSize));
			};

			tracker.PitchDetected += (t, r) =>
			{
				if (r.Pitch == 0f) return;
				Pitches.Add(r.Pitch);
			};

			void OnActiveDeviceOnDataReady(object sender, AudioInputEventArgs args)
			{
				var (j, k) = 0d;
				var frameSize = FrameSize;

				var frame0 = args.Frame.Skip(0).Take(FrameSize).ToArray();
				var frame1 = args.Frame.Skip(ShiftSize).Take(FrameSize).ToArray();
				var activeWindow = ActiveWindow;
				if (activeWindow.Is(Rectangle)) goto SkipApodization;
				for (var i = 0; i < frameSize; i++)
				{
					frame0[i] *= activeWindow(i, frameSize);
					frame1[i] *= activeWindow(i, frameSize);
				}

			SkipApodization:

				var floats = frame0.Select(v => (float)(v.Real/short.MaxValue)).ToArray();
				tracker.SampleRate = SampleRate;
				tracker.ProcessBuffer(floats);

				if (IsPaused || args.Frame.Length < frameSize + ShiftSize) return;
				var spectrum0 = frame0.DecimationInTime(true);
				var spectrum1 = frame1.DecimationInTime(true);

				for (var i = 0; i < frameSize; i++)
				{
					spectrum0[i] /= frameSize;
					spectrum1[i] /= frameSize;
				}

				var outSample = spectrum0.DecimationInTime(false);
				WaveInData = args.Frame.Take(outSample.Length).ToDictionary(c => j++, c => c.Real);
				WaveOutData = outSample.ToDictionary(c => k++, c => c.Real);

				var spectrum = Filtering.GetJoinedSpectrum(spectrum0, spectrum1, ShiftsPerFrame, SampleRate);
				if (UseAliasing) spectrum = Filtering.Antialiasing(spectrum);

				CurrentSpectrum = spectrum;
			}

			this[() => ActiveDevice].PropertyChanging += (sender, args) =>
			{
				if (ActiveDevice.IsNot()) return;
				ActiveDevice.Stop();
				ActiveDevice.DataReady -= OnActiveDeviceOnDataReady;
			};

			this[() => ActiveDevice].PropertyChanged += (sender, args) =>
			{
				if (ActiveDevice.IsNot()) return;
				SampleRates = ActiveDevice.SampleRates.ToSet();
				ActiveDevice.DataReady += OnActiveDeviceOnDataReady;
				ActiveDevice.StartWith();
			};

			EvokePropertyChanged(() => ActiveDevice);
		}
	}
}
