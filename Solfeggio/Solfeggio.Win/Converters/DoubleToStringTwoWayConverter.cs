using Ace;

using Solfeggio.Presenters;

namespace Solfeggio.Converters
{
	public class DoubleToStringTwoWayConverter : Ace.Converters.Patterns.AValueConverter
	{
		static readonly MusicalPresenter MusicalPresenter = Store.Get<MusicalPresenter>(); 

		double ToDouble(object value) =>
			value is double d ? d :
			value is float f ? f :
			0d;

		public override object Convert(object value) => ToDouble(value).ToString(MusicalPresenter.CommonNumericFormat);

		public override object ConvertBack(object value) => value is double d
			? d 
			: double.TryParse((string)value, out var v) ? v : default;
	}
}
