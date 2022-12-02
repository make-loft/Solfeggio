using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Ace;

using Rainbow;

using SkiaSharp;
using SkiaSharp.Views.Forms;
using Solfeggio.Presenters;
using Solfeggio.Presenters.Options;
using Solfeggio.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Grid = System.Windows.Controls.Grid;

namespace Solfeggio.Views
{
    [XamlCompilation (XamlCompilationOptions.Skip)]
    public partial class SolfeggioView
	{
        readonly Dictionary<long, SKPath> _inProgressPaths = new();
        readonly List<SKPath> _completedPaths = new();

        SKPaint paint = new()
		{
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Blue,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

		SKPoint fromOrigin = new();
		SKPoint tillOrigin = new();
		double density = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Density;

		void OnTouchEffectAction(object sender, SKTouchEventArgs args)
        {

            args.Handled = true;

			var control = (Canvas)sender;
			var height = Height;
			var width = Width;

            switch (args.ActionType)
            {
                case SKTouchAction.Pressed:
					fromOrigin = args.Location;

                    if (!_inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = new();
                        path.MoveTo(args.Location);
                        _inProgressPaths.Add(args.Id, path);
                        MagnitudeCanvas.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Moved:

					tillOrigin = args.Location;
					if (fromOrigin.Is(tillOrigin))
						return;

					var bandwidth = musicalPresenter.Spectrum.Frequency;
					var from = new Point(fromOrigin.X / density, fromOrigin.Y / density);
					var till = new Point(tillOrigin.X / density, tillOrigin.Y / density);

					fromOrigin = tillOrigin;

					var deltaX = from.X - till.X;
					var deltaY = from.Y - till.Y;
					var isHorizontalMove = deltaX * deltaX > deltaY * deltaY;

					if (isHorizontalMove)
					{
						var originalScaler = MusicalPresenter.GetScaleTransformer(bandwidth, width);
						var originalFromOffset = originalScaler.GetLogicalOffset(from.X);
						var originalTillOffset = originalScaler.GetLogicalOffset(till.X);

						bandwidth.ShiftThreshold(originalFromOffset, originalTillOffset);
					}
					else
					{
						bandwidth.TransformRelative(width, height, from, till);
					}

					RequestFullSpectrogramRefresh(256);

					if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        SKPath path = _inProgressPaths[args.Id];
                        path.LineTo(args.Location);
                        MagnitudeCanvas.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        _completedPaths.Add(_inProgressPaths[args.Id]);
                        _inProgressPaths.Remove(args.Id);
                        MagnitudeCanvas.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Cancelled:
                    if (_inProgressPaths.ContainsKey(args.Id))
                    {
                        _inProgressPaths.Remove(args.Id);
                        MagnitudeCanvas.InvalidateSurface();
                    }
                    break;

				case SKTouchAction.WheelChanged:
					if (_inProgressPaths.ContainsKey(args.Id))
					{
						_inProgressPaths.Remove(args.Id);
						MagnitudeCanvas.InvalidateSurface();
					}
					break;
			}
        }

		MusicalPresenter musicalPresenter = Store.Get<MusicalPresenter>();
		ProcessingManager processingManager = Store.Get<ProcessingManager>();

		public SolfeggioView()
        {
            InitializeComponent();
            Loop(100, () =>
            {
				Render();

				PianoCanvas.InvalidateSurface();
				MagnitudeCanvas.InvalidateSurface();
				SkiaSpectrogramCanvas.InvalidateSurface();
				MagnitudeRawFrameCanvas.InvalidateSurface();
				FlowerStrokeCanvas.InvalidateSurface();
				FlowerFillCanvas.InvalidateSurface();
            });
        }

        private async void Loop(int milliseconds, Action actionToExecute)
        {
            while (true)
            {
                await Task.Delay(milliseconds);
                actionToExecute();
            }
        }

		struct SpectrogramFrame
		{
			public IList<Bin> SpectrumInterpolated { get; }
			public IList<Models.PianoKey> PianoKeys { get; }
			public SpectrogramFrame(IList<Bin> spectrumInterpolated, IList<Models.PianoKey> pianoKeys)
			{
				SpectrumInterpolated = spectrumInterpolated;
				PianoKeys = pianoKeys;
			}
		}

		void OnCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs args) => sender.To<Canvas>().Draw(args);

		void Render()
		{
			FlowerStrokeCanvas.WidthRequest = MagnitudeRawFrameCanvas.Height;
			FlowerFillCanvas.WidthRequest = MagnitudeRawFrameCanvas.Height;

			var spectrum = processingManager.SpectrumBetter;

			if (spectrum.IsNot()) return;

			var pianoCanvas = PianoCanvas;
			pianoCanvas.Children.Clear();

			var pianoKeys = musicalPresenter.DrawPiano(pianoCanvas.Children, spectrum, pianoCanvas.Width, pianoCanvas.Height, processingManager.Peaks);

			var activeProfile = processingManager.ActiveProfile;
			var geometryFill = MusicalPresenter.DrawGeometry(processingManager.Peaks, activeProfile.FrameSize, activeProfile.SampleRate,
				1d, 1d / activeProfile.FrameSize, Pi.Half);
			var geometryStroke = MusicalPresenter.DrawGeometry(processingManager.Peaks, activeProfile.FrameSize, activeProfile.SampleRate,
				1d, 0d, Pi.Half);

			var centerX = MagnitudeRawFrameCanvas.Width / 2d;
			var centerY = MagnitudeRawFrameCanvas.Height / 2d;
			var radius = Math.Max(Math.Min(centerX, centerY), 1d);

			FlowerFillPolyline.Points.Clear();
			FlowerStrokePolyline.Points.Clear();
			geometryFill.ForEach(p => FlowerFillPolyline.Points.Add(new(centerY - p.X * radius, centerY - p.Y * radius)));
			geometryStroke.ForEach(p => FlowerStrokePolyline.Points.Add(new(centerY - p.X * radius, centerY - p.Y * radius)));

			var waveInData = processingManager.OuterFrame;
			var waveOutData = processingManager.InnerFrame;

			MagnitudeRawFramePolyline.Points.Clear();
			musicalPresenter
				.DrawFrame(waveInData, MagnitudeRawFrameCanvas.Width, MagnitudeRawFrameCanvas.Height)
				.Use(MagnitudeRawFramePolyline.Points.AddRange);

			MagnitudePolyline.Points.Clear();
			musicalPresenter
				.DrawMagnitude(spectrum, MagnitudeCanvas.Width, MagnitudeCanvas.Height)
				.Use(MagnitudePolyline.Points.AddRange);

			MagnitudeCanvas.Children.Clear();
			MagnitudeCanvas.Children.Add(MagnitudePolyline);
			//MagnitudeCanvas.Children.Add(FlowerFillPolyline);
			//MagnitudeCanvas.Children.Add(FlowerStrokePolyline);

			if (pianoKeys.Is())
			{
				var labels = musicalPresenter.DrawPeakLabels(pianoKeys, MagnitudeCanvas.Width, MagnitudeCanvas.Height);

				labels.ForEach(p =>
				{
					p.HorizontalOptions = LayoutOptions.Start;
					p.VerticalOptions = LayoutOptions.Start;
					Canvas.Measure(p);
					MagnitudeCanvas.Children.Add(p);
					p.Margin = new(p.Margin.Left - p.WidthRequest / 2d, p.Margin.Top - p.Height / 8d, 0d, 0d);
				});
			}

			var w = SkiaSpectrogramCanvas.Width;
			var h = SkiaSpectrogramCanvas.Height;

			var actualBand = musicalPresenter.Spectrum.Frequency;
			var transformer = MusicalPresenter.GetScaleTransformer(actualBand, w);

			var count = 32;
			if (SpectrogramCanvas.Children.Count > count)
				SpectrogramCanvas.Children.RemoveAt(count);

			var hh = SkiaSpectrogramCanvas.Height / count;
			var ww = SkiaSpectrogramCanvas.Width;

			var magnitudeProjection = musicalPresenter.Spectrum.Magnitude.VisualScaleFunc;
			SpectrogramCanvas.Children.Insert(0, DrawKeys(new Grid
			{
				BindingContext = new SpectrogramFrame(spectrum, pianoKeys),
				Background = GetSpectrogramLineBrush(spectrum, transformer, w, magnitudeProjection),
				WidthRequest = ww,
				HeightRequest = hh,
			}));

			Grid DrawKeys(Grid grid)
			{
				var pressedHalfToneKeyColor = (Color)App.Current.Resources["PressedHalfToneKeyColor"];
				var pressedFullToneKeyColor = (Color)App.Current.Resources["PressedFullToneKeyColor"];

				grid.Children.Clear();
				grid.BindingContext.To<SpectrogramFrame>().PianoKeys.Select(k => new Border
				{
					Tag = (MusicalOptions.Tones[k.NoteNumber] ? pressedFullToneKeyColor : pressedHalfToneKeyColor).To(out var color),
					Fill = new SolidColorBrush(SetAlpha(color, magnitudeProjection(k.Magnitude))),
					Margin = new(transformer.GetVisualOffset(k.LowerFrequency), 0, ww - transformer.GetVisualOffset(k.UpperFrequency), 0),
					Width = transformer.GetVisualOffset(k.UpperFrequency) - transformer.GetVisualOffset(k.LowerFrequency),
					Height = hh,
				})
				.ForEach(grid.Children.Add);

				grid.BindingContext.To<SpectrogramFrame>().PianoKeys.Select(k => new Border
				{
					Tag = (MusicalOptions.Tones[k.NoteNumber] ? pressedFullToneKeyColor : pressedHalfToneKeyColor).To(out var color),
					Fill = new SolidColorBrush(FromArgb
					(
						//1d,
						//System.Math.Sqrt(magnitudeProjection(k.Magnitude)),
						0.8 + 0.2 * magnitudeProjection(k.Magnitude),
						0.0,
						1.0 - 2.0 * Math.Abs(k.DeltaFrequency) / (k.UpperFrequency - k.LowerFrequency).To(out var d),
						0.5 + 1.0 * d
					)),
					Height = hh,
					Width = (0.2d * (transformer.GetVisualOffset(k.UpperFrequency) - transformer.GetVisualOffset(k.LowerFrequency))).To(out var w),
					Margin = new(transformer.GetVisualOffset(k.Harmonic.Frequency).To(out var x) - w / 2d, 0, ww - (x + w / 2d), 0),
				})
				.ForEach(grid.Children.Add);

				return grid;
			}

			if (_fullSpectrogramRefresh)
			{
				_fullSpectrogramRefresh = false;
				SpectrogramCanvas.Children
					.OfType<Grid>()
					.ForEach(g => DrawKeys(g).Background = GetSpectrogramLineBrush(
						g.BindingContext.To<SpectrogramFrame>().SpectrumInterpolated, transformer, w, magnitudeProjection));
			}
#if false

#endif
			//SpectrogramCanvas.Children.OfType<Canvas>().Last().InvalidateSurface();

			//foreach (var path in _completedPaths)
			//{
			//    canvas.DrawPath(path, paint);
			//}

			//foreach (var path in _inProgressPaths.Values)
			//{
			//    canvas.DrawPath(path, paint);
			//}
		}

		bool _fullSpectrogramRefresh;
		int _requestsCount;

		async void RequestFullSpectrogramRefresh(int delay)
		{
			_requestsCount++;
			await Task.Delay(delay);
			_requestsCount--;
			if (_requestsCount > 0)
				return;

			_fullSpectrogramRefresh = true;
		}


		static LinearGradientBrush GetSpectrogramLineBrush(IList<Bin> bins, ScaleTransformer transformer, double width, Projection magnitudeProjection)
		{
			var from = transformer.GetLogicalOffset(0);
			var till = transformer.GetLogicalOffset(width);

			var color = (Color)App.Current.Resources["ColorD"];
			var stops = bins.Where(p => from <= p.Frequency && p.Frequency <= till)
				.Select(p => new GradientStop(SetAlpha(color, magnitudeProjection(p.Magnitude)), (float)(transformer.GetVisualOffset(p.Frequency) / width)));
			return new(new GradientStopCollection().AppendRange(stops));
		}

		static Color SetAlpha(Color color, double alpha) => Color.FromRgba(color.R, color.G, color.B , alpha);
		static Color FromArgb(double a, double r, double g, double b) => Color.FromRgba(r, g, b, a);
	}
}
