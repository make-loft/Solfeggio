using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Solfeggio.Palettes
{
	public class Colors : ResourceDictionary
	{
		public Colors()
		{
			MagnitudeFillColor = Brushes.AliceBlue.Color;
			MagnitudeStrokeColor = Brushes.Violet.Color;
		}

		public Color MagnitudeFillColor
		{
			get => (Color)this[nameof(MagnitudeFillColor)];
			set => this[nameof(MagnitudeFillColor)] = value;
		}

		public Color MagnitudeStrokeColor
		{
			get => (Color)this[nameof(MagnitudeStrokeColor)];
			set => this[nameof(MagnitudeStrokeColor)] = value;
		}
	}
}
