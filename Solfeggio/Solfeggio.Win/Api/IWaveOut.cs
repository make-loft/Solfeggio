using System;

namespace Solfeggio.Api
{
	interface IWaveOut
	{
		void Play();
		void Stop();
		void Pause();
		void Init(IWaveProvider waveProvider, int bufferSize);

		PlaybackState PlaybackState { get; }
		float Volume { get; set; }
	}

	public interface IWavePosition
	{
		long GetPosition();
		WaveFormat OutputWaveFormat { get; }
	}

	public interface IWaveProvider
	{
		WaveFormat WaveFormat { get; }
		int Read(byte[] buffer, int offset, int count);
	}

	public enum PlaybackState
	{
		Stopped,
		Playing,
		Paused
	}
}
