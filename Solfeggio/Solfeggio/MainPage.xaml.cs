using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ace;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Solfeggio.ViewModels;

namespace Solfeggio
{
    public partial class MainPage
    {
        readonly Dictionary<long, SKPath> _inProgressPaths = new Dictionary<long, SKPath>();
        readonly List<SKPath> _completedPaths = new List<SKPath>();

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
                    if (!_inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = new SKPath();
                        path.MoveTo(ConvertToPixel(args.Location));
                        _inProgressPaths.Add(args.Id, path);
                        CanvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Moved:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = _inProgressPaths[args.Id];
                        path.LineTo(ConvertToPixel(args.Location));
                        CanvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        _completedPaths.Add(_inProgressPaths[args.Id]);
                        _inProgressPaths.Remove(args.Id);
                        CanvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Cancelled:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        _inProgressPaths.Remove(args.Id);
                        CanvasView.InvalidateSurface();
                    }
                    break;
            }
        }

        SKPoint ConvertToPixel(SKPoint pt)
        {
            return new SKPoint((float) (CanvasView.CanvasSize.Width * pt.X / CanvasView.Width),
                (float) (CanvasView.CanvasSize.Height * pt.Y / CanvasView.Height));
        }

        public MainPage()
        {
            InitializeComponent();
            WaitAndExecute(100, () => CanvasView.InvalidateSurface());
        }

        private async void WaitAndExecute(int milisec, Action actionToExecute)
        {
            while (true)
            {
                await Task.Delay(milisec);
                actionToExecute();
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            var canvas = args.Surface.Canvas;
            canvas.Clear();

            OnCanvasViewPaintSurface1(sender, args);
            foreach (var path in _completedPaths)
            {
                canvas.DrawPath(path, paint);
            }

            foreach (var path in _inProgressPaths.Values)
            {
                canvas.DrawPath(path, paint);
            }
        }

        void OnCanvasViewPaintSurface1(object sender, SKPaintSurfaceEventArgs args)
        {
            var info = args.Info;
            var surface = args.Surface;
            var canvas = surface.Canvas;
            var spectrum = Store.Get<SpectralViewModel>().CurrentSpectrum;
            var magnitudes = spectrum.Select(c => (float) c.Magnitude).ToList();
            var max = magnitudes.Max();
            var scale = info.Height / max;

            var path = new SKPath();
            path.MoveTo(0.0f, info.Height);
            for (var i = 0; i < spectrum.Length; i++)
            {
                path.LineTo(i, info.Height - magnitudes[i] * scale);
            }

            path.LineTo(info.Width, info.Height);
            path.Close();

            var colors = new [] { SKColors.LightSteelBlue, SKColors.IndianRed };
            var startPoint = new SKPoint(0f, 0f);
            var endPoint =new SKPoint(0f, info.Height);
            var shader = SKShader.CreateLinearGradient(startPoint, endPoint, colors, null, SKShaderTileMode.Clamp);

            var back = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Shader = shader
            };

            var fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Yellow
            };

            var strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Gold,
                StrokeWidth = 1
            };

            canvas.Clear();
            canvas.DrawRect(0f,0f, info.Width, info.Height, back);
            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }
    }
}
