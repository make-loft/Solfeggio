using System;
using System.Windows.Threading;
using Ace;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Rainbow;
using Solfeggio.Api;

namespace Solfeggio
{
    class Microphone : IAudioInputDevice
    {
        public static readonly Microphone Default = new Microphone();
                
        private WaveIn wi = new WaveIn();
		
        public double[] SampleRates { get; } = {(double) 44100};
        public double SampleRate => 44100;
        public int FrameSize { get; private set; }

        //private byte[] _bytes;

        public void StartWith(double sampleRate = default, int desiredFrameSize = default)
        {
            Stop();
            wi.DeviceNumber = 0;
            wi.WaveFormat = new WaveFormat((int)SampleRate, 1);
            FrameSize = desiredFrameSize.Is(default) ? 4096 : desiredFrameSize;
            var actualSampleRate = sampleRate.Is(default) ? SampleRate : sampleRate;
            var milliseconds = 1000d * FrameSize / actualSampleRate;
            milliseconds = Math.Ceiling(milliseconds/100)*100;
            //Device.BufferDuration = TimeSpan.FromMilliseconds(milliseconds);
            //var bufferSize = Device.GetSampleSizeInBytes(Device.BufferDuration);
            //_bytes = new byte[bufferSize];
			
            wi.DataAvailable += (sender, args) =>
            {
                var sampleSizeInBytes = args.Buffer.Length;
                var sampleSize = sampleSizeInBytes/2;
                var frame = new Complex[sampleSize];
                for (var i = 0; i < sampleSize; i++)
                {
                    frame[i] = args.Buffer[i];
                }

                DataReady?.Invoke(this, new AudioInputEventArgs {Frame = frame, Source = this});
            };
			
            var timer = new DispatcherTimer();
            timer.Tick += (sender, args) => FrameworkDispatcher.Update();
            timer.Start();
            Start();
        }

        private bool inRecording;
        
        public void Start()
        {
            if (inRecording.Is(true)) return;
            wi.StartRecording();
            inRecording = true;
        }

        public void Stop()
        {
            if (inRecording.Is(false)) return;
            wi.StartRecording();
            inRecording = false;
        }

        public event EventHandler<AudioInputEventArgs> DataReady;
    }
}