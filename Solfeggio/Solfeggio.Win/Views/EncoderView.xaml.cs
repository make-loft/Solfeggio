using Ace;

using Microsoft.Win32;

using Rainbow;

using Solfeggio.Extensions;
using Solfeggio.Headers;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Solfeggio.Views
{
	public partial class EncoderView
	{
		public EncoderView() => InitializeComponent();

		public bool KeepPhase { get; set; }
		public bool UseOverlapping { get; set; }

		string binFramesPath, txtFramesPath, txtMetaPath, zipPackagePath, resultWavPath;

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			var path = TargetFile.Text;

			var sourceDirectory = Path.GetDirectoryName(path);
			var name = Path.GetFileNameWithoutExtension(path);
			var targetDirectory = Path.Combine(sourceDirectory, name);
			Directory.CreateDirectory(targetDirectory);

			binFramesPath = Path.Combine(targetDirectory, "frames.bin");
			txtFramesPath = Path.Combine(targetDirectory, "frames.txt");
			txtMetaPath = Path.Combine(targetDirectory, "meta.txt");
			zipPackagePath = Path.Combine(sourceDirectory, name + ".zip");
			resultWavPath = Path.Combine(sourceDirectory, name + ".zip.wav");

			await Task.Run(Encode);
			await Task.Run(Decode);
		}

		void Encode()
		{
			if (File.Exists(txtFramesPath))
				return;

			using var stream = Container.GetDataStream();
			using var reader = new BinaryReader(stream);

			Dispatcher.Invoke(() =>
			{
				EncodeProgress.Minimum = 0;
				EncodeProgress.Maximum = stream.Length;
			});

			var metadata = $"Sample Rate: {Container.Header.SampleRate}\nFrame Size: {FrameSize}";
			File.WriteAllText(txtMetaPath, metadata);

			while (stream.Position < stream.Length)
			{
				Dispatcher.Invoke(() =>
				{
					EncodeProgress.Value = stream.Position;
				});

				var signal = new short[FrameSize];
				for (var i = 0; i < FrameSize && stream.Position < stream.Length; i++)
				{
					signal[i] = reader.ReadInt16();
				}

				FlushFrame(signal);
			}
		}

		void Decode()
		{
			using var zip = ZipFile.Open(zipPackagePath, ZipArchiveMode.Update);

			using var meta = (zip.GetEntry("meta.txt") ?? zip.CreateEntry("meta.txt")).Open();
			using var metaWriter = new StreamWriter(meta);
			//meta.Seek(0, SeekOrigin.End);
			var metadata = $"Sample Rate: {Container.Header.SampleRate}\nFrame Size: {FrameSize}";
			metaWriter.WriteLine(metadata);

			using var frames = (zip.GetEntry("frames.txt") ?? zip.CreateEntry("frames.txt")).Open();
			using var framesWriter = new StreamWriter(frames);
			//frames.Seek(0, SeekOrigin.End);


			using var txtStream = File.OpenRead(txtFramesPath);
			using var wavStream = File.OpenWrite(resultWavPath);
			using var txtReader = new StreamReader(txtStream);
			using var wavWriter = new BinaryWriter(wavStream);


			Dispatcher.Invoke(() =>
			{
				DecodeProgress.Minimum = 0;
				DecodeProgress.Maximum = txtStream.Length;
			});

			var headerBytes = Container.Header.ToBytes();
			wavWriter.Write(headerBytes);
			wavWriter.Flush();

			while (txtStream.Position < txtStream.Length)
			{
				Dispatcher.Invoke(() =>
				{
					DecodeProgress.Value = txtStream.Position;
				});

				var line = txtReader.ReadLine();
				framesWriter.WriteLine(line);

				var peaks = line.SplitByChars("|").Select(l => l.SplitByChars("\t ")).Select(l => new Bin
				{
					Magnitude = double.Parse(l[0]),
					Frequency = double.Parse(l[1]),
					Phase = l.Length is 3 ? double.Parse(l[2]) : 0d,
				}).ToList();

				var sample = Next(peaks, UseOverlapping);
				for (var i = 0; i < sample.Length; i++)
					wavWriter.Write(sample[i]);
			}
		}

		double[] overlap;
		public short[] Next(IList<Bin> peaks, bool useOverlap)
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

			var signal = profile.GenerateSignalSample(FrameSize, SampleRate, false);

			if (useOverlap)
			{
				if (overlap.Is() && Console.CapsLock)
					signal = signal.Raise(0.00, 0.25).Add(overlap.Fade(0.00, 0.25));
				overlap = profile.GenerateSignalSample(FrameSize, SampleRate, false);
			}

			var bins = signal.Select(d => (short)(d * short.MaxValue)).ToArray();
			return bins;
		}

		string WaveFilter = "Pulse-code modulation files (*.wav)|*.wav|(*.wav)|*.wav|All files (*.*)|*.*";

		public string SourcePath
		{
			get => SourceFile.Text;
			set => SourceFile.Text = value;
		}

		public string TargetPath
		{
			get => TargetFile.Text;
			set => TargetFile.Text = value;
		}

		public WaveContainer Container { get; set; }
		public int FrameSize { get; set; } = 1024;
		public int[] FrameSizes { get; } = { 256, 512, 1024, 2048 };
		public int SampleRate => Container.Header.SampleRate;

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog { Filter = WaveFilter };
			if (dialog.ShowDialog().IsNot(true))
				return;

			SourcePath = dialog.FileName;
			TargetPath = dialog.FileName.Replace(".wav", "") + ".zip";

			Container = new(SourcePath);
			SourceMetadata.DataContext = Container.Header;
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog { FileName = TargetFile.Text };
			if (dialog.ShowDialog() is true)
			{
				TargetFile.Text = dialog.FileName;
			}
		}

		void FlushFrame(short[] frame)
		{
			var timeFrame = frame.Select(b => new Complex((double)b / short.MaxValue)).ToList();
			var spectralFrame = Butterfly.Transform(timeFrame, true);
			var spectrum = Filtering.GetSpectrum(spectralFrame, Container.Header.SampleRate).ToArray();
			var silenceThreshold = 2d / frame.Length;
			Filtering.Interpolate(spectrum, out var peaks).ToList();
			var sample = peaks.OrderByDescending(b => b.Magnitude)
				.ToString(b => KeepPhase
					? $"{b.Magnitude:.000}\t{b.Frequency:F2}\t{(b.Phase < 0 ? b.Phase + Pi.Double : b.Phase):F3}"
					: $"{b.Magnitude:.000}\t{b.Frequency:F2}\t",
				"|");

			try
			{
				File.AppendAllText(txtFramesPath, sample + "\n");

				using var writer = new BinaryWriter(File.Open(binFramesPath, FileMode.Append));
				{
					writer.Write(peaks.Count);
					foreach (var peak in peaks)
					{
						writer.Write((float)peak.Magnitude);
						writer.Write((float)peak.Frequency);
						if (KeepPhase)
							writer.Write((float)peak.Phase);
					}
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString());
			}
		}
	}
}
