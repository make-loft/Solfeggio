using Solfeggio.Api;

namespace Solfeggio.Processors
{
	abstract class ASoftwareSignalProcessor : IProcessor
	{
		public IProcessor Source { get; set; }
		public float Level { get; set; } = 1f;
		public float Boost { get; set; } = 1f;
		public int SampleRate { get; private set; }
		public int SampleSize { get; private set; }

		public event System.EventHandler<ProcessingEventArgs> DataAvailable;

		protected void EvokeDataAvailable(float[] sample) => DataAvailable?.Invoke(this, new(this, sample));

		public ASoftwareSignalProcessor(int sampleRate, int sampleSize)
		{
			SampleRate = sampleRate;
			SampleSize = sampleSize;
		}

		public void Tick() => DataAvailable?.Invoke(this, new(this, Next().StretchArray(Level * Boost)));
		public abstract float[] Next();

		public void Free() { }
		public void Lull() { }
		public void Wake() { }
	}
}
