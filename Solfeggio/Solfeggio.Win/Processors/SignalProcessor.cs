using Ace;
using Rainbow;
using Solfeggio.Api;
using System;
using System.Diagnostics;

namespace Solfeggio.Processors
{
	public abstract class SignalProcessor : ContextObject, IAudioInputDevice
	{
		public WaveFormat WaveFormat => new WaveFormat((int)SampleRate, 16, 1);
		public double[] SampleRates { get; } = AudioInputDevice.StandardSampleRates;
		public double SampleRate { get; set; } = AudioInputDevice.DefaultSampleRate;
		public int SampleSize { get; protected set; }
		//public TimeSpan SampleDuration => TimeSpan.FromMilliseconds(inputProcessor.BufferMilliseconds);

		private IProcessor inputProcessor;
		private IProcessor outputProcessor;

		protected abstract IProcessor CreateInputProcessor();
		protected abstract IProcessor CreateOutputProcessor();

		public void StartWith(double sampleRate = default, int desiredFrameSize = default)
		{
			if (inputProcessor.Is())
			{
				inputProcessor.DataAvailable -= OnInputDataAvailable;
				Stop();
			}

			var sampleSize = desiredFrameSize.Is(default) ? 4096 : desiredFrameSize;
			var actualSampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
			var milliseconds = 1000d * sampleSize / actualSampleRate;
			milliseconds = Math.Ceiling(milliseconds / 10) * 10;
			SampleSize = (int)(milliseconds * actualSampleRate / 1000d);
			SampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
			inputProcessor = CreateInputProcessor();
			outputProcessor = CreateOutputProcessor();
			inputProcessor.DataAvailable += OnInputDataAvailable;

			Start();
			EvokePropertyChanged(nameof(SampleRate));
			EvokePropertyChanged(nameof(SampleSize));
		}

		private void OnInputDataAvailable(object sender, ProcessingEventArgs args)
		{
			_signal = args.Bins;
			var sampleSize = args.Bins.Length;
			var frame = new Complex[sampleSize];
			for (var i = 0; i < sampleSize; i++)
			{
				frame[i] = (args.Bins[i] + 0.5d) / short.MaxValue;
			}

			DataReady?.Invoke(this, new AudioInputEventArgs { Frame = frame, Source = this });
		}

		public event EventHandler<AudioInputEventArgs> DataReady;

		public void EvokeDataReady(Complex[] frame) => DataReady?.Invoke(this, new AudioInputEventArgs()
		{
			Frame = frame,
			Source = this
		});

		public short[] _signal;

		public void Start()
		{
			inputProcessor.Wake();
			outputProcessor.Wake();
			Debug.WriteLine($"Started {this}");
		}
		public void Stop()
		{
			inputProcessor.Free();
			outputProcessor.Free();
			Debug.WriteLine($"Stopped {this}");
		}

		public short[] Fill(in short[] buffer, int offset, int count)
		{
			var signal = _signal;
			if (signal.IsNot() || _signal.Length < count - offset) return default;

			for (var n = 0; n < offset; n++)
				buffer[n] = default;

			for (var i = offset; i < count; i++)
			{
				buffer[i] = signal[i];
			}

			for (var n = offset + count; n < buffer.Length; n++)
				buffer[n] = default;

			return buffer;
		}
	}
}
