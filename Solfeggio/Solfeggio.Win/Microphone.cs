using System;
using Ace;
using Rainbow;
using Solfeggio.Api;

namespace Solfeggio
{
    class Microphone : SmartObject, IAudioInputDevice
    {
        public static readonly Microphone Default = new Microphone();

		public WaveFormat WaveFormat => new WaveFormat((int)SampleRate, 16, 1);
		public double[] SampleRates { get; } = {44100};
        public double SampleRate => 44100;
        public int SampleSize { get; private set; }
        public TimeSpan SampleDuration => TimeSpan.FromMilliseconds(processor.BufferMilliseconds);
		private Wave.In.Processor processor;

		public void StartWith(double sampleRate = default, int desiredFrameSize = default)
        {
            if (processor.Is())
            {
				Stop();
				processor.DataAvailable -= OnWiOnDataAvailable;
				processor.Dispose();
			}

			processor = new Wave.In.Processor(Wave.In.DefaultDevice.CreateSession(WaveFormat));

			var sampleSize = desiredFrameSize.Is(default) ? 4096 : desiredFrameSize;
            var actualSampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
            var milliseconds = 1000d * sampleSize / actualSampleRate;
            milliseconds = Math.Ceiling(milliseconds / 10) * 10;
			SampleSize = (int)(milliseconds * actualSampleRate / 1000d);
			processor.BufferMilliseconds = (int) milliseconds;
			processor.DataAvailable += OnWiOnDataAvailable;

            Start();
            EvokePropertyChanged(nameof(SampleSize));
            EvokePropertyChanged(nameof(SampleDuration));
        }

        private void OnWiOnDataAvailable(object sender, ProcessingEventArgs args)
        {
            var sampleSize = args.Bins.Length;
            var frame = new Complex[sampleSize];
            for (var i = 0; i < sampleSize; i++)
            {
				frame[i] = (args.Bins[i] + 0.5d) / short.MaxValue;
            }

            DataReady?.Invoke(this, new AudioInputEventArgs {Frame = frame, Source = this});
        }

		public void Start() => processor.Wake();
		public void Stop() => processor.Free();

		public event EventHandler<AudioInputEventArgs> DataReady;
    }
}