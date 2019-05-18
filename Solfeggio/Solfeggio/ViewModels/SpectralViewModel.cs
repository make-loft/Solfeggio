using System;
using System.Collections.Generic;
using System.Linq;
using Ace;
using Rainbow;
using Solfeggio.Models;
using static Rainbow.Windowing;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class SpectralViewModel : ContextObject, IExposable
	{
		public ProcessingProfile ActiveProfile
		{
			get => Get(() => ActiveProfile, Profiles.FirstOrDefault());
			set => Set(() => ActiveProfile, value);
		}

		public ProcessingProfile[] Profiles { get; } =
		{
			new ProcessingProfile(),
			new ProcessingProfile()
		};

		public bool IsPaused
		{
			get => Get(() => IsPaused);
			set => Set(() => IsPaused, value);
		}

		public IList<Bin> Spectrum { get; private set; }
		public IList<Complex> OuterFrame { get; private set; }
		public IList<Complex> InnerFrame { get; private set; }


		public SmartSet<double> Pitches { get; } = new SmartSet<double>();

		public void Expose()
		{
			this[() => ActiveProfile].PropertyChanging += (sender, args) =>
			{
				if (ActiveProfile.IsNot()) return;
				ActiveProfile.SampleReady -= OnActiveProfileOnDataReady;
				ActiveProfile.Stop();
			};

			this[() => ActiveProfile].PropertyChanged += (sender, args) =>
			{
				if (ActiveProfile.IsNot()) return;
				ActiveProfile.SampleReady += OnActiveProfileOnDataReady;
				ActiveProfile.Start();
			};

			EvokePropertyChanged(nameof(ActiveProfile));
		}

		private void OnActiveProfileOnDataReady(object sender, AudioInputEventArgs args)
		{
			var (j, k) = 0d;
			var frameSize = args.Source.FrameSize;

			var frame0 = args.Sample.Skip(0).Take(frameSize).ToArray();
			var frame1 = args.Sample.Skip(args.Source.ShiftSize).Take(frameSize).ToArray();
			var activeWindow = args.Source.ActiveWindow;
			if (activeWindow.Is(Rectangle)) goto SkipApodization;
			for (var i = 0; i < frameSize; i++)
			{
				frame0[i] *= activeWindow(i, frameSize);
				frame1[i] *= activeWindow(i, frameSize);
			}

		SkipApodization:

			var floats = frame0.Select(v => (float)(v.Real / short.MaxValue)).ToArray();

			if (IsPaused || args.Sample.Length < frameSize + args.Source.ShiftSize) return;
			var spectrum0 = frame0.Decimation(true);
			var spectrum1 = frame1.Decimation(true);

			for (var i = 0; i < frameSize; i++)
			{
				spectrum0[i] /= frameSize;
				spectrum1[i] /= frameSize;
			}

			var innerFrame = spectrum0.Decimation(false);
			var frameLength = innerFrame.Length;
			OuterFrame = args.Sample.Take(frameLength).Select(c => new Complex(j++ / frameLength, c.Real)).ToArray();
			InnerFrame = innerFrame.Select(c => new Complex(k++, c.Real)).ToArray();

			var spectrum = Filtering.GetSpectrum(spectrum0, args.SampleRate).ToArray();
			//ShiftsPerFrame.Is(0d)
			//? Filtering.GetSpectrum(spectrum0, SampleRate)
			//: Filtering.GetJoinedSpectrum(spectrum0, spectrum1, ShiftsPerFrame, SampleRate);
			if (args.Source.UseSpectralInterpolation)
				spectrum = Filtering.Interpolate(spectrum).ToArray();

			Spectrum = spectrum;
		}
	}
}
