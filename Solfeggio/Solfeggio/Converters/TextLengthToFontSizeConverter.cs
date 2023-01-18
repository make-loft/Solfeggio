using Ace;
using Ace.Markup.Patterns;

using System.Globalization;
using Xamarin.Essentials;

namespace Solfeggio.Converters
{
	class TextLengthToFontSizeConverter : AValueConverter.Reflected
	{
		public double BasicFontSize => App.Current.Resources.GetValue("BasicFontSize", 14.2);
		public double BasicFontScale => App.Current.Resources.GetValue("BasicFontScale", 1.0);
		public double DefaultLengthStretchFactor { get; set; } = 0.016;
		public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
		private double LengthSqueezeFactor { get; } = DeviceDisplay.MainDisplayInfo.Density;

		public override object Convert(object value, object parameter)
		{
			if (value.IsNot()) return default;
			var text = value.ToString();
			var parameters = (parameter.To<string>() ?? "").SplitByChars(" ,");
			var basicFontSize = parameters.Length > 0 ? double.Parse(parameters[0], Culture) : BasicFontSize;
			var lengthStretchFactor = parameters.Length > 1 ? double.Parse(parameters[1], Culture) : DefaultLengthStretchFactor;
			var finalFontSize = BasicFontScale * basicFontSize / (1 + text.Length * lengthStretchFactor / LengthSqueezeFactor);
			//if (finalFontSize.Is(16.000d)) finalFontSize += 0.001d; /* 16 is a magic font size value on Android */
			return finalFontSize;
		}
	}
}
