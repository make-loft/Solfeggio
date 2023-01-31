using Rainbow;

using Solfeggio.Api;
using Solfeggio.Extensions;

using System;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace Solfeggio.Processors
{
	public class AudioStreamReader : Wave.In.DeviceInfo
	{
		public AudioStreamReader() : base(int.MinValue) { }

		public override string ProductName => "Audio Stream Reader";

		public override WaveInCapabilities GetCapabilities() => throw new NotImplementedException();

		public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
			new FileStreamProcessor(waveFormat.SampleRate, sampleSize);
	}

	class FileStreamProcessor : IProcessor
	{
		public IProcessor Source { get; set; }

		public int BufferMilliseconds
		{
			get => (int)_timer.Interval.TotalMilliseconds;
			set => _timer.Interval = TimeSpan.FromMilliseconds(value);
		}

		public float Level { get; set; } = 1f;
		public float Boost { get; set; } = 1f;

		public event EventHandler<ProcessingEventArgs> DataAvailable;

		private readonly DispatcherTimer _timer = new();
		private readonly int _sampleRate;
		private int _sampleSize;

		public FileStreamProcessor(int sampleRate, int sampleSize)
		{
			sampleSize = 2048;
			_sampleRate = sampleRate;
			_sampleSize = sampleSize;
			_timer.Interval = TimeSpan.FromSeconds(0.5 * sampleSize / sampleRate);
			_timer.Tick += OnTimerTick;
			_bins = Next();
		}

		public void Tick()
		{
			DataAvailable?.Invoke(this, new ProcessingEventArgs(this, _bins));
			_bins = Next();
		}

		long _offset;

		public float[] Next()
		{
			using var stream = File.OpenRead("D:\\data.rec");
			stream.Seek(_offset += _sampleSize, SeekOrigin.Begin);
			using var reader = new BinaryReader(stream);
			var bins = new short[_sampleSize];
			for (var i = 0; i < _sampleSize && stream.Position < stream.Length; i++)
				bins[i] = reader.ReadInt16();

			if (stream.Position >= stream.Length)
				Lull();
			//return bins.Scale(Boost);

			var timeFrame = bins.Select(b => new Complex((double)b / ushort.MaxValue)).ToList();
			var spectralFrame = Butterfly.Transform(timeFrame, true);
			var spectrum = Filtering.GetSpectrum(spectralFrame, _sampleRate).ToArray();
			Filtering.Interpolate(spectrum, out var peaks);

			var profile = new Models.Harmonic.Profile
			{
				Harmonics = new(peaks.Select(b => new Models.Harmonic
				{
					Magnitude = b.Magnitude,
					Frequency = b.Frequency,
					Phase = b.Phase,
				}))
			};

			var signal = profile.GenerateSignalSample(_sampleSize, _sampleRate, false);
			return signal.StretchArray(Level * Boost);
		}

		float[] _bins;

		public bool IsTimerEnabled { get; set; }
#if NETSTANDARD
		= true;
#endif

		private void OnTimerTick(object sender, EventArgs e)
		{
			if (IsTimerEnabled) Tick();
		}

		public void Free() => _timer.Stop();
		public void Lull() => _timer.Stop();
		public void Wake() => _timer.Start();
	}
}
