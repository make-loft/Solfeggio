using System;
using System.Globalization;
using System.Windows.Data;
using Ace;

namespace Solfeggio.Converters
{
	class DelegateToNameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
			value.To<Delegate>().Method.Name;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}
