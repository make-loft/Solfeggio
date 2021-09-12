using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using Ace;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Solfeggio.Presenters;
using Solfeggio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Xamarin.Forms.Color;

namespace Solfeggio.Views
{
    [XamlCompilation (XamlCompilationOptions.Skip)]
    public partial class SolfeggioView
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
                        SpectrumCanvas.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Moved:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = _inProgressPaths[args.Id];
                        path.LineTo(ConvertToPixel(args.Location));
                        SpectrumCanvas.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        _completedPaths.Add(_inProgressPaths[args.Id]);
                        _inProgressPaths.Remove(args.Id);
                        SpectrumCanvas.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Cancelled:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        _inProgressPaths.Remove(args.Id);
                        SpectrumCanvas.InvalidateSurface();
                    }
                    break;
            }
        }

        SKPoint ConvertToPixel(SKPoint pt)
        {
            return new SKPoint((float) (SpectrumCanvas.CanvasSize.Width * pt.X / SpectrumCanvas.Width),
                (float) (SpectrumCanvas.CanvasSize.Height * pt.Y / SpectrumCanvas.Height));
        }

        public SolfeggioView()
        {
            InitializeComponent();
            WaitAndExecute(100, () =>
            {
	            SpectrumCanvas.InvalidateSurface();
				PianoCanvas.InvalidateSurface();
            });
        }

        private async void WaitAndExecute(int milliseconds, Action actionToExecute)
        {
            while (true)
            {
                await Task.Delay(milliseconds);
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

	    private ProcessingManager _spectralViewModel = Store.Get<ProcessingManager>();

		void OnCanvasViewPaintSurfaceP(object sender, SKPaintSurfaceEventArgs args)
	    {
		    var width = (float)args.Info.Width;
		    var height = (float)args.Info.Height;
		    var canvas = args.Surface.Canvas;
		    var pianoCanvas = new Panel(args);

		    var spectrum = _spectralViewModel.Spectrum;
		    if (spectrum.IsNot()) return;

		    pianoCanvas.Children.Clear();

		    var presenter = Store.Get<MusicalPresenter>();
		    //var max = spectrum.Values.Max() * 0.7;
		    //if (max > presenter.MaxMagnitude) presenter.MaxMagnitude = max;

		    presenter.DrawPiano(pianoCanvas.Children, spectrum, pianoCanvas.ActualWidth, pianoCanvas.ActualHeight);
		    //var waveCorrectionMargin = presenter.UseHorizontalLogScale ? WaveCanvas.Margin : new Thickness();
		    //presenter.DrawWave(WaveCanvas, waveOutData, WaveOutLine, waveCorrectionMargin);
		    //var tops = presenter.DrawPiano(PianoCanvas, spectrum);
		    //presenter.DrawTops(SpectrumCanvas, tops);

		    pianoCanvas.Draw();
		}

        void OnCanvasViewPaintSurface1(object sender, SKPaintSurfaceEventArgs args)
        {
	        var width = (float)args.Info.Width;
	        var height = (float)args.Info.Height;
	        var canvas = args.Surface.Canvas;
	        var spectrumCanvas = new Panel(args) { Background = CreateBackgroundBrush(height)};

			var spectrum = _spectralViewModel.Spectrum;
			if (spectrum.IsNot()) return;

	        var waveInData = _spectralViewModel.OuterFrame;
			var waveOutData = _spectralViewModel.InnerFrame;

			spectrumCanvas.Children.Add(SpectrumPolyline);

	        var presenter = Store.Get<MusicalPresenter>();
			var spectralViewModel = Store.Get<ProcessingManager>();
			//var max = spectrum.Values.Max() * 0.7;
			//if (max > presenter.MaxMagnitude) presenter.MaxMagnitude = max;

			SpectrumPolyline.Points.Clear();

			if (presenter.Spectrum.Magnitude.IsVisible)
				presenter.DrawMagnitude(spectrum, width, height).
				Use(SpectrumPolyline.Points.AppendRange);

			//if (presenter.Spectrum.Phase.IsVisible)
			//	presenter.DrawPhase(spectrum, width, height).
			//	Use(PhasePolyline.Points.AppendRange);

			if (presenter.Frame.Level.IsVisible)
				presenter.DrawFrame(spectralViewModel.OuterFrame, width, height).
				Use(WaveInLine.Points.AppendRange);

			if (presenter.Frame.Level.IsVisible && spectralViewModel.ActiveProfile.ActiveWindow.IsNot(Rainbow.Windowing.Rectangle))
				presenter.DrawFrame(spectralViewModel.InnerFrame, width, height).
				Use(WaveOutLine.Points.AppendRange);

			spectrumCanvas.Draw();
		}

	    private static LinearGradientBrush CreateBackgroundBrush(double h) => new()
	    {
		    EndPoint = new Point(0, h),
		    GradientStops =
		    {
			    new GradientStop
			    {
				    Color = FromHex("#FF616187"),
				    Offset = 0
			    },
			    new GradientStop
			    {
				    Color = IndianRed,
				    Offset = 1
			    }
		    }
	    };

	    private readonly Polyline SpectrumPolyline = new Polyline
	    {
		    Fill = new SolidColorBrush(Yellow),
		    Stroke = new SolidColorBrush(Gold),
			StrokeThickness = 1
	    };

	    private readonly Polyline WaveInLine = new Polyline
	    {
		    Stroke = new SolidColorBrush(DarkRed),
		    StrokeThickness = 1
	    };

		private readonly Polyline WaveOutLine = new Polyline
		{
			Stroke = new SolidColorBrush(GreenYellow),
			StrokeThickness = 1
		};
	}
}
