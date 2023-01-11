using Android.Media;

using Solfeggio.Api;

using System;
using System.Linq;

namespace Solfeggio.Droid
{
	public abstract class AudioDevice<TDevice> : IProcessor
	{
		public event EventHandler<ProcessingEventArgs> DataAvailable;
		protected void EvokeDataAvailable(float[] sample) =>
			DataAvailable?.Invoke(this, new ProcessingEventArgs(this, sample));

		public Encoding Format { get; set; } = Encoding.PcmFloat;
		public double[] SampleRates => GetValidSampleRates(Format);
		public double SampleRate { get; set; } = 22050;
		public int DesiredFrameSize { get; set; } = 2048;
		public float Level { get; set; } = 1f;
		public float Boost { get; set; } = 1f;
		
		protected TDevice Device;

		public IProcessor Source { get; set; }

		public static double[] GetValidSampleRates(Encoding format) =>
			AudioInputDevice.StandardSampleRates.Where(f => IsValidSampleRate(f, format)).ToArray();

		public static bool IsValidSampleRate(double frequency, Encoding format)
		{
			try
			{
				var sRate = (int)Math.Round(frequency);
				
				var minTrackBufferSize = AudioTrack.GetMinBufferSize((int)frequency, ChannelOut.Mono, format);
				using (var r = new AudioTrack(Stream.Music, sRate, ChannelOut.Mono, format, minTrackBufferSize, AudioTrackMode.Stream))
					r.Release();

				var minRecordBufferSize = AudioRecord.GetMinBufferSize((int)frequency, ChannelIn.Mono, format);
				using (var r = new AudioRecord(AudioSource.Mic, sRate, ChannelIn.Mono, format, minRecordBufferSize))
					r.Release();
				
				return true;
			}
			catch
			{
				return false;
			}
		}

		public abstract void Wake();

		public abstract void Lull();

		public abstract void Free();

		public virtual void Tick() { }

		public abstract float[] Next();
	}
}