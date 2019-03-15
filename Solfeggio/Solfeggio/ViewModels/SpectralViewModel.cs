﻿using System;
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

		public IAudioInputDevice[] Devices { get; } = { Store.Get<IAudioInputDevice>(), new Generator() };

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
			get => Get(() => FramePow, 11);
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
		public IList<Complex> CurrentSpectrum { get; set; }
		public IList<Complex> WaveInData { get; set; }
		public IList<Complex> WaveOutData { get; set; }

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

		[DataMember] public bool UseAliasing { get; set; } = true;
		[DataMember] public double ShiftsPerFrame { get; set; } = 0;
		private int ShiftSize => ShiftsPerFrame.Is(0d) ? 0 : (int)(FrameSize / ShiftsPerFrame);

		public SmartSet<double> Pitches { get; } = new SmartSet<double>();

		public void Expose()
		{
			this[() => FramePow].PropertyChanged += (sender, args) =>
			{
				ActiveDevice.StartWith(ActiveDevice.SampleRate, FrameSize + ShiftSize);
				EvokePropertyChanged(nameof(FrameDuration));
				EvokePropertyChanged(nameof(FrameSize));
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

				if (IsPaused || args.Frame.Length < frameSize + ShiftSize) return;
				var spectrum0 = frame0.DecimationInTime(true);
				var spectrum1 = frame1.DecimationInTime(true);

				for (var i = 0; i < frameSize; i++)
				{
					spectrum0[i] /= frameSize;
					spectrum1[i] /= frameSize;
				}

				var outSample = spectrum0.DecimationInTime(false);
				WaveInData = args.Frame.Take(outSample.Length).Select(c => new Complex(j++, c.Real)).ToArray();
				WaveOutData = outSample.Select(c => new Complex(k++, c.Real)).ToArray();

				var spectrum = ShiftsPerFrame.Is(0d)
					? Filtering.GetSpectrum(spectrum0, SampleRate)
					: Filtering.GetJoinedSpectrum(spectrum0, spectrum1, ShiftsPerFrame, SampleRate);
				if (UseAliasing) spectrum = Filtering.Correct(spectrum);

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
				ActiveDevice.StartWith(default, FrameSize + ShiftSize);
			};

			EvokePropertyChanged(() => ActiveDevice);
		}
	}
}
