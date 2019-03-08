using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Ace;
using Rainbow;
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

		public static SolidColorBrush GridBrush = new SolidColorBrush(Colors.Violet) { Opacity = 0.3d };
		public static SolidColorBrush NoteBrush = new SolidColorBrush(Colors.Purple) { Opacity = 0.3d };
		public static SolidColorBrush HzBrush = new SolidColorBrush(Colors.BlanchedAlmond);
	}

	public static class ScaleFuncs
	{
		public static double Lineal(double value) => value;
		public static double Log2(double value) => value.Is(0d) ? 0d : Math.Log(value, 2d);
		public static double Log(double value) => value.Is(0d) ? 0d : Math.Log(value);
		public static double Exp(double value) => Math.Exp(value);
	}

	[DataContract]
	public class MusicalPresenterOptions : ContextObject
	{
		[DataMember] public bool ShowHz { get; set; } = true;
		[DataMember] public bool ShowNotes { get; set; } = true;
		[DataMember] public bool ShowWave { get; set; } = true;
		[DataMember] public bool ShowSpectrum { get; set; } = true;
		[DataMember] public bool ShowNotesGrid { get; set; } = true;
		[DataMember] public bool ShowDiscreteFourierGrid { get; set; } = true;

		public Func<double, double> FrequancyScaleFunc { get; set; } = ScaleFuncs.Log2;
		public Func<double, double> MagnitudeScaleFunc { get; set; } = ScaleFuncs.Lineal;
		public Func<double, double> PhaseScaleFunc { get; set; } = ScaleFuncs.Lineal;

		[DataMember] public bool UseNoteFilter { get; set; }
		[DataMember] public bool AutoSensitive { get; set; } = true;
		[DataMember] public double AutoSensitiveStep { get; set; } = 0.02d;
		[DataMember] public double AutoSensitiveDelayInSeconds { get; set; } = 0.03d;
		[DataMember] public DateTime AutoSensitiveTimestamp { get; set; }
	}

	[DataContract]
	public class MusicalPresenter : MusicalPresenterOptions
	{
		public class PianoKey
		{
			public int NoteNumber { get; set; }
			public string NoteName { get; set; }
			public double Magnitude { get; set; }
			public double LowFrequency { get; set; }
			public double TopFrequency { get; set; }
			public double EthalonFrequency { get; set; }
			public Complex Peak { get; set; }
			public int Hits { get; set; }

			public static PianoKey Construct(double[] oktaveNotes, int noteNumber, int oktaveNumber, string note)
			{
				var lowNoteNumber = 0;
				var topNoteNumber = oktaveNotes.Length - 1;

				var activeNote = oktaveNotes[noteNumber];
				var lowNote = noteNumber.Is(lowNoteNumber) ? oktaveNotes[topNoteNumber] / 2 : oktaveNotes[noteNumber - 1];
				var topNote = noteNumber.Is(topNoteNumber) ? oktaveNotes[lowNoteNumber] * 2 : oktaveNotes[noteNumber + 1];

				var ethalonOffset = Math.Log(activeNote, 2d);
				var lowOffset = (Math.Log(lowNote, 2d) + ethalonOffset) / 2d;
				var topOffset = (ethalonOffset + Math.Log(topNote, 2d)) / 2d;

				var lowFrequency = Math.Pow(2d, lowOffset);
				var topFrequency = Math.Pow(2d, topOffset);
				var ethalonFrequency = activeNote; // Math.Pow(2d, ethalonOffset)

				return new PianoKey
				{
					NoteNumber = noteNumber,
					NoteName = note + oktaveNumber,
					LowFrequency = lowFrequency,
					TopFrequency = topFrequency,
					EthalonFrequency = activeNote,
				};
			}
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

		public SmartSet<double> PitchStandards { get; } = new[] { 415d, 420d, 432d, 435d, 440d, 444d }.ToSet();
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

		[DataMember] public double LowMagnitude { get; set; } = 0d;
		[DataMember] public double TopMagnitude { get; set; } = 1d;
		[DataMember] public double LowFrequency { get; set; } = 20d;
		[DataMember] public double TopFrequency { get; set; } = 9000d;

		private static double ScaleMagnitude(double value, Func<double, double> scaleFunc) => scaleFunc(value);
		public static double GetVisualOffset(double value, double widthStep, double lowWidth, Func<double, double> scaleFunc) =>
			scaleFunc(value) * widthStep - lowWidth;

		private void GetVisuals(double width, double height,
			out double lowFrequency, out double topFrequency,
			out double topWidth, out double lowWidth,
			out double widthStep, out double heightScale)
		{
			var frequancyScaleFunc = FrequancyScaleFunc;
			lowFrequency = LowFrequency;
			topFrequency = TopFrequency;

			var lowOffset = frequancyScaleFunc(lowFrequency);
			var topOffset = frequancyScaleFunc(topFrequency);
			lowWidth = width * lowOffset / (topOffset - lowOffset);
			topWidth = width * topOffset / (topOffset - lowOffset);
			widthStep = topWidth / topOffset;
			heightScale = height * 0.8d / TopMagnitude;
		}

		public void DrawGrid(System.Collections.IList items, double width, double height, double step)
		{
			var frequancyScaleFunc = FrequancyScaleFunc;
			GetVisuals(width, height,
				out var lowFrequency, out var topFrequency,
				out var _, out var lowWidth,
				out var widthStep, out var _);

			var startFrequency = Math.Ceiling(LowFrequency / step) * step;
			var finishFrequency = TopFrequency;
			for (var i = startFrequency; i < finishFrequency; i += step)
			{
				var x = GetVisualOffset(i, widthStep, lowWidth, frequancyScaleFunc);
				ConstructVerticalLine(x, height, AppPalette.GridBrush).AddToUntyped(items);
			}
		}

		private IEnumerable<double> EnumerateNotes(double breakNote)
		{
			for (var j = 0; ; j++)
			{
				for (var i = 0; i < _baseOktaveFrequencySet.Length; i++)
				{
					var note = _baseOktaveFrequencySet[i] * Math.Pow(2, j);
					if (note > breakNote) yield break;
					else yield return note;
				}
			}
		}

		public void DrawNotes(System.Collections.IList items, double width, double height, double step)
		{
			var frequancyScaleFunc = FrequancyScaleFunc;
			GetVisuals(width, height,
				out var lowFrequency, out var topFrequency,
				out var _, out var lowWidth,
				out var widthStep, out var _);

			var startFrequency = Math.Ceiling(LowFrequency / step) * step;

			foreach (var note in EnumerateNotes(topFrequency))
			{
				var x = GetVisualOffset(note, widthStep, lowWidth, frequancyScaleFunc);
				ConstructVerticalLine(x, height, AppPalette.NoteBrush).AddToUntyped(items);
			}
		}

		public void DrawWave(ICollection<Point> points, IList<Complex> data, double width, double height)
		{
			var step = width / data.Count;
			foreach (var pair in data)
			{
				var binIndex = pair.Real;
				var binValue = pair.Imaginary;
				var x = binIndex * step;
				var y = height * (0.5d + binValue / 500000);
				points.Add(new Point(x, y));
			}
		}
		
		public void DrawSpectrum(ICollection<Point> points, IList<Complex> data, double width, double height)
		{
			var frequancyScaleFunc = FrequancyScaleFunc;
			var magnitudeScaleFunc = MagnitudeScaleFunc;
			var autoSensitive = AutoSensitive;

			GetVisuals(width, height,
				out var lowFrequency, out var topFrequency,
				out var _, out var lowWidth,
				out var widthStep, out var heightScale);

			points.Add(new Point(0d, height));

			foreach (var pair in data)
			{
				var sampleFrequency = pair.Real;
				if (topFrequency < sampleFrequency) break;
				var sampleValue = pair.Imaginary;
				var magnitude = ScaleMagnitude(sampleValue, magnitudeScaleFunc);
				var x = GetVisualOffset(sampleFrequency, widthStep, lowWidth, frequancyScaleFunc);
				var y = height - magnitude * heightScale;

				points.Add(new Point(x, y));

				if (autoSensitive)
				{
					if (magnitude > 0.7d * TopMagnitude)
					{
						AutoSensitiveTimestamp = DateTime.Now;
					}

					if (magnitude > TopMagnitude)
					{
						AutoSensitiveTimestamp = DateTime.Now;
						TopMagnitude *= (1 + AutoSensitiveStep * 9);
						EvokePropertyChanged(nameof(TopMagnitude));
					}
					else if (AutoSensitiveTimestamp.AddSeconds(AutoSensitiveDelayInSeconds) < DateTime.Now)
					{
						AutoSensitiveTimestamp = DateTime.Now;
						TopMagnitude *= (1 - AutoSensitiveStep);
						EvokePropertyChanged(nameof(TopMagnitude));
					}
				}
			}

			points.Add(new Point(width, height));
		}

		public void DrawTops(System.Collections.IList items, IList<PianoKey> keys, double width, double height)
		{	
			var showHz = ShowHz;
			var showNotes = ShowNotes;
			var maxMagnitude = TopMagnitude;

			var frequancyScaleFunc = FrequancyScaleFunc;
			var magnitudeScaleFunc = MagnitudeScaleFunc;
			var autoSensitive = AutoSensitive;

			GetVisuals(width, height,
				out var lowFrequency, out var topFrequency,
				out var _, out var lowWidth,
				out var widthStep, out var heightScale);

			foreach (var key in keys)
			{
				var sampleFrequency = key.Peak.Real;
				if (sampleFrequency < lowFrequency) continue;
				if (topFrequency < sampleFrequency) break;
				var sampleValue = key.Peak.Imaginary;
				var magnitude = ScaleMagnitude(sampleValue, magnitudeScaleFunc);
				var x = GetVisualOffset(sampleFrequency, widthStep, lowWidth, frequancyScaleFunc);

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
				var expressionLevel = (1d + sampleValue / TopMagnitude);

				if (showHz)
				{
					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 8.0 * expressionLevel,
						Foreground = AppPalette.HzBrush,
						Text = sampleFrequency.ToString("F")
					});
				}

				if (showNotes)
				{
					if (showHz)
					{
						panel.Children.Add(new TextBlock
						{
							Opacity = 0.5 * expressionLevel,
							FontSize = 8.0 * expressionLevel,
							Foreground = AppPalette.NoteBrush,
							Text = key.EthalonFrequency.ToString("F")
						});
					}

					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 12.0 * expressionLevel,
						Foreground = AppPalette.NoteBrush,
						Text = key.NoteName
					});
				}

				var y = magnitude * heightScale / 2;
				//y = sampleValue / MaxMagnitude;
				panel.UpdateLayout();
				if (double.IsInfinity(y)) continue;
				panel.Margin = new Thickness(x - panel.ActualWidth / 2, y - panel.ActualHeight / 2, 0, 0);
			}
		}

		public List<PianoKey> DrawPiano(System.Collections.IList items, IList<Complex> data, double width, double height)
		{
			var frequancyScaleFunc = FrequancyScaleFunc;
			var useNoteFilter = UseNoteFilter;
			var maxMagnitude0 = TopMagnitude;

			GetVisuals(width, height,
				out var lowFrequency, out var topFrequency,
				out var _, out var lowWidth,
				out var widthStep, out var heightScale);

			var keys = new List<PianoKey>();
			var oktavesCount = HalfTonesCount + 1;

			for (var oktaveNumber = 1; oktaveNumber < oktavesCount; oktaveNumber++)
			{
				var oktaveNotes = _baseOktaveFrequencySet.Select(d => d * Math.Pow(2, oktaveNumber)).ToArray();
				var notesCount = oktaveNotes.Length;
				for (var noteIndex = 0; noteIndex < notesCount; noteIndex++)
					PianoKey.Construct(oktaveNotes, noteIndex, oktaveNumber, DiesNotation[noteIndex]).AddTo(keys);
			}

			var m = 0;
			var averageMagnitude = 0d;
			foreach (var d in data)
			{
				var sampleFrequency = d.Real;
				if (sampleFrequency < lowFrequency) continue;
				if (topFrequency < sampleFrequency) break;
				var sampleValue = d.Imaginary;

				m++;
				averageMagnitude += sampleFrequency;

				var key =
					keys.FirstOrDefault(k => k.LowFrequency < sampleFrequency && sampleFrequency <= k.TopFrequency);
				if (key is null) continue;
				if (useNoteFilter)
				{
					var range = key.TopFrequency - key.LowFrequency;
					var half = key.EthalonFrequency - key.LowFrequency;
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
			foreach (var key in keys.Where(k => k.LowFrequency < topFrequency))
			{
				var gradientBrush = new LinearGradientBrush { EndPoint = new Point(0d, 1d) };
				var basicColor = OktaveBrushes[key.NoteNumber].As<SolidColorBrush>()?.Color ?? Colors.Transparent;
				var tmp = 255 * key.Magnitude / maxMagnitude;
				var red = tmp > 255 ? (byte)255 : (byte)tmp;
				var noteNumber = key.NoteNumber;
				var isTone = Tones[key.NoteNumber];
				red = isTone ? (byte)(basicColor.Red() - red) : red;
				var pressColor = isTone
					? Color.FromArgb(255, basicColor.Red(), red, red)
					: Color.FromArgb(255, red, basicColor.Green(), basicColor.Blue());

				gradientBrush.GradientStops.Merge
				(
					new GradientStop {Color = basicColor, Offset = 0.0d},
					new GradientStop {Color = pressColor, Offset = 0.5d}
				);

				var lowOffset = GetVisualOffset(key.LowFrequency, widthStep, lowWidth, frequancyScaleFunc);
				var topOffset = GetVisualOffset(key.TopFrequency, widthStep, lowWidth, frequancyScaleFunc);
				var ethalonOffset = GetVisualOffset(key.EthalonFrequency, widthStep, lowWidth, frequancyScaleFunc);
				var actualHeight = isTone ? height : height * 0.7d;
				var strokeThickness = topOffset - lowOffset;

				ConstructVerticalLine(ethalonOffset, actualHeight, gradientBrush, strokeThickness).AddToUntyped(items);
			}

			return tops;
		}

		private static Line ConstructVerticalLine(double offset, double length, Brush strokeBrush, double strokeThickness = 1d) => new Line
		{
			Y1 = 0d,
			Y2 = length,
			X1 = offset,
			X2 = offset,
			Stroke = strokeBrush,
			StrokeThickness = strokeThickness
		};
	}
}