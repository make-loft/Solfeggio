using System;
using Android.Media;
using Encoding = Android.Media.Encoding;

namespace Solfeggio.Droid
{
    public class Microphone : IAudioInputDevice
    {
        public static readonly Microphone Default = new Microphone();

        private bool _isStarted;
        private byte[] _buffer;
        private AudioRecord _recorder;

        public int SampleRate { get; set; } = 16000;
	    public int BufferSize { get; private set; }
	    public TimeSpan BufferDuration => TimeSpan.FromSeconds((float) BufferSize / SampleRate);

		public Microphone() => Initialize();

        public void Initialize()
        {
            try
            {
                BufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
                _buffer = new byte[BufferSize];
                _recorder = new AudioRecord(AudioSource.Mic, SampleRate, ChannelIn.Mono, Encoding.Pcm16bit, _buffer.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async void StartDataLooper()
        {
            while (_isStarted)
            {
                var readLength = await _recorder.ReadAsync(_buffer, 0, _buffer.Length);
	            DataReady?.Invoke(this, new AudioInputEventArgs {Buffer = _buffer, ReadLength = readLength, Source = this});
            }
        }

        public void Start()
        {
            _recorder.StartRecording();
            _isStarted = true;
            StartDataLooper();
        }

        public void Stop()
        {
            _recorder.Stop();
            _isStarted = false;
        }

        public event EventHandler<AudioInputEventArgs> DataReady;
    }
}