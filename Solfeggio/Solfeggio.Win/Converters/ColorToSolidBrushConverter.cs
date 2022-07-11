using Ace;

using System.Windows.Media;

namespace Solfeggio.Converters
{
	public class ColorToSolidBrushConverter : Ace.Markup.Patterns.AValueConverter
	{
		public override object Convert(object value) =>
				value.Is(out Color color) ? new SolidColorBrush(color) : Brushes.Transparent;
	}
}
