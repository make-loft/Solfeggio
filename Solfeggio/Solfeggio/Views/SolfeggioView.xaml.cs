﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Ace;
using Ace.Extensions;

using Rainbow;

using SkiaSharp;
using SkiaSharp.Views.Forms;
using Solfeggio.Presenters;
using Solfeggio.Presenters.Options;
using Solfeggio.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using static Ace.Extensions.Colority;

using Grid = System.Windows.Controls.Grid;

namespace Solfeggio.Views
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
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

					var bandwidth = MusicalPresenter.Spectrum.Frequency;
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

		static readonly AppViewModel AppViewModel = Store.Get<AppViewModel>();
		static readonly MusicalPresenter MusicalPresenter = Store.Get<MusicalPresenter>();
		static readonly ProcessingManager ProcessingManager = Store.Get<ProcessingManager>();

		public SolfeggioView()
		{
			InitializeComponent();

			AppPalette.VisualThemeChanged += () =>
			{
				FlowerStrokePolyline.Stroke = AppPalette.GetBrush("Stroke.Geometry");
				SpiralStrokePolyline.Stroke = AppPalette.GetBrush("Stroke.Geometry");

				MagnitudePolyline.Fill = AppPalette.GetBrush("Fill.MagnitudePMI");
				MagnitudePolyline.Stroke = AppPalette.GetBrush("Stroke.MagnitudePMI");

				MagnitudeRawFramePolyline.Fill = AppPalette.GetBrush("Fill.MagnitudeRawFrame");
				MagnitudeRawFramePolyline.Stroke = AppPalette.GetBrush("Stroke.MagnitudeRawFrame");
			};

			SpectrogramCanvas.SizeChanged += (o, e) => _fullSpectrogramRefresh = true;

			Loop(100, () =>
			{
				Render();

				PianoCanvas.InvalidateSurface();
				MagnitudeCanvas.InvalidateSurface();
				SpectrogramCanvas.InvalidateSurface();
				MagnitudeRawFrameCanvas.InvalidateSurface();
				FlowerStrokeCanvas.InvalidateSurface();
				SpiralStrokeCanvas.InvalidateSurface();
				FlowerCanvas.InvalidateSurface();

				//foreach (var path in _completedPaths)
				//{
				//    canvas.DrawPath(path, paint);
				//}

				//foreach (var path in _inProgressPaths.Values)
				//{
				//    canvas.DrawPath(path, paint);
				//}
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
			SpiralStrokeCanvas.WidthRequest = MagnitudeRawFrameCanvas.Height;

			var spectrum = ProcessingManager.SpectrumBetter;
			if (spectrum.IsNot())
				return;

			var pianoCanvas = PianoCanvas;
			pianoCanvas.Children.Clear();

			var pianoKeys = MusicalPresenter.DrawPiano(pianoCanvas.Children, spectrum, pianoCanvas.Width, pianoCanvas.Height, ProcessingManager.Peaks);

			var activeProfile = ProcessingManager.ActiveProfile;
			var geometryFill = MusicalPresenter.DrawGeometry(ProcessingManager.Peaks, activeProfile.FrameSize, activeProfile.SampleRate,
				MusicalPresenter.Geometry.SpiralApproximationLevel, 1d / activeProfile.FrameSize, Pi.Half);
			var geometryStroke = MusicalPresenter.DrawGeometry(ProcessingManager.Peaks, activeProfile.FrameSize, activeProfile.SampleRate,
				MusicalPresenter.Geometry.FlowerApproximationLevel, 0d, Pi.Half);

			var centerX = MagnitudeRawFrameCanvas.Width / 2d;
			var centerY = MagnitudeRawFrameCanvas.Height / 2d;
			var radius = Math.Max(Math.Min(centerX, centerY), 1d);

			SpiralStrokePolyline.Points.Clear();
			FlowerStrokePolyline.Points.Clear();
			geometryFill.ForEach(p => SpiralStrokePolyline.Points.Add(new(centerY - p.X * radius, centerY - p.Y * radius)));
			geometryStroke.ForEach(p => FlowerStrokePolyline.Points.Add(new(centerY - p.X * radius, centerY - p.Y * radius)));

			if (FlowerCanvas.Width > 0)
			{
				centerX = FlowerCanvas.Width / 2d;
				centerY = FlowerCanvas.Height / 2d;
				radius = Math.Max(Math.Min(centerX, centerY), 1d);

				FlowerFillPolyline_.Points.Clear();
				FlowerStrokePolyline_.Points.Clear();
				if (SpiralSwitch.IsToggled)
					geometryFill.ForEach(p => FlowerFillPolyline_.Points.Add(new(centerY - p.X * radius, centerY - p.Y * radius)));
				if (FlowerSwitch.IsToggled)
					geometryStroke.ForEach(p => FlowerStrokePolyline_.Points.Add(new(centerY - p.X * radius, centerY - p.Y * radius)));
			}

			var waveInData = ProcessingManager.OuterFrame;
			var waveOutData = ProcessingManager.InnerFrame;

			MagnitudeRawFramePolyline.Points.Clear();
			MusicalPresenter
				.DrawFrame(waveInData, MagnitudeRawFrameCanvas.Width, MagnitudeRawFrameCanvas.Height)
				.Use(MagnitudeRawFramePolyline.Points.AddRange);

			MagnitudePolyline.Points.Clear();
			MusicalPresenter
				.DrawMagnitude(spectrum, MagnitudeCanvas.Width, MagnitudeCanvas.Height)
				.Use(MagnitudePolyline.Points.AddRange);

			MagnitudeCanvas.Children.Clear();

			var peaks = pianoKeys.SelectMany(h => h.Peaks);
			MusicalPresenter.DrawMarkers(MagnitudeCanvas.Children, MagnitudeCanvas.Width, MagnitudeCanvas.Height,
				AppPalette.GetBrush("MagnitudePeakBrush"), default, peaks.Select(p => p.Frequency));

			MagnitudeCanvas.Children.Add(MagnitudePolyline);

			if (pianoKeys.Is() && pianoKeys.Any())
			{
				var maxMagnitude = pianoKeys.Max(k => k.Magnitude);
				var minOpacity = 0.632d;
				var topOpacity = 1 - minOpacity;
				pianoKeys.ForEach(k => k.RelativeOpacity = minOpacity + topOpacity * k.Magnitude / maxMagnitude);

				var labels = MusicalPresenter.DrawPeakTitles(pianoKeys, MagnitudeCanvas.Width, MagnitudeCanvas.Height);
				labels.ForEach(p =>
				{
					p.HorizontalOptions = LayoutOptions.Start;
					p.VerticalOptions = LayoutOptions.Start;
					Canvas.Measure(p);
					MagnitudeCanvas.Children.Add(p);
					p.Margin = new(p.Margin.Left - p.Width / 2d, p.Margin.Top - p.Height / 8d, 0d, 0d);
				});
			}

			AppViewModel.Harmonics.Value = pianoKeys;

			var w = SpectrogramCanvas.Width;
			var h = SpectrogramCanvas.Height;

			var actualBand = MusicalPresenter.Spectrum.Frequency;
			var transformer = MusicalPresenter.GetScaleTransformer(actualBand, w);

			var count = 32;
			if (SpectrogramStack.Children.Count > count)
				SpectrogramStack.Children.RemoveAt(count);

			var magnitudeProjection = MusicalPresenter.Spectrum.Magnitude.VisualScaleFunc;
			var hh = SpectrogramCanvas.Height / count;
			var ww = SpectrogramCanvas.Width;

			if (ProcessingManager.IsPaused.Not())
			{
				SpectrogramStack.Children.Insert(0, DrawKeys(new Grid
				{
					BindingContext = new SpectrogramFrame(spectrum, pianoKeys),
					Background = GetSpectrogramLineBrush(spectrum, transformer, w, magnitudeProjection),
					Width = ww,
					Height = hh,
				}));
			}

			Grid DrawKeys(Grid grid)
			{
				var pressedHalfToneKeyColor = (Color)App.Current.Resources["PressedHalfToneKeyColor"];
				var pressedFullToneKeyColor = (Color)App.Current.Resources["PressedFullToneKeyColor"];

				grid.Children.Clear();

				grid.BindingContext.To<SpectrogramFrame>().PianoKeys.Select(k => new Border
				{
					Tag = (MusicalOptions.Tones[k.NoteNumber] ? pressedFullToneKeyColor : pressedHalfToneKeyColor).To(out var color),
					Fill = new SolidColorBrush(color.Mix(Channel.A, magnitudeProjection(k.Magnitude))),
					Margin = new(transformer.GetVisualOffset(k.LowerFrequency), 0, ww - transformer.GetVisualOffset(k.UpperFrequency), 0),
					Width = transformer.GetVisualOffset(k.UpperFrequency) - transformer.GetVisualOffset(k.LowerFrequency),
					Height = hh,
				})
				.ForEach(grid.Children.Add);

				grid.BindingContext.To<SpectrogramFrame>().PianoKeys.Select(k => new Border
				{
					Tag = (MusicalOptions.Tones[k.NoteNumber] ? pressedFullToneKeyColor : pressedHalfToneKeyColor).To(out var color),
					Fill = new SolidColorBrush(Palettes.Converters.GetOffsetColor(k, magnitudeProjection)),
					Width = (0.2d * (transformer.GetVisualOffset(k.UpperFrequency) - transformer.GetVisualOffset(k.LowerFrequency))).To(out var w),
					Margin = new(transformer.GetVisualOffset(k.Harmonic.Frequency).To(out var x) - w / 2d, 0, ww - (x + w / 2d), 0),
					Height = hh,
				})
				.ForEach(grid.Children.Add);

				return grid;
			}

			if (_fullSpectrogramRefresh)
			{
				_fullSpectrogramRefresh = false;
				SpectrogramStack.Children
					.OfType<Grid>()
					.ForEach(g =>
					{
						g.Width = w;
						g.BindingContext.To<SpectrogramFrame>().SpectrumInterpolated.To(out var spectrum);
						g.Background = GetSpectrogramLineBrush(spectrum, transformer, w, magnitudeProjection);
						DrawKeys(g);
					});
			}
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

		static LinearGradientBrush GetSpectrogramLineBrush(IList<Bin> bins, ScaleTransformer transformer, double length, Projection magnitudeProjection)
		{
			var from = transformer.GetLogicalOffset(0);
			var till = transformer.GetLogicalOffset(length);

			var color = (Color)App.Current.Resources["ColorD"];
			var stops = bins.Where(p => from <= p.Frequency && p.Frequency <= till)
				.Select(p => new GradientStop(color.Mix(Channel.A, magnitudeProjection(p.Magnitude)), (float)(transformer.GetVisualOffset(p.Frequency) / length)));
			return new(new GradientStopCollection().AppendRange(stops));
		}

		private void SwitchGeometry() => FlowerCanvas?
			.With(FlowerCanvas.WidthRequest = FlowerSwitch.IsToggled || SpiralSwitch.IsToggled ? Height : 0d);

		private void ContentPage_LayoutChanged(object sender, EventArgs e) => SwitchGeometry();
		private void GeometrySwitch_Toggled(object sender, ToggledEventArgs e) => SwitchGeometry();

		private void HideOptionsButton_Clicked(object sender, EventArgs e) => OptionsSwitch.IsToggled = false;
	}
}
