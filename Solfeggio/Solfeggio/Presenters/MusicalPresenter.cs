using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Ace;
using Rainbow;
using Solfeggio.Models;

#if NETSTANDARD
using Xamarin.Forms;
using Colors = Xamarin.Forms.Color;
using Thickness = Xamarin.Forms.Thickness;
#else
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
#endif

namespace Solfeggio.Presenters
{
	[DataContract]
	public class MusicalPresenter : ContextObject, IExposable
	{
		[DataMember] public FrameOptions Frame { get; set; } = new();
		[DataMember] public MusicalOptions Music { get; set; } = new();
		[DataMember] public SpectralOptions Spectrum { get; set; } = new();
		[DataMember] public VisualProfile VisualProfile { get; set; } = new();

		[DataMember] public bool UseNoteFilter { get; set; } = true;
		[DataMember] public int MaxHarmonicsCount { get; set; } = 10;
		[DataMember] public string[] NumericFormats { get; set; } = { "F0", "F1", "F2", "F3", "F4", "F5" };
		
		[DataMember] public string CommonNumericFormat
		{
			get => Get(() => CommonNumericFormat, "F2");
			set => Set(() => CommonNumericFormat, value);
		}

		[DataMember] public string NoteNumericFormat
		{
			get => Get(() => NoteNumericFormat, "F1");
			set => Set(() => NoteNumericFormat, value);
		}

		public void Expose()
		{
			this[() => CommonNumericFormat].PropertyChanged += (o, e) => Xamarin.Forms.Entry.GlobalTextBindingRefresh();
			this[() => NoteNumericFormat].PropertyChanged += (o, e) =>
			{
				var digitsCountPart = NoteNumericFormat.Length > 1 ? NoteNumericFormat.Substring(1) : "1";
				var digitsCount = digitsCountPart.TryParse(out int v) ? v : 1;
				var zeros = new string('0', digitsCount);
				VisualProfile.TopProfiles[nameof(VisualProfile.DeltaFrequancy)].StringFormat = $"+0.{zeros};-0.{zeros}; 0.{zeros}";
			};
		}

