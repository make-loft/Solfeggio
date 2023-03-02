using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;
using Ace;
using Ace.Controls;

using SkiaSharp;
using SkiaSharp.Views.Forms;
using Solfeggio.Presenters;

using Xamarin.Forms;

using static Xamarin.Forms.LayoutOptions;

using Ext = Solfeggio.Presenters.Ext;

namespace Solfeggio.Presenters
{
	public static class Ext
	{
		public static SKColor WithOpacity(this SKColor c, double opacity) =>
			new(c.Red, c.Green, c.Blue, (byte)(c.Alpha * opacity));

		public static Color FromArgb(byte a, byte r, byte g, byte b) => Color.FromRgba(r, g, b, a);

		public static SKPoint ToSkPoint(this Point point) => new()
		{
			X = (float)(point.X * density),
			Y = (float)(point.Y * density),
		};

		public static Brush Freeze(this Brush brush) => brush;
		public static Brush Clone(this Brush brush) => brush.As<LinearGradientBrush>()?.Clone() ?? brush;
		public static Brush Clone(this LinearGradientBrush brush) => new LinearGradientBrush
		(
			new GradientStopCollection().AppendRange(brush.GradientStops.Select(s => new GradientStop(s.Color, s.Offset))),
			brush.StartPoint,
			brush.EndPoint
		);

		public static SKPaint ToStrokeSkPaint(this Brush brush, double strokeThickness, double w, double h, double opacity)
		{
			var skBrush = brush.ToSkPaint(w, h, opacity);
			skBrush.StrokeWidth = (float)(strokeThickness * Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density);
			skBrush.Style = SKPaintStyle.Stroke;
			return skBrush;
		}
	
		public static SKPaint ToSkPaint(this Brush brush, double w, double h, double opacity) =>
			brush.As<LinearGradientBrush>()?.ToSkPaint(w, h, opacity) ??
			brush.As<RadialGradientBrush>()?.ToSkPaint(w, h, opacity) ??
			brush.As<SolidColorBrush>()?.ToSkPaint(opacity);

		public static SKPaint ToSkPaint(this SolidColorBrush brush, double opacity) => new()
		{
			Style = SKPaintStyle.Fill,
			Color = brush.Color.ToSKColor().WithOpacity(opacity),
		};

		public static double density = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;

		public static SKPaint ToSkPaint(this LinearGradientBrush brush, double w, double h, double opacity)
		{
			var startPoint = new SKPoint { X = (float)(brush.StartPoint.X * w * density), Y = (float)(brush.StartPoint.Y * h * density) };
			var endPoint = new SKPoint { X = (float)(brush.EndPoint.X * w * density), Y = (float)(brush.EndPoint.Y * h * density) };
			var colors = brush.GradientStops.Select(s => s.Color.ToSKColor().WithOpacity(opacity)).ToArray();
			var offsets = brush.GradientStops.Select(s => (float)s.Offset).ToArray();
			var shader = SKShader.CreateLinearGradient(startPoint, endPoint, colors, offsets, SKShaderTileMode.Clamp);
			return new SKPaint
			{
				Style = SKPaintStyle.Fill,
				Shader = shader
			};
		}

		public static SKPaint ToSkPaint(this RadialGradientBrush brush, double w, double h, double opacity)
		{
			var c = System.Math.Min(w, h) * density;
			var center = new SKPoint { X = (float)(brush.Center.X * c), Y = (float)(brush.Center.Y * c) };
			var radius = (float)(brush.Radius * c);
			var colors = brush.GradientStops.Select(s => s.Color.ToSKColor().WithOpacity(opacity)).ToArray();
			var offsets = brush.GradientStops.Select(s => (float)s.Offset).ToArray();
			var shader = SKShader.CreateRadialGradient(center, radius, colors, offsets, SKShaderTileMode.Clamp);
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

	public class Panel
	{
		public static void SetZIndex(object element, int index) { }
	}

	public class Label : Border
	{
		public string Text { get; set; }
		public Color TextColor { get; set; }
		public double FontSize { get; set; }
		public string FontFamily { get; set; }
	}

	public class Grid : View
	{
		public List<View> Children { get; } = new();
	}

	public class Stack : View
	{
		public List<View> Children { get; } = new();

		public StackOrientation Orientation { get; set; }
	}

	public class View : Primitive
	{
		public Thickness Margin { get; set; }
		public Thickness CornerRadius { get; set; }
		public Thickness BorderThickness { get; set; }
		public Brush BorderBrush { set => Stroke = value; }
		public Brush Background { set => Fill = value; }

		public object BindingContext { get; set; }
		public void Measure() => Canvas.Measure(this);
		public object Tag { get; set; }
		public double StrokeThikness { get => BorderThickness.Top; }

		public LayoutOptions VerticalOptions { get; set; }
		public LayoutOptions HorizontalOptions { get; set; }

		public override SKPath ToSkPath()
		{
			var X = Margin.Left;
			var Y = Margin.Top;

			var Points = new Point[]
			{
				new(X, Y),
				new(X + Width, Y),
				new(X + Width, Y + Height),
				new(X, Y + Height)
			};

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

	public class Border : View
	{
		static SolidColorBrush TransparentBrush = new(Color.Transparent);

		public Border() => Background = TransparentBrush;

		public View Content { get; set; }
	}

	[ContentProperty("Children")]
	public class Canvas : SKCanvasView
	{
		public Canvas() => PaintSurface += (s, e) => s.To<Canvas>().Draw(e);
		public float ActualWidth { get; set; }
		public float ActualHeight { get; set; }

		public List<Primitive> Children { get; } = new();

		public void Draw(SKPaintSurfaceEventArgs _args)
		{
			var canvas = _args.Surface.Canvas;
			canvas.Clear();
			ActualWidth = _args.Info.Width;
			ActualHeight = _args.Info.Height;
			if (Background.Is())
			{
				using var paint = Background.ToSkPaint(ActualWidth / density, ActualHeight / density, 1.0d);
				canvas.DrawRect(0f, 0f, ActualWidth, ActualHeight, paint);
			}

			foreach (var child in Children)
			{
				if (child.Is(out View view))
					Draw(canvas, view, 0, 0, ActualWidth / density, ActualHeight / density, child.Opacity);
				else
				{
					using var path = child.ToSkPath();
					//child.Measure();

					var w = child.Width < 0 ? ActualWidth / density : child.Width;
					var h = child.Height < 0 ? ActualHeight / density : child.Height;

					if (child.Fill.Is())
					{
						using var paint = child.Fill.ToSkPaint(w, h, child.Opacity);
						canvas.DrawPath(path, paint);
					}

					if (child.Stroke.Is())
					{
						using var paint = child.GetStrokeSkPaint(w, h, child.Opacity);
						canvas.DrawPath(path, paint);
					}
				}
			}

			foreach (var child in Children.OfType<Canvas>())
			{
				child.InvalidateSurface();
			}
		}

		static double density = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;

		public static void Measure(View view, double w = -1d, double h = -1d)
		{
			if (view.IsNot())
				return;

			var wr = view.Width;
			var hr = view.Height;
			var skipMeasure = wr > 0d && hr > 0d;

			if (skipMeasure)
				return;

			switch (view)
			{
				case Label l:
					l.Margin = new(4);

					using (SKPath roundRectPath = new())
					{
						using var stroke = GetStroke(l, 1.0d);
						var rect = new SKRect();
						var text = l.Text.Replace('♯', '#').Replace('♭', 'b');
						stroke.MeasureText(text, ref rect);
						var correction = 1.0 + l.FontSize * density / 200d;
						l.Width = correction * rect.Size.Width / density;
						l.Height = rect.Size.Height / density;
					}
					break;
				case Border g:
					{
						if (g.Width < 0d)
							g.Width = w - (g.Margin.Left + g.Margin.Right);

						if (g.Height < 0d)
							g.Height = h - (g.Margin.Top + g.Margin.Bottom);
					}
					break;
				case Grid g:
					{
						if (g.HorizontalOptions.Is(Center))
							g.Width = w - (g.Margin.Left + g.Margin.Right);
						else if (g.Width < 0d)
						{
							g.Children.ForEach(c => Measure(c));
							g.Width = g.Children.Any() ? g.Children.Max(c => c.Width) : w;
						}

						if (g.VerticalOptions.Is(Center))
							g.Height = h - (g.Margin.Top + g.Margin.Bottom);
						else if (g.Height < 0d)
						{
							g.Children.ForEach(c => Measure(c));
							g.Height = g.Children.Any() ? g.Children.Max(c => c.Height) : h;
						}

						g.Children.ForEach(c => Measure(c, g.Width, g.Height));
					}
					break;
				case Stack s:
					{

						if (s.Orientation.Is(StackOrientation.Horizontal))
						{
							s.Width = 0d;

							foreach (var child in s.Children)
							{
								if (child.Width < 0) Measure(child);

								var width = child.Width + child.Margin.Left + child.Margin.Right;
								s.Width += width;

								var height = child.Height + child.Margin.Top + child.Margin.Bottom;
								if (s.Height < height)
									s.Height = height;

							}
						}

						if (s.Orientation.Is(StackOrientation.Vertical))
						{
							s.Height = 0d;

							foreach (var child in s.Children)
							{
								if (child.Height < 0) Measure(child);

								var height = child.Height + child.Margin.Top + child.Margin.Bottom;
								s.Height += height;

								var width = child.Width + child.Margin.Left + child.Margin.Right;
								if (s.Width < width)
									s.Width = width;
							}
						}
					}
					break;
			}
		}

		static string GetCorrectFontFamilyName(string name) => name.Is("Consolas") ? "monospace" : name;

		static SKPaint GetStroke(Label label, double opacity) => new()
		{
			Color = label.TextColor.ToSKColor().WithOpacity(label.Opacity * opacity),
			TextSize = (float)(label.FontSize * density),
			Typeface = SKTypeface.FromFamilyName(GetCorrectFontFamilyName(label.FontFamily)),
			TextEncoding = SKTextEncoding.Utf16,
			TextAlign = SKTextAlign.Left,
		};

		public void Draw(SKCanvas canvas, View view, double x, double y, double w, double h, double baseOpacity)
		{
			if (view.IsNot())
				return;

			Measure(view, w - x, h - y);

			var rect = new SKRect(
				(float)((x + view.Margin.Left) * density),
				(float)((y + view.Margin.Top) * density),
				(float)((x + view.Margin.Left + view.Width) * density),
				(float)((y + view.Margin.Top + view.Height) * density)
				);

			var opacity = view.Opacity * baseOpacity;
			switch (view)
			{
				case Label l:
					using (SKPath roundRectPath = new())
					{
						using var fill = l.Fill.ToSkPaint(view.Width, view.Height, opacity);

						using var stroke = GetStroke(l, opacity);

						stroke.MeasureText(l.Text, ref rect);

						//roundRectPath.AddRect(rect);
						//canvas.DrawPath(roundRectPath, fill);

						var left = l.HorizontalOptions.Alignment switch
						{
							LayoutAlignment.Start => x + l.Margin.Left,
							LayoutAlignment.End => x + (w - l.Margin.Right),
							_ => x + (w - l.Width) / 2d,
						};

						var top = l.VerticalOptions.Alignment switch
						{
							LayoutAlignment.Start => y + l.Margin.Top + l.Height,
							LayoutAlignment.End => y + (h - l.Margin.Bottom),
							_ => y + (h + l.Height) / 2d,
						};

						var text = l.Text.Replace('♯', '#').Replace('♭', 'b');
						canvas.DrawText(text,
							(float)(left * density),
							(float)(top * density),
							stroke);
					}
					break;

				case Border b:
					{
						void DrawBorder(SKPaint paint)
						{

							if (b.CornerRadius.Left > 0)
							{
								using SKPath roundRectPath = new();
								roundRectPath.AddRoundRect(rect,
									(float)(b.CornerRadius.Left * density),
									(float)(b.CornerRadius.Top * density));
								canvas.DrawPath(roundRectPath, paint);
							}
							else canvas.DrawRect(rect, paint);
						}

						if (b.Fill.Is())
						{
							using var fill = b.Fill.ToSkPaint(view.Width, view.Height, opacity);
							{
								DrawBorder(fill);
							}
						}

						if (b.Stroke.Is())
						{
							using var stroke = b.Stroke.ToStrokeSkPaint(b.StrokeThikness, view.Width, view.Height, opacity);
							{
								DrawBorder(stroke);
							}
						}

						Draw(canvas, b.Content, x + b.Margin.Left, y + b.Margin.Top, w - b.Margin.Right, h - b.Margin.Bottom, opacity);
					}
					break;
				case Grid g:
					{
						using var fill = g.Fill.ToSkPaint(view.Width, view.Height, opacity);
						if (fill.Is()) canvas.DrawRect(rect, fill);

						foreach (var child in g.Children)
						{
							Draw(canvas, child, x + g.Margin.Left, y + g.Margin.Top, w - g.Margin.Right, h - g.Margin.Bottom, opacity);
						}
					}
					break;
				case Stack g:
					{
						using var fill = g.Fill.ToSkPaint(view.Width, view.Height, opacity);
						if (fill.Is()) canvas.DrawRect(rect, fill);

						var offsetX = 0d;
						var offsetY = 0d;
						foreach (var child in g.Children)
						{
							if (g.Orientation.Is(StackOrientation.Horizontal))
							{
								Draw(canvas, child, x + offsetX + g.Margin.Left, y + offsetY + g.Margin.Top, child.Width, view.Height, opacity);

								offsetX += child.Width + child.Margin.Left + child.Margin.Right;
							}

							if (g.Orientation.Is(StackOrientation.Vertical))
							{
								Draw(canvas, child, x + offsetX + g.Margin.Left, y + offsetY + g.Margin.Top, view.Width, child.Height, opacity);

								offsetY += child.Height + child.Margin.Top + child.Margin.Bottom;
							}
						}
					}
					break;
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

		public static LayoutOptions ToLayoutOptions(this HorizontalAlignment o) =>

			o.Is(HorizontalAlignment.Center) ? Center :
			o.Is(HorizontalAlignment.Left) ? Start :
			o.Is(HorizontalAlignment.Right) ? End :
			o.Is(HorizontalAlignment.Stretch) ? Fill :

			throw new ArgumentException();

		public static LayoutOptions ToLayoutOptions(this VerticalAlignment o) =>

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
		public double Width { get; set; } = -1;
		public double Height { get; set; } = -1;

		public double Opacity { get; set; } = 1d;

		public virtual Brush Fill { get; set; }
		public virtual Brush Stroke { get; set; }

		public double StrokeThickness { get; set; }

		public abstract SKPath ToSkPath();
		public SKPaint GetStrokeSkPaint(double w, double h, double opacity)
		{
			var skBrush = Stroke.ToSkPaint(w, h, opacity);
			skBrush.StrokeWidth = (float)(StrokeThickness * Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density);
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
			path.MoveTo((float)(X1 * Ext.density), (float)(Y1 * Ext.density));
			path.LineTo((float)(X2 * Ext.density), (float)(Y2 * Ext.density));
			path.Close();
			return path;
		}
	}
}
