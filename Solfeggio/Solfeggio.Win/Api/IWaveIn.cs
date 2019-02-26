using System;

namespace Solfeggio.Api
{
    public interface IWaveIn : IDisposable
    {
        WaveFormat WaveFormat { get; set; }

        void StartRecording();
        void StopRecording();

        event EventHandler<WaveInEventArgs> DataAvailable;
        event EventHandler<StoppedEventArgs> RecordingStopped;
    }
    
    public class WaveInEventArgs : EventArgs
    {
        public WaveInEventArgs(short[] buffer, int binsCount)
        {
            Bins = buffer;
			BinsCount = binsCount;
        }

        public short[] Bins { get; }
        public int BinsCount { get; }
    }
    
    public class StoppedEventArgs : EventArgs
    {
        public StoppedEventArgs(Exception exception = null) => Exception = exception;

        public Exception Exception { get; }
    }
}
