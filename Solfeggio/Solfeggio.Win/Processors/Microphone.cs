using System;
using System.Diagnostics;
using Ace;
using Rainbow;
using Solfeggio.Api;

namespace Solfeggio.Processors
{
	class Microphone : SmartObject, IAudioInputDevice, IDataSource<short>
	{
		public static readonly Microphone Default = new Microphone();

		public WaveFormat WaveFormat => new WaveFormat((int)SampleRate, 16, 1);
		public double[] SampleRates { get; } = AudioInputDevice.StandardSampleRates;
		public double SampleRate { get; set; } = AudioInputDevice.DefaultSampleRate;
		public int SampleSize { get; private set; }
		public TimeSpan SampleDuration => TimeSpan.FromMilliseconds(inputProcessor.BufferMilliseconds);
		private Wave.In.Processor inputProcessor;
		private Wave.Out.Processor outputProcessor;

		public void StartWith(double sampleRate = default, int desiredFrameSize = default)
		{
			if (inputProcessor.Is())
			{
				Stop();
				inputProcessor.DataAvailable -= OnWiOnDataAvailable;
			}

			SampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
			inputProcessor = new Wave.In.Processor(Wave.In.DefaultDevice.CreateSession(WaveFormat));
			outputProcessor = new Wave.Out.Processor(Wave.Out.DefaultDevice.CreateSession(WaveFormat), this);

			var sampleSize = desiredFrameSize.Is(default) ? 4096 : desiredFrameSize;
			var actualSampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
			var milliseconds = 1000d * sampleSize / actualSampleRate;
			milliseconds = Math.Ceiling(milliseconds / 10) * 10;
			SampleSize = (int)(milliseconds * actualSampleRate / 1000d);
			inputProcessor.BufferMilliseconds = (int)milliseconds;
			inputProcessor.DataAvailable += OnWiOnDataAvailable;

			Start();
			EvokePropertyChanged(nameof(SampleSize));
			EvokePropertyChanged(nameof(SampleDuration));
		}

		private void OnWiOnDataAvailable(object sender, ProcessingEventArgs args)
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

		private short[] _signal;

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
			if (signal.IsNot() || _signal.Length < count - offset) return new short[0];

			for (var n = 0; n < offset; n++)
				buffer[n] = default;

			for (var i = offset; i < count; i++)
			{
				//var j = _binSize * (offset + i);
				buffer[i] = signal[i];
			}

			for (var n = offset + count; n < buffer.Length; n++)
				buffer[n] = default;

			return buffer;
		}

		public event EventHandler<AudioInputEventArgs> DataReady;
		public event EventHandler<ProcessingEventArgs> DataProcessed;
	}
}