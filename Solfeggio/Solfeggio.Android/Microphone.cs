using System;
using System.Linq;
using Ace;
using Android.Media;
using Java.Nio;
using Rainbow;
using Encoding = Android.Media.Encoding;

namespace Solfeggio.Droid
{
    public class Microphone : IAudioInputDevice
    {
        public static readonly Microphone Default = new Microphone();

	    public int FrameSize { get; private set; }
	    public int MinFrameSize { get; private set; }
	    public double[] SampleRates { get; } = GetValidSampleRates();
	    public double SampleRate => _recorder.Is() ? _recorder.SampleRate : double.NaN;
		public bool IsRecodingState => _recorder.Is() && _recorder.RecordingState.Is(RecordState.Recording);

        private ByteBuffer _bytes;
        private AudioRecord _recorder;

        public static double[] GetValidSampleRates() =>
            AudioInputDevice.StandardSampleRates.Where(IsValidSampleRate).ToArray();

        public static bool IsValidSampleRate(double frequency)
        {
            try
            {
                var sRate = (int) Math.Round(frequency);
                var minBufferSize = AudioRecord.GetMinBufferSize((int) frequency, ChannelIn.Mono, Encoding.Pcm16bit);
                new AudioRecord(AudioSource.Mic, sRate, ChannelIn.Mono, Encoding.Pcm16bit, minBufferSize).Release();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void StartWith(double sampleRate = default, int desiredFrameSize = default)
        {
            if (_recorder.Is()) _recorder.Release();

            var sRate = sampleRate.Is(default) ? 48000 : (int) Math.Round(sampleRate);
            var minBufferSizeInBytes = AudioRecord.GetMinBufferSize(sRate, ChannelIn.Mono, Encoding.Pcm16bit);
            MinFrameSize = minBufferSizeInBytes / sizeof(short);
            var desiredBufferSize = sizeof(short) * desiredFrameSize;
            var bytesCount = desiredBufferSize < minBufferSizeInBytes ? minBufferSizeInBytes : desiredBufferSize;

            _bytes = ByteBuffer.AllocateDirect(bytesCount).Order(ByteOrder.NativeOrder());
            _recorder = new AudioRecord(AudioSource.Mic, sRate, ChannelIn.Mono, Encoding.Pcm16bit, _bytes.Capacity());
            if (_recorder.State.Is(State.Uninitialized)) throw new Exception();

            FrameSize = bytesCount / 2;

            _recorder.StartRecording();
            RunReadLooper();
        }

        private async void RunReadLooper()
        {
            while (IsRecodingState)
            {
                var currentRecorder = _recorder;
                var readLengthInBytes = await currentRecorder.ReadAsync(_bytes, _bytes.Capacity());
                if (currentRecorder.IsNot(_recorder)) return;

                var frameSize = readLengthInBytes / sizeof(short);
                var frame = new Complex[frameSize];
                var shorts = _bytes.AsShortBuffer();
                for (var i = 0; i < frameSize; i++)
                {
                    frame[i] = shorts.Get();
                }

                DataReady?.Invoke(this, new AudioInputEventArgs {Frame = frame, Source = this});
            }
        }

        public void Start()
        {
            if (_recorder.IsNot()) StartWith();
            if (IsRecodingState) return;
            _recorder.StartRecording();
        }

        public void Stop() => _recorder.Stop();

        public event EventHandler<AudioInputEventArgs> DataReady;
    }
}