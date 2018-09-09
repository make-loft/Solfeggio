using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Ace;

using Colors = SkiaSharp.SKColors;
using Thickness = Xamarin.Forms.Thickness;

namespace Solfeggio.Presenters
{
	public static class Brushes
	{
		public static SolidColorBrush White = new SolidColorBrush(Colors.White);
		public static SolidColorBrush Black = new SolidColorBrush(Colors.Black);
		public static SolidColorBrush Purple = new SolidColorBrush(Colors.Purple);
	}

	[DataContract]
	internal class Presenter : ContextObject
	{
		public class PianoKey
		{
			public int Number { get; set; }
			public double Frequency { get; set; }
			public double LeftFrequency { get; set; }
			public double RigthFrequency { get; set; }
			public string Note { get; set; }
			public KeyValuePair<double, double> Peak { get; set; }

			public double MiddleOffset => UseLogScale ? Math.Log(Frequency, 2) : Frequency;
			public double LeftOffset => UseLogScale ? Math.Log(LeftFrequency, 2) : LeftFrequency;
			public double RigthOffset => UseLogScale ? Math.Log(RigthFrequency, 2) : RigthFrequency;

			public bool IsTone { get; set; }
			public int Hits { get; set; }
			public double Magnitude { get; set; }
			public bool UseLogScale { private get; set; }
			public SolidColorBrush SolidColorBrush { get; set; }
		}

		private static readonly double[] BaseOktaveFrequencySet =
		{
			16.352, 17.324, 18.354, 19.445, 20.602, 21.827,
			23.125, 24.500, 25.957, 27.500, 29.135, 30.868
		};

		private static readonly bool[] BaseOktaveToneSet =
		{
			true, false, true, false, true, true, false, true, false,
			true, false, true
		};

		private static readonly SolidColorBrush[] BaseOktaveColorSet =
		{
			Brushes.White,
			Brushes.Black,
			Brushes.White,
			Brushes.Black,
			Brushes.White,
			Brushes.White,
			Brushes.Black,
			Brushes.White,
			Brushes.Black,
			Brushes.White,
			Brushes.Black,
			Brushes.White
		};

		public int ScaleA { get; set; }
		public int ScaleB { get; set; }

		public double MaxAmplitude
		{
			get { return Get(() => MaxAmplitude); }
			set { Set(() => MaxAmplitude, value); }
		}

		[DataMember]
		public bool AutoSensetive
		{
			get { return Get(() => AutoSensetive); }
			set { Set(() => AutoSensetive, value); }
		}

		public double Delay
		{
			get { return Get(() => Delay, 5d); }
			set { Set(() => Delay, value); }
		}

		private double _yScale = 1.0 / 512;

		[DataMember]
		public double LimitFrequency
		{
			get { return Get(() => LimitFrequency, 2150); }
			set { Set(() => LimitFrequency, value); }
		}

		[DataMember]
		public double TopFrequency
		{
			get { return Get(() => TopFrequency, 22096); }
			set { Set(() => TopFrequency, value); }
		}

		public double FrequencyScale
		{
			get { return Get(() => FrequencyScale, 1.0); }
			set { Set(() => FrequencyScale, value); }
		}

		[DataMember]
		public bool UseHorizontalLogScale
		{
			get { return Get(() => UseHorizontalLogScale, true); }
			set { Set(() => UseHorizontalLogScale, value); }
		}

		[DataMember]
		public bool UseVerticalLogScale
		{
			get { return Get(() => UseVerticalLogScale); }
			set { Set(() => UseVerticalLogScale, value); }
		}

		[DataMember]
		public bool UseNoteFilter
		{
			get { return Get(() => UseNoteFilter); }
			set { Set(() => UseNoteFilter, value); }
		}

		[DataMember]
		public bool ShowHz
		{
			get { return Get(() => ShowHz, true); }
			set { Set(() => ShowHz, value); }
		}

		[DataMember]
		public bool ShowNotes
		{
			get { return Get(() => ShowNotes, true); }
			set { Set(() => ShowNotes, value); }
		}

		private static double GetVisualOffset(double xOffset, bool useHorizontalLogScale) =>
			xOffset > 0 && useHorizontalLogScale ? Math.Log(xOffset, 2) : xOffset;

		private DateTime Timestamp { get; set; }

