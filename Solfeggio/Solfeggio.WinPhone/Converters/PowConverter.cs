using System;
using System.Globalization;
using System.Windows.Data;

namespace Solfeggio.Converters
{
	public class PowConverter : IValueConverter
	{
		public PowConverter()
		{
			Base = Math.E;
		}

		public double Base { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Math.Log((double) value, Base);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Math.Pow(Base, (double) value); //Math.Exp((double) value);
		}
	}
}
