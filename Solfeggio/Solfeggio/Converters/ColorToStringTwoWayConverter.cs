#if NETSTANDARD
using Xamarin.Forms;
#else
using System.Windows.Media;
#endif

namespace Solfeggio.Converters
{
	public class ColorToStringTwoWayConverter : Ace.Markup.Patterns.AValueConverter
	{
		public override object Convert(object value) => value?.ToString();
#if !NETSTANDARD
		public override object ConvertBack(object value)
		{
			try
			{
				return value is Color c
					? c
					: ColorConverter.ConvertFromString((string)value);
			}
			catch
			{
				return Colors.Gray;
			}
		}
#endif
	}
}
