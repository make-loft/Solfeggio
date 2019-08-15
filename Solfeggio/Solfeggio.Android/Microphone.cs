using System;
using System.Linq;
using Ace;
using Android.Media;
using Java.Nio;
using Rainbow;
using Solfeggio.Api;
using Encoding = Android.Media.Encoding;

namespace Solfeggio.Droid
{
    public class Microphone : IProcessor
    {
        public static readonly Microphone Default = new Microphone();

	    public int SampleSize { get; private set; }
	    public int MinFrameSize { get; private set; }
	    public double[] SampleRates { get; } = GetValidSampleRates();
	    public double SampleRate => _recorder.Is() ? _recorder.SampleRate : double.NaN;
		public bool IsRecodingState => _recorder.Is() && _recorder.RecordingState.Is(RecordState.Recording);

		public float Level { get; set; }
		public double Boost { get; set; } = 8;

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
				using (var r = new AudioRecord(AudioSource.Mic, sRate, ChannelIn.Mono, Encoding.Pcm16bit, minBufferSize))
					r.Release();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void StartWith(double sampleRate = default, int desiredFrameSize = 2048)
        {
            if (_recorder.Is()) _recorder.Release();

            var sRate = sampleRate.Is(default) ? 22050 : (int) Math.Round(sampleRate);
            var minBufferSizeInBytes = AudioRecord.GetMinBufferSize(sRate, ChannelIn.Mono, Encoding.Pcm16bit);
            MinFrameSize = minBufferSizeInBytes / sizeof(short);
            var desiredBufferSize = sizeof(short) * desiredFrameSize;
            var bytesCount = desiredBufferSize < minBufferSizeInBytes ? minBufferSizeInBytes : desiredBufferSize;

            _bytes = ByteBuffer.AllocateDirect(bytesCount).Order(ByteOrder.NativeOrder());
            _recorder = new AudioRecord(AudioSource.Mic, sRate, ChannelIn.Mono, Encoding.Pcm16bit, _bytes.Capacity());
            if (_recorder.State.Is(State.Uninitialized)) throw new Exception();

            SampleSize = bytesCount / 2;

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
                var frame = new short[frameSize];
                var shorts = _bytes.AsShortBuffer();
                for (var i = 0; i < frameSize; i++)
                {
                    frame[i] = shorts.Get();
                }

				DataAvailable?.Invoke(this, new ProcessingEventArgs(this, frame.Scale(2), frameSize));
			}
		}

        public void Wake()
        {
            if (_recorder.IsNot()) StartWith();
            if (IsRecodingState) return;
            _recorder.StartRecording();
        }

        public void Lull() => _recorder.Stop();

		public event EventHandler<ProcessingEventArgs> DataAvailable;

		public override string ToString() => "Microphone";

		public void Free()
		{
			throw new NotImplementedException();
		}

		public void Tick()
		{
			throw new NotImplementedException();
		}

		public short[] Next()
		{
			throw new NotImplementedException();
		}
	}
}