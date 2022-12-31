using System;
using System.Collections.Generic;
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

        public async void StartWith(double sampleRate = default, int desiredFrameSize = 2048)
        {
            if (_track.Is()) _track.Release();

            var sRate = sampleRate.Is(default) ? 22050 : (int) Math.Round(sampleRate);
            var minBufferSizeInBytes = AudioRecord.GetMinBufferSize(sRate, ChannelIn.Mono, Encoding.Pcm16bit);
            MinFrameSize = minBufferSizeInBytes / sizeof(short);
            var desiredBufferSize = sizeof(short) * desiredFrameSize;
            var bytesCount = desiredBufferSize < minBufferSizeInBytes ? minBufferSizeInBytes : desiredBufferSize;

            //_shorts = new short[bytesCount / sizeof(short)];
            _track = new AudioTrack(Stream.Music, sRate, ChannelOut.Mono, Encoding.Pcm16bit, bytesCount, AudioTrackMode.Stream);
            if (_track.State.Is(AudioTrackState.Uninitialized)) throw new Exception("Can not access to a device microphone");

            SampleSize = bytesCount / 2;

			Source.DataAvailable += Source_DataAvailable;

            ThreadPool.QueueUserWorkItem(o => StartWriteLoop());
        }

		private void Source_DataAvailable(object sender, ProcessingEventArgs e)
		{
            _sample = e.Bins.Scale(0.1);
        }

		short[] _sample;

        public IProcessor Source { get; set; }

        private async void StartWriteLoop()
        {
            var track = _track;

            Loop:

            if (IsPlayingState.Not())
            {
                await Task.Delay(8);
                goto Loop;
            }

            if (_track.IsNot(track))
                return;

            var sample = _sample;
            if (sample.Is())
            {
                _sample = default;
                var length = track.Write(sample, 0, sample.Length);
                Source.Tick();
            }

            DataAvailable?.Invoke(this, new ProcessingEventArgs(this, sample));
            goto Loop;
        }

        public void Wake()
        {
            if (_track.IsNot()) StartWith();
            if (IsPlayingState) return;

            if (_track.State.Is(AudioTrackState.Uninitialized))
                StartWith();

            if (_track.State.Is(AudioTrackState.Initialized))
                _track.Play();

            Source.Tick();
        }

        public void Lull() => _track.Stop();

		public event EventHandler<ProcessingEventArgs> DataAvailable;

		public override string ToString() => "Speaker";

        public void Free()
        {
            try
            {
                Source.DataAvailable -= Source_DataAvailable;
                _track.Stop();
            }
            catch (Exception e)
            {
            }
            finally
			{
                _track.Release();
                _track = default;
            }
        }

		public void Tick() => throw new NotImplementedException();

		public short[] Next() => throw new NotImplementedException();
	}
}