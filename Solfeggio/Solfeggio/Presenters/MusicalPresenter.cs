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
	
	public static class AppPalette
	{
		public static SolidColorBrush FullToneKeyBrush = new SolidColorBrush(Colors.White);
		public static SolidColorBrush HalfToneKeyBrush = new SolidColorBrush(Colors.Black);

		public static SolidColorBrush NoteBrush = new SolidColorBrush(Colors.Purple);
		public static SolidColorBrush HzBrush = new SolidColorBrush(Colors.BlanchedAlmond);
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

		private static readonly string[] DiesNotation =
			{"C","C♯","D","D♯","E","F","F♯","G","G♯","A","A♯","B"};

		private static readonly string[] BemoleNotation =
			{"C","D♭","D","E♭","E","F","G♭","G","A♭","A","B♭","B"};

		private static readonly string[] Notes =
			{"C|B♯","C♯|D♭","D","D♯|E♭","E|F♭","F|E♯","F♯|G♭","G","G♯|A♭","A","A♯|B♭","B|C♭"};

		private static readonly bool[] Tones = Notes.Select(n => n.Contains("♯|").Not()).ToArray();
		private static readonly Brush[] OktaveBrushes =
			Tones.Select(t => t ? AppPalette.FullToneKeyBrush : AppPalette.HalfToneKeyBrush).Cast<Brush>().ToArray();

		public SmartSet<double> PitchStandards { get; } = new[] { 415d, 432d, 435d, 415d, 440d, 444d }.ToSet();
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

		private static double HalfTonesCount { get; } = 12;
		private static double GetBaseFrequancy(double pitchStandard) => pitchStandard / 16d;
		private static double GetHalftoneStep(double halfTonesCount) => Math.Pow(2d, 1d / halfTonesCount);

		private static double[] GetBaseOktaveFrequencySet(double pitchStandard) =>
			GetBaseOktaveFrequencySet(GetBaseFrequancy(pitchStandard), GetHalftoneStep(HalfTonesCount));

		private static double[] GetBaseOktaveFrequencySet(double baseFrequancy, double halftoneStep) =>
			Enumerable.Range(-9, 12).Select(dt => baseFrequancy * Math.Pow(halftoneStep, dt)).ToArray();

		private double[] _baseOktaveFrequencySet = GetBaseOktaveFrequencySet(DefaultPitchStandard);

		[DataMember] public double MaxMagnitude { get; set; } = 1d;
		[DataMember] public double LimitFrequency { get; set; } = 9000d;
		[DataMember] public double TopFrequency { get; set; } = 22096d;

		[DataMember] public bool UseHorizontalLogScale { get; set; } = true;
		[DataMember] public bool UseVerticalLogScale { get; set; }
		[DataMember] public bool UseNoteFilter { get; set; }
		[DataMember] public bool ShowHz { get; set; } = true;
		[DataMember] public bool ShowNotes { get; set; } = true;
		[DataMember] public bool AutoSensitive { get; set; } = true;
		[DataMember] public double AutoSensitiveStep { get; set; } = 0.02d;
		[DataMember] public double AutoSensitiveDelayInSeconds { get; set; } = 0.03d;
		[DataMember] public DateTime AutoSensitiveTimestamp { get; set; }

		private static double ScaleMagnitude(double value, bool useLogScale) => useLogScale ? 100d * Math.Pow(value, 2) : value;
		private static double GetVisualOffset(double xOffset, bool useHorizontalLogScale) =>
			xOffset > 0 && useHorizontalLogScale ? Math.Log(xOffset, 2) : xOffset;

		public void DrawWave(ICollection<Point> points, Dictionary<double, double> data, double width, double height)
		{
			var pixelStep = width / data.Count;
			foreach (var pair in data)
			{
				var binIndex = pair.Key;
				var binValue = pair.Value;
				var x = binIndex * pixelStep;
				var y = height * (0.5d + binValue / 500000);
				points.Add(new Point(x, y));
			}
		}
		
		public void DrawSpectrum(ICollection<Point> points, Dictionary<double, double> data, double width, double height)
		{
			var useHorizontalLogScale = UseHorizontalLogScale;
			var useVerticalLogScale = UseVerticalLogScale;
			var autoSensitive = AutoSensitive;
			var limitFrequency = LimitFrequency;
			var pixelStep = width / GetVisualOffset(limitFrequency, useHorizontalLogScale);
			var scale = height * 0.8d / MaxMagnitude;

			points.Add(new Point(0d, height));

			foreach (var pair in data)
			{
				var sampleFrequency = pair.Key;
				if (sampleFrequency > limitFrequency) break;
				var sampleValue = pair.Value;
				var magnitude = ScaleMagnitude(sampleValue, useVerticalLogScale);
				var x = GetVisualOffset(sampleFrequency, useHorizontalLogScale) * pixelStep;
				var y = height - magnitude * scale;

				points.Add(new Point(x, y));

				if (autoSensitive)
				{
					if (magnitude > 0.7d * MaxMagnitude)
					{
						AutoSensitiveTimestamp = DateTime.Now;
					}

					if (magnitude > MaxMagnitude)
					{
						AutoSensitiveTimestamp = DateTime.Now;
						MaxMagnitude *= (1 + AutoSensitiveStep * 9);
						EvokePropertyChanged(nameof(MaxMagnitude));
					}
					else if (AutoSensitiveTimestamp.AddSeconds(AutoSensitiveDelayInSeconds) < DateTime.Now)
					{
						AutoSensitiveTimestamp = DateTime.Now;
						MaxMagnitude *= (1 - AutoSensitiveStep);
						EvokePropertyChanged(nameof(MaxMagnitude));
					}
				}
			}

			points.Add(new Point(width, height));
		}

		public void DrawTops(System.Collections.IList items, List<PianoKey> keys, double width, double height)
		{	
			var showHz = ShowHz;
			var showNotes = ShowNotes;
			var maxMagnitude = MaxMagnitude;
			var useVerticalLogScale = UseVerticalLogScale;
			var useHorizontalLogScale = UseHorizontalLogScale;
			var pixelStep = width / GetVisualOffset(LimitFrequency, useHorizontalLogScale);
			var scale = height * 0.8 / maxMagnitude;

			foreach (var key in keys)
			{
				var sampleFrequency = key.Peak.Key;
				if (sampleFrequency > LimitFrequency) break;
				var sampleValue = key.Peak.Value;
				var magnitude = ScaleMagnitude(sampleValue, useVerticalLogScale);
				var x = GetVisualOffset(sampleFrequency, useHorizontalLogScale) * pixelStep;

				if (showNotes.Not() && showHz.Not()) return;

				var panel = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};

#if !NETSTANDARD
				// todo
				items.Add(panel);
#endif
				var expressionLevel = (1d + sampleValue / MaxMagnitude);

				if (showHz)
				{
					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 11.0 * expressionLevel,
						Foreground = AppPalette.HzBrush,
						Text = sampleFrequency.ToString("F")
					});
				}

				if (showNotes)
				{
					var hz = ShowHz ? $"({key.Frequency })" : "";
					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 11.0 * expressionLevel,
						Foreground = AppPalette.NoteBrush,
						Text = key.Note + hz
					});
				}

				var y = magnitude * scale / 2;
				//y = sampleValue / MaxMagnitude;
				panel.UpdateLayout();
				if (double.IsInfinity(y)) continue;
				panel.Margin = new Thickness(x - panel.ActualWidth / 2, y - panel.ActualHeight / 2, 0, 0);
			}
		}

		public List<PianoKey> DrawPiano(System.Collections.IList items, Dictionary<double, double> data, double width, double height)
		{
			var useLogScale = UseHorizontalLogScale;
			var limitFrequency = LimitFrequency;
			var useNoteFilter = UseNoteFilter;
			var maxMagnitude0 = MaxMagnitude;
			var pixelStep = width / GetVisualOffset(limitFrequency, useLogScale);

			var keys = new List<PianoKey>();
			var maxKeyNumber = HalfTonesCount + 1;
			for (var j = 1; j < maxKeyNumber; j++)
			{
				keys.AddRange(_baseOktaveFrequencySet.Select((t, i) =>
					CreatePianoKey(
						_baseOktaveFrequencySet,
						Tones,
						OktaveBrushes,
						i, j, DiesNotation[i], useLogScale)));
			}

			var m = 0;
			var averageMagnitude = 0d;
			foreach (var d in data)
			{
				var sampleValue = d.Value;
				var sampleFrequency = d.Key;
				if (sampleFrequency > limitFrequency) break;

				m++;
				averageMagnitude += sampleFrequency;

				var key =
					keys.FirstOrDefault(k => k.LeftFrequency < sampleFrequency && sampleFrequency <= k.RightFrequency);
				if (key is null) continue;
				if (useNoteFilter)
				{
					var range = key.RightFrequency - key.LeftFrequency;
					var half = key.Frequency - key.LeftFrequency;
					sampleValue = (float)(sampleValue * Rainbow.Windowing.Gausse(half, range));
				}

				key.Hits++;
				if (sampleValue > key.Magnitude)
				{
					key.Magnitude = sampleValue;
					key.Peak = d;
				}
			}

			averageMagnitude /= m;
			var minMagnitude = maxMagnitude0 * 0.07;
			minMagnitude = averageMagnitude * 0.4;
			var tops = keys.OrderByDescending(k => k.Magnitude).Take(10).Where(k => k.Magnitude > minMagnitude).ToList();

			//keys.ForEach(k => k.Magnitude = k.Magnitude/k.Hits);
			keys.ForEach(k => k.Magnitude = k.Magnitude * k.Magnitude);
			var maxMagnitude = maxMagnitude0 * maxMagnitude0 * 0.32; // keys.Max(k => k.Magnitude);
																	 //if (MaxMagnitude1 > maxMagnitude) maxMagnitude = MaxMagnitude1*0.7;

			//MaxMagnitude1 = maxMagnitude;
			foreach (var key in keys.Where(k => k.LeftFrequency < limitFrequency))
			{
				var gradientBrush = new LinearGradientBrush { EndPoint = new Point(0d, 1d) };
				var basicColor = key.Brush.As<SolidColorBrush>()?.Color ?? Colors.Transparent;
				var tmp = 255 * key.Magnitude / maxMagnitude;
				var red = tmp > 255 ? (byte)255 : (byte)tmp;
				red = key.IsTone ? (byte)(basicColor.Red() - red) : red;
				var pressColor = key.IsTone
					? Color.FromArgb(255, basicColor.Red(), red, red)
					: Color.FromArgb(255, red, basicColor.Green(), basicColor.Blue());

				gradientBrush.GradientStops.Merge
				(
					new GradientStop {Color = basicColor, Offset = 0.0d},
					new GradientStop {Color = pressColor, Offset = 0.5d}
				);
				
				items.Add(new Line
				{
					VerticalAlignment = VerticalAlignment.Stretch,
					X1 = key.MiddleOffset * pixelStep,
					X2 = key.MiddleOffset * pixelStep,
					Y1 = 0d,
					Y2 = key.IsTone ? height : height * 0.7d,
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
			if (keyNumber.Is(0))
			{
				leftFrequency = oktave[oktave.Length - 1] / 2;
				rightFrequency = oktave[keyNumber + 1];
			}
			else if (keyNumber.Is(oktave.Length - 1))
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
			var rightOffset = (middleOffset + Math.Log(rightFrequency, 2d)) / 2d;

			leftFrequency = Math.Pow(2d, leftOffset);
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