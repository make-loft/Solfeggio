using Microsoft.Win32;

using Rainbow;

using Solfeggio.Api;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace Solfeggio.Processors
{
	class EncodeProcessor : IProcessor
	{
		public class DeviceInfo : Wave.Out.DeviceInfo
		{
			public DeviceInfo() : base(int.MinValue) { }

			public override string ProductName => "Encoder";

			public override WaveOutCapabilities GetCapabilities() => throw new NotImplementedException();
			public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount, IProcessor source) =>
				new EncodeProcessor(waveFormat.SampleRate, sampleSize, source);
		}

		public double Level { get; set; } = 1d;
		public double Boost { get; set; } = 1d;

		public event EventHandler<ProcessingEventArgs> DataAvailable;

		private readonly int _sampleRate;
		private readonly int _sampleSize;

		string binPath;
		string txtPath;
		string pcmPath;

		public EncodeProcessor(int sampleRate, int sampleSize, IProcessor source)
		{
			_sampleRate = sampleRate;
			_sampleSize = sampleSize;

			var dialog = new SaveFileDialog { Filter = FileDialogFilters.Pcm };
			dialog.ShowDialog();

			binPath = dialog.FileName + ".bin";
			txtPath = dialog.FileName + ".txt";
			pcmPath = dialog.FileName;

			source.DataAvailable += (o, e) =>
			{
				if (source is ASoftwareSignalProcessor sp)
				{
					sp.IsAutoTickEnabledOnce = true;
				}

				FlushFrame(e.Bins);
			};
		}

		void FlushFrame(short[] frame)
		{
			var timeFrame = frame.Select(b => new Complex((double)b / short.MaxValue)).ToList();
			var spectralFrame = Butterfly.Transform(timeFrame, true);
			var spectrum = Filtering.GetSpectrum(spectralFrame, _sampleRate).ToArray();
			var silenceThreshold = 2d / frame.Length;
			Filtering.Interpolate(spectrum, out var peaks).ToList();
			var sample = peaks.OrderByDescending(b => b.Magnitude)
				.ToString(b => $"{b.Magnitude:.000}\t{b.Frequency:F2}\t{(b.Phase < 0 ? b.Phase + Pi.Double : b.Phase):F3}", "|");

			try
			{
				//using var zip = ZipFile.Open(pcmPath + ".zip", ZipArchiveMode.Update);
		
				//using var frames = (zip.GetEntry("frames.txt") ?? zip.CreateEntry("frames.txt")).Open();
				//using var framesWriter = new StreamWriter(frames);
				//frames.Seek(0, SeekOrigin.End);
				//framesWriter.WriteLine(sample);

				File.AppendAllText(txtPath, sample + "\n");

				using var writer = new BinaryWriter(File.Open(binPath, FileMode.Append));
				{
					writer.Write(peaks.Count);
					foreach (var peak in peaks)
					{
						writer.Write((float)peak.Magnitude);
						writer.Write((float)peak.Frequency);
						writer.Write((float)peak.Phase);
					}
				}

				using var pcmWriter = new BinaryWriter(File.Open(pcmPath, FileMode.Append));
				{
					foreach (var value in frame)
						pcmWriter.Write(value);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString());
			}
		}
		public void Free() { }

		public void Lull() { }

		public short[] Next() => default;

		public void Tick() { }

		public void Wake() { }
	}
}
