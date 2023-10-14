using Ace;
using Ace.Dictionaries;
using Ace.Markup;

#if NETSTANDARD
using Xamarin.Forms;
#else
using System.Windows;
using System.Windows.Media;
#endif

using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Solfeggio.Palettes;

using Brushes = Solfeggio.Palettes.Brushes;


namespace Solfeggio
{
	static class AppPalette


	{
		static AppPalette() => Application.Current.Resources = new Map();

		public static void Load() => Resources.MergedDictionaries
			.Use(r => r.Add(new AppConverters()))
			.Use(r => r.Add(Values = new Values()))
			.Use(r => r.Add(ColorPalettes = new ColorPalettes()))
			.Use(r => r.Add(new Map((Map)Resources["Nature"])))
			.Use(r => r.Add(Brushes = new Brushes()))
			.Use(r => r.Add(new Palettes.Converters()))
			.Use(r => r.Add(new Sets()))
			.Use(r => r.Add(new Styles()))
			.Use(r => r.Add(new Templates()))
#if !NETSTANDARD
			.Use(r => r.Add(new TemplatesDesktop()))
			.Use(r => r.Add(new Sets_Theme()))
#endif
			;

		public static Map Values { get; private set; }
		public static Map ColorPalettes { get; private set; }
		public static Map Brushes { get; private set; }

		public static Map Resources => (Map)Application.Current.Resources;

		public static Map Colors
		{
			get => (Map)Resources.MergedDictionaries.To<IList>()[3];
			set
			{
				Colors.Changed -= Color_Changed;

				ColorPaletteChanging?.Invoke();
				ChangeColorPalette(value);
				ColorPaletteChanged?.Invoke();
				
				Colors.Changed += Color_Changed;
			}
		}

		private static int counter;
		private static async void Color_Changed(MapChangeArgs args)
		{
			if (args.IsValueChanged.Not())
				return;

			counter++;
			await Task.Delay(128);
			counter--;

			if (counter > 0)
				return;

			new Brushes().ForEach(p => Resources[p.Key] = p.Value);
		}

		static void ChangeColorPalette(Map value)
		{
			var colors = Resources.MergedDictionaries.To<IList>()[3].To<Map>();
			Map.EnumerateResources(value).ToList().ForEach(p => colors[p.Key] = p.Value);
			new Brushes().ForEach(p => Brushes[p.Key] = p.Value);
			//Resources.MergedDictionaries.To<IList>()[4] = new Brushes();
		}

		public static event System.Action ColorPaletteChanging;
		public static event System.Action ColorPaletteChanged;

		public static Brush GetBrush([CallerMemberName] string key = default) => (Brush)Resources[key];

		public static Brush PressedHalfToneKeyBrush => GetBrush();
		public static Brush PressedFullToneKeyBrush => GetBrush();
		public static Brush FullToneKeyBrush => GetBrush();
		public static Brush HalfToneKeyBrush => GetBrush();

		public static Brush MarkerBrush => GetBrush();
		public static Brush MagnitudeGridBrush => GetBrush("Stroke.MagnitudeGrid");
		public static Brush ButterflyGridBrush => GetBrush("Stroke.FrequencyDiscreteGrid");
		public static Brush NoteGridBrush => GetBrush("Stroke.FrequencyNotesGrid");
		public static Brush NoteBrush => GetBrush();
		public static Brush HzBrush => GetBrush();
	}
}
