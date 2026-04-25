using Ace;
using Ace.Markup.Patterns;

using Solfeggio.Models;
using Solfeggio.Presenters;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Ace.Extensions;
using Span = Solfeggio.Presenters.Span;
using System.Collections.Generic;
using Rainbow;

#if NETSTANDARD
using Colors = Xamarin.Forms.Color;
#else
using System.Windows.Media;
#endif

namespace Solfeggio.Palettes;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Converters
{
	public Converters() => InitializeComponent();

	public static Colority.GradientPoint[] GradientPoints = new Colority.GradientPoint[]
	{
		new() { Color = Colority.FromRGBA(0x00, 0xFF, 0x00), Offset = 0.0 },
		new() { Color = Colority.FromRGBA(0x00, 0x00, 0xFF), Offset = 1.0 },
	};

	public static Color GetOffsetColor(KeyValuePair<Bin, PianoKey> pair, Projection magnitudeProjection)
		=> Colority.GetColor(GradientPoints, Math.Abs(pair.Value.GetRelativeOffset(pair.Key.Frequency)))
		.Mix(Colority.Channel.A, 0.368 + 0.632 * magnitudeProjection(pair.Key.Magnitude));

	private object HarmonicOffsetToBrush_Convert(ConvertArgs args) => args.Value is KeyValuePair<Bin, PianoKey> key
		? new SolidColorBrush(GetOffsetColor(key, v => v))
		: args.Value;

	private object HarmonicOffset_Convert(ConvertArgs args) => args.Value is KeyValuePair<Bin, PianoKey> pair
		? pair.Value.GetOffsetFrequency(pair.Key.Frequency)
		: args.Value;

/* Unmerged change from project 'Solfeggio'
Before:
	Bandwidth FrequencyBandwidth = Store.Get<MusicalPresenter>().Spectrum.Frequency;
After:
	Presenters.Span FrequencyBandwidth = Store.Get<MusicalPresenter>().Spectrum.Frequency;
*/
	Span FrequencySpan = Store.Get<MusicalPresenter>().Spectrum.Frequency;
	object ToVisualValue(ConvertArgs args) => FrequencySpan.VisualScaleFunc((double)args.Value);
	object ToLogicalValue(ConvertArgs args) => FrequencySpan.LogicalScaleFunc((double)args.Value);

	object Debug_Convert(ConvertArgs args)
		=> args.Value;
}
