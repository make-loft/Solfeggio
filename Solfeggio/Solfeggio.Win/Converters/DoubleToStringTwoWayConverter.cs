using Ace;

using Solfeggio.Presenters;

namespace Solfeggio.Converters
{
	public class DoubleToStringTwoWayConverter : Ace.Converters.Patterns.AValueConverter
	{
		static readonly MusicalPresenter MusicalPresenter = Store.Get<MusicalPresenter>();

		public string Head { get; set; }
		public string Tail { get; set; }

		double ToDouble(object value) =>
			value is double d ? d :
			value is float f ? f :
			0d;

		public override object Convert(object value) =>
			$"{Head}{ToDouble(value).ToString(MusicalPresenter.MonitorNumericFormat)}{Tail}";

		public override object ConvertBack(object value) => value is double d
			? d 
			: Clip(value).TryParse(out d) ? d : default;

		string Clip(object value) => value.Is(out string str)
			? Clip(str,
				Head.Is() && str.StartsWith(Head) ? Head.Length : 0,
				Tail.Is() && str.EndsWith(Tail) ? str.Length - Tail.Length - 1 : str.Length - 1)
			: value?.ToString();

		string Clip(string str, int from, int till) => str.Substring(from, till - from);
	}
}
