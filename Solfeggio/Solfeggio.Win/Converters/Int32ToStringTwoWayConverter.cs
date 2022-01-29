namespace Solfeggio.Converters
{
	public class Int32ToStringTwoWayConverter : Ace.Converters.Patterns.AValueConverter
	{
		public override object Convert(object value) => ((int)(value ?? 0)).ToString();

		public override object ConvertBack(object value) => value is int i
			? i
			: int.TryParse((string)value, out var v) ? v : default;
	}
}
