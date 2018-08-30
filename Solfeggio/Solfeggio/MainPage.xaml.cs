using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Solfeggio
{
	public partial class MainPage
	{
	    Dictionary<long, SKPath> inProgressPaths = new Dictionary<long, SKPath>();
	    List<SKPath> completedPaths = new List<SKPath>();

	    SKPaint paint = new SKPaint
	    {
	        Style = SKPaintStyle.Stroke,
	        Color = SKColors.Blue,
	        StrokeWidth = 10,
	        StrokeCap = SKStrokeCap.Round,
	        StrokeJoin = SKStrokeJoin.Round
	    };

	    void OnTouchEffectAction(object sender, SKTouchEventArgs args)
	    {
	        args.Handled = true;
	        switch (args.ActionType)
	        {
	            case SKTouchAction.Pressed:
	                if (!inProgressPaths.ContainsKey(args.Id))
	                {
	                    SKPath path = new SKPath();
	                    path.MoveTo(ConvertToPixel(args.Location));
	                    inProgressPaths.Add(args.Id, path);
	                    canvasView.InvalidateSurface();
	                }
	                break;

	            case SKTouchAction.Moved:
	                if (inProgressPaths.ContainsKey(args.Id))
	                {
	                    SKPath path = inProgressPaths[args.Id];
	                    path.LineTo(ConvertToPixel(args.Location));
	                    canvasView.InvalidateSurface();
	                }
	                break;

	            case SKTouchAction.Released:
	                if (inProgressPaths.ContainsKey(args.Id))
	                {
	                    completedPaths.Add(inProgressPaths[args.Id]);
	                    inProgressPaths.Remove(args.Id);
	                    canvasView.InvalidateSurface();
	                }
	                break;

	            case SKTouchAction.Cancelled:
	                if (inProgressPaths.ContainsKey(args.Id))
	                {
	                    inProgressPaths.Remove(args.Id);
	                    canvasView.InvalidateSurface();
	                }
	                break;
	        }
	    }

	    SKPoint ConvertToPixel(SKPoint pt)
	    {
	        return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
	            (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
	    }

        public MainPage()
	    {
            InitializeComponent();
            return;
	        //var canvasView = new SKCanvasView();
	        canvasView.PaintSurface += OnCanvasViewPaintSurface;
	        Content = canvasView;

	        var timer = new Timer(1000);
            timer.Elapsed += (sender, args) => canvasView.InvalidateSurface();
            timer.Start();
        }

	    void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
	    {
	        SKCanvas canvas = args.Surface.Canvas;
	        canvas.Clear();

            OnCanvasViewPaintSurface1(sender, args);
	        foreach (SKPath path in completedPaths)
	        {
	            canvas.DrawPath(path, paint);
	        }

	        foreach (SKPath path in inProgressPaths.Values)
	        {
	            canvas.DrawPath(path, paint);
	        }
	    }

        void OnCanvasViewPaintSurface1(object sender, SKPaintSurfaceEventArgs args)
	    {
	        SKImageInfo info = args.Info;
	        SKSurface surface = args.Surface;
	        SKCanvas canvas = surface.Canvas;

	        canvas.Clear();

	        // Create the path
	        var path = new SKPath();

	        // Define the first contour
	        path.MoveTo(0.0f, info.Height);
	        var spectrum = App.CurrentSpectrum;
	        var magnitudes = spectrum.Select(c => (float) c.Magnitude).ToList();
	        var max = magnitudes.Max();
	        var scale = info.Height / max;
	        for (var i = 0; i < spectrum.Length; i++)
	        {
	            path.LineTo(i, magnitudes[i] * scale);
	        }

	        path.LineTo(info.Width, info.Height);
            path.Close();

	        // Create two SKPaint objects
	        SKPaint strokePaint = new SKPaint
	        {
	            Style = SKPaintStyle.Stroke,
	            Color = SKColors.Magenta,
	            StrokeWidth = 3
	        };

	        SKPaint fillPaint = new SKPaint
	        {
	            Style = SKPaintStyle.Fill,
	            Color = SKColors.Cyan
	        };

	        // Fill and stroke the path
	        canvas.DrawPath(path, fillPaint);
	        canvas.DrawPath(path, strokePaint);
	    }
	}
}
