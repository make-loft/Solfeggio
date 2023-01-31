using Ace;

using Microsoft.Win32;

using Rainbow;

using Solfeggio.Api;
using Solfeggio.Extensions;
using Solfeggio.Headers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Solfeggio.Processors
{
	abstract class AStreamProcessor<TReader> : ASoftwareSignalProcessor, IExposable, IDisposable
	{
		public AStreamProcessor(int sampleRate, int sampleSize, int buffersCount)
			: base(sampleRate, sampleSize) => Expose();

		protected TReader reader;
		protected FileStream stream;
		protected AutoResetEvent allowReadChunkEvent = new(true);
		protected virtual string Filter { get; } = FileDialogFilters.Pcm;

		public void Dispose()
		{
			allowReadChunkEvent.Dispose();
			stream.Dispose();
		}

		public void Expose()
		{
			new Models.Harmonic.Profile();

			var dialog = new OpenFileDialog { Filter = Filter };
			dialog.ShowDialog();

			var path = dialog.FileName;
			if (path.IsNullOrEmpty())
				return;

			stream = File.OpenRead(path);
			reader = CreateReader(stream);

			ThreadPool.QueueUserWorkItem(BackgroundReadLoop);
		}

		public List<float[]> Frames = new(64);

		public override float[] Next()
		{
			if (stream.IsNot())
				return new float[0];

			var frame = Frames.FirstOrDefault();
			Frames.Remove(frame);

			if (stream.Position < stream.Length)
				allowReadChunkEvent.Set();
			return frame;
		}

		public abstract TReader CreateReader(Stream stream);
		public abstract float[] ReadFrame();

		void BackgroundReadLoop(object state)
		{
			try
			{
				while (true)
				{
					allowReadChunkEvent.WaitOne();
					if (Frames.Count < Frames.Capacity)
					{
						Frames.Add(ReadFrame());
						allowReadChunkEvent.Set();
					}
				}
			}
			catch (Exception exception)
			{
				return;
			}
		}
	}

	class PcmReader : AStreamProcessor<BinaryReader>
	{
		public class DeviceInfo : Wave.In.DeviceInfo
		{
			public DeviceInfo() : base(int.MinValue) { }

			public override string ProductName => "PCM Reader";

			public override WaveInCapabilities GetCapabilities() => throw new NotImplementedException();

			public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
				new PcmReader(waveFormat.SampleRate, sampleSize, buffersCount);
		}

		public PcmReader(int sampleRate, int sampleSize, int buffersCount)
			: base(sampleRate, sampleSize, buffersCount)
		{
			if (reader.Is())
				Container = new(reader);
		}

		protected override string Filter { get; } = FileDialogFilters.Pcm;

		public WaveContainer Container { get; set; }

		public override float[] ReadFrame()
		{
			var frame = new float[SampleSize];
			for (var i = 0; i < frame.Length; i++)
				frame[i] = reader.ReadInt16();

			var timeFrame = frame.Select(b => new Complex(b)).ToList();
			var spectralFrame = Butterfly.Transform(timeFrame, true);
			var spectrum = Filtering.GetSpectrum(spectralFrame, Container.Header.SampleRate).ToArray();
			var x = Filtering.Interpolate(spectrum, out var peaks).ToList();
			var valuePeaks = peaks.Where(p => p.Magnitude >= 0.001).ToList();

			var signal = Next(valuePeaks);

			return signal.StretchArray(Level * Boost);
		}

		float[] overlap;
		public float[] Next(IList<Bin> peaks)
		{
			var profile = new Models.Harmonic.Profile
			{
				Harmonics = new(peaks.Select(b => new Models.Harmonic
				{
					Magnitude = b.Magnitude,
					Frequency = b.Frequency,
					Phase = b.Phase,
				}))
			};

			var signal = profile.GenerateSignalSample(SampleSize, SampleRate, false);

			if (Console.CapsLock)
			{
				if (overlap.Is() && Console.CapsLock)
					signal = signal.Raise(0.00f, 0.5f).Add(overlap.Fade(0.00f, 0.5f));
				overlap = profile.GenerateSignalSample(SampleSize, SampleRate, false);
			}

			return signal.StretchArray(Level * Boost);
		}

		public override BinaryReader CreateReader(Stream stream) => new(stream);
	}
}
