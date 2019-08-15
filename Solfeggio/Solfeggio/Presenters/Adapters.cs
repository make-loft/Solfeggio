﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;
using Ace;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Solfeggio.Presenters;
using static Xamarin.Forms.LayoutOptions;

namespace Solfeggio.Presenters
{
	public struct Point
	{
		public readonly double X, Y;

		public Point(double x, double y)
		{
			X = x;
			Y = y;
		}

		public SKPoint ToSkPoint() => new SKPoint((float) X, (float) Y);
	}

	public abstract class Brush
	{
		public double Opacity { get; set; }

		public abstract SKPaint ToSkPaint();

		public abstract Brush Clone();

		public void Freeze() { }
	}

	public class SolidColorBrush : Brush
	{
		public SolidColorBrush() { }
		//public SolidColorBrush(Color color) => Color = color;
		public SolidColorBrush(Xamarin.Forms.Color color) => Color = color;
		public Xamarin.Forms.Color Color { get; set; }

		public override Brush Clone() => new SolidColorBrush(Color);

		public override SKPaint ToSkPaint() => new SKPaint {Color = Color.ToSKColor()};

		public SolidColorBrush DoFreeze() => this;
	}

	public static class ColorConverters
	{
		//public static Color SkToPresenter(this SKColor c) => new Color {SkColor = c};
		public static Xamarin.Forms.Color SkToXamarin(this SKColor c) => Xamarin.Forms.Color.FromRgb(c.Red, c.Green, c.Blue);
		public static SKColor XamarinToSk(this Xamarin.Forms.Color c) => new SKColor((byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
	}

	public class LinearGradientBrush : Brush
	{
		public Point StartPoint { get; set; }
		public Point EndPoint { get; set; }
		public List<GradientStop> GradientStops { get; private set; } = new List<GradientStop>();

		public override Brush Clone() => new LinearGradientBrush
		{
			GradientStops = GradientStops.ToList(),
			StartPoint = StartPoint,
			EndPoint = EndPoint
		};

		public override SKPaint ToSkPaint()
		{
			var startPoint = StartPoint.ToSkPoint();
			var endPoint = EndPoint.ToSkPoint();
			var colors = GradientStops.Select(s => s.Color.ToSKColor()).ToArray();
			var offsets = GradientStops.Select(s => (float) s.Offset).ToArray();
			var shader = SKShader.CreateLinearGradient(startPoint, endPoint, colors, offsets, SKShaderTileMode.Clamp);
			return new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Shader = shader
			};
		}
	}

	public class GradientStop
	{
		public Xamarin.Forms.Color Color { get; set; }
		public double Offset { get; set; }
	}
}

namespace System.Windows.Controls
{
	public class FontWeight
	{
		public static FontWeight FromOpenTypeWeight(int value) => default;
	}

	public class TextBlock : Xamarin.Forms.Label
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
			get => new CornerRadius(base.CornerRadius);
			set => base.CornerRadius = value.Value;
		}

		public Xamarin.Forms.Thickness BorderThickness { get; set; }

		public Brush BorderBrush
		{
			get => new SolidColorBrush(BorderColor);
			set => BorderColor = value.To<SolidColorBrush>().Color;
		}

		public Brush Background
		{
			get => new SolidColorBrush(BackgroundColor);
			set => BackgroundColor = value.To<SolidColorBrush>().Color;
		}
	}

	public class Grid : Xamarin.Forms.Grid
	{
	}

	public class StackPanel : Xamarin.Forms.StackLayout
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

	public class Panel
	{
		private readonly SKCanvas _canvas;
		private readonly SKPaintSurfaceEventArgs _args;

		public Panel(SKPaintSurfaceEventArgs args)
		{
			_args = args;
			_canvas = args.Surface.Canvas;
			ActualWidth = _args.Info.Width;
			ActualHeight = _args.Info.Height;
		}

		public float ActualWidth;
		public float ActualHeight;


		public List<Primitive> Children { get; } = new List<Primitive>();
		public Brush Background;

		public void Draw()
		{
			_canvas.Clear();
			if (Background.Is()) _canvas.DrawRect(0f, 0f, _args.Info.Width, _args.Info.Height, Background.ToSkPaint());
			foreach (var child in Children)
			{
				var path = child.ToSkPath();

				if (child.Fill.Is())
					_canvas.DrawPath(path, child.Fill.ToSkPaint());

				if (child.Stroke.Is())
					_canvas.DrawPath(path, child.GetStrokeSkPaint());
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
		public abstract SKPath ToSkPath();
		public Brush Fill;
		public Brush Stroke;
		public double StrokeThickness;

		public SKPaint GetStrokeSkPaint()
		{
			var skBrush = Stroke.ToSkPaint();
			skBrush.StrokeWidth = (float)StrokeThickness;
			skBrush.Style = SKPaintStyle.Stroke;
			return skBrush;
		}
	}

	public class Polyline : Primitive
	{
		public List<Point> Points { get; } = new List<Point>();

		public override SKPath ToSkPath()
		{
			var points = Points.Select(p => p.ToSkPoint()).ToList();
			var path = new SKPath();
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

//namespace Xamarin.Media
//{
//	public abstract class Brush
//	{
//		public double Opacity { get; set; }

//		public abstract SKPaint ToSkPaint();

//		public abstract Brush Clone();

//		public void Freeze() { }
//	}

//	public class SolidColorBrush : Brush
//	{
//		public SolidColorBrush() { }
//		public SolidColorBrush(Forms.Color color) => Color = color;
//		public Forms.Color Color { get; set; }

//		public override Brush Clone() => new SolidColorBrush(Color);

//		public override SKPaint ToSkPaint() => new SKPaint { Color = Color };

//		public SolidColorBrush DoFreeze() => this;
//	}

//	public class GradientStop
//	{
//		public Forms.Color Color { get; set; }
//		public double Offset { get; set; }
//	}

//	public class LinearGradientBrush : Brush
//	{
//		public Point StartPoint { get; set; }
//		public Point EndPoint { get; set; }
//		public List<GradientStop> GradientStops { get; private set; } = new List<GradientStop>();

//		public override Brush Clone() => new LinearGradientBrush
//		{
//			GradientStops = GradientStops.ToList(),
//			StartPoint = StartPoint,
//			EndPoint = EndPoint
//		};

//		public override SKPaint ToSkPaint()
//		{
//			var startPoint = StartPoint.ToSkPoint();
//			var endPoint = EndPoint.ToSkPoint();
//			var colors = GradientStops.Select(s => s.Color).ToArray();
//			var offsets = GradientStops.Select(s => (float)s.Offset).ToArray();
//			var shader = SKShader.CreateLinearGradient(startPoint, endPoint, colors, offsets, SKShaderTileMode.Clamp);
//			return new SKPaint
//			{
//				Style = SKPaintStyle.Fill,
//				Shader = shader
//			};
//		}
//	}
//}