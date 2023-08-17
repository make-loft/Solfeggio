using Ace;

namespace Solfeggio.Converters
{
	public class Int32ToDoubleTwoWayConverter : Ace.Markup.Patterns.AValueConverter
	{
		public override object Convert(object value) => value.To<double>();

		public override object ConvertBack(object value) => value.To<int>();
	}
}
