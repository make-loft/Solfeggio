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

		public override object Convert(object value) => ((double)(value ?? 0d)).ToString(StringFormat);

		public override object ConvertBack(object value) => double.Parse((string)value);
	}
}
