using System;

namespace Solfeggio.Api;

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
	void Wake();
	void Lull();
	void Free();
	void Tick();
}

public class ProcessingEventArgs(IProcessor source, float[] sample) : EventArgs
{
	public IProcessor Source { get; } = source;
	public float[] Sample { get; } = sample;
}
