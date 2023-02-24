using Ace;
using Ace.Markup.Patterns;

using Solfeggio.Models;
using Solfeggio.Presenters;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
#if NETSTANDARD
		static Color FromArgb(double a, double r, double g, double b) => new Color(a, r, g, b);
#else
		static Color FromArgb(double a, double r, double g, double b) =>
			Color.FromArgb((byte)(a * 255), (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
#endif
		public Converters() => InitializeComponent();

		private object HarmonicOffsetToColor_Convert(ConvertArgs args)
		{
			var k = (PianoKey)args.Value;
			if (k.IsNot())
				return Colors.Transparent;

			var color = FromArgb
			(
				0.0,
				1.0 - 2.0 * Math.Abs(k.OffsetFrequency) / (k.UpperFrequency - k.LowerFrequency).To(out var d),
				0.5 + 1.0 * d,
				0.8 + 0.2 * k.Magnitude
			);

			return color;
		}

		Bandwidth FrequencyBandwidth = Store.Get<MusicalPresenter>().Spectrum.Frequency;
		object ToVisualValue(ConvertArgs args) => FrequencyBandwidth.VisualScaleFunc((double)args.Value);
		object ToLogicalValue(ConvertArgs args) => FrequencyBandwidth.LogicalScaleFunc((double)args.Value);

		object Debug_Convert(ConvertArgs args) =>
			args.Value;
	}
}
