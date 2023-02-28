using Ace;
using Ace.Markup;

using System.Runtime.CompilerServices;
using System.Collections;
using Ace.Dictionaries;
using Solfeggio.Palettes;

#if NETSTANDARD
using Xamarin.Forms;
#else
using System.Windows;
using System.Windows.Media;
#endif

namespace Solfeggio
{
	static class AppPalette
	{
		static AppPalette() => Application.Current.Resources = new Map();

		public static void Load() => Resources.MergedDictionaries
			.Use(r => r.Add(new AppConverters()))
			.Use(r => r.Add(Values = new Values()))
			.Use(r => r.Add(ColorPalettes = new ColorPalettes()))
			.Use(r => r.Add((Map)Resources["Nature"]))
			.Use(r => r.Add(Brushes = new Brushes()))
			.Use(r => r.Add(new Palettes.Converters()))
			.Use(r => r.Add(new Sets()))
			.Use(r => r.Add(new Templates()))
			.Use(r => r.Add(new Styles()))
			;

		public static Map Resources => (Map)Application.Current.Resources;
		public static Map Values { get; private set; }
		public static Map ColorPalettes { get; private set; }
		public static Map Colors
		{
			get => (Map)Resources.MergedDictionaries.To<IList>()[3];
			set => Resources.MergedDictionaries.To<IList>()[3] = value;
		}

		public static Map Brushes { get; private set; }

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
