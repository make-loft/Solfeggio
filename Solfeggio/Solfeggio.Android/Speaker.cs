using System;
using System.Threading;
using System.Threading.Tasks;

using Ace;

using Android.Media;

using Solfeggio.Api;

namespace Solfeggio.Droid
{
	public class Speaker : AudioDevice<AudioTrack>
	{
		public static readonly Speaker Default = new Speaker();

		public bool IsReady => Device.Is() && Device.PlayState.Is(PlayState.Playing);

		public void Setup()
		{
			if (Device.Is()) Free();

			var sRate = SampleRate.Is(default) ? 22050 : (int)Math.Round(SampleRate);
			var minBufferSizeInBytes = AudioTrack.GetMinBufferSize(sRate, ChannelOut.Mono, Format);
			var desiredBufferSizeInBytes = sizeof(float) * DesiredFrameSize;
			var bytesCount = desiredBufferSizeInBytes < minBufferSizeInBytes ? minBufferSizeInBytes : desiredBufferSizeInBytes;

			Device = new AudioTrack(Stream.Music, sRate, ChannelOut.Mono, Format, bytesCount, AudioTrackMode.Stream);
			if (Device.State.Is(AudioTrackState.Uninitialized)) throw new Exception("Can not access to a device microphone");

			ThreadPool.QueueUserWorkItem(o => StartWriteLoop());
		}

		private async void StartWriteLoop()
		{
			var track = Device;

			Loop:

			if (IsReady.Not())
			{
				await Task.Delay(8);
				goto Loop;
			}

			if (Device.IsNot(track))
				return;

			var sample = Source.Next()?.StretchArray(Level * Boost);
			if (sample.Is())
			{
				var length = track.Write(sample, 0, sample.Length, WriteMode.Blocking);
				EvokeDataAvailable(sample);
			}

			goto Loop;
		}

		public override void Wake()
		{
			if (Device.IsNot())
				Setup();

			Device?.Play();
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

		public override float[] Next() => Source?.Next();
	}
}