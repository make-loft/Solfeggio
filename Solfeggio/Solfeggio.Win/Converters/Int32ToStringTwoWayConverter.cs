using System.Windows;

namespace Solfeggio.Converters
{
	public class Int32ToStringTwoWayConverter : Ace.Converters.Patterns.AValueConverter
	{
		public static DependencyProperty StringFormatProperty =
			DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(Int32ToStringTwoWayConverter));

		public string StringFormat
		{
			get => GetValue(StringFormatProperty) as string;
			set => SetValue(StringFormatProperty, value);
		}

		public override object Convert(object value) => ((int)(value ?? 0)).ToString(StringFormat);

		public override object ConvertBack(object value) => int.Parse((string)value);
	}
}
