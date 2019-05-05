using System;
using Rainbow;

namespace Solfeggio
{
	public interface IAudioInputDevice
	{
		double[] SampleRates { get; }
		double SampleRate { get; }
		int SampleSize { get; }

		void StartWith(double sampleRate = default, int desiredFrameSize = default);
		void Start();
		void Stop();
		event EventHandler<AudioInputEventArgs> SampleReady;
	}

	public static class AudioInputDevice
	{
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
			352800d, 2822400,
			5644800d,
		};
	}

	public class AudioInputEventArgs : EventArgs
	{
		public IAudioInputDevice Source { get; set; }
		public Complex[] Sample { get; set; }
	}
}
