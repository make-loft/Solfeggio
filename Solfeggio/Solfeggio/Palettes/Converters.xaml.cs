using Ace;
using Ace.Markup.Patterns;

using Solfeggio.Models;
using Solfeggio.Presenters;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Ace.Extensions;

#if NETSTANDARD
using Colors = Xamarin.Forms.Color;
#else
using System.Windows.Media;
#endif

namespace Solfeggio.Palettes
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Converters
	{
		public Converters() => InitializeComponent();

		public static Colority.GradientPoint[] GradientPoints = new Colority.GradientPoint[]
		{
			new() { Color = Colority.FromRGBA(0x00, 0xFF, 0x00), Offset = 0.0 },
			new() { Color = Colority.FromRGBA(0x00, 0x00, 0xFF), Offset = 1.0 },
		};

		public static Color GetOffsetColor(PianoKey key, Rainbow.Projection magnitudeProjection) =>
			Colority.GetColor(GradientPoints, Math.Abs(key.RelativeOffset))
			.Mix(Colority.Channel.A, 0.368 + 0.632 * magnitudeProjection(key.Magnitude));

		private object HarmonicOffsetToBrush_Convert(ConvertArgs args) => args.Value is PianoKey key
			? new SolidColorBrush(GetOffsetColor(key, v => v))
			: args.Value;

		Bandwidth FrequencyBandwidth = Store.Get<MusicalPresenter>().Spectrum.Frequency;
		object ToVisualValue(ConvertArgs args) => FrequencyBandwidth.VisualScaleFunc((double)args.Value);
		object ToLogicalValue(ConvertArgs args) => FrequencyBandwidth.LogicalScaleFunc((double)args.Value);

		object Debug_Convert(ConvertArgs args) =>
			args.Value;
	}
}
