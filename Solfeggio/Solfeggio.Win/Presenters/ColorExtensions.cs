using System.Windows.Media;

namespace Solfeggio.Presenters
{
	public static class ColorExtensions
	{
		public static byte A(this Color c) => c.A;
		public static byte R(this Color c) => c.R;
		public static byte G(this Color c) => c.G;
		public static byte B(this Color c) => c.B;

		public static byte Alpha(this Color c) => c.A;
		public static byte Red(this Color c) => c.R;
		public static byte Green(this Color c) => c.G;
		public static byte Blue(this Color c) => c.B;

		public static Color FromArgb(byte a, byte r, byte g, byte b) => Color.FromArgb(255, r, g, b);
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
