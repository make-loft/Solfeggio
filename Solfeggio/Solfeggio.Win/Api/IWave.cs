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

		void Init(IDataSource<T> dataSource, int bufferSize);
	}

	public interface IDataSource<T>
	{
		WaveFormat WaveFormat { get; }

		int SampleSize { get; }

		T[] Fill(in T[] buffer, int offset, int count);
	}

	public class ProcessingEventArgs : EventArgs
	{
		public ProcessingEventArgs(short[] buffer, int binsCount)
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