		public void DrawWave(Panel canvas, Dictionary<double, double> data, Polyline polyline, Thickness margin)
		{
			if (canvas == null || polyline == null) return;
			canvas.Children.Add(polyline);
			polyline.Points.Clear();
			var pixelStep = (canvas.ActualWidth + margin.Left + margin.Right) / data.Count;

			foreach (var pair in data)
			{
				var sampleFrequency = pair.Key;
				var sampleValue = pair.Value;
				var x = sampleFrequency * pixelStep;
				var y = canvas.ActualHeight / 2 + sampleValue * canvas.ActualHeight / 100000;
				polyline.Points.Add(new Point(x - margin.Left, y));
			}
		}

		public void DrawSpectrum(Panel canvas, Dictionary<double, double> data, Polyline polyline)
		{
			if (canvas == null || polyline == null) return;
			var useHorizontalLogScale = UseHorizontalLogScale;
			var useVerticalLogScale = UseVerticalLogScale;
			var autoSensetive = AutoSensetive;
			var limitFrequency = LimitFrequency;

			canvas.Children.Add(polyline);
			polyline.Points.Clear();
			var pixelStep = canvas.ActualWidth / GetVisualOffset(limitFrequency, useHorizontalLogScale);

			polyline.Points.Add(new Point(0, canvas.ActualHeight));
			foreach (var pair in data)
			{
				var sampleFrequency = pair.Key;
				if (sampleFrequency > limitFrequency) break;
				var sampleValue = pair.Value;
				var x = GetVisualOffset(sampleFrequency, useHorizontalLogScale) * pixelStep;
				var amplitude = useVerticalLogScale ? 100 * Math.Log(sampleValue) : sampleValue;
				if (autoSensetive)
				{
					if (amplitude > 0.7 * MaxAmplitude)
					{
						Timestamp = DateTime.Now;
					}

					if (amplitude > MaxAmplitude)
					{
						MaxAmplitude = amplitude * 1.2;
					}
					else if (Timestamp.AddSeconds(Delay) < DateTime.Now)
					{
						Timestamp = DateTime.Now;
						MaxAmplitude *= 0.8;
					}
				}

				_yScale = canvas.ActualHeight * 0.8 / MaxAmplitude;
				var y = amplitude * _yScale;
				y = canvas.ActualHeight - y;
				polyline.Points.Add(new Point(x, y));
			}

			polyline.Points.Add(new Point(canvas.ActualWidth, canvas.ActualHeight));
		}

		public void DrawTops(Panel canvas, List<PianoKey> keys)
		{
			if (canvas == null) return;
			var useVerticalLogScale = UseVerticalLogScale;
			var pixelStep = canvas.ActualWidth / GetVisualOffset(LimitFrequency, useVerticalLogScale);

			foreach (var key in keys)
			{
				var sampleFrequency = key.Peak.Key;
				if (sampleFrequency > LimitFrequency) break;
				var sampleValue = key.Peak.Value;
				var x = GetVisualOffset(sampleFrequency, useVerticalLogScale) * pixelStep;
				var amplitude = sampleValue;

				_yScale = canvas.ActualHeight * 0.8 / MaxAmplitude;
				var y = amplitude * _yScale;
				y = canvas.ActualHeight - y;

				if (!ShowNotes && !ShowHz) return;

				var panel = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};

				// todo
				//canvas.Children.Add(panel);

				if (ShowHz)
				{
					panel.Children.Add(new TextBlock
					{
						FontSize = 9.0,
						//Foreground = Brushes.Magenta,
						Text = sampleFrequency.ToString("F")
					});
				}

				if (ShowNotes)
				{
					var hz = ShowHz ? " (" + key.Frequency + ")" : string.Empty;
					panel.Children.Add(new TextBlock
					{
						FontSize = 11.0,
						Foreground = Brushes.Purple,
						Text = key.Note + hz
					});
				}

				panel.UpdateLayout();
				panel.Margin = new Thickness(x - panel.ActualWidth / 2, y - panel.ActualHeight, 0, 0);
			}
		}

		private static readonly string[] Notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

		public double MaxAmplitude1 { get; set; }

