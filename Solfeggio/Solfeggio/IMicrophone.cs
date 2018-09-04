using System;

namespace Solfeggio
{
	public interface IMicrophone
	{
		int SampleRate { get; set; }

		void Start();
		void Stop();
		event EventHandler<DataReadyEventArgs> DataReady;
	}

	public class DataReadyEventArgs : EventArgs
	{
		public byte[] Buffer { get; set; }
		public int ReadLength { get; set; }
		public IMicrophone Source { get; set; }
	}
}
