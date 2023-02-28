using System;
using Ace;
using Rainbow;
using Solfeggio.Models;

namespace Solfeggio.Api
{
	public interface IAudioInputDevice : IExposable, IDisposable
	{
		double[] SampleRates { get; }
		double SampleRate { get; set; }
		int SampleSize { get; set; }

		event EventHandler<AudioInputEventArgs> SampleReady;
	}

	public static class AudioInputDevice
	{
		public static IAudioInputDevice DefaultDevice => Store.Get<IAudioInputDevice>();

		public static TimeSpan GetBufferDuration(this IAudioInputDevice device) =>
			TimeSpan.FromSeconds(device.SampleSize / device.SampleRate);

		public static double DefaultSampleRate = 22050d;

		public static readonly double[] StandardSampleRates =
		{
			04800d, 08000d,
			11025d, 16000d,
			22050d, 32000d,
			44100d, 48000d,
			88200d, 96000d,
			176400d, 192000d,
			352800d, // 2822400d, 5644800d,
		};
	}

	public class AudioInputEventArgs : EventArgs
	{
		public AudioInputEventArgs(ProcessingProfile source, Complex[] sample, double sampleRate)
		{
			Source = source;
			Sample = sample;
			SampleRate = sampleRate;
		}

		public ProcessingProfile Source { get; }
		public Complex[] Sample { get; }
		public double SampleRate { get; }
	}
}