		public List<PianoKey> DrawPiano(Panel canvas, Dictionary<double, double> data)
		{
			if (canvas == null) return new List<PianoKey>();
			var useLogScale = UseHorizontalLogScale;
			var limitFrequency = LimitFrequency;
			var useNoteFilter = UseNoteFilter;
			var frequencyScale = FrequencyScale;
			var maxAmplitude0 = MaxAmplitude;

			canvas.Children.Clear();
			var pixelStep = canvas.ActualWidth / GetVisualOffset(limitFrequency, useLogScale);

			var keys = new List<PianoKey>();
			for (var j = 1; j < 13; j++)
			{
				keys.AddRange(BaseOktaveFrequencySet.Select((t, i) =>
					CreatePianoKey(
						BaseOktaveFrequencySet,
						BaseOktaveToneSet,
						BaseOktaveColorSet,
						i, j, Notes[i], useLogScale)));
			}

			foreach (var d in data)
			{
				var sampleValue = d.Value;
				var sampleFrequency = d.Key;
				if (sampleFrequency > limitFrequency) break;
				var key =
					keys.FirstOrDefault(k => k.LeftFrequency < sampleFrequency && sampleFrequency <= k.RigthFrequency);
				if (key == null) continue;
				if (useNoteFilter)
				{
					var range = key.RigthFrequency - key.LeftFrequency;
					var half = key.Frequency * frequencyScale - key.LeftFrequency;
					sampleValue = (float)(sampleValue * Rainbow.Windowing.Gausse(half, range));
				}

				key.Hits++;
				if (sampleValue > key.Magnitude)
				{
					key.Magnitude = sampleValue;
					key.Peak = d;
				}
			}

			var minAmplitude = maxAmplitude0 * 0.07;
			var tops = keys.OrderByDescending(k => k.Magnitude).Take(10).Where(k => k.Magnitude > minAmplitude).ToList();

			//keys.ForEach(k => k.Amplitude = k.Amplitude/k.Hits);
			keys.ForEach(k => k.Magnitude = k.Magnitude * k.Magnitude);
			var maxAmplitude = maxAmplitude0 * maxAmplitude0 * 0.32; // keys.Max(k => k.Amplitude);
																	 //if (MaxAmplitude1 > maxAmplitude) maxAmplitude = MaxAmplitude1*0.7;

			//MaxAmplitude1 = maxAmplitude;
			foreach (var key in keys.Where(k => k.LeftFrequency < limitFrequency))
			{
				var tmp = 255 * key.Magnitude / maxAmplitude;
				var red = tmp > 255 ? (byte)255 : (byte)tmp;
				var gradientBrush = new LinearGradientBrush { EndPoint = new Point(0, 1) };
				var baseColor = key.SolidColorBrush.Color;
				red = key.IsTone ? (byte)(baseColor.R - red) : red;
				var pressColor = key.IsTone
					? Color.FromArgb(255, baseColor.R, red, red)
					: Color.FromArgb(255, red, baseColor.G, baseColor.B);
				gradientBrush.GradientStops.Add(new GradientStop { Color = baseColor, Offset = 0 });
				gradientBrush.GradientStops.Add(new GradientStop { Color = pressColor, Offset = 1 });
				canvas.Children.Add(new Line
				{
					VerticalAlignment = VerticalAlignment.Stretch,
					X1 = key.MiddleOffset * pixelStep,
					X2 = key.MiddleOffset * pixelStep,
					Y1 = 0,
					Y2 = key.IsTone ? canvas.ActualHeight : canvas.ActualHeight * 0.7,
					Stroke = gradientBrush,
					StrokeThickness = (key.RigthOffset - key.LeftOffset) * pixelStep,
				});
			}

			return tops;
		}

		private static PianoKey CreatePianoKey(double[] oktave, IList<bool> oktaveToneSet,
			IList<SolidColorBrush> oktaveColorSet, int keyNumber, int oktaveNumber, string note, bool useLogScale)
		{
			oktave = oktave.Select(d => d * Math.Pow(2, oktaveNumber)).ToArray();
			var middleFrequency = oktave[keyNumber];

			double leftFrequency;
			double rigthFrequency;
			if (keyNumber == 0)
			{
				leftFrequency = oktave[oktave.Length - 1] / 2;
				rigthFrequency = oktave[keyNumber + 1];
			}
			else if (keyNumber == oktave.Length - 1)
			{
				leftFrequency = oktave[keyNumber - 1];
				rigthFrequency = oktave[0] * 2;
			}
			else
			{
				leftFrequency = oktave[keyNumber - 1];
				rigthFrequency = oktave[keyNumber + 1];
			}

			var leftOffset = (Math.Log(leftFrequency, 2) + Math.Log(middleFrequency, 2)) / 2;
			leftFrequency = Math.Pow(2, leftOffset);
			var rigthOffset = (Math.Log(middleFrequency, 2) + Math.Log(rigthFrequency, 2)) / 2;
			rigthFrequency = Math.Pow(2, rigthOffset);
			return new PianoKey
			{
				Number = keyNumber,
				UseLogScale = useLogScale,
				Frequency = middleFrequency,
				LeftFrequency = leftFrequency,
				RigthFrequency = rigthFrequency,
				IsTone = oktaveToneSet[keyNumber],
				SolidColorBrush = oktaveColorSet[keyNumber],
				Note = note + oktaveNumber,
			};
		}
	}
}