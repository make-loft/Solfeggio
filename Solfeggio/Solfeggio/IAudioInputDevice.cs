using System;

namespace Solfeggio
{
	public interface IAudioInputDevice
	{
		int SampleRate { get; set; }
		int BufferSize { get; }

		void Start();
		void Stop();
		event EventHandler<AudioInputEventArgs> DataReady;
	}

	public class AudioInputEventArgs : EventArgs
	{
		public IAudioInputDevice Source { get; set; }
		public byte[] Buffer { get; set; }
		public int ReadLength { get; set; }
	}
}
