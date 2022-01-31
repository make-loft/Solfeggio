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

		public override IEnumerable<ProcessingProfile> CreateDefaultProfiles()
		{
			yield return new()
			{
				Title = "Musical Tuning & Vocal Trainings",
				FramePow = 11
			};

			yield return Create().To(out var b).With
			(
				b.Title = "Research of Ideal Signals",
				b.OutputLevel = 0.4f,
				b.ActiveInputDevice = b.InputDevices.LastOrDefault(),
				b.FramePow = 10
			);

			yield return new()
			{
				Title = "Low Latency Realtime Analize",
				FramePow = 10
			};
		}

		public override void Expose()
		{
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
			var spectralFrame = timeFrame.Transform(true);
			return spectralFrame;
		}
	}
}
