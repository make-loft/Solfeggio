using Ace;
using Ace.Markup.Patterns;

using Solfeggio.Models;

using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Solfeggio.Palettes
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Converters
	{
		public Converters() => InitializeComponent();

		private object HarmonicOffsetToColor_Convert(ConvertArgs args)
		{
			var k = (PianoKey)args.Value;
			if (k.IsNot())
				return Color.Transparent;

			var color = new Color
			(
				0.0,
				1.0 - 2.0 * Math.Abs(k.OffsetFrequency) / (k.UpperFrequency - k.LowerFrequency).To(out var d),
				0.5 + 1.0 * d,
				0.8 + 0.2 * k.Magnitude
			);

			return color;
		}

		object LogBase2(ConvertArgs args) => args.Value.To(out double d) > 0d ? Math.Log(d, 2d) : -16d;
		object PowBase2(ConvertArgs args) => Math.Pow(2d, (double)args.Value);

		object Debug_Convert(Ace.Markup.Patterns.ConvertArgs args) =>
			args.Value;
	}
}
