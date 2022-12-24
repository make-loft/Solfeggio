using System;
using System.Linq;
using Ace;
using Android.Media;
using Rainbow;
using Solfeggio.Api;
using Encoding = Android.Media.Encoding;

namespace Solfeggio.Droid
{
    public class Speaker : IProcessor
    {
        public static readonly Speaker Default = new Speaker();

	    public int SampleSize { get; private set; }
	    public int MinFrameSize { get; private set; }
	    public double[] SampleRates { get; } = GetValidSampleRates();
	    public double SampleRate => _track.Is() ? _track.SampleRate : double.NaN;
		public bool IsPlayingState => _track.Is() && _track.PlayState.Is(PlayState.Playing);

		public double Level { get; set; }
        public double Boost { get; set; } = 0.20;

		private short[] _shorts;
        private AudioTrack _track;

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
            if (_track.Is()) _track.Release();

            var sRate = sampleRate.Is(default) ? 22050 : (int) Math.Round(sampleRate);
            var minBufferSizeInBytes = AudioRecord.GetMinBufferSize(sRate, ChannelIn.Mono, Encoding.Pcm16bit);
            MinFrameSize = minBufferSizeInBytes / sizeof(short);
            var desiredBufferSize = sizeof(short) * desiredFrameSize;
            var bytesCount = desiredBufferSize < minBufferSizeInBytes ? minBufferSizeInBytes : desiredBufferSize;

            _shorts = new short[bytesCount / sizeof(short)];
            _track = new AudioTrack(Stream.Music, sRate, ChannelOut.Mono, Encoding.Pcm16bit, bytesCount, AudioTrackMode.Stream);
            if (_track.State.Is(AudioTrackState.Uninitialized)) throw new Exception("Can not access to a device microphone");

            SampleSize = bytesCount / 2;

            //_track.Play();
            //RunReadLooper();
            Source.DataAvailable += async (o, e) =>
            {
                _shorts = e.Bins;
                var currentWriter = _track;
                var readLengthInShorts = await currentWriter.WriteAsync(_shorts, 0, _shorts.Length);
                if (currentWriter.IsNot(_track)) return;
                
                DataAvailable?.Invoke(this, new ProcessingEventArgs(this, _shorts.Scale(Boost), _shorts.Length));
            };
        }

        public IProcessor Source { get; set; }

        private async void RunReadLooper()
        {
            while (IsPlayingState)
            {
                var currentWriter = _track;
                var readLengthInShorts = await currentWriter.WriteAsync(_shorts, 0, _shorts.Length);
                if (currentWriter.IsNot(_track)) return;

				DataAvailable?.Invoke(this, new ProcessingEventArgs(this, _shorts.Scale(0.2), _shorts.Length));
			}
		}

        public void Wake()
        {
            if (_track.IsNot()) StartWith();
            if (IsPlayingState) return;

            if (_track.State.Is(AudioTrackState.Uninitialized))
                StartWith();

            //if (_track.State.Is(AudioTrackState.Initialized))
            //    _track.Play();
        }

        public void Lull() => _track.Stop();

		public event EventHandler<ProcessingEventArgs> DataAvailable;

		public override string ToString() => "Speaker";

        public void Free() => _track.Release();

		public void Tick() => throw new NotImplementedException();

		public short[] Next() => throw new NotImplementedException();
	}
}