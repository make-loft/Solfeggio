using System;
using Ace;
using Rainbow;
using Solfeggio.Api;

namespace Solfeggio
{
    class Microphone : SmartObject, IAudioInputDevice
    {
        public static readonly Microphone Default = new Microphone();

        public double[] SampleRates { get; } = {44100};
        public double SampleRate => 44100;
        public int SampleSize { get; private set; }
        public TimeSpan SampleDuration => TimeSpan.FromMilliseconds(_wi.BufferMilliseconds);
        private readonly Wave _wi = new Wave(DirectionKind.In);
        
        public void StartWith(double sampleRate = default, int desiredFrameSize = default)
        {
            if (_wi.Is())
            {
				Stop();
				_wi.DataAvailable -= OnWiOnDataAvailable;
                _wi.Dispose();
			}

			_wi.With(_wi.DeviceNumber = 0, _wi.WaveFormat = new WaveFormat((int)SampleRate, 1));
            var sampleSize = desiredFrameSize.Is(default) ? 4096 : desiredFrameSize;
            var actualSampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
            var milliseconds = 1000d * sampleSize / actualSampleRate;
            milliseconds = Math.Ceiling(milliseconds / 10) * 10;
			SampleSize = (int)(milliseconds * actualSampleRate / 1000d);
            _wi.BufferMilliseconds = (int) milliseconds;
            _wi.DataAvailable += OnWiOnDataAvailable;

            Start();
            EvokePropertyChanged(nameof(SampleSize));
            EvokePropertyChanged(nameof(SampleDuration));
        }

        private void OnWiOnDataAvailable(object sender, WaveInEventArgs args)
        {
            var sampleSize = args.Bins.Length;
            var frame = new Complex[sampleSize];
            for (var i = 0; i < sampleSize; i++)
            {
				frame[i] = (args.Bins[i] + 0.5d) / short.MaxValue;
            }

            DataReady?.Invoke(this, new AudioInputEventArgs {Frame = frame, Source = this});
        }

		public void Start() => _wi.Wake();
		public void Stop() => _wi.Free();

		public event EventHandler<AudioInputEventArgs> DataReady;
    }
}