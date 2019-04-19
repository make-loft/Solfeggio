using System;

namespace Solfeggio.Api
{
	public enum ProcessingState
	{
		Hibernation,
		Processing,
		Suspending
	}

	interface IWaveState<T>
	{
		void Free();
		void Wake();
		void Lull();

		ProcessingState State { get; }

		void Init(IWaveProvider<T> waveProvider, int bufferSize);
	}

	public enum DirectionKind
	{
		In, Out
	}

	public interface IWavePosition
	{
		long GetPosition();
		WaveFormat WaveFormat { get; }
	}

	public interface IWaveProvider<T>
	{
		WaveFormat WaveFormat { get; }

		event EventHandler<WaveInEventArgs> DataAvailable;

		int Read(T[] buffer, int offset, int count);
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
