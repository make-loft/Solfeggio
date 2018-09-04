using System;
using Android.Media;
using Encoding = Android.Media.Encoding;

namespace Solfeggio.Droid
{
    public class Microphone : IMicrophone
    {
        public static readonly Microphone Default = new Microphone();

        private bool _isStarted;
        private byte[] _buffer;
        private AudioRecord _recorder;

        public int SampleRate { get; set; } = 16000;

        public Microphone() => Initialize();

        public void Initialize()
        {
            try
            {
                var bufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelIn.Mono, Encoding.Pcm16bit);
                _buffer = new byte[bufferSize];
                _recorder = new AudioRecord(AudioSource.Mic, SampleRate, ChannelIn.Mono, Encoding.Pcm16bit, _buffer.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async void InLoop()
        {
            while (_isStarted)
            {
                var readLength = await _recorder.ReadAsync(_buffer, 0, _buffer.Length);
	            DataReady?.Invoke(this, new DataReadyEventArgs {Buffer = _buffer, ReadLength = readLength, Source = this});
            }
        }

        public void Start()
        {
            _recorder.StartRecording();
            _isStarted = true;
            InLoop();
        }

        public void Stop()
        {
            _recorder.Stop();
            _isStarted = false;
        }

        public event EventHandler<DataReadyEventArgs> DataReady;
    }
}