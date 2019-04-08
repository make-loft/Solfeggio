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
	[DataContract]
	public class MusicalPresenter : ContextObject
	{
		private static SmartRange ConstructRange(double lower, double upper) => new SmartRange
		{
			Lower = lower,
			Upper = upper
		};

		[DataMember]
		public Bandwidth Frequency { get; set; } = new Bandwidth
		{
			Limit = ConstructRange(10d, 22000d),
			Threshold = ConstructRange(20d, 3000d),
			VisualScaleFunc = ScaleFuncs.Log2,
			NumericFormat = "F0",
		};

		[DataMember]
		public Bandwidth Magnitude { get; set; } = new Bandwidth
		{
			Limit = ConstructRange(0.00d, 1d),
			Threshold = ConstructRange(0.00d, 0.4d),
			IsVisible = true
		};

		[DataMember]
		public Bandwidth Phase { get; set; } = new Bandwidth
		{
			Limit = ConstructRange(-Pi.Single, +Pi.Single),
			Threshold = ConstructRange(-Pi.Single, +Pi.Single),
		};

		[DataMember]
		public Bandwidth Level { get; set; } = new Bandwidth
		{
			Limit = ConstructRange(-1d, +1d),
			Threshold = ConstructRange(-1d, +1d),
			VisualScaleFunc = ScaleFuncs.Lineal,
		};

		[DataMember]
		public Bandwidth Time { get; set; } = new Bandwidth
		{
			Limit = ConstructRange(+0d, +1d),
			Threshold = ConstructRange(+0d, +1d),
			VisualScaleFunc = ScaleFuncs.Lineal,
		};

		[DataMember] public MusicalOptions Music { get; set; } = new MusicalOptions();

		[DataMember] public int MaxDominantsCount { get; set; } = 10;

		[DataMember] public string[] NumericFormats { get; } = new[] { "F0", "F1", "F2", "F3", "F4", "F5" };
		[DataMember] public VisualStates Show { get; set; } = new VisualStates();

		[DataMember] public bool UseNoteFilter { get; set; }

		public void DrawMarkers(System.Collections.IList items, double width, double height,
			Brush lineBrush, Brush textBrush, IEnumerable<double> markers, double vLabelOffset = 0d)
		{
			Frequency.Threshold.Deconstruct(width,
				Frequency.VisualScaleFunc.To(out var frequencyVisualScaleFunc),
				out var lowerFrequency, out var upperFrequency,
				out var hLowerVisualOffset, out _,
				out var hVisualStretchFactor);

			var allMarkers = markers.ToArray();
			var skip = allMarkers.Length > 8 ? allMarkers.Length / 8 : 0;
			var i = 0;
			var opacityLineBrush = lineBrush.Clone();
			opacityLineBrush.Opacity *= 0.3;
			opacityLineBrush.Freeze();

			foreach (var activeFrequency in markers)
			{
				var skipLabel = skip > 0 && i++ % skip > 0;
				if (activeFrequency < lowerFrequency) continue;
				if (activeFrequency > upperFrequency) break;

				frequencyVisualScaleFunc(activeFrequency).
					Stretch(hVisualStretchFactor).
					Decrement(hLowerVisualOffset).
					To(out var hVisualOffset);

				ConstructVerticalLine(hVisualOffset, height, skipLabel ? opacityLineBrush : lineBrush).Use(items.Add);

				if (skipLabel) continue;

				var panel = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};

				var fontSize = frequencyVisualScaleFunc.Is(ScaleFuncs.Lineal) ? 12 : 8 * width / hVisualOffset;
				fontSize = fontSize > 20d ? 20d : fontSize;
				panel.Children.Add(new TextBlock
				{
					FontSize = fontSize,
					Foreground = textBrush,
					Text = activeFrequency.ToString(Frequency.NumericFormat)
				});

				items.Add(panel);
				panel.UpdateLayout();
				panel.Margin = new Thickness(hVisualOffset - panel.ActualWidth/2d, height * vLabelOffset, 0d, 0d);
			}
		}

		public IEnumerable<double> EnumerateGrid(double frequencyStep)
		{
			Frequency.Threshold.Deconstruct(out _, out var upperFrequency);
			var startFrequency = 0d; //Math.Ceiling(lowerFrequency / frequencyStep) * frequencyStep;

			for (var value = startFrequency; value < upperFrequency; value += frequencyStep)
			{
				yield return value;
			}
		}

		public IEnumerable<double> EnumerateNotes(double breakFrequency = default)
		{
			Frequency.Threshold.Deconstruct(out _, out var upperFrequency);
			breakFrequency = breakFrequency.Is(default) ? upperFrequency : breakFrequency;

			for (var j = 0; ; j++)
			{
				for (var i = 0; i < Music.BaseOktaveFrequencySet.Length; i++)
				{
					var note = Music.BaseOktaveFrequencySet[i] * Math.Pow(2, j);
					if (note > breakFrequency) yield break;
					else yield return note;
				}
			}
		}

		public IEnumerable<Point> DrawFrame(IList<Complex> data, double width, double height)
		{
			var hStretchFactor = width / data.Count;

			Level.Threshold.Deconstruct(height,
				Level.VisualScaleFunc.To(out var vVisualScaleFunc),
				out _, out _,
				out var vLowerVisualOffset, out _,
				out var vVisualLengthStretchFactor);

			foreach (var pair in data)
			{
				pair.Deconstruct(out var binOffset, out var magnitude);

				binOffset.
					Stretch(hStretchFactor).
					To(out var hVisualOffset);

				vVisualScaleFunc(magnitude).
					Stretch(vVisualLengthStretchFactor).
					Decrement(vLowerVisualOffset).
					Negation().Increment(height).
					To(out var vVisualOffset);

				yield return new Point(hVisualOffset, vVisualOffset);
			}
		}

		public IEnumerable<Point> DrawPhase(IEnumerable<Bin> spectrum, double width, double height)
		{
			Frequency.Threshold.Deconstruct(width,
				Frequency.VisualScaleFunc.To(out var hVisualScaleFunc),
				out var lowerFrequency, out var upperFrequency,
				out var hLowerVisualOffset, out _,
				out var hVisualLengthStretchFactor);

			Phase.Threshold.Deconstruct(height,
				Phase.VisualScaleFunc.To(out var vVisualScaleFunc),
				out var lowerPhase, out var upperPhase,
				out var vLowerVisualOffset, out _,
				out var vVisualLengthStretchFactor);

			foreach (var bin in spectrum)
			{
				bin.Deconstruct(out var activeFrequency, out _, out var activePhase);
				if (activeFrequency < lowerFrequency) continue;
				if (activeFrequency > upperFrequency) break;

				hVisualScaleFunc(activeFrequency).
					Stretch(hVisualLengthStretchFactor).
					Decrement(hLowerVisualOffset).
					To(out var hVisualOffset);

				vVisualScaleFunc(activePhase).
					Stretch(vVisualLengthStretchFactor).
					Decrement(vLowerVisualOffset).
					Negation().Increment(height).
					To(out var vVisualOffset);

				yield return new Point(hVisualOffset, vVisualOffset);
			}
		}

		public IEnumerable<Point> DrawSpectrum(IEnumerable<Bin> spectrum, double width, double height)
		{
			Frequency.Threshold.Deconstruct(width,
				Frequency.VisualScaleFunc.To(out var frequencyVisualScaleFunc),
				out var lowerFrequency, out var upperFrequency,
				out var hLowerVisualOffset, out _,
				out var hVisualLengthStretchFactor);

			Magnitude.Threshold.Deconstruct(height,
				Magnitude.VisualScaleFunc.To(out var magnitudeVisualScaleFunc),
				out var lowerMagnitude, out var upperMagnitude,
				out var vLowerVisualOffset, out _,
				out var vVisualStretchFactor);

			yield return new Point(0d, height);

			foreach (var bin in spectrum)
			{
				bin.Deconstruct(out var activeFrequency, out var activeMagnitude, out _);
				if (activeFrequency < lowerFrequency) continue;
				if (activeFrequency > upperFrequency) break;

				frequencyVisualScaleFunc(activeFrequency).
					Stretch(hVisualLengthStretchFactor).
					Decrement(hLowerVisualOffset).
					To(out var hVisualOffset);

				magnitudeVisualScaleFunc(activeMagnitude).
					Stretch(vVisualStretchFactor).
					Decrement(vLowerVisualOffset).
					Negation().Increment(height).
					To(out var vVisualOffset);

				yield return new Point(hVisualOffset, vVisualOffset);
			}

			yield return new Point(width, height);
		}

		public void DrawTops(System.Collections.IList items, IList<PianoKey> keys, double width, double height,
			bool showActualFrequncy, bool showActualMagnitude, bool showEthalonFrequncy, bool showNotes)
		{
			Frequency.Threshold.Deconstruct(width,
				Frequency.VisualScaleFunc.To(out var frequencyVisualScaleFunc),
				out var lowerFrequency, out var upperFrequency,
				out var hLowerVisualOffset, out _,
				out var hVisualStretchFactor);

			Magnitude.Threshold.Deconstruct(height,
				Magnitude.VisualScaleFunc.To(out var magnitudeVisualScaleFunc),
				out var lowerMagnitude, out var upperMagnitude,
				out var vLowerVisualOffset, out _,
				out var vVisualStretchFactor);

			foreach (var key in keys)
			{
				key.Peak.Deconstruct(out var activeFrequency, out var activeMagnitude, out _);
				if (activeFrequency < lowerFrequency) continue;
				if (activeFrequency > upperFrequency) break;

				frequencyVisualScaleFunc(activeFrequency).
					Stretch(hVisualStretchFactor).
					Decrement(hLowerVisualOffset).
					To(out var hVisualOffset);

				magnitudeVisualScaleFunc(activeMagnitude).
					Stretch(vVisualStretchFactor).
					Decrement(vLowerVisualOffset).
					To(out var vVisualOffset);

				if (showActualFrequncy.Not() && showEthalonFrequncy.Not() && showNotes.Not()) return;

				var panel = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					VerticalAlignment = VerticalAlignment.Top
				};

#if !NETSTANDARD
				// todo
				items.Add(panel);
#endif
				var expressionLevel = (1d + activeMagnitude / upperMagnitude);
				expressionLevel = double.IsInfinity(expressionLevel) ? 1d : expressionLevel;

				if (showActualFrequncy)
				{
					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 8.0 * expressionLevel,
						Foreground = AppPalette.HzBrush,
						Text = activeFrequency.ToString(Frequency.NumericFormat)
					});
				}

				if (showActualMagnitude)
				{
					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 8.0 * expressionLevel,
						Foreground = AppPalette.HzBrush,
						Text = activeMagnitude.ToString(Magnitude.NumericFormat)
					});
				}

				if (showEthalonFrequncy)
				{
					panel.Children.Add(new TextBlock
					{
						Opacity = 0.5 * expressionLevel,
						FontSize = 8.0 * expressionLevel,
						Foreground = AppPalette.NoteBrush,
						Text = key.EthalonFrequency.ToString(Frequency.NumericFormat)
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

				if (double.IsInfinity(vVisualOffset) || double.IsNaN(vVisualOffset)) continue;

				panel.UpdateLayout();
				panel.Margin = new Thickness(hVisualOffset - panel.ActualWidth / 2d, vVisualOffset - panel.ActualHeight / 2d, 0d, 0d);
			}
		}

		public List<PianoKey> DrawPiano(System.Collections.IList items, IList<Bin> data, double width, double height)
		{
			Magnitude.Threshold.Deconstruct(out var lowMagnitude, out var upperMagnitude);

			var vVisualStretchFactor = height.Squeeze(upperMagnitude);
			var frequencyVisualScaleFunc = Frequency.VisualScaleFunc;
			var useNoteFilter = UseNoteFilter;
            var noteNames = Music.ActiveNotation.Value ?? (Music.ActiveNotation = Music.Notations.First()).Value;

			Frequency.Threshold.Deconstruct(width, frequencyVisualScaleFunc,
				out var lowerFrequency, out var upperFrequency,
				out var hLowerVisualOffset, out _,
				out var hVisualStretchFactor);

			var keys = new List<PianoKey>();
			var oktavesCount = MusicalOptions.HalfTonesCount + 1;

			for (var oktaveNumber = 1; oktaveNumber < oktavesCount; oktaveNumber++)
			{
				var oktaveNotes = Music.BaseOktaveFrequencySet.Select(d => d * Math.Pow(2, oktaveNumber)).ToArray();
				var notesCount = oktaveNotes.Length;
				for (var noteIndex = 0; noteIndex < notesCount; noteIndex++)
					PianoKey.Construct(oktaveNotes, noteIndex, oktaveNumber, noteNames[noteIndex]).Use(keys.Add);
			}

			var m = 0;
			var averageMagnitude = 0d;
			foreach (var bin in data)
			{
				bin.Deconstruct(out var activeFrequency, out var activeMagnitude, out _);
				if (activeFrequency < lowerFrequency) continue;
				if (activeFrequency > upperFrequency) break;

				m++;
				averageMagnitude += activeMagnitude;

				var key =
					keys.FirstOrDefault(k => k.LowerFrequency < activeFrequency && activeFrequency <= k.UpperFrequency);
				if (key is null) continue;
				if (useNoteFilter)
				{
					var range = key.UpperFrequency - key.LowerFrequency;
					var half = key.EthalonFrequency - key.LowerFrequency;
					activeMagnitude = (float)(activeMagnitude * Windowing.Gausse(half, range));
				}

				key.Hits++;
				if (activeMagnitude > key.Magnitude)
				{
					key.Magnitude = activeMagnitude;
					key.Peak = bin;
				}
			}

			averageMagnitude /= m;
			var minMagnitude = upperMagnitude * 0.2;
			var dominants = keys.OrderByDescending(k => k.Magnitude).Take(MaxDominantsCount)
				.Where(k => k.Magnitude > minMagnitude).ToList();

			//keys.ForEach(k => k.Magnitude = k.Magnitude/k.Hits);
			keys.ForEach(k => k.Magnitude *= k.Magnitude);
			var maxMagnitude = upperMagnitude * upperMagnitude * 0.32; // keys.Max(k => k.Magnitude);
																	 //if (MaxMagnitude1 > maxMagnitude) maxMagnitude = MaxMagnitude1*0.7;

			foreach (var key in keys.Where(k => k.LowerFrequency < upperFrequency))
			{
				var gradientBrush = new LinearGradientBrush { EndPoint = new Point(0d, 1d) };
				var basicColor = MusicalOptions.OktaveBrushes[key.NoteNumber].As<SolidColorBrush>()?.Color ?? Colors.Transparent;
				var tmp = 255 * key.Magnitude / maxMagnitude;
				var red = tmp > 255 ? (byte)255 : (byte)tmp;
				var noteNumber = key.NoteNumber;
				var isTone = MusicalOptions.Tones[key.NoteNumber];
				red = isTone ? (byte)(basicColor.Red() - red) : red;
				var pressColor = isTone
					? Color.FromArgb(255, basicColor.Red(), red, red)
					: Color.FromArgb(255, red, basicColor.Green(), basicColor.Blue());

				gradientBrush.GradientStops.Merge
				(
					new GradientStop {Color = basicColor, Offset = 0.0d},
					new GradientStop {Color = pressColor, Offset = 0.5d}
				);

				var lowerOffset = frequencyVisualScaleFunc(key.LowerFrequency).Stretch(hVisualStretchFactor).Decrement(hLowerVisualOffset);
				var upperOffset = frequencyVisualScaleFunc(key.UpperFrequency).Stretch(hVisualStretchFactor).Decrement(hLowerVisualOffset);
				var ethalonOffset = frequencyVisualScaleFunc(key.EthalonFrequency).Stretch(hVisualStretchFactor).Decrement(hLowerVisualOffset);

				var actualHeight = isTone ? height : height * 0.7d;
				var strokeThickness = upperOffset - lowerOffset;

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