using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Ace;
#if NETSTANDARD
using Colors = SkiaSharp.SKColors;
using Thickness = Xamarin.Forms.Thickness;
#else
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
#endif

namespace Solfeggio.Presenters
{
	#if !NETSTANDARD
	public static class ColorEx
	{
		public static byte A(this Color c) => c.A;
		public static byte R(this Color c) => c.R;
		public static byte G(this Color c) => c.G;
		public static byte B(this Color c) => c.B;
		
		public static byte Alpha(this Color c) => c.A;
		public static byte Red(this Color c) => c.R;
		public static byte Green(this Color c) => c.G;
		public static byte Blue(this Color c) => c.B;
	}
	#endif
	
	public static class Brushes
	{
		public static SolidColorBrush White = new SolidColorBrush(Colors.White);
		public static SolidColorBrush Black = new SolidColorBrush(Colors.Black);
		public static SolidColorBrush Purple = new SolidColorBrush(Colors.Purple);
		public static SolidColorBrush Magenta = new SolidColorBrush(Colors.Magenta);
	}

	[DataContract]
	internal class MusicalPresenter : ContextObject
	{
		public class PianoKey
		{
			public int Number { get; set; }
			public double Frequency { get; set; }
			public double LeftFrequency { get; set; }
			public double RightFrequency { get; set; }
			public string Note { get; set; }
			public KeyValuePair<double, double> Peak { get; set; }

			public double MiddleOffset => UseLogScale ? Math.Log(Frequency, 2) : Frequency;
			public double LeftOffset => UseLogScale ? Math.Log(LeftFrequency, 2) : LeftFrequency;
			public double RightOffset => UseLogScale ? Math.Log(RightFrequency, 2) : RightFrequency;

			public bool IsTone { get; set; }
			public int Hits { get; set; }
			public double Magnitude { get; set; }
			public bool UseLogScale { private get; set; }
			public Brush Brush { get; set; }
		}

		private static readonly string[] Notes =
			{"C|B♯","C♯|D♭","D","D♯|E♭","E|F♭","F|E♯","F♯|G♭","G","G♯|A♭","A","A♯|B♭","B|C♭"};

		private static readonly bool[] IsToneSet = Notes.Select(n => n.Contains("♯|").Not()).ToArray();
		private static readonly Brush[] BaseOktaveColorSet =
			IsToneSet.Select(t => t ? Brushes.White : Brushes.Black).Cast<Brush>().ToArray();

		public static double[] PitchStandards { get; } = { 415d, 432d, 435d, 415d, 440d, 444d };
		public static double DefaultPitchStandard = 440d;

		[DataMember]
		public double ActivePitchStandard
		{
			get => Get(() => ActivePitchStandard, DefaultPitchStandard);
			set
			{
				Set(() => ActivePitchStandard, value);
				_baseOktaveFrequencySet = GetBaseOktaveFrequencySet(value);
			}
		}

		private static double HalftonesCount { get; } = 12;
		private static double GetBaseFrequancy(double pitchStandard) => pitchStandard / 16d;
		private static double GetHalftoneStep(double halftonesCount) => Math.Pow(2d, 1d / halftonesCount);

		private static double[] GetBaseOktaveFrequencySet(double pitchStandard) =>
			GetBaseOktaveFrequencySet(GetBaseFrequancy(pitchStandard), GetHalftoneStep(HalftonesCount));

		private static double[] GetBaseOktaveFrequencySet(double baseFrequancy, double halftoneStep) =>
			Enumerable.Range(-9, 12).Select(dt => baseFrequancy * Math.Pow(halftoneStep, dt)).ToArray();

		private double[] _baseOktaveFrequencySet = GetBaseOktaveFrequencySet(DefaultPitchStandard);

		public double MaxMagnitude { get; set; } = 1d;
		public double FrequencyScale { get; set; } = 1d;
		public double DelayInSeconds { get; set; } = 5d;

		private double _yScale;

		[DataMember] public bool AutoSensetive { get; set; } = true;
		[DataMember] public double LimitFrequency { get; set; } = 9000d;
		[DataMember] public double TopFrequency { get; set; } = 22096d;
		[DataMember] public bool UseHorizontalLogScale { get; set; } = true;
		[DataMember] public bool UseVerticalLogScale { get; set; }
		[DataMember] public bool UseNoteFilter { get; set; }
		[DataMember] public bool ShowHz { get; set; } = true;
		[DataMember] public bool ShowNotes { get; set; } = true;
		private DateTime Timestamp { get; set; }

		private static double GetVisualOffset(double xOffset, bool useHorizontalLogScale) =>
			xOffset > 0 && useHorizontalLogScale ? Math.Log(xOffset, 2) : xOffset;

		public void DrawWave(Panel canvas, Dictionary<double, double> data, Polyline polyline, Thickness margin)
		{
			if (canvas is null || polyline is null) return;
			canvas.Children.Add(polyline);
			polyline.Points.Clear();
			var pixelStep = (canvas.ActualWidth + margin.Left + margin.Right) / data.Count;

			foreach (var pair in data)
			{
				var sampleFrequency = pair.Key;
				var sampleValue = pair.Value;
				var x = sampleFrequency * pixelStep;
				var y = canvas.ActualHeight / 2d + sampleValue * canvas.ActualHeight / 100000;
				polyline.Points.Add(new Point(x - margin.Left, y));
			}
		}

		private static double ScaleMagnitude(double value, bool useLogScale) =>
			useLogScale ? 100 * Math.Log(value) : value;
		
		public void DrawSpectrum(Panel canvas, Dictionary<double, double> data, Polyline polyline)
		{
			if (canvas is null || polyline is null) return;
			var useHorizontalLogScale = UseHorizontalLogScale;
			var useVerticalLogScale = UseVerticalLogScale;
			var autoSensitive = AutoSensetive;
			var limitFrequency = LimitFrequency;
			var actualWidth = canvas.ActualWidth;
			var actualHeight = canvas.ActualHeight;


			if (canvas.Children.Contains(polyline).Not()) canvas.Children.Add(polyline);
			polyline.Points.Clear();
			var pixelStep = actualWidth / GetVisualOffset(limitFrequency, useHorizontalLogScale);

			_yScale = actualHeight * 0.8d / MaxMagnitude;

			polyline.Points.Add(new Point(0d, actualHeight));
			foreach (var pair in data)
			{
				var sampleFrequency = pair.Key;
				if (sampleFrequency > limitFrequency) break;
				var sampleValue = pair.Value;
				var x = GetVisualOffset(sampleFrequency, useHorizontalLogScale) * pixelStep;
				var magnitude = ScaleMagnitude(sampleValue, useVerticalLogScale);
				var y = magnitude * _yScale;
				y = actualHeight - y;
				polyline.Points.Add(new Point(x, y));

				if (autoSensitive)
				{
					if (magnitude > 0.7d * MaxMagnitude)
					{
						Timestamp = DateTime.Now;
					}

					if (magnitude > MaxMagnitude)
					{
						MaxMagnitude = magnitude * 1.2d;
						Timestamp = DateTime.Now;
					}
					else if (Timestamp.AddSeconds(DelayInSeconds) < DateTime.Now)
					{
						Timestamp = DateTime.Now;
						MaxMagnitude *= 0.8d;
					}
				}
			}

			polyline.Points.Add(new Point(actualWidth, actualHeight));
		}

		public void DrawTops(Panel canvas, List<PianoKey> keys)
		{
			if (canvas is null) return;
			
			var showHz = ShowHz;
			var showNotes = ShowNotes;
			var maxMagnitude = MaxMagnitude;
			var useVerticalLogScale = UseVerticalLogScale;
			var useHorizontalLogScale = UseHorizontalLogScale;
			var pixelStep = canvas.ActualWidth / GetVisualOffset(LimitFrequency, useHorizontalLogScale);

			foreach (var key in keys)
			{
				var sampleFrequency = key.Peak.Key;
				if (sampleFrequency > LimitFrequency) break;
				var sampleValue = key.Peak.Value;
				var x = GetVisualOffset(sampleFrequency, useHorizontalLogScale) * pixelStep;
				var magnitude = ScaleMagnitude(sampleValue, useVerticalLogScale);

				_yScale = canvas.ActualHeight * 0.8 / maxMagnitude;
				var y = magnitude * _yScale;
				y = canvas.ActualHeight - y;

				if (!showNotes && !showHz) return;

				var panel = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};

#if !NETSTANDARD
				// todo
				canvas.Children.Add(panel);
#endif

				if (showHz)
				{
					panel.Children.Add(new TextBlock
					{
						FontSize = 9.0,
						Foreground = Brushes.Magenta,
						Text = sampleFrequency.ToString("F")
					});
				}

				if (showNotes)
				{
					var hz = ShowHz ? $"({key.Frequency })" : "";
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

		public List<PianoKey> DrawPiano(Panel canvas, Dictionary<double, double> data)
		{
			if (canvas is null) return new List<PianoKey>();
			var useLogScale = UseHorizontalLogScale;
			var limitFrequency = LimitFrequency;
			var useNoteFilter = UseNoteFilter;
			var frequencyScale = FrequencyScale;
			var maxMagnitude0 = MaxMagnitude;

			canvas.Children.Clear();
			var pixelStep = canvas.ActualWidth / GetVisualOffset(limitFrequency, useLogScale);

			var keys = new List<PianoKey>();
			var maxKeyNumber = HalftonesCount + 1;
			for (var j = 1; j < maxKeyNumber; j++)
			{
				keys.AddRange(_baseOktaveFrequencySet.Select((t, i) =>
					CreatePianoKey(
						_baseOktaveFrequencySet,
						IsToneSet,
						BaseOktaveColorSet,
						i, j, Notes[i], useLogScale)));
			}

			foreach (var d in data)
			{
				var sampleValue = d.Value;
				var sampleFrequency = d.Key;
				if (sampleFrequency > limitFrequency) break;
				var key =
					keys.FirstOrDefault(k => k.LeftFrequency < sampleFrequency && sampleFrequency <= k.RightFrequency);
				if (key is null) continue;
				if (useNoteFilter)
				{
					var range = key.RightFrequency - key.LeftFrequency;
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

			var minMagnitude = maxMagnitude0 * 0.07;
			var tops = keys.OrderByDescending(k => k.Magnitude).Take(10).Where(k => k.Magnitude > minMagnitude).ToList();

			//keys.ForEach(k => k.Magnitude = k.Magnitude/k.Hits);
			keys.ForEach(k => k.Magnitude = k.Magnitude * k.Magnitude);
			var maxMagnitude = maxMagnitude0 * maxMagnitude0 * 0.32; // keys.Max(k => k.Magnitude);
																	 //if (MaxMagnitude1 > maxMagnitude) maxMagnitude = MaxMagnitude1*0.7;

			//MaxMagnitude1 = maxMagnitude;
			foreach (var key in keys.Where(k => k.LeftFrequency < limitFrequency))
			{
				var gradientBrush = new LinearGradientBrush { EndPoint = new Point(0, 1) };
				var basicColor = key.Brush.As<SolidColorBrush>()?.Color ?? Colors.Transparent;
				var tmp = 255 * key.Magnitude / maxMagnitude;
				var red = tmp > 255 ? (byte)255 : (byte)tmp;
				red = key.IsTone ? (byte)(basicColor.Red() - red) : red;
				var pressColor = key.IsTone
					? Color.FromArgb(255, basicColor.Red(), red, red)
					: Color.FromArgb(255, red, basicColor.Green(), basicColor.Blue());
				gradientBrush.GradientStops.Merge
				(
					new GradientStop {Color = basicColor, Offset = 0.0},
					new GradientStop {Color = pressColor, Offset = 0.5}
				);
				canvas.Children.Add(new Line
				{
					VerticalAlignment = VerticalAlignment.Stretch,
					X1 = key.MiddleOffset * pixelStep,
					X2 = key.MiddleOffset * pixelStep,
					Y1 = 0,
					Y2 = key.IsTone ? canvas.ActualHeight : canvas.ActualHeight * 0.7,
					Stroke = gradientBrush,
					StrokeThickness = (key.RightOffset - key.LeftOffset) * pixelStep,
				});
			}

			return tops;
		}

		private static PianoKey CreatePianoKey(double[] oktave, IList<bool> oktaveToneSet,
			IList<Brush> oktaveColorSet, int keyNumber, int oktaveNumber, string note, bool useLogScale)
		{
			oktave = oktave.Select(d => d * Math.Pow(2, oktaveNumber)).ToArray();
			var middleFrequency = oktave[keyNumber];

			double leftFrequency;
			double rightFrequency;
			if (keyNumber == 0)
			{
				leftFrequency = oktave[oktave.Length - 1] / 2;
				rightFrequency = oktave[keyNumber + 1];
			}
			else if (keyNumber == oktave.Length - 1)
			{
				leftFrequency = oktave[keyNumber - 1];
				rightFrequency = oktave[0] * 2;
			}
			else
			{
				leftFrequency = oktave[keyNumber - 1];
				rightFrequency = oktave[keyNumber + 1];
			}

			var middleOffset = Math.Log(middleFrequency, 2d);
			var leftOffset = (Math.Log(leftFrequency, 2d) + middleOffset) / 2d;
			leftFrequency = Math.Pow(2d, leftOffset);
			var rightOffset = (middleOffset + Math.Log(rightFrequency, 2d)) / 2d;
			rightFrequency = Math.Pow(2d, rightOffset);
			return new PianoKey
			{
				Number = keyNumber,
				UseLogScale = useLogScale,
				Frequency = middleFrequency,
				LeftFrequency = leftFrequency,
				RightFrequency = rightFrequency,
				IsTone = oktaveToneSet[keyNumber],
				Brush = oktaveColorSet[keyNumber],
				Note = note + oktaveNumber,
			};
		}
	}
}