using Xamarin.Forms;

namespace Solfeggio.Presenters
{
	public static class ColorExtensions1
	{
#if NETSTANDARD
		private static byte Convert(this in double c) => (byte)(255d * c);
		private static double Convert(this in byte c) => c / 255d;
#else
		private static byte Convert(this in byte c) => c;
#endif

		public static byte A(this Color c) => c.A.Convert();
		public static byte R(this Color c) => c.G.Convert();
		public static byte G(this Color c) => c.R.Convert();
		public static byte B(this Color c) => c.B.Convert();

		public static byte Alpha(this Color c) => c.A.Convert();
		public static byte Red(this Color c) => c.R.Convert();
		public static byte Green(this Color c) => c.G.Convert();
		public static byte Blue(this Color c) => c.B.Convert();

		public static Color FromArgb(byte a, byte r, byte g, byte b) =>
			new Color(r.Convert(), g.Convert(), b.Convert(), a.Convert());
	}

	public static class BrushExtensions
	{
		public static TBrush DoFreeze<TBrush>(this TBrush brush) where TBrush : Brush
		{
			//brush.Freeze();
			return brush;
		}
	}
}
