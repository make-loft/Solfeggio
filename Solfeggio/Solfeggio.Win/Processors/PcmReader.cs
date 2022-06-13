using Ace;

using Microsoft.Win32;

using Solfeggio.Api;

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
			: base(sampleRate, sampleSize, buffersCount) => Expose();

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

		public List<short[]> Frames = new(64);

		public override short[] Next()
		{
			if (stream.IsNot())
				return new short[0];

			var frame = Frames.FirstOrDefault();
			Frames.Remove(frame);

			if (stream.Position < stream.Length)
				allowReadChunkEvent.Set();
			return frame;
		}

		public abstract TReader CreateReader(Stream stream);
		public abstract short[] ReadFrame();

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

		short[] nextFrame = { };
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
			: base(sampleRate, sampleSize, buffersCount) { }

		protected override string Filter { get; } = FileDialogFilters.Pcm;

		public override short[] ReadFrame()
		{
			var sample = new short[_sampleSize];
			for (var i = 0; i < sample.Length; i++)
				sample[i] = reader.ReadInt16();

			return sample.Scale(Boost);
		}

		public override BinaryReader CreateReader(Stream stream) => new(stream);
	}
}
