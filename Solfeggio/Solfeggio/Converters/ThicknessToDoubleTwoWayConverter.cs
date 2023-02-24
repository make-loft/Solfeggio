#if NETSTANDARD
using Xamarin.Forms;
#else
using System.Windows;
#endif

namespace Solfeggio.Converters
{
	public class ThicknessToDoubleTwoWayConverter : Ace.Markup.Patterns.AValueConverter
	{
		public override object Convert(object value) => value is Thickness thickness
			? thickness.Top
			: 1d;

		public override object ConvertBack(object value) => value is double @double
			? new Thickness(@double)
			: new Thickness(1d);
	}
}
