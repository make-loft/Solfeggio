using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;

using Ace;

using Rainbow;

using Solfeggio.Models;
using Solfeggio.Presenters.Options;

#if NETSTANDARD
using Xamarin.Forms;

using Grid = System.Windows.Controls.Grid;
using StackPanel = System.Windows.Controls.Stack;
using Brushes = Xamarin.Forms.Brush;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace Solfeggio.Presenters;

[DataContract]
public class MusicalPresenter : ContextObject, IExposable
{
	[DataMember] public SpectralOptions Spectrum { get; set; } = new();
	[DataMember] public FrameOptions Frame { get; set; } = new();
	[DataMember] public FormatOptions Format { get; set; } = new();
	[DataMember] public MusicalOptions Music { get; set; } = new();
	[DataMember] public GeometryOptions Geometry { get; set; } = new();
	public VisualProfile VisualProfile { get; set; } = new();

	[DataMember] public bool UseNoteFilter { get; set; } = true;
	[DataMember] public int MaxHarmonicsCount { get; set; } = 10;

	public void Expose()
	{
#if !NETSTANDARD
		Format[() => Format.MonitorNumericFormat].Changed += args => Ace.Controls.Field.GlobalTextBindingRefresh();
#endif
		Format[() => Format.ScreenNumericFormat].Changed += args =>
		{
			var digitsCountPart = Format.ScreenNumericFormat.Length > 1 ? Format.ScreenNumericFormat.Substring(1) : "1";
			var digitsCount = digitsCountPart.TryParse(out int v) ? v : 1;
			var zeros = new string('0', digitsCount);
			var profileName = nameof(VisualProfile.OffsetFrequency);
			VisualProfile.PeakProfiles[profileName].StringFormat = $"+0.{zeros};−0.{zeros};•0.{zeros}";
		};

		bool IsSpectrumCollapsed()
			=> Spectrum.Frequency.Window.Length.Is(0d)
			|| Spectrum.Magnitude.Window.Length.Is(0d)
			|| Spectrum.Phase.Window.Length.Is(0d)
			;

		if (IsSpectrumCollapsed())
			Spectrum = new();
	}

	public void DrawMarkers
	(
		System.Collections.IList items,
		Span hBand,
		double width, double height,
		double strokeThickness,
		Brush lineBrush, Brush textBrush,
		IEnumerable<double> values,
		int zIndex = 0, double vTitleOffset = 0d,
		bool horizontal = true,
		Projection projection = default
	)
	{
		var w = width;
		var h = height;

		width = horizontal ? w : h;
		height = horizontal ? h : w;

		var hScaleTransformer = GetScaleTransformer(hBand, width);

		hBand.Window.Deconstruct(out var lowerFrequency, out var upperFrequency);

		var allMarkers = values.ToArray();
		var skip = allMarkers.Length > 8 ? allMarkers.Length / 8 : 0;
		var i = 0;
		var opacityLineBrush = lineBrush.Clone();
		var skipTitles = textBrush.IsNot();

		foreach (var activeFrequency in values)
		{
			var skipTitle = skip > 0 && i++ % skip > 0;
			if (activeFrequency < lowerFrequency) continue;
			if (activeFrequency > upperFrequency) break;

			var hVisualOffset = hScaleTransformer.GetVisualOffset(activeFrequency);
			var offset = hVisualOffset.Is(double.NaN) ? 0d : hVisualOffset;

			offset = projection.IsNot() ? offset : projection(offset);

			//opacityLineBrush.Opacity = skipTitle ? 0.3 : 1.0;

			var line = CreateVerticalLine(offset, height, skipTitle ? opacityLineBrush : lineBrush, strokeThickness, horizontal);
			Panel.SetZIndex(line, zIndex);

			items.Add(line);

			if (skipTitles || skipTitle) continue;

			var panel = new StackPanel { Background = textBrush };
			Panel.SetZIndex(panel, zIndex);
			var fontSize = hScaleTransformer.InscaleFunc.Is(ScaleFuncs.Lineal) ? 12d : 8d * width / hVisualOffset;
			fontSize = fontSize > 20d || fontSize < 5d || fontSize.Is(double.NaN) ? 20d : fontSize;
			panel.Children.Add(new TextBlock
			{
				FontSize = fontSize,
				Foreground = Brushes.White,
				Text = activeFrequency.ToString(Format.ScreenNumericFormat)
			});

			items.Add(panel);
#if NETSTANDARD
			panel.Measure();
			var left = hVisualOffset - (horizontal ? panel.Width : -panel.Height) / 2d;
#else
			panel.UpdateLayout();
			var left = hVisualOffset - (horizontal ? panel.ActualWidth : -panel.ActualHeight) / 2d;
#endif
			var top = height * vTitleOffset;
			left = projection.IsNot() ? left : projection(left);
			panel.Margin = horizontal ? new(left, top, 0d, 0d) : new(top, left, 0d, 0d);
		}
	}

