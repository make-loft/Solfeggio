using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Ace;
using Android.Media;
using Rainbow;
using Solfeggio.Api;
using Encoding = Android.Media.Encoding;

namespace Solfeggio.Droid
{
    public class Microphone : IProcessor
    {
        public IProcessor Source { get; set; }

        public static readonly Microphone Default = new Microphone();

        public int SampleSize { get; private set; }
        public int MinFrameSize { get; private set; }
        public double[] SampleRates { get; } = GetValidSampleRates();
        public double SampleRate { get; set; } = 22050;

        public bool IsRecodingState => _recorder.Is() && _recorder.RecordingState.Is(RecordState.Recording);

        public double Level { get; set; }
        public double Boost { get; set; } = 1d;

        private short[] _sample;
        private AudioRecord _recorder;

        public static double[] GetValidSampleRates() =>
            AudioInputDevice.StandardSampleRates.Where(IsValidSampleRate).ToArray();

        public static bool IsValidSampleRate(double frequency)
        {
            try
            {
                var sRate = (int)Math.Round(frequency);
                var minBufferSize = AudioRecord.GetMinBufferSize((int)frequency, ChannelIn.Mono, Encoding.Pcm16bit);
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

            var sRate = (int)Math.Round(SampleRate);
            var minBufferSizeInBytes = AudioRecord.GetMinBufferSize(sRate, ChannelIn.Mono, Encoding.Pcm16bit);
            MinFrameSize = minBufferSizeInBytes / sizeof(short);
            var desiredBufferSize = sizeof(short) * desiredFrameSize;
            var bytesCount = desiredBufferSize < minBufferSizeInBytes ? minBufferSizeInBytes : desiredBufferSize;

            _sample = new short[bytesCount / sizeof(short)];
            _recorder = new AudioRecord(AudioSource.Mic, sRate, ChannelIn.Mono, Encoding.Pcm16bit, bytesCount);
            if (_recorder.State.Is(State.Uninitialized)) throw new Exception("Can not access to a device microphone");

            SampleSize = bytesCount / 2;

            ThreadPool.QueueUserWorkItem(o => StartReadLoop());
        }

        private async void StartReadLoop()
        {
            var recorder = _recorder;

            Loop:
            
            if (IsRecodingState.Not())
            {
                await Task.Delay(8);
                goto Loop;
            }

            if (_recorder.IsNot(recorder))
                return;

            var length = _recorder.Read(_sample, 0, _sample.Length);

            DataAvailable?.Invoke(this, new ProcessingEventArgs(this, _sample));
            goto Loop;
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

        public void Free() => _recorder.Release();

        public void Tick() { }

		public short[] Next() => throw new NotImplementedException();
	}
}