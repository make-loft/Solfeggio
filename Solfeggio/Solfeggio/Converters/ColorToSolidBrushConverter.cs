#if NETSTANDARD
using Xamarin.Forms;
#else
using System.Windows.Media;
#endif

namespace Solfeggio.Converters
{
	public class ColorToSolidBrushConverter : Ace.Markup.Patterns.AValueConverter
	{
		public override object Convert(object value) =>
				value is Color color ? new SolidColorBrush(color) : new();
	}
}
