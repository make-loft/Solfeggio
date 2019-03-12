using System;
using Ace;
using Rainbow;
using Solfeggio.Api;

namespace Solfeggio
{
    class Microphone : SmartObject, IAudioInputDevice
    {
        public static readonly Microphone Default = new Microphone();

        public double[] SampleRates { get; } = {(double) 44100};
        public double SampleRate => 44100;
        public int SampleSize { get; private set; }
        public TimeSpan SampleDuration => TimeSpan.FromMilliseconds(_wi.BufferMilliseconds);
        private WaveIn _wi;
        
        public void StartWith(double sampleRate = default, int desiredFrameSize = default)
        {
            if (_wi.Is())
            {
                Stop();
                _wi.DataAvailable -= OnWiOnDataAvailable;
                _wi.Dispose();
            }
            
            _wi = new WaveIn {DeviceNumber = 0, WaveFormat = new WaveFormat((int) SampleRate, 1)};
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
                frame[i] = (double) args.Bins[i] / short.MaxValue;
            }

            DataReady?.Invoke(this, new AudioInputEventArgs {Frame = frame, Source = this});
        }

        private bool _inRecording;

        public void Start()
        {
            if (_inRecording.Is(true)) return;
            _wi.StartRecording();
            _inRecording = true;
        }

        public void Stop()
        {
            if (_inRecording.Is(false)) return;
            _wi.StopRecording();
            _inRecording = false;
        }

        public event EventHandler<AudioInputEventArgs> DataReady;
    }
}