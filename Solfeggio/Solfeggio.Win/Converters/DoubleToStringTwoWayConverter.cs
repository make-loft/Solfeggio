using Ace;

using System.Windows;

namespace Solfeggio.Converters
{
	public class DoubleToStringTwoWayConverter : Ace.Converters.Patterns.AValueConverter
	{
		public static DependencyProperty StringFormatProperty =
			DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(DoubleToStringTwoWayConverter));

		public string StringFormat
		{
			get => GetValue(StringFormatProperty) as string;
			set => SetValue(StringFormatProperty, value);
		}

		double ToDouble(object value) =>
			value.Is(out double d) ? d :
			value.Is(out float f) ? f :
			0d;

		public override object Convert(object value) => ToDouble(value).ToString(StringFormat);

		public override object ConvertBack(object value) => double.Parse((string)value);
	}
}
