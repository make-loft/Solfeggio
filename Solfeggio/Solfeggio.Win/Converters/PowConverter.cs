using System;
using System.Globalization;
using System.Windows.Data;
using Ace;

namespace Solfeggio.Converters
{
	public class PowConverter : IValueConverter
	{
		public PowConverter() => Base = Math.E;

		public double Base { get; set; }

		private double GetBase(object parameter) =>
			parameter.Is(out double value) || double.TryParse(parameter.ToString(), out value) ? value : Base;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			Math.Log((double)value, GetBase(parameter));

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			Math.Pow(GetBase(parameter), (double)value);
	}
}
