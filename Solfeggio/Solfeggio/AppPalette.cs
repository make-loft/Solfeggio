using System.Runtime.CompilerServices;

#if NETSTANDARD
using Xamarin.Forms;
#else
using Ace;
using Ace.Markup;

using System.Collections;
using System.Windows;
using System.Windows.Media;
#endif

namespace Solfeggio
{
	class AppPalette
	{
#if !NETSTANDARD
		public static Map Resources => (Map)Application.Current.Resources;
		public static Map Values => (Map)Resources.MergedDictionaries.To<IList>()[1];
		public static Map ColorPalettes => (Map)Resources.MergedDictionaries.To<IList>()[2];
		public static Map Colors => (Map)Resources.MergedDictionaries.To<IList>()[3];
		public static Map Brushes => (Map)Resources.MergedDictionaries.To<IList>()[4];
#else
		public static ResourceDictionary Resources => Application.Current.Resources;
#endif

		public static Brush GetBrush([CallerMemberName] string key = default) => (Brush)Resources[key];

		public static Brush PressHalfToneKeyBrush => GetBrush();
		public static Brush PressToneKeyBrush => GetBrush();
		public static Brush FullToneKeyBrush => GetBrush();
		public static Brush HalfToneKeyBrush => GetBrush();

		public static Brush MarkerBrush => GetBrush();
		public static Brush ButterflyGridBrush => GetBrush("Stroke.FrequencyDiscreteGrid");
		public static Brush NoteGridBrush => GetBrush("Stroke.FrequencyNotesGrid");
		public static Brush NoteBrush => GetBrush();
		public static Brush HzBrush => GetBrush();
	}
}
