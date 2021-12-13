using System.Runtime.CompilerServices;
#if NETSTANDARD
using Xamarin.Forms;
using static Xamarin.Forms.Application;
#else
using System.Windows.Media;
using static System.Windows.Application;
#endif
namespace Solfeggio.Presenters
{
	class AppPalette
	{
		public static Brush GetBrush([CallerMemberName]string key = default) =>	(Brush)Current.Resources[key];

		public static Brush PressToneKeyBrush => GetBrush();
		public static Brush FullToneKeyBrush => GetBrush();
		public static Brush HalfToneKeyBrush => GetBrush();

		public static Brush MarkerBrush => GetBrush();
		public static Brush ButterflyGridBrush => GetBrush();
		public static Brush NoteGridBrush => GetBrush();
		public static Brush NoteBrush => GetBrush();
		public static Brush HzBrush => GetBrush();
	}
}
