using System;

namespace Solfeggio.Api
{
	public enum ProcessingState
	{
		Hibernation,
		Processing,
		Suspending
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
