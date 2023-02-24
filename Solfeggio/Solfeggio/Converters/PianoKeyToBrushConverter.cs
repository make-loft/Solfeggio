using Ace;
using Ace.Markup.Patterns;

using Solfeggio.Models;

#if NETSTANDARD
using Xamarin.Forms;

using static Xamarin.Forms.Color;
#else
using System.Windows.Media;

using static System.Windows.Media.Colors;
#endif


namespace Solfeggio.Converters
{
	class PianoKeyToBrushConverter : AValueConverter
	{
		public override object Convert(object value) => new SolidColorBrush(ConvertToColor(value));

		public Color ConvertToColor(object value) => value is PianoKey key && key.NoteNumber < VisualProfile.Rainbow.Length
			? VisualProfile.Rainbow[value.To<PianoKey>().NoteNumber]
			: Transparent;
	}
}
