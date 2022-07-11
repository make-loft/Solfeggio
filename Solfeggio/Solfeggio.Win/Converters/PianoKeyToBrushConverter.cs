using Ace;
using Ace.Markup.Patterns;

using Solfeggio.Models;

using System.Windows.Media;

namespace Solfeggio.Converters
{
	class PianoKeyToBrushConverter : AValueConverter
	{
		public override object Convert(object value) => new SolidColorBrush(ConvertToColor(value));

		public Color ConvertToColor(object value) => value is PianoKey key && key.NoteNumber < VisualProfile.Rainbow.Length
			? VisualProfile.Rainbow[value.To<PianoKey>().NoteNumber]
			: Colors.Transparent;
	}
}
