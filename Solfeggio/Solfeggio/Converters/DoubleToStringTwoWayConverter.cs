using Ace;

using Solfeggio.Presenters;

using System.Globalization;

namespace Solfeggio.Converters;

public class DoubleToStringTwoWayConverter : Ace.Markup.Patterns.AValueConverter
{
	static readonly MusicalPresenter MusicalPresenter = Store.Get<MusicalPresenter>();

	public string Head { get; set; }
	public string Tail { get; set; }

	double ToDouble(object value)
		=>
		value is double d ? d :
		value is float f ? f :
		0d;

	public override object Convert(object value) => value is string s
		? s
		: $"{Head}{ToDouble(value).ToString(MusicalPresenter.Format.MonitorNumericFormat)}{Tail}"
		;

	public override object ConvertBack(object value) => value is double d
		? d 
		: Clip(value.To<string>()).TryParse(out d, NumberFormatInfo.CurrentInfo) ? d : default
		;

	private string Clip(string value) => value.Clip
	(
		Head.Is() && value.StartsWith(Head) ? Head.Length : 0,
		Tail.Is() && value.EndsWith(Tail) ? value.Length - Tail.Length : value.Length
	);
}
