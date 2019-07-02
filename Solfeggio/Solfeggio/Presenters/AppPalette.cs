using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Solfeggio.Presenters
{
	class AppPalette
	{
#if NETSTANDARD
		public static Brush GetBrush([CallerMemberName]string key = default) =>
			default;
#else
		public static Brush GetBrush([CallerMemberName]string key = default) =>
			(Brush)System.Windows.Application.Current.Resources[key];
#endif

		public static Brush FullToneKeyBrush => GetBrush();
		public static Brush HalfToneKeyBrush => GetBrush();

		public static Brush MarkerBrush => GetBrush();
		public static Brush ButterflyGridBrush => GetBrush();
		public static Brush NoteGridBrush => GetBrush();
		public static Brush NoteBrush => GetBrush();
		public static Brush HzBrush => GetBrush();
	}
}
