using Ace;

using System;
using System.Globalization;

namespace Solfeggio.Views
{
	public partial class ValuePicker
	{
		public ValuePicker() => InitializeComponent();

		private object Converter_Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value.To(out double d) > 0d ? Math.Log(d, 2d) : -16d;

		private object Converter_ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			Math.Pow(2d, (double)value);

		private object ExpandRange(object value, Type targetType, object parameter, CultureInfo culture) =>
			ExpandRange((double)value);

		double ExpandRange(double value)
		{
			if (Maximum < value) Maximum = value;
			if (Minimum > value) Minimum = value;
			return value;
		}
	}
}
