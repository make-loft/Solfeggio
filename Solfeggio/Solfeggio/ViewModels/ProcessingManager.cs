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
	public class ProcessingManager : AManager<ProcessingProfile>
	{
		public bool IsPaused
		{
			get => Get(() => IsPaused);
			set => Set(() => IsPaused, value);
		}

		public IList<Bin> Spectrum { get; private set; }
		public IList<Bin> SpectrumBetter { get; private set; }
		public IList<Complex> OuterFrame { get; private set; }
		public IList<Complex> InnerFrame { get; private set; }

		public override ProcessingProfile Create() => new();

		public override void Expose()
		{
			if (Profiles.Count.Is(0))
			{
				var p = default(ProcessingProfile);

				Create().To(out p).With
				(
					p.Title = "Musical Tuning & Vocal Trainings",
					p.FramePow = 11
				).Use(Profiles.Add);

				Create().To(out p).With
				(
					p.Title = "Research of Ideal Signals",
					p.FramePow = 10,
					p.ActiveInputDevice = p.InputDevices.LastOrDefault(),
					p.OutputLevel = 1.0f
				).Use(Profiles.Add);

				Create().To(out p).With
				(
					p.Title = "Low Latency Realtime Analize",
					p.FramePow = 10
				).Use(Profiles.Add);
			}

			base.Expose();

			this[() => ActiveProfile].PropertyChanging += (sender, args) =>
			{
				if (ActiveProfile.IsNot()) return;
				ActiveProfile.SampleReady -= OnActiveProfileOnDataReady;
				ActiveProfile.Dispose();
			};

			this[() => ActiveProfile].PropertyChanged += (sender, args) =>
			{
				if (ActiveProfile.IsNot()) return;
				ActiveProfile.SampleReady += OnActiveProfileOnDataReady;
				ActiveProfile.Expose();
			};

			EvokePropertyChanged(nameof(ActiveProfile));
		}

		private void OnActiveProfileOnDataReady(object sender, AudioInputEventArgs args)
		{
			var frameSize = args.Source.FrameSize;
			if (IsPaused || args.Sample.Length < frameSize + args.Source.ShiftSize) return;

			var timeFrame = args.Sample.Skip(0).Take(frameSize).ToArray();
			var spectralFrame = GetSpectrum(timeFrame, args.Source.ActiveWindow);
			var shiftsPerFrame = (int)args.Source.ShiftsPerFrame;
			if (shiftsPerFrame > 0)
			{
				var timeFrame_ = args.Sample.Skip(args.Source.ShiftSize).Take(frameSize).ToArray();
				var spectralFrame_ = GetSpectrum(timeFrame_, args.Source.ActiveWindow);
				var spectrum = Filtering.GetJoinedSpectrum(spectralFrame, spectralFrame_, shiftsPerFrame, args.SampleRate).ToArray();
				SpectrumBetter = spectrum;
				Spectrum = spectrum;
			}
			else
			{
				var spectrum = Filtering.GetSpectrum(spectralFrame, args.SampleRate).ToArray();
				SpectrumBetter = Filtering.Interpolate(spectrum).ToArray();
				Spectrum = spectrum;
			}

			var (j, k) = 0d;
			//var innerFrame = spectralFrame.Decimation(false);
			InnerFrame = timeFrame.Select(c => new Complex(k++ / frameSize, c.Real)).ToArray();
			OuterFrame = args.Sample.Take(frameSize).Select(c => new Complex(j++ / frameSize, c.Real)).ToArray();
		}

		private Complex[] GetSpectrum(Complex[] timeFrame, ApodizationFunc activeWindow)
		{
			var frameSize = timeFrame.Length;
			if (activeWindow.Is(Rectangle)) goto SkipApodization;
			for (var i = 0; i < frameSize; i++)
			{
				timeFrame[i] *= activeWindow(i, frameSize);
			}

		SkipApodization:

			var floats = timeFrame.Select(v => (float)(v.Real / short.MaxValue)).ToArray();
			var spectralFrame = timeFrame.Decimation(true);
			return spectralFrame;
		}
	}
}
