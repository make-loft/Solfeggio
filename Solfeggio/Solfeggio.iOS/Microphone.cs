using System;
using System.Linq;
using System.Threading.Tasks;
using Ace;
using AudioToolbox;
using Rainbow;

namespace Solfeggio.iOS
{
    public class Microphone : IAudioInputDevice
    {
        public static readonly Microphone Default = new Microphone();

        public bool IsRecodingState => _recorder.Is() && _recorder.IsRunning;

        private AudioQueueBuffer _bytes;
        private InputAudioQueue _recorder;

        public static double[] GetValidSampleRates() =>
            AudioInputDevice.StandardSampleRates.Where(IsValidSampleRate).ToArray();

        public static bool IsValidSampleRate(double frequency)
        {
            try
            {
	            new InputAudioQueue(AudioStreamBasicDescription.CreateLinearPCM(frequency));
				return true;
            }
            catch
            {
                return false;
            }
        }

        public double[] SampleRates { get; } = GetValidSampleRates();

        public double SampleRate => _recorder.Is() ? _recorder.SampleRate : double.NaN;

        public int SampleSize { get; private set; }

        public int MinFrameSize { get; private set; }

        public void StartWith(double sampleRate = 44100d, int desiredFrameSize = 0)
        {
            //if (_recorder.Is()) _recorder.Release();


		
			var sRate = (int) Math.Round(sampleRate);
            var minBufferSizeInBytes = desiredFrameSize;
            MinFrameSize = minBufferSizeInBytes / sizeof(short);
            var desiredBufferSize = sizeof(short) * desiredFrameSize;
            var bytesCount = desiredBufferSize < minBufferSizeInBytes ? minBufferSizeInBytes : desiredBufferSize;

	        _recorder = new InputAudioQueue(AudioStreamBasicDescription.CreateLinearPCM(SampleRate, 1));
	        _recorder.InputCompleted += (sender, args) =>
	        {
		        _bytes = args.Buffer;
	        };
            if (_recorder.AllocateBuffer(bytesCount, out IntPtr _).IsNot(AudioQueueStatus.Ok)) throw new Exception();


            SampleSize = bytesCount / 2;

            _recorder.Start();
            RunReadLooper();
        }

        private async void RunReadLooper()
        {
            while (IsRecodingState)
            {
				var currentRecorder = _recorder;
	            await Task.Delay(200);
				if (currentRecorder.IsNot(_recorder)) return;

	            var readLengthInBytes = _bytes.AudioDataByteSize;

				var frameSize = readLengthInBytes / sizeof(short);
                var frame = new Complex[frameSize];
                var shorts  = new byte[frameSize];
	            System.Runtime.InteropServices.Marshal.Copy(_bytes.AudioData, shorts, 0, (int)_bytes.AudioDataByteSize);
				for (var i = 0; i < frameSize; i++)
                {
                    frame[i] = shorts[i];
                }

                DataReady?.Invoke(this, new AudioInputEventArgs {Frame = frame, Source = this});
            }
        }

        public void Start()
        {
            if (_recorder.IsNot()) StartWith();
            if (IsRecodingState) return;
            _recorder.Start();
        }

        public void Stop() => _recorder.Stop(true);

        public event EventHandler<AudioInputEventArgs> DataReady;
    }
}