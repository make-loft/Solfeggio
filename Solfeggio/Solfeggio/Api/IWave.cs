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
		short[] Next();

		event EventHandler<ProcessingEventArgs> DataAvailable;
	}

	public interface IProcessor : IDataSource
	{
		IProcessor Source { get; set; }
		double Level { get; set; }
		double Boost { get; set; }
		void Wake();
		void Lull();
		void Free();
		void Tick();
	}

	public class ProcessingEventArgs : EventArgs
	{
		public ProcessingEventArgs(IProcessor source, short[] buffer)
		{
			Source = source;
			Bins = buffer;
		}

		public IProcessor Source { get; }
		public short[] Bins { get; }
	}
}
