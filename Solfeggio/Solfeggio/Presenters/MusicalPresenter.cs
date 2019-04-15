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
		[DataMember] public VisualStates Show { get; set; } = new VisualStates();
		[DataMember] public FrameOptions Frame { get; set; } = new FrameOptions();
		[DataMember] public MusicalOptions Music { get; set; } = new MusicalOptions();
		[DataMember] public SpectralOptions Spectrum { get; set; } = new SpectralOptions();

		[DataMember] public int MaxDominantsCount { get; set; } = 10;
		[DataMember] public string[] NumericFormats { get; } = new[] { "F0", "F1", "F2", "F3", "F4", "F5" };
		[DataMember] public bool UseNoteFilter { get; set; } = true;

		public void DrawMarkers(System.Collections.IList items, double width, double height,
			Brush lineBrush, Brush textBrush, IEnumerable<double> markers, double vLabelOffset = 0d)
		{
			Spectrum.Frequency.Threshold.Deconstruct(width,
				Spectrum.Frequency.VisualScaleFunc.To(out var hVisualScaleFunc),
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

				activeFrequency.
					Scale(hVisualScaleFunc).
					Stretch(hVisualStretchFactor).
					Decrement(hLowerVisualOffset).
					To(out var hVisualOffset);

				CreateVerticalLine(hVisualOffset, height, skipLabel ? opacityLineBrush : lineBrush).Use(items.Add);

				if (skipLabel) continue;

				var panel = new StackPanel();

				var fontSize = hVisualScaleFunc.Is(ScaleFuncs.Lineal) ? 12 : 8 * width / hVisualOffset;
				fontSize = fontSize > 20d ? 20d : fontSize;
				panel.Children.Add(new TextBlock
				{
					FontSize = fontSize,
					Foreground = textBrush,
					Text = activeFrequency.ToString(Spectrum.Frequency.NumericFormat)
				});

				items.Add(panel);
				panel.UpdateLayout();
				panel.Margin = new Thickness(hVisualOffset - panel.ActualWidth / 2d, height * vLabelOffset, 0d, 0d);
			}
		}

		public IEnumerable<double> EnumerateGrid(double frequencyStep)
		{
			Spectrum.Frequency.Threshold.Deconstruct(out _, out var upperFrequency);
			var startFrequency = 0d; //Math.Ceiling(lowerFrequency / frequencyStep) * frequencyStep;

			for (var value = startFrequency; value < upperFrequency; value += frequencyStep)
			{
				yield return value;
			}
		}

		public IEnumerable<double> EnumerateNotes(double breakFrequency = default)
		{
			Spectrum.Frequency.Threshold.Deconstruct(out _, out var upperFrequency);
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

		public delegate TOut Create<TOut>(in double x, in double y);
		public delegate TOut CreateWithContent<TIn, TOut>(in double x, in double y,
			TIn item, double activeMagnitude, double activeFreqency, double upperMagnitude);

		public delegate void Deconstruct<TIn, TOut>(in TIn p, out TOut h, out TOut v);

		private static bool TryMoveToStartPoint<TIn>(
			IEnumerator<TIn> enumerator,
			Deconstruct<TIn, double> deconstruct,
			double hLowerValue,
			double hUpperValue,
			out TIn startPoint)
		{
			startPoint = default;

			while (enumerator.MoveNext())
			{
				var currentPoint = enumerator.Current;
				deconstruct(in currentPoint, out var hActiveValue, out _);
				if (hActiveValue > hUpperValue) return false;
				if (hActiveValue >= hLowerValue)
				{
					if (startPoint.Is(default)) startPoint = currentPoint;
					return true;
				}

				startPoint = currentPoint;
			}

			return false;
		}

		public static IEnumerable<TOut> Draw<TIn, TOut>(
			IEnumerable<TIn> points,
			Create<TOut> create,
			CreateWithContent<TIn, TOut> createWithContent,
			Deconstruct<TIn, double> deconstruct,
			Bandwidth hBand, Bandwidth vBand,
			double hLength, double vLength,
			ScaleFunc hCorrection,
			ScaleFunc vCorrection)
		{
			hBand.Threshold.Deconstruct(hLength,
				hBand.VisualScaleFunc.To(out var hVisualScaleFunc),
				out var hLowerValue, out var hUpperValue,
				out var hLowerVisualOffset, out _,
				out var hVisualLengthStretchFactor);

			vBand.Threshold.Deconstruct(vLength,
				vBand.VisualScaleFunc.To(out var vVisualScaleFunc),
				out _, out var vUpperValue,
				out var vLowerVisualOffset, out _,
				out var vVisualLengthStretchFactor);

			points.GetEnumerator().To(out var enumerator);

			if (TryMoveToStartPoint(enumerator, deconstruct, hLowerValue, hUpperValue, out var activePoint).Not())
				yield break;

			vLowerVisualOffset.Increment(vLength).To(out var vZeroLevel);
			if (createWithContent is null) yield return create(0d, in vZeroLevel);

			do
			{
				deconstruct(in activePoint, out var hActiveValue, out var vActiveValue);

				var hVisualOffset = hActiveValue.
					Scale(hVisualScaleFunc).
					Stretch(hVisualLengthStretchFactor).
					Decrement(hLowerVisualOffset).
					Scale(hCorrection);

				var vVisualOffset = vActiveValue.
					Scale(vVisualScaleFunc).
					Stretch(vVisualLengthStretchFactor).
					Decrement(vLowerVisualOffset).
					Scale(vCorrection);

				yield return createWithContent is null
					? create(in hVisualOffset, in vVisualOffset)
					: createWithContent(in hVisualOffset, in vVisualOffset, activePoint, vActiveValue, hActiveValue, vUpperValue);

				if (hActiveValue <= hUpperValue && enumerator.MoveNext())
					activePoint = enumerator.Current;
				else break;

			} while (true);

			if (createWithContent is null) yield return create(in hLength, in vZeroLevel);
		}

		public IEnumerable<Point> DrawFrame(IEnumerable<Complex> frame, double width, double height) => Draw
		(
			frame,
			(in double x, in double y) => new Point(x, y), default,
			(in Complex p, out double h, out double v) => p.Deconstruct(out h, out v),
			Frame.Offset, Frame.Level,
			width, height,
			default,
			v => v.Negation().Increment(height)
		);

		public IEnumerable<Point> DrawPhase(IEnumerable<Bin> spectrum, double width, double height) => Draw
		(
			spectrum,
			(in double x, in double y) => new Point(x, y), default,
			(in Bin p, out double h, out double v) => p.Deconstruct(out h, out _, out v),
			Spectrum.Frequency, Spectrum.Phase,
			width, height,
			default,
			v => v.Negation().Increment(height)
		);

		public IEnumerable<Point> DrawMagnitude(IEnumerable<Bin> spectrum, double width, double height) => Draw
		(
			spectrum,
			(in double x, in double y) => new Point(x, y), default,
			(in Bin p, out double h, out double v) => p.Deconstruct(out h, out v, out _),
			Spectrum.Frequency, Spectrum.Magnitude,
			width, height,
			default,
			v => v.Negation().Increment(height)
		);

		public IEnumerable<StackPanel> DrawTops(IList<PianoKey> keys, double width, double height,
			bool showActualFrequncy, bool showActualMagnitude, bool showEthalonFrequncy, bool showNotes) => Draw
		(
			keys, default,
			(in double x, in double y,
				PianoKey item, double activeMagnitude, double activeFrequency, double upperMagnitude) =>
			{
				var panel = new StackPanel();
				var expressionLevel = 1d + activeMagnitude / upperMagnitude;
				expressionLevel = double.IsInfinity(expressionLevel) ? 1d : expressionLevel;

				EnumeratePanelContent(item, activeFrequency, activeMagnitude, expressionLevel,
					showActualFrequncy, showActualMagnitude, showEthalonFrequncy, showNotes).
					ForEach(panel.Children.Add);

				panel.Margin = new Thickness(x, y, 0d, 0d);
				return panel;
			},
			(in PianoKey p, out double h, out double v) => p.Peak.Deconstruct(out h, out v, out _),
			Spectrum.Frequency, Spectrum.Magnitude,
			width, height, default, default
		);

		private IEnumerable<TextBlock> EnumeratePanelContent(
			PianoKey key, double activeFrequency, double activeMagnitude, double expressionLevel,
			bool showActualFrequncy, bool showActualMagnitude, bool showEthalonFrequncy, bool showNotes)
		{
			if (showActualFrequncy) yield return new TextBlock
			{
				Opacity = 0.5 * expressionLevel,
				FontSize = 8.0 * expressionLevel,
				Foreground = AppPalette.HzBrush,
				Text = activeFrequency.ToString(Spectrum.Frequency.NumericFormat)
			};

			if (showActualMagnitude) yield return new TextBlock
			{
				Opacity = 0.5 * expressionLevel,
				FontSize = 8.0 * expressionLevel,
				Foreground = AppPalette.HzBrush,
				Text = activeMagnitude.ToString(Spectrum.Magnitude.NumericFormat)
			};

			if (showEthalonFrequncy) yield return new TextBlock
			{
				Opacity = 0.5 * expressionLevel,
				FontSize = 8.0 * expressionLevel,
				Foreground = AppPalette.NoteBrush,
				Text = key.EthalonFrequency.ToString(Spectrum.Frequency.NumericFormat)
			};

			if (showNotes) yield return new TextBlock
			{
				Opacity = 0.5 * expressionLevel,
				FontSize = 12.0 * expressionLevel,
				Foreground = AppPalette.NoteBrush,
				Text = key.NoteName
			};
		}

		public List<PianoKey> DrawPiano(System.Collections.IList items, IList<Bin> data, double width, double height)
		{
			Spectrum.Magnitude.Threshold.Deconstruct(out var lowMagnitude, out var upperMagnitude);

			var vVisualStretchFactor = height.Squeeze(upperMagnitude);
			var useNoteFilter = UseNoteFilter;
			var noteNames = Music.ActiveNotation.Value ?? (Music.ActiveNotation = Music.Notations.First()).Value;

			Spectrum.Frequency.Threshold.Deconstruct(width,
				Spectrum.Frequency.VisualScaleFunc.To(out var frequencyVisualScaleFunc),
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
				if (key.Is(default)) continue;
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
					new GradientStop { Color = basicColor, Offset = 0.0d },
					new GradientStop { Color = pressColor, Offset = 0.5d }
				);

				var lowerOffset = frequencyVisualScaleFunc(key.LowerFrequency).Stretch(hVisualStretchFactor).Decrement(hLowerVisualOffset);
				var upperOffset = frequencyVisualScaleFunc(key.UpperFrequency).Stretch(hVisualStretchFactor).Decrement(hLowerVisualOffset);
				var ethalonOffset = frequencyVisualScaleFunc(key.EthalonFrequency).Stretch(hVisualStretchFactor).Decrement(hLowerVisualOffset);

				var actualHeight = isTone ? height : height * 0.7d;
				var strokeThickness = upperOffset - lowerOffset;

				CreateVerticalLine(ethalonOffset, actualHeight, gradientBrush, strokeThickness).Use(items.Add);
			}

			return dominants;
		}

		private static Line CreateVerticalLine(double offset, double length, Brush strokeBrush, double strokeThickness = 1d) => new Line
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