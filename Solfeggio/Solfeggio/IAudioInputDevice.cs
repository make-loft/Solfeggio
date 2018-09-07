using System;
using Rainbow;

namespace Solfeggio
{
	public interface IAudioInputDevice
	{
	    double[] SampleRates { get; }
        double SampleRate { get; }
		int FrameSize { get; }

		void StartWith(double sampleRate = 16000, int desiredFrameSize = 0);
		void Start();
		void Stop();
		event EventHandler<AudioInputEventArgs> DataReady;
	}

	public static class AudioInputDevice
	{
		public static TimeSpan GetBufferDuration(this IAudioInputDevice device) =>
			TimeSpan.FromSeconds(device.FrameSize / device.SampleRate);

	    public static readonly double[] StandardSampleRates =
	    {
	        5644800, 2822400, 352800, 192000, 176400, 96000,
	        88200, 50400, 50000, 48000, 47250, 44100, 44056, 37800, 32000, 22050, 16000, 11025, 8000, 4800
        };
    }

	public class AudioInputEventArgs : EventArgs
	{
		public IAudioInputDevice Source { get; set; }
		public Complex[] Frame { get; set; }
	}
}