		public void DrawMarkers(System.Collections.IList items, double width, double height,
			Brush lineBrush, Brush textBrush, IEnumerable<double> markers, int zIndex = 0, double vLabelOffset = 0d)
		{
			var hBand = Spectrum.Frequency;
			var hScaleTransformer = GetScaleTransformer(hBand, width);

			hBand.Threshold.Deconstruct(out var lowerFrequency, out var upperFrequency);

			var allMarkers = markers.ToArray();
			var skip = allMarkers.Length > 8 ? allMarkers.Length / 8 : 0;
			var i = 0;
			var opacityLineBrush = lineBrush.Clone();
			var skipLabels = textBrush.IsNot();

			foreach (var activeFrequency in markers)
			{
				var skipLabel = skip > 0 && i++ % skip > 0;
				if (activeFrequency < lowerFrequency) continue;
				if (activeFrequency > upperFrequency) break;

				var hVisualOffset = hScaleTransformer.GetVisualOffset(activeFrequency);
				var offset = hVisualOffset.Is(double.NaN) ? 0d : hVisualOffset;

				var line = CreateVerticalLine(offset, height, skipLabel ? opacityLineBrush : lineBrush);
				Panel.SetZIndex(line, zIndex);

				items.Add(line);

				if (skipLabels || skipLabel) continue;

				var panel = new StackPanel();
				Panel.SetZIndex(panel, zIndex);
				var fontSize = hScaleTransformer.InscaleFunc.Is(ScaleFuncs.Lineal) ? 12 : 8 * width / hVisualOffset;
				fontSize = fontSize > 20d || fontSize < 5d ? 20d : fontSize;
				panel.Children.Add(new TextBlock
				{
					FontSize = fontSize,
					Foreground = textBrush,
					Text = activeFrequency.ToString(NoteNumericFormat)
				});

				items.Add(panel);
				panel.UpdateLayout();
				panel.Margin = new(hVisualOffset - panel.ActualWidth / 2d, height * vLabelOffset, 0d, 0d);
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

		private static IEnumerable<TIn> EnumerateActivePoints<TIn>(
			IEnumerable<TIn> items,
			Deconstruct<TIn, double> deconstruct,
			double hLowerValue,
			double hUpperValue)
		{
			var enumerator = items.GetEnumerator();
			var startPoint = default(TIn);

			while (enumerator.MoveNext())
			{
				var currentPoint = enumerator.Current;
				deconstruct(in currentPoint, out var hActiveValue, out _);
				if (hActiveValue > hUpperValue) yield break;
				if (hActiveValue < hLowerValue)
				{
					startPoint = currentPoint;
					continue;
				}

				if (startPoint.IsNot(default))
					yield return startPoint;
				yield return currentPoint;
				break;
			}

			while (enumerator.MoveNext())
			{
				var currentPoint = enumerator.Current;
				yield return currentPoint;
				deconstruct(in currentPoint, out var hActiveValue, out _);
				if (hActiveValue > hUpperValue) yield break;
			}
		}

		public static ScaleTransformer GetScaleTransformer(Bandwidth band, double visualLength,
			Projection correction = default) => new(band.VisualScaleFunc, visualLength,
				band.Threshold.Lower, band.Threshold.Upper, correction);

		public static IEnumerable<TOut> Draw<TIn, TOut>(
			IEnumerable<TIn> points,
			Create<TOut> create,
			CreateWithContent<TIn, TOut> createWithContent,
			Deconstruct<TIn, double> deconstruct,
			Bandwidth hBand, Bandwidth vBand,
			double hLength, double vLength,
			Projection hCorrection,
			Projection vCorrection)
		{
			var hScaleTransformer = GetScaleTransformer(hBand, hLength, hCorrection);
			var vScaleTransformer = GetScaleTransformer(vBand, vLength, vCorrection);

			hBand.Threshold.Deconstruct(out var hLowerValue, out var hUpperValue);
			vBand.Threshold.Deconstruct(out var vLowerValue, out var vUpperValue);
			vScaleTransformer.GetVisualOffset(0d).To(out var vZeroLevel);

			if (createWithContent.IsNot()) yield return create(0d, in vZeroLevel);

			foreach(var activePoint in EnumerateActivePoints(points, deconstruct, hLowerValue, hUpperValue))
			{
				deconstruct(in activePoint, out var hActiveValue, out var vActiveValue);

				var hVisualOffset = hScaleTransformer.GetVisualOffset(hActiveValue);
				var vVisualOffset = vScaleTransformer.GetVisualOffset(vActiveValue);

				yield return createWithContent.IsNot()
					? create(in hVisualOffset, in vVisualOffset)
					: createWithContent(in hVisualOffset, in vVisualOffset, activePoint, vActiveValue, hActiveValue, vUpperValue);
			}

			if (createWithContent.IsNot())
				yield return create(in hLength, in vZeroLevel);
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

		public IEnumerable<Grid> DrawTops(IList<PianoKey> keys, double width, double height) => Draw
		(
			keys, default,
			(in double x, in double y,
				PianoKey pianoKey, double activeMagnitude, double activeFrequency, double upperMagnitude) =>
			{
				var expressionLevel = 1d + activeMagnitude / upperMagnitude;
				expressionLevel = double.IsInfinity(expressionLevel) ? 1d : expressionLevel;

				var strokeBorder = new Border
				{
					BorderThickness = new Thickness(1 + 4d * activeMagnitude),
					BorderBrush = VisualProfile.NoteTextBrushes[pianoKey.NoteNumber],
					CornerRadius = new(4d * expressionLevel * height / 256),
				};

				var fillBorder = new Border
				{
					Opacity = 0.25,
					CornerRadius = new(4d * expressionLevel * height / 256),
					Background = VisualProfile.NoteBrushes[pianoKey.NoteNumber] //VisualProfile.TopBrush
				};

				var infoPanel = new StackPanel();
				EnumeratePanelContent(pianoKey, activeFrequency, activeMagnitude, expressionLevel, width, height).
					ForEach(infoPanel.Children.Add);

				var l = x.Is(double.NaN) ? 0d : x;
				var t = y.Is(double.NaN) ? 0d : y;
				var grid = new Grid
				{
					Children = { fillBorder, strokeBorder, infoPanel },
					Margin = new(l, t / 2d, 0d, 0d),
					Opacity = 0.5d + activeMagnitude
				};

				Panel.SetZIndex(grid, (int)(expressionLevel * 100));
				return grid;
			},
			(in PianoKey p, out double h, out double v) => p.Harmonic.Deconstruct(out h, out v, out _),
			Spectrum.Frequency, Spectrum.Magnitude,
			width, height, default, default
		);

		private IEnumerable<TextBlock> EnumeratePanelContent(
			PianoKey pianoKey, double activeFrequency, double activeMagnitude, double expressionLevel, double width, double height)
		{
			var weightValue = (int)(300d * expressionLevel);
			var fontWeight = FontWeight.FromOpenTypeWeight(weightValue > 999 ? 999 : weightValue < 1 ? 1 : weightValue);

			double GetValueByKey(string key) =>
				key.Is("ActualMagnitude") ? activeMagnitude :
				key.Is("ActualFrequancy") ? activeFrequency :
				key.Is("DeltaFrequancy") ? pianoKey.DeltaFrequency :
				key.Is("EthalonFrequncy") ? pianoKey.EthalonFrequency :
				default;

			return VisualProfile.TopProfiles.Where(p => p.Value.IsVisible).Select(p => new TextBlock
			{
				Text = p.Key.Is("NoteName")
					? pianoKey.NoteName
					: GetValueByKey(p.Key).ToString(p.Value.StringFormat ?? NoteNumericFormat),
#if NETSTANDARD
				FontFamily = p.Value.FontFamilyName,
#else
				FontFamily = new FontFamily(p.Value.FontFamilyName),
#endif
				FontWeight = p.Key.Is("NoteName") ? FontWeights.SemiBold : fontWeight,
				Foreground = p.Value.Brush ?? VisualProfile.NoteTextBrushes[pianoKey.NoteNumber],
				FontSize = p.Value.FontSize * expressionLevel * (height > 0.1 ? height : 0.1) / 256,
				HorizontalAlignment = HorizontalAlignment.Center,
			});
		}

		public List<PianoKey> DrawPiano(System.Collections.IList items, IList<Bin> data, double width, double height, out Bin[] peaks)
		{
			Spectrum.Magnitude.Threshold.Deconstruct(out var lowMagnitude, out var upperMagnitude);

			var vVisualStretchFactor = height.Squeeze(upperMagnitude);
			var useNoteFilter = UseNoteFilter;
			var noteNames = Music.Notations.TryGetValue(Music.ActiveNotation ?? "", out var names)
				? names
				: Music.Notations[Music.ActiveNotation = Music.Notations.First().Key];

			var hBand = Spectrum.Frequency;
			var hScaleTransformer = GetScaleTransformer(hBand, width);

			hBand.Threshold.Deconstruct(out var lowerFrequency, out var upperFrequency);

			var keys = new List<PianoKey>();
			var oktavesCount = MusicalOptions.HalfTonesCount + 1;

			for (var oktaveNumber = 1; oktaveNumber < oktavesCount; oktaveNumber++)
			{
				var oktaveNotes = Music.BaseOktaveFrequencySet.Select(d => d * Math.Pow(2, oktaveNumber)).ToArray();
				var notesCount = oktaveNotes.Length;
				for (var noteIndex = 0; noteIndex < notesCount; noteIndex++)
					PianoKey.Construct(oktaveNotes, noteIndex, oktaveNumber, noteNames[noteIndex]).Use(keys.Add);
			}

			peaks = data.EnumeratePeaks().OrderByDescending(p => p.Magnitude).Take(10).ToArray();

			var m = 0;
			var averageMagnitude = 0d;
			foreach (var bin in peaks)
			{
				bin.Deconstruct(out var activeFrequency, out var activeMagnitude, out _);
				if (activeFrequency < lowerFrequency || activeFrequency > upperFrequency) continue;

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
				key.Peaks.Add(bin);

				if (activeMagnitude > key.Magnitude)
				{
					key.Magnitude = activeMagnitude;
					key.Harmonic = bin;
				}
			}

			averageMagnitude /= m;
			var minMagnitude = upperMagnitude * 0.01;
			var harmonics = keys.OrderByDescending(k => k.Magnitude).Take(MaxHarmonicsCount)
				.Where(k => k.Magnitude > minMagnitude).ToList();

			foreach (var key in keys.Where(k => k.LowerFrequency < upperFrequency))
			{
				var basicBrush = MusicalOptions.OktaveBrushes()[key.NoteNumber];
				var noteNumber = key.NoteNumber;
				var isTone = MusicalOptions.Tones[key.NoteNumber];
				var lowerOffset = hScaleTransformer.GetVisualOffset(key.LowerFrequency);
				var upperOffset = hScaleTransformer.GetVisualOffset(key.UpperFrequency);
				var ethalonOffset = hScaleTransformer.GetVisualOffset(key.EthalonFrequency);
				var offset = ethalonOffset.Is(double.NaN) ? 0d : ethalonOffset;

				var actualHeight = isTone ? height : height * 0.618d;
				var strokeThickness = upperOffset - lowerOffset;

				CreateBorder(lowerOffset, upperOffset, actualHeight, basicBrush).Use(items.Add);

				if (key.Peaks.Count.Is(0)) continue;
				var brush = isTone ? AppPalette.PressToneKeyBrush : AppPalette.PressHalfToneKeyBrush;
				var gradientBrush = (LinearGradientBrush)brush.Clone();
				var value = Math.Sqrt(key.Magnitude);
				gradientBrush.GradientStops[0].Offset = 1.0 - value;

				CreateBorder(lowerOffset, upperOffset, actualHeight, gradientBrush).Use(items.Add);
			}

			return harmonics;
		}

		private static Border CreateBorder(double lowerOffset, double upperOffset, double height, Brush strokeBrush) => new()
		{
			Width = (upperOffset - lowerOffset).To(out var width),
			CornerRadius = new(width / 16, width / 16, width / 4, width / 4),
			Margin = new(lowerOffset, 0d, 0d, 0d),
			Background = strokeBrush,
			Height = height,
		};

		private static Line CreateVerticalLine(double offset, double length, Brush strokeBrush, double strokeThickness = 1d) => new()
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