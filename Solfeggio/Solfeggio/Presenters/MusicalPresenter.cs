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
	public delegate double ScaleFunc(double value);

	public static class ScaleFuncs
	{
		public static double Lineal(double value) => value;
		public static double Log2(double value) => value.Is(0d) ? 0d : Math.Log(value, 2d);
		public static double Log(double value) => value.Is(0d) ? 0d : Math.Log(value);
		public static double Exp(double value) => Math.Exp(value);
	}

	[DataContract]
	public class MusicalPresenter : ContextObject
	{
		public class PianoKey
		{
			public int NoteNumber { get; set; }
			public string NoteName { get; set; }
			public double Magnitude { get; set; }
			public double LowFrequency { get; set; }
			public double TopFrequency { get; set; }
			public double EthalonFrequency { get; set; }
			public double DeltaFrequency => Peak.Real - EthalonFrequency;
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
					EthalonFrequency = ethalonFrequency,
				};
			}
		}

		[DataContract]
		public class VisualStates
		{
			[DataMember] public bool Dominants { get; set; } = true;
			[DataMember] public bool ActualFrequncy { get; set; } = true;
			[DataMember] public bool EthalonFrequncy { get; set; } = true;
			[DataMember] public bool Notes { get; set; } = true;
			[DataMember] public bool Wave { get; set; } = false;
			[DataMember] public bool Spectrum { get; set; } = true;
			[DataMember] public bool NotesGrid { get; set; } = false;
			[DataMember] public bool DiscreteGrid { get; set; } = true;
		}

		[DataMember] public VisualStates Show { get; set; } = new VisualStates();

		[DataMember] public int MaxDominantsCount { get; set; } = 10;
		[DataMember] public double LowMagnitude { get; set; } = 0d;
		[DataMember] public double TopMagnitude { get; set; } = 1d;
		[DataMember] public double LowFrequency { get; set; } = 20d;
		[DataMember] public double TopFrequency { get; set; } = 3000d;

		[DataMember] public bool UseNoteFilter { get; set; }
		[DataMember] public bool AutoSensitive { get; set; } = true;
		[DataMember] public double AutoSensitiveStep { get; set; } = 0.02d;
		[DataMember] public double AutoSensitiveDelayInSeconds { get; set; } = 0.03d;
		[DataMember] public DateTime AutoSensitiveTimestamp { get; set; }
		[DataMember] public string NumericFormat { get; set; } = "F0";

		[DataMember] public string[] NumericFormats { get; }  = new[] { "F0", "F1", "F2", "F3" };

		[DataMember] public double[] PitchStandards { get; } = new[] { 415d, 420d, 432d, 435d, 440d, 444d };
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

		private static readonly ScaleFunc[] AllScaleFuncs = new ScaleFunc[]
			{ ScaleFuncs.Lineal, ScaleFuncs.Log2, ScaleFuncs.Log, ScaleFuncs.Exp };

		[DataMember] public ScaleFunc[] InlinedScaleFuncs { get; set; } = AllScaleFuncs;
		[DataMember] public ScaleFunc[] FrequancyScaleFuncs { get; set; } = AllScaleFuncs;
		[DataMember] public ScaleFunc[] MagnitudeScaleFuncs { get; set; } = AllScaleFuncs;

		[DataMember] public ScaleFunc FrequancyScaleFunc { get; set; } = ScaleFuncs.Log2;
		[DataMember] public ScaleFunc MagnitudeScaleFunc { get; set; } = ScaleFuncs.Lineal;
		[DataMember] public ScaleFunc PhaseScaleFunc { get; set; } = ScaleFuncs.Lineal;

		protected static readonly string[] DiesNotation =
			{"C","C♯","D","D♯","E","F","F♯","G","G♯","A","A♯","B"};

		protected static readonly string[] BemoleNotation =
			{"C","D♭","D","E♭","E","F","G♭","G","A♭","A","B♭","B"};

		protected static readonly string[] Notes =
			{"C|B♯","C♯|D♭","D","D♯|E♭","E|F♭","F|E♯","F♯|G♭","G","G♯|A♭","A","A♯|B♭","B|C♭"};

		protected static readonly bool[] Tones = Notes.Select(n => n.Contains("♯|").Not()).ToArray();
		protected static readonly Brush[] OktaveBrushes =
			Tones.Select(t => t ? AppPalette.FullToneKeyBrush : AppPalette.HalfToneKeyBrush).Cast<Brush>().ToArray();

		protected static double HalfTonesCount { get; } = 12;
		private static double GetBaseFrequancy(double pitchStandard) => pitchStandard / 16d;
		private static double GetHalftoneStep(double halfTonesCount) => Math.Pow(2d, 1d / halfTonesCount);

		private static double[] GetBaseOktaveFrequencySet(double pitchStandard) =>
			GetBaseOktaveFrequencySet(GetBaseFrequancy(pitchStandard), GetHalftoneStep(HalfTonesCount));

		private static double[] GetBaseOktaveFrequencySet(double baseFrequancy, double halftoneStep) =>
			Enumerable.Range(-9, 12).Select(dt => baseFrequancy * Math.Pow(halftoneStep, dt)).ToArray();

		protected double[] _baseOktaveFrequencySet = GetBaseOktaveFrequencySet(DefaultPitchStandard);

		private void SetVariables(double visualSize, out double visualStretchFactor,
			out double lowVisualIncrementOffset, out double topVisualIncrementOffset,
			out double lowFrequency, out double topFrequency)
		{
			lowFrequency = LowFrequency;
			topFrequency = TopFrequency;

			var frequancyScaleFunc = FrequancyScaleFunc;
			var lowLogicalOffset = frequancyScaleFunc(lowFrequency);
			var topLogicalOffset = frequancyScaleFunc(topFrequency);

			lowVisualIncrementOffset = visualSize * lowLogicalOffset / (topLogicalOffset - lowLogicalOffset);
			topVisualIncrementOffset = visualSize * topLogicalOffset / (topLogicalOffset - lowLogicalOffset);
			visualStretchFactor = topVisualIncrementOffset / topLogicalOffset;
		}

		public void DrawMarkers(System.Collections.IList items, double width, double height,
			Brush lineBrush, Brush textBrush, IEnumerable<double> markers, double vLabelOffset = 0d)
		{
			SetVariables(width, out var hVisualStretchFactor,
				out var hVisualDecrementOffset, out var _,
				out var lowFrequency, out var topFrequency);

			var frequancyScaleFunc = FrequancyScaleFunc;

			var allMarkers = markers.ToArray();
			var skip = allMarkers.Length > 8 ? allMarkers.Length / 8 : 0;
			var i = 0;
			var opacityLineBrush = lineBrush.Clone();
			opacityLineBrush.Opacity *= 0.3;
			opacityLineBrush.Freeze();

			foreach (var activeFrequency in markers)
			{
				var skipLabel = skip > 0 && i++ % skip > 0;

				var hVisualOffset = frequancyScaleFunc(activeFrequency).Stretch(hVisualStretchFactor).Decrement(hVisualDecrementOffset);
				if (hVisualOffset < 0) continue;
				ConstructVerticalLine(hVisualOffset, height, skipLabel ? opacityLineBrush : lineBrush).Use(items.Add);

				if (skipLabel) continue;

				var panel = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};

				var fontSize = frequancyScaleFunc.Is(ScaleFuncs.Lineal) ? 12 : 8 * width / hVisualOffset;
				fontSize = fontSize > 20d ? 20d : fontSize;
				panel.Children.Add(new TextBlock
				{
					FontSize = fontSize,
					Foreground = textBrush,
					Text = activeFrequency.ToString(NumericFormat)
				});

				items.Add(panel);
				panel.UpdateLayout();
				panel.Margin = new Thickness(hVisualOffset - panel.ActualWidth/2d, height * vLabelOffset, 0d, 0d);
			}
		}

		public IEnumerable<double> EnumerateGrid(double frequencyStep)
		{
			var finishFrequency = TopFrequency;
			var startFrequency = 0d; //Math.Ceiling(LowFrequency / frequencyStep) * frequencyStep;

			for (var value = startFrequency; value < finishFrequency; value += frequencyStep)
			{
				yield return value;
			}
		}

		public IEnumerable<double> EnumerateNotes(double breakFrequancy = default)
		{
			breakFrequancy = breakFrequancy.Is(default) ? TopFrequency : breakFrequancy;

			for (var j = 0; ; j++)
			{
				for (var i = 0; i < _baseOktaveFrequencySet.Length; i++)
				{
					var note = _baseOktaveFrequencySet[i] * Math.Pow(2, j);
					if (note > breakFrequancy) yield break;
					else yield return note;
				}
			}
		}

		public void DrawWave(ICollection<Point> points, IList<Complex> data, double width, double height)
		{
			var vIncrementOffset = height / 2d;
			var vStretchFactor = height / 4d;
			var hStretchFactor = width / data.Count;
			foreach (var pair in data)
			{
				pair.Deconstruct(out var frequancy, out var magnitude);
				var x = frequancy.Stretch(hStretchFactor);
				var y = magnitude.Stretch(vStretchFactor).Increment(vIncrementOffset);
				points.Add(new Point(x, y));
			}
		}
		
		public void DrawSpectrum(ICollection<Point> points, IList<Complex> data, double width, double height)
		{
			SetVariables(width, out var hVisualStretchFactor,
				out var hVisualDecrementOffset, out var _,
				out var lowFrequency, out var topFrequency);

			var vVisualStretchFactor = height.Stretch(0.7d).Squeeze(TopMagnitude);
			var frequancyScaleFunc = FrequancyScaleFunc;
			var magnitudeScaleFunc = MagnitudeScaleFunc;
			var autoSensitive = AutoSensitive;

			points.Add(new Point(0d, height));

			foreach (var pair in data)
			{
				pair.Deconstruct(out var activeFrequency, out var activeMagnitude);
				if (activeFrequency < lowFrequency) continue;
				if (activeFrequency > topFrequency) break;

				var hVisualOffset = frequancyScaleFunc(activeFrequency).Stretch(hVisualStretchFactor).Decrement(hVisualDecrementOffset);
				var vVisualOffset = magnitudeScaleFunc(activeMagnitude).Stretch(vVisualStretchFactor).Negation().Increment(height);

				points.Add(new Point(hVisualOffset, vVisualOffset));

				if (autoSensitive)
				{
					if (activeMagnitude > 0.7d * TopMagnitude)
					{
						AutoSensitiveTimestamp = DateTime.Now;
					}

					if (activeMagnitude > TopMagnitude)
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

		public void DrawTops(System.Collections.IList items, IList<PianoKey> keys, double width, double height,
			bool showActualFrequncy, bool showEthlonFrequncy, bool showNotes)
		{
			SetVariables(width, out var hVisualStretchFactor,
				out var hVisualDecrementOffset, out var _,
				out var lowFrequency, out var topFrequency);

			var topMagnitude = TopMagnitude;
			var vVisualStretchFactor = height.Stretch(0.7d).Squeeze(topMagnitude);

			var frequancyScaleFunc = FrequancyScaleFunc;
			var magnitudeScaleFunc = MagnitudeScaleFunc;
			var autoSensitive = AutoSensitive;

			foreach (var key in keys)
			{
				key.Peak.Deconstruct(out var activeFrequency, out var activeMagnitude);
				if (activeFrequency < lowFrequency) continue;
				if (activeFrequency > topFrequency) break;

				var hVisualOffset = frequancyScaleFunc(activeFrequency).Stretch(hVisualStretchFactor).Decrement(hVisualDecrementOffset);
				var vVisualOffset = magnitudeScaleFunc(activeMagnitude).Stretch(vVisualStretchFactor);

				if (showActualFrequncy.Not() && showEthlonFrequncy.Not() && showNotes.Not()) return;

				var panel = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};

#if !NETSTANDARD
				// todo
				items.Add(panel);
#endif
				var expressionLevel = (1d + activeMagnitude / topMagnitude);
				expressionLevel = double.IsInfinity(expressionLevel) ? 1d : expressionLevel;

				if (showActualFrequncy)
				{
					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 8.0 * expressionLevel,
						Foreground = AppPalette.HzBrush,
						Text = activeFrequency.ToString(NumericFormat)
					});
				}

				if (showEthlonFrequncy)
				{
					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 8.0 * expressionLevel,
						Foreground = AppPalette.NoteBrush,
						Text = key.EthalonFrequency.ToString(NumericFormat)
					});
				}

				if (showNotes)
				{
					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 12.0 * expressionLevel,
						Foreground = AppPalette.NoteBrush,
						Text = key.NoteName
					});
				}

				if (double.IsInfinity(vVisualOffset)) continue;

				panel.UpdateLayout();
				panel.Margin = new Thickness(hVisualOffset - panel.ActualWidth / 2d, vVisualOffset - panel.ActualHeight / 2d, 0d, 0d);
			}
		}

		public List<PianoKey> DrawPiano(System.Collections.IList items, IList<Complex> data, double width, double height)
		{
			SetVariables(width, out var hVisualStretchFactor,
				out var hVisualDecrementOffset, out var _,
				out var lowFrequency, out var topFrequency);

			var vVisualStretchFactor = height.Squeeze(TopMagnitude);
			var frequancyScaleFunc = FrequancyScaleFunc;
			var useNoteFilter = UseNoteFilter;
			var topMagnitude = TopMagnitude;

			var keys = new List<PianoKey>();
			var oktavesCount = HalfTonesCount + 1;

			for (var oktaveNumber = 1; oktaveNumber < oktavesCount; oktaveNumber++)
			{
				var oktaveNotes = _baseOktaveFrequencySet.Select(d => d * Math.Pow(2, oktaveNumber)).ToArray();
				var notesCount = oktaveNotes.Length;
				for (var noteIndex = 0; noteIndex < notesCount; noteIndex++)
					PianoKey.Construct(oktaveNotes, noteIndex, oktaveNumber, DiesNotation[noteIndex]).Use(keys.Add);
			}

			var m = 0;
			var averageMagnitude = 0d;
			foreach (var peak in data)
			{
				peak.Deconstruct(out var activeFrequency, out var activeMagnitude);
				if (activeFrequency < lowFrequency) continue;
				if (activeFrequency > topFrequency) break;

				m++;
				averageMagnitude += activeMagnitude;

				var key =
					keys.FirstOrDefault(k => k.LowFrequency < activeFrequency && activeFrequency <= k.TopFrequency);
				if (key is null) continue;
				if (useNoteFilter)
				{
					var range = key.TopFrequency - key.LowFrequency;
					var half = key.EthalonFrequency - key.LowFrequency;
					activeMagnitude = (float)(activeMagnitude * Rainbow.Windowing.Gausse(half, range));
				}

				key.Hits++;
				if (activeMagnitude > key.Magnitude)
				{
					key.Magnitude = activeMagnitude;
					key.Peak = peak;
				}
			}

			averageMagnitude /= m;
			var minMagnitude = TopMagnitude * 0.2;
			var dominants = keys.OrderByDescending(k => k.Magnitude).Take(MaxDominantsCount)
				.Where(k => k.Magnitude > minMagnitude).ToList();

			//keys.ForEach(k => k.Magnitude = k.Magnitude/k.Hits);
			keys.ForEach(k => k.Magnitude *= k.Magnitude);
			var maxMagnitude = topMagnitude * topMagnitude * 0.32; // keys.Max(k => k.Magnitude);
																	 //if (MaxMagnitude1 > maxMagnitude) maxMagnitude = MaxMagnitude1*0.7;

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

				var lowOffset = frequancyScaleFunc(key.LowFrequency).Stretch(hVisualStretchFactor).Decrement(hVisualDecrementOffset);
				var topOffset = frequancyScaleFunc(key.TopFrequency).Stretch(hVisualStretchFactor).Decrement(hVisualDecrementOffset);
				var ethalonOffset = frequancyScaleFunc(key.EthalonFrequency).Stretch(hVisualStretchFactor).Decrement(hVisualDecrementOffset);

				var actualHeight = isTone ? height : height * 0.7d;
				var strokeThickness = topOffset - lowOffset;

				ConstructVerticalLine(ethalonOffset, actualHeight, gradientBrush, strokeThickness).Use(items.Add);
			}

			return dominants;
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