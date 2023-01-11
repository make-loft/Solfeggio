using System;

namespace Solfeggio.Api
{
	public enum ProcessingState
	{
		Hibernation,
		Processing,
		Suspending
	}

	public interface IDataSource
	{
		float[] Next();

		event EventHandler<ProcessingEventArgs> DataAvailable;
	}

	public interface IProcessor : IDataSource
	{
		IProcessor Source { get; set; }
		float Level { get; set; }
		float Boost { get; set; }
		void Wake();
		void Lull();
		void Free();
		void Tick();
	}

	public class ProcessingEventArgs : EventArgs
	{
		public ProcessingEventArgs(IProcessor source, float[] sample)
		{
			Source = source;
			Sample = sample;
		}

		public IProcessor Source { get; }
		public float[] Sample { get; }
	}
}
