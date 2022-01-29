namespace Solfeggio.Converters
{
	class ByteToHexStringTwoWayConverter : Ace.Converters.Patterns.AValueConverter
	{
		public override object Convert(object value) => ((byte)value).ToString("X2");
		public override object ConvertBack(object value)
		{
			try
			{
				return byte.Parse((string)value, System.Globalization.NumberStyles.HexNumber);
			}
			catch
			{
				return default;
			}
		}
	}
}
