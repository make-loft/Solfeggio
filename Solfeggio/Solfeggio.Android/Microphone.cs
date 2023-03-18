using System;

using Ace;

using Android.Media;

using Solfeggio.Extensions;

namespace Solfeggio.Droid
{
	public class Microphone : AudioDevice<AudioRecord>
	{
		public static readonly Microphone Default = new();

		public bool IsReady => Device.Is() && Device.RecordingState.Is(RecordState.Recording);

		protected float[] _sample;

		public void Setup()
		{
			if (Device.Is()) Free();

			var sRate = (int)Math.Round(SampleRate);
			var minBufferSizeInBytes = AudioRecord.GetMinBufferSize(sRate, ChannelIn.Mono, Format);
			var desiredBufferSizeInBytes = sizeof(float) * DesiredFrameSize;
			var bytesCount = desiredBufferSizeInBytes < minBufferSizeInBytes ? minBufferSizeInBytes : desiredBufferSizeInBytes;

			_sample = new float[bytesCount / sizeof(float)];
			Device = new AudioRecord(AudioSource.Mic, sRate, ChannelIn.Mono, Format, bytesCount);
			if (Device.State.Is(State.Uninitialized)) throw new Exception("Can not access to a device microphone");
		}

		public override void Wake()
		{
			if (Device.IsNot())
				Setup();

			Device?.StartRecording();
			Source?.Tick();
		}

		public override void Lull() => Device.Stop();

		public override void Free()
		{
			try
			{
				Device?.Stop();
			}
			catch (Exception e)
			{
			}
			finally
			{
				Device?.Release();
				Device = default;
			}
		}

		public override float[] Next()
		{
			if (IsReady.Not())
				return default;

			try
			{
				var length = Device.Read(_sample, 0, _sample.Length, 0);
				var sample = _sample?.StretchArray(Level);

				EvokeDataAvailable(sample);

				return sample;
			}
			catch
			{
				return default;
			}
		}
	}
}