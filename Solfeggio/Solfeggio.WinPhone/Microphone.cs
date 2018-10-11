using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Ace;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Rainbow;
using Solfeggio;
using Mic = Microsoft.Xna.Framework.Audio.Microphone;

namespace Solfeggio
{
	class Microphone : IAudioInputDevice
	{
		public static readonly Microphone Default = new Microphone();
		
		public double[] SampleRates { get; } = {(double) Mic.Default.SampleRate};
		public double SampleRate => Device.SampleRate;
		public int FrameSize { get; private set; }

		private Mic Device { get; } = Mic.Default;
		private byte[] _bytes;

		public void StartWith(double sampleRate = default, int desiredFrameSize = default)
		{
			Stop();
			FrameSize = desiredFrameSize.Is(default) ? 4096 : desiredFrameSize;
			var actualSampleRate = sampleRate.Is(default) ? Device.SampleRate : sampleRate;
			var milliseconds = 1000d * FrameSize / actualSampleRate;
			milliseconds = Math.Ceiling(milliseconds/100)*100;
			Device.BufferDuration = TimeSpan.FromMilliseconds(milliseconds);
			var bufferSize = Device.GetSampleSizeInBytes(Device.BufferDuration);
			_bytes = new byte[bufferSize];
			
			Device.BufferReady += (sender, args) =>
			{
				var sampleSizeInBytes = Device.GetData(_bytes);
				var sampleSize = sampleSizeInBytes/2;
				var frame = new Complex[sampleSize];
				for (var i = 0; i < sampleSize; i++)
				{
					frame[i] = Convert.ToDouble(BitConverter.ToInt16(_bytes, 2*i));
				}

				DataReady?.Invoke(this, new AudioInputEventArgs {Frame = frame, Source = this});
			};
			
			var timer = new DispatcherTimer();
			timer.Tick += (sender, args) => FrameworkDispatcher.Update();
			timer.Start();
			Start();
		}

		public void Start()
		{
			if (Device.State.Is(MicrophoneState.Started)) return;
			FrameworkDispatcher.Update();
			Device.Start();
		}

		public void Stop()
		{
			if (Device.State.Is(MicrophoneState.Stopped)) return;
			FrameworkDispatcher.Update();
			Device.Stop();
		}

		public event EventHandler<AudioInputEventArgs> DataReady;
	}
}
