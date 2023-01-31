using Ace;

using Rainbow;

using Solfeggio.Api;
using Solfeggio.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Solfeggio.Processors
{
	public static class FileDialogFilters
	{
		public static string Pcm = "Pulse-code modulation files (*.wav)|*.wav|(*.raw)|*.raw|All files (*.*)|*.*";
		public static string PcmTxt = "Frame files (*.txt)|*.txt|All files (*.*)|*.*";
		public static string PcmBin = "Frame files (*.bin)|*.bin|All files (*.*)|*.*";
	}

	class PcmBinDecoder : AStreamProcessor<BinaryReader>
	{

		public class DeviceInfo : Wave.In.DeviceInfo
		{
			public DeviceInfo() : base(int.MinValue) { }

			public override string ProductName => "PCM.BIN Decoder";

			public override WaveInCapabilities GetCapabilities() => throw new NotImplementedException();

			public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
				new PcmBinDecoder(waveFormat.SampleRate, sampleSize, buffersCount);
		}

	public PcmBinDecoder(int sampleRate, int sampleSize, int buffersCount)
			: base(sampleRate, sampleSize, buffersCount) { }


		protected override string Filter { get; } = FileDialogFilters.PcmBin;

		public override float[] ReadFrame()
		{
			var peaksCount = reader.ReadInt16();
			var peaks = new List<Bin>();
			for (var i = 0; i < peaksCount; i++)
			{
				var divisor = (float)short.MaxValue;

				var peak = new Bin
				{
					Magnitude = reader.ReadInt16() / divisor,
					Frequency = reader.ReadSingle() / divisor,
					Phase = reader.ReadInt16() / divisor,
				};

				peaks.Add(peak);
			}


			return Next(peaks);
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

	class StreamPcmTxtDecoder : AStreamProcessor<StreamReader>
	{
		public class DeviceInfo : Wave.In.DeviceInfo
		{
			public DeviceInfo() : base(int.MinValue) { }

			public override string ProductName => "PCM.TXT Decoder";

			public override WaveInCapabilities GetCapabilities() => throw new NotImplementedException();

			public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
				new StreamPcmTxtDecoder(waveFormat.SampleRate, sampleSize, buffersCount);
		}


		public StreamPcmTxtDecoder(int sampleRate, int sampleSize, int buffersCount)
			: base(sampleRate, sampleSize, buffersCount) { }

		protected override string Filter { get; } = FileDialogFilters.PcmTxt;

		public override StreamReader CreateReader(Stream stream) => new(stream);

		public override float[] ReadFrame()
		{
			var line = reader.ReadLine();

			var peaks = line.SplitByChars("|").Select(l => l.SplitByChars("\t ")).Select(l => new Bin
			{
				Magnitude = double.Parse(l[0]),
				Frequency = double.Parse(l[1]),
				Phase = l.Length is 3 ? double.Parse(l[2]) : 0d,
			}).ToList();

			return Next(peaks);
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
					signal = signal.Raise(0.00f, 0.25f).Add(overlap.Fade(0.00f, 0.25f));
				overlap = profile.GenerateSignalSample(SampleSize, SampleRate, false);
			}

			return signal.StretchArray(Level * Boost);
		}
	}
}
