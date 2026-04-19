using Solfeggio.Api;
using Solfeggio.Extensions;

namespace Solfeggio.Processors;

abstract class ASoftwareSignalProcessor(int sampleRate, int sampleSize) : IProcessor
{
	public IProcessor Source { get; set; }
	public float Level { get; set; } = 1f;
	public int SampleRate { get; private set; } = sampleRate;
	public int SampleSize { get; private set; } = sampleSize;

	public event System.EventHandler<ProcessingEventArgs> DataAvailable;

	protected void EvokeDataAvailable(float[] sample) => DataAvailable?.Invoke(this, new(this, sample));

	public void Tick() => DataAvailable?.Invoke(this, new(this, Next().StretchArray(Level)));
	public abstract float[] Next();

	public void Free() { }
	public void Lull() { }
	public void Wake() { }
}
