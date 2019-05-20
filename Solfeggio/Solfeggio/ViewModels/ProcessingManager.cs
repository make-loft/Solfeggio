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
		public IList<Complex> OuterFrame { get; private set; }
		public IList<Complex> InnerFrame { get; private set; }

		public override ProcessingProfile Create() => new ProcessingProfile();

		public override void Expose()
		{
			base.Expose();

			if (Profiles.Count.Is(0))
			{
				var p = default(ProcessingProfile);

				Create().To(out p).With
				(
					p.Title = "Low Latency Realtime Analize",
					p.FramePow = 10
				).Use(Profiles.Add);

				Create().To(out p).With
				(
					p.Title = "Research of Ideal Signals",
					p.FramePow = 10,
					p.ActiveInputDevice = p.InputDevices.LastOrDefault()
				).Use(Profiles.Add);

				Create().To(out p).With
				(
					p.Title = "Musical Tuning & Vocal Trainings",
					p.FramePow = 11
				).Use(Profiles.Add);
			}

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
			var (j, k) = 0d;
			var frameSize = args.Source.FrameSize;

			var timeFrame = args.Sample.Skip(0).Take(frameSize).ToArray();
			var activeWindow = args.Source.ActiveWindow;
			if (activeWindow.Is(Rectangle)) goto SkipApodization;
			for (var i = 0; i < frameSize; i++)
			{
				timeFrame[i] *= activeWindow(i, frameSize);
			}

		SkipApodization:

			var floats = timeFrame.Select(v => (float)(v.Real / short.MaxValue)).ToArray();

			if (IsPaused || args.Sample.Length < frameSize + args.Source.ShiftSize) return;
			var spectralFrame = timeFrame.Decimation(true);
			var spectrum = Filtering.GetSpectrum(spectralFrame, args.SampleRate).ToArray();
			Spectrum = args.Source.UseSpectralInterpolation
				? Filtering.Interpolate(spectrum).ToArray()
				: spectrum;

			var innerFrame = spectralFrame.Decimation(false);
			OuterFrame = args.Sample.Take(frameSize).Select(c => new Complex(j++ / frameSize, c.Real)).ToArray();
			InnerFrame = innerFrame.Select(c => new Complex(k++, c.Real)).ToArray();
		}
	}
}
