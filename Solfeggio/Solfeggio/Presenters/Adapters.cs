using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;
using Ace;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Solfeggio.Presenters;

using Xamarin.Forms;

using static Xamarin.Forms.LayoutOptions;

namespace Solfeggio.Presenters
{
	public static class Ext
	{
		public static SKPoint ToSkPoint(this Point point) => new()
		{
			X = (float)point.X,
			Y = (float)point.Y,
		};

		public static Brush Freeze(this Brush brush) => brush;
		public static Brush Clone(this Brush brush) => brush.As<LinearGradientBrush>()?.Clone() ?? brush;
		public static Brush Clone(this LinearGradientBrush brush) => new LinearGradientBrush(brush.GradientStops);

		public static SKPaint ToSkPaint(this Brush brush, double w, double h) =>
			brush.As<LinearGradientBrush>()?.ToSkPaint(w, h) ?? brush.As<SolidColorBrush>()?.ToSkPaint();

		public static SKPaint ToSkPaint(this SolidColorBrush brush) => new()
		{
			Style = SKPaintStyle.Fill,
			Color = brush.Color.ToSKColor(),
		};

		public static SKPaint ToSkPaint(this LinearGradientBrush brush, double w, double h)
		{
			var startPoint = brush.StartPoint.ToSkPoint();
			var endPoint = new SKPoint { X = (float)(brush.EndPoint.X * w), Y = (float)(brush.EndPoint.Y * h) };
			var colors = brush.GradientStops.Select(s => s.Color.ToSKColor()).ToArray();
			var offsets = brush.GradientStops.Select(s => (float)s.Offset).ToArray();
			var shader = SKShader.CreateLinearGradient(startPoint, endPoint, colors, offsets, SKShaderTileMode.Clamp);
			return new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Shader = shader
			};
		}
	}

	public static class ColorConverters
	{
		//public static Color SkToPresenter(this SKColor c) => new Color {SkColor = c};
		public static Color SkToXamarin(this SKColor c) => Xamarin.Forms.Color.FromRgb(c.Red, c.Green, c.Blue);
		public static SKColor XamarinToSk(this Xamarin.Forms.Color c) => new SKColor((byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
	}
}

namespace System.Windows.Controls
{
	public class FontWeight
	{
		public static FontWeight FromOpenTypeWeight(int value) => default;
	}

	public class TextBlock : Label
	{
		public Brush Foreground
		{
			get => new SolidColorBrush(TextColor);
			set => TextColor = value.To<SolidColorBrush>().Color;
		}

		public HorizontalAlignment HorizontalAlignment
		{
			get => HorizontalOptions.ToHorizontalAlignment();
			set => HorizontalOptions = value.ToLayoutOptions();
		}

		public FontWeight FontWeight { get; set; }
	}

	public enum HorizontalAlignment
	{
		Left, Right, Center, Stretch
	}

	public enum VerticalAlignment
	{
		Top, Bottom, Center, Stretch
	}

	public struct CornerRadius
	{
		public float Value { get; }
		public CornerRadius(double value) => Value = (float)value;
	}

	public class Border : Xamarin.Forms.Frame
	{
		public new CornerRadius CornerRadius
		{
			get => new(base.CornerRadius);
			set => base.CornerRadius = value.Value;
		}

		public Thickness BorderThickness { get; set; }

		public Brush BorderBrush
		{
			get => new SolidColorBrush(BorderColor);
			set => BorderColor = value.To<SolidColorBrush>().Color;
		}
	}

	public class StackPanel : StackLayout
	{
		public void UpdateLayout() => InvalidateLayout();

		public VerticalAlignment VerticalAlignment
		{
			get => VerticalOptions.ToVerticalAlignment();
			set => VerticalOptions = value.ToLayoutOptions();
		}

		public HorizontalAlignment HorizontalAlignment
		{
			get => HorizontalOptions.ToHorizontalAlignment();
			set => HorizontalOptions = value.ToLayoutOptions();
		}

		public double ActualWidth => Width;
		public double ActualHeight => Height;
	}

	[ContentProperty("Children")]
	public class Canvas : SKCanvasView
	{
		public float ActualWidth { get; set; }
		public float ActualHeight { get; set; }

		public List<Primitive> Children { get; } = new();

		public void Draw(SKPaintSurfaceEventArgs _args)
		{
			var canvas = _args.Surface.Canvas;
			canvas.Clear();
			ActualWidth = _args.Info.Width;
			ActualHeight = _args.Info.Height;
			if (Background.Is()) canvas.DrawRect(0f, 0f, ActualWidth, ActualHeight, Background.ToSkPaint(ActualWidth, ActualHeight));
			foreach (var child in Children)
			{
				var path = child.ToSkPath();

				if (child.Fill.Is())
					canvas.DrawPath(path, child.Fill.ToSkPaint(ActualWidth, ActualHeight));

				if (child.Stroke.Is())
					canvas.DrawPath(path, child.GetStrokeSkPaint(ActualWidth, ActualHeight));
			}
		}
	}

	public static class LayoutOptionsConverters
	{
		public static VerticalAlignment ToVerticalAlignment(this Xamarin.Forms.LayoutOptions o) =>

			o.Is(CenterAndExpand) ? VerticalAlignment.Center :
			o.Is(StartAndExpand) ? VerticalAlignment.Top :
			o.Is(EndAndExpand) ? VerticalAlignment.Bottom :
			o.Is(Fill) ? VerticalAlignment.Stretch :

			o.Is(CenterAndExpand) ? VerticalAlignment.Center :
			o.Is(StartAndExpand) ? VerticalAlignment.Top :
			o.Is(EndAndExpand) ? VerticalAlignment.Bottom :
			o.Is(FillAndExpand) ? VerticalAlignment.Stretch :

			throw new ArgumentException();

		public static HorizontalAlignment ToHorizontalAlignment(this Xamarin.Forms.LayoutOptions o) =>

			o.Is(Center) ? HorizontalAlignment.Center :
			o.Is(Start) ? HorizontalAlignment.Left :
			o.Is(End) ? HorizontalAlignment.Right :
			o.Is(Fill) ? HorizontalAlignment.Stretch :

			o.Is(CenterAndExpand) ? HorizontalAlignment.Center :
			o.Is(StartAndExpand) ? HorizontalAlignment.Left :
			o.Is(EndAndExpand) ? HorizontalAlignment.Right :
			o.Is(FillAndExpand) ? HorizontalAlignment.Stretch :

			throw new ArgumentException();

		public static Xamarin.Forms.LayoutOptions ToLayoutOptions(this HorizontalAlignment o) =>

			o.Is(HorizontalAlignment.Center) ? Center :
			o.Is(HorizontalAlignment.Left) ? Start :
			o.Is(HorizontalAlignment.Right) ? End :
			o.Is(HorizontalAlignment.Stretch) ? Fill :

			throw new ArgumentException();

		public static Xamarin.Forms.LayoutOptions ToLayoutOptions(this VerticalAlignment o) =>

			o.Is(VerticalAlignment.Center) ? Center :
			o.Is(VerticalAlignment.Top) ? Start :
			o.Is(VerticalAlignment.Bottom) ? End :
			o.Is(VerticalAlignment.Stretch) ? Fill :

			throw new ArgumentException();
	}
}

namespace System.Windows.Shapes
{
	public abstract class Primitive
	{
		public Brush Fill { get; set; }
		public Brush Stroke { get; set; }
		public double StrokeThickness { get; set; }

		public abstract SKPath ToSkPath();
		public SKPaint GetStrokeSkPaint(double w, double h)
		{
			var skBrush = Stroke.ToSkPaint(w, h);
			skBrush.StrokeWidth = (float)StrokeThickness;
			skBrush.Style = SKPaintStyle.Stroke;
			return skBrush;
		}
	}

	public class Polyline : Primitive
	{
		public List<Point> Points { get; } = new();

		public override SKPath ToSkPath()
		{
			var points = Points.Select(p => p.ToSkPoint()).ToList();
			var path = new SKPath();
			if (points.Count is 0)
				return path;

			path.MoveTo(points[0]);
			for (var i = 1; i < points.Count; i++)
			{
				path.LineTo(points[i]);
			}

			return path;
		}
	}

	public class Line : Primitive
	{
		public double X1, X2, Y1, Y2;
		public VerticalAlignment VerticalAlignment;

		public override SKPath ToSkPath()
		{
			var path = new SKPath();
			path.MoveTo((float) X1, (float) Y1);
			path.LineTo((float) X2, (float) Y2);
			path.Close();
			return path;
		}
	}
}