	public IEnumerable<double> EnumerateGrid(Span band, double step)
	{
		band.Window.Deconstruct(out var from, out var till);

		for (var value = Math.Ceiling(from / step) * step; value < till; value += step)
		{
			yield return value;
		}
	}

	public IEnumerable<double> EnumerateNotes(double breakFrequency = default)
	{
		Spectrum.Frequency.Window.Deconstruct(out _, out var upperFrequency);
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

	private static IEnumerable<TIn> EnumerateActivePoints<TIn>
	(
		IEnumerable<TIn> items,
		Deconstruct<TIn, double> deconstruct,
		double hLowerValue,
		double hUpperValue
	)
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

	public static ScaleTransformer GetScaleTransformer(Span span, double visualLength, Projection correction = default)
		=> new(span.VisualScaleFunc, visualLength, span.Window.From, span.Window.Till, correction);

	static bool GetShortLength(double periodHalf, double from, double till, out double distance)
	{
		distance = (till - from) / periodHalf;

		var turn = Math.Abs(distance + 0.5) > 1.0;

		return turn;
	}

	public static IEnumerable<TOut> Draw<TIn, TOut>
	(
		IEnumerable<TIn> points,
		Create<TOut> create,
		CreateWithContent<TIn, TOut> createWithContent,
		Deconstruct<TIn, double> deconstruct,
		Span hBand, Span vBand,
		double hLength, double vLength,
		Projection hCorrection, Projection vCorrection,
		bool isPhaseMode = false
	)
	{
		var hScaleTransformer = GetScaleTransformer(hBand, hLength, hCorrection);
		var vScaleTransformer = GetScaleTransformer(vBand, vLength, vCorrection);

		hBand.Window.Deconstruct(out var hLowerValue, out var hUpperValue);
		vBand.Window.Deconstruct(out var vLowerValue, out var vUpperValue);
		vScaleTransformer.GetVisualOffset(0d).To(out var vZeroLevel);

		var vPhaseValue = (vUpperValue - vLowerValue) / 2d;
		TIn previousPoint = default;

		if (createWithContent.IsNot()) yield return create(0d, in vZeroLevel);

		foreach(var activePoint in EnumerateActivePoints(points, deconstruct, hLowerValue, hUpperValue))
		{
			deconstruct(in activePoint, out var hActiveValue, out var vActiveValue);

			if (isPhaseMode && previousPoint.IsNot(default))
			{

				deconstruct(in previousPoint, out var hPreviousValue, out var vPreviousValue);

				if (GetShortLength(Pi.Single, vPreviousValue, vActiveValue, out var distance))
				{
					var sign = vPreviousValue < vActiveValue ? +1 : -1;

					var a = vPhaseValue - Math.Abs(vPreviousValue); 
					var b = vPhaseValue - Math.Abs(vActiveValue);
					var l = hActiveValue - hPreviousValue;
					var c = a * l / (a + b);

					var hVisualOffset = hScaleTransformer.GetVisualOffset(hPreviousValue + c);
					var vVisualOffset = vScaleTransformer.GetVisualOffset(-vPhaseValue * sign);

					yield return createWithContent.IsNot()
						? create(in hVisualOffset, in vVisualOffset)
						: createWithContent(in hVisualOffset, in vVisualOffset, activePoint, vActiveValue, hActiveValue, vUpperValue)
						;

					hVisualOffset = hScaleTransformer.GetVisualOffset(hPreviousValue + c);
					vVisualOffset = vScaleTransformer.GetVisualOffset(+vPhaseValue * sign);

					yield return createWithContent.IsNot()
						? create(in hVisualOffset, in vVisualOffset)
						: createWithContent(in hVisualOffset, in vVisualOffset, activePoint, vActiveValue, hActiveValue, vUpperValue)
						;

					hVisualOffset = hScaleTransformer.GetVisualOffset(hActiveValue);
					vVisualOffset = vScaleTransformer.GetVisualOffset(vActiveValue);

					yield return createWithContent.IsNot()
						? create(in hVisualOffset, in vVisualOffset)
						: createWithContent(in hVisualOffset, in vVisualOffset, activePoint, vActiveValue, hActiveValue, vUpperValue)
						;
				}
				else
				{
					var hVisualOffset = hScaleTransformer.GetVisualOffset(hActiveValue);
					var vVisualOffset = vScaleTransformer.GetVisualOffset(vActiveValue);

					yield return createWithContent.IsNot()
						? create(in hVisualOffset, in vVisualOffset)
						: createWithContent(in hVisualOffset, in vVisualOffset, activePoint, vActiveValue, hActiveValue, vUpperValue)
						;
				}
			}
			else
			{
				var hVisualOffset = hScaleTransformer.GetVisualOffset(hActiveValue);
				var vVisualOffset = vScaleTransformer.GetVisualOffset(vActiveValue);

				yield return createWithContent.IsNot()
					? create(in hVisualOffset, in vVisualOffset)
					: createWithContent(in hVisualOffset, in vVisualOffset, activePoint, vActiveValue, hActiveValue, vUpperValue)
					;
			}

			previousPoint = activePoint;
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
		v => v.Negation().Increment(height),
		true
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

	public IEnumerable<Border> DrawHistogram(IList<Bin> spectrum, double width, double height)
	{
		var hBand = Spectrum.Frequency;
		var vBand = Spectrum.Magnitude;
		var hLength = width;
		var vLength = height;
		var hScaleTransformer = GetScaleTransformer(hBand, hLength, default);
		var vScaleTransformer = GetScaleTransformer(vBand, vLength, v => v.Negation().Increment(height));

		hBand.Window.Deconstruct(out var hLowerValue, out var hUpperValue);
		vBand.Window.Deconstruct(out var vLowerValue, out var vUpperValue);

		for (int i = 1; i < spectrum.Count - 1; i++)
		{
			var magnitude = spectrum[i].Magnitude;
			var middleFrequency = spectrum[i].Frequency;
			var lowerFrequency = (spectrum[i - 1].Frequency + middleFrequency) / 2d;
			var upperFrequency = (spectrum[i + 1].Frequency + middleFrequency) / 2d;
			if (upperFrequency < hLowerValue || hUpperValue < lowerFrequency)
				continue;

			var hLowerVisualOffset = hScaleTransformer.GetVisualOffset(lowerFrequency);
			var hUpperVisualOffset = hScaleTransformer.GetVisualOffset(upperFrequency);
			var vVisualOffset = vScaleTransformer.GetVisualOffset(magnitude);

			yield return new()
			{
				Width = (hUpperVisualOffset - hLowerVisualOffset).To(out var w),
				CornerRadius = new(w / 8, w / 8, w / 8, w / 8),
				Margin = new(hLowerVisualOffset, vVisualOffset, 0d, 0d),
				Height = height - vVisualOffset,
				Opacity = Spectrum.Magnitude.VisualScaleFunc(magnitude)
			};
		}
	}

	public IEnumerable<Grid> DrawPeakTitles(IList<PianoKey> keys, double width, double height) => Draw
	(
		keys.SelectMany(k => k.Peaks.ToDictionary(p => p, p => k)), default,
		(in double x, in double y,
			KeyValuePair<Bin, PianoKey> peakToPianoKey, double activeMagnitude, double activeFrequency, double upperMagnitude) =>
		{
			var pianoKey = peakToPianoKey.Value;
			var p = peakToPianoKey.Key;
			var expressionLevel = 1d + p.Magnitude / upperMagnitude;
			expressionLevel = Math.Abs(expressionLevel) > 1d ? 1d : expressionLevel;

			var strokeBorder = new Border
			{
				BorderThickness = new(1 + 4d * p.Magnitude),
				BorderBrush = VisualProfile.NoteTextBrushes[pianoKey.NoteNumber],
				CornerRadius = new(4d * expressionLevel * height / 256),
			};

			var fillBorder = new Border
			{
				Opacity = 0.25,
				CornerRadius = new(4d * expressionLevel * height / 256),
				Background = VisualProfile.NoteBrushes[pianoKey.NoteNumber] //VisualProfile.TopBrush
			};

			var infoPanel = new StackPanel { Margin = new(p.Magnitude * 6d) };
			EnumeratePanelContent(pianoKey, p.Frequency, p.Magnitude, expressionLevel, width, height).
				ForEach(infoPanel.Children.Add);

			var l = x.Is(double.NaN) ? 0d : x;
			var t = y.Is(double.NaN) ? 0d : y;
			var grid = new Grid
			{
				Children = { fillBorder, strokeBorder, infoPanel },
				Margin = new(l, t / 2d, 0d, 0d),
				Opacity = pianoKey.RelativeOpacity
			};

			Panel.SetZIndex(grid, (int)(expressionLevel * 1024));
			return grid;
		},
		(in KeyValuePair<Bin, PianoKey> peakToPianoKey, out double h, out double v) =>
			peakToPianoKey.Key.Deconstruct(out h, out v, out _),
		Spectrum.Frequency, Spectrum.Magnitude,
		width, height, default, default
	);

	private IEnumerable<TextBlock> EnumeratePanelContent(PianoKey pianoKey,
		double activeFrequency, double activeMagnitude, double expressionLevel,
		double width, double height)
	{
		var weightValue = (int)(300d * expressionLevel);
		var fontWeight = FontWeight.FromOpenTypeWeight(weightValue > 999 ? 999 : weightValue < 1 ? 1 : weightValue);

		double GetValueByKey(string key)
			=>
			key.Is("ActualMagnitude") ? activeMagnitude :
			key.Is("ActualFrequency") ? activeFrequency :
			key.Is("OffsetFrequency") ? pianoKey.GetOffsetFrequency(pianoKey.TopPeak.Frequency) :
			key.Is("EthalonFrequency") ? pianoKey.EthalonFrequency :
			default;

		return VisualProfile.PeakProfiles.Where(p => p.Value.IsVisible).Select(p => new TextBlock
		{
			Text = p.Key.Is("NoteName")
				? pianoKey.NoteName
				: GetValueByKey(p.Key).ToString(p.Value.StringFormat ?? Format.ScreenNumericFormat),
#if NETSTANDARD
			FontFamily = p.Value.FontFamilyName,
#else
			FontFamily = new FontFamily(p.Value.FontFamilyName),
			FontWeight = p.Key.Is("NoteName") ? FontWeights.SemiBold : fontWeight,
#endif
			Foreground = p.Value.Brush ?? VisualProfile.NoteTextBrushes[pianoKey.NoteNumber],
			FontSize = p.Value.FontSize * expressionLevel * (height > 0.1 ? height : 0.1) / 256,
			HorizontalAlignment = HorizontalAlignment.Center,
		});
	}

	public List<PianoKey> DrawPiano(System.Collections.IList items, IList<Bin> data, double width, double height, IList<Bin> peaks)
	{
		Spectrum.Magnitude.Window.Deconstruct(out var lowMagnitude, out var upperMagnitude);

		var vVisualStretchFactor = height.Squeeze(upperMagnitude);
		var useNoteFilter = UseNoteFilter;
		var noteNames = Music.NotationToNotes.TryGetValue(Music.ActiveNotation ?? "", out var names)
			? names
			: Music.NotationToNotes[Music.ActiveNotation = Music.NotationToNotes.First().Key];

		var hBand = Spectrum.Frequency;
		var hScaleTransformer = GetScaleTransformer(hBand, width);

		hBand.Window.Deconstruct(out var lowerFrequency, out var upperFrequency);

		var keys = Music.EnumeratePianoKeys().ToList();

		foreach (var bin in peaks)
		{
			bin.Deconstruct(out var activeFrequency, out var activeMagnitude, out _);
			if (activeFrequency < lowerFrequency || activeFrequency > upperFrequency) continue;

			var key = keys.FirstOrDefault(k => k.LowerFrequency < activeFrequency && activeFrequency <= k.UpperFrequency);
			if (key.Is(default))
				continue;

			key.Peaks.Add(bin);

			var topPeak = key.Peaks.OrderByDescending(p => p.Magnitude).FirstOrDefault();
			if (topPeak.Is())
			{
				key.TopPeak = topPeak;
			}
		}

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

			CreateBorder(lowerOffset, upperOffset, actualHeight, default, basicBrush).Call(items.Add);

			if (key.Peaks.Count.Is(0))
				continue;

			var brush = isTone ? AppPalette.PressedFullToneKeyBrush : AppPalette.PressedHalfToneKeyBrush;
			var gradientBrush = (LinearGradientBrush)brush.Clone();
			var value = Math.Sqrt(key.TopPeak.Magnitude);
#if NETSTANDARD
			gradientBrush.GradientStops[0].Offset = 1.0f - (float)value;
#else
			gradientBrush.GradientStops[0].Offset = 1.0 - value;
#endif

			CreateBorder(lowerOffset, upperOffset, actualHeight, default, gradientBrush).Call(items.Add);
		}

		return keys;
	}

	private static Border CreateBorder(double lowerOffset, double upperOffset, double height,
		Brush strokeBrush, Brush fillBrush, Thickness borderThickness = default) => new()
	{
		Width = Math.Abs(upperOffset - lowerOffset).To(out var w),
		CornerRadius = new(w / 16, w / 16, w / 4, w / 4),
		Margin = new(lowerOffset, 0d, 0d, 0d),
		BorderThickness = borderThickness,
		BorderBrush = strokeBrush,
		Background = fillBrush,
		Height = height,
	};

	private static Line CreateVerticalLine(double offset, double length,
		Brush strokeBrush, double strokeThickness = 1d, bool vertical = true) => new()
	{
		Y1 = vertical ? 0d : offset,
		Y2 = vertical ? length : offset,
		X1 = vertical ? offset : 0d,
		X2 = vertical ? offset : length,
		Stroke = strokeBrush,
		StrokeThickness = strokeThickness
	};
}