using Ace;
using Rainbow;
using Solfeggio.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Solfeggio.Processors
{
	public abstract class SignalProcessor : ContextObject, IAudioInputDevice, IExposable, IDisposable
	{
		public WaveFormat WaveFormat => new WaveFormat((int)SampleRate, 16, 1);
		public double[] SampleRates { get; } = AudioInputDevice.StandardSampleRates;
		public double SampleRate { get; set; } = AudioInputDevice.DefaultSampleRate;
		public int SampleSize { get; protected set; }
		//public TimeSpan SampleDuration => TimeSpan.FromMilliseconds(inputProcessor.BufferMilliseconds);

		protected IProcessor inputProcessor;
		protected IProcessor outputProcessor;

		protected abstract IProcessor CreateInputProcessor();
		protected abstract IProcessor CreateOutputProcessor();

		public void Expose()
		{
			if (inputProcessor.Is() || SampleSize.Is(default) || SampleRate.Is(default))
				return;

			inputProcessor = CreateInputProcessor();
			outputProcessor = CreateOutputProcessor();

			inputProcessor.DataAvailable += OnInputDataAvailable;

			inputProcessor.Wake();
			outputProcessor.Wake();
		}

		public void Dispose()
		{
			if (inputProcessor.IsNot())
				return;

			inputProcessor.Free();
			outputProcessor.Free();

			inputProcessor.DataAvailable -= OnInputDataAvailable;

			outputProcessor = default;
			inputProcessor = default;
		}

		public void StartWith(double sampleRate = default, int desiredFrameSize = default)
		{
			var sampleSize = desiredFrameSize.Is(default) ? 4096 : desiredFrameSize;
			//var actualSampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
			//var milliseconds = 1000d * sampleSize / actualSampleRate;
			//milliseconds = Math.Ceiling(milliseconds / 10) * 10;
			//SampleSize = (int)(milliseconds * actualSampleRate / 1000d);
			SampleSize = sampleSize;
			SampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;

			Dispose();
			Expose();

			EvokePropertyChanged(nameof(SampleRate));
			EvokePropertyChanged(nameof(SampleSize));
		}

		private void OnInputDataAvailable(object sender, ProcessingEventArgs args)
		{
			var sampleSize = args.Bins.Length;
			var frame = new Complex[sampleSize];
			for (var i = 0; i < sampleSize; i++)
			{
				frame[i] = (args.Bins[i] + 0.5d) / short.MaxValue;
			}

			SampleReady?.Invoke(this, new AudioInputEventArgs { Sample = frame, Source = this });
		}

		public event EventHandler<AudioInputEventArgs> SampleReady;

		public void Start() => Expose();
		public void Stop() => Dispose();
	}
}
