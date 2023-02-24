namespace Solfeggio.Converters
{
	class DelegateToNameConverter : Ace.Markup.Patterns.AValueConverter
	{
		public override object Convert(object value) => value is System.Delegate d ? d.Method.Name : value;
	}
}
