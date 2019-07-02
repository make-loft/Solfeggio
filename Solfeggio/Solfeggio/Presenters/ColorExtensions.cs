using Xamarin.Forms;

namespace Solfeggio.Presenters
{
	public static class ColorExtensions
	{
		public static byte A(this Color c) => (byte)c.A;
		public static byte R(this Color c) => (byte)c.R;
		public static byte G(this Color c) => (byte)c.G;
		public static byte B(this Color c) => (byte)c.B;

		public static byte Alpha(this Color c) => (byte)c.A;
		public static byte Red(this Color c) => (byte)c.R;
		public static byte Green(this Color c) => (byte)c.G;
		public static byte Blue(this Color c) => (byte)c.B;

		public static Color FromArgb(byte a, byte r, byte g, byte b) =>
			new Color(r, g, b, a);
	}

	public static class BrushExtensions
	{
		public static TBrush DoFreeze<TBrush>(this TBrush brush) where TBrush : Brush
		{
			brush.Freeze();
			return brush;
		}
	}
}
