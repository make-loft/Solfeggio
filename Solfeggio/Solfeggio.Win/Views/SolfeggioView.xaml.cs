using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ace;

using Rainbow;

using Solfeggio.Presenters;
using Solfeggio.ViewModels;

namespace Solfeggio.Views
{
	public partial class SolfeggioView
	{
		static void Shift(Bandwidth bandwidth, double delta, double lowerDirection, double upperDirection)
		{
			var scaleFunc = bandwidth.VisualScaleFunc;
			var center = (bandwidth.Threshold.Upper - bandwidth.Threshold.Lower) / 2;
			var centerS = (scaleFunc(bandwidth.Threshold.Upper) - scaleFunc(bandwidth.Threshold.Lower)) / 2;
			var offsetS = delta * bandwidth.Threshold.Length * centerS / (8 * center);

			var lowerOffsetS = lowerDirection * offsetS;
			var upperOffsetS = upperDirection * offsetS;

			bandwidth.Threshold.Shift(lowerOffsetS, upperOffsetS, scaleFunc, bandwidth.Limit.Lower, bandwidth.Limit.Upper);
		}

		public SolfeggioView()
		{
			InitializeComponent();
			Focus();

			var appViewModel = Store.Get<AppViewModel>();
			var musicalPresenter = Store.Get<MusicalPresenter>();
			var processingManager = Store.Get<ProcessingManager>();

			MouseLeftButtonUp += (o, e) => Mouse.Capture(default);
			MouseLeftButtonDown += (o, e) => Mouse.Capture(e.OriginalSource as Canvas);
			PreviewKeyDown += (o, e) => processingManager.IsPaused = e.Key.Is(Key.Space)
				? processingManager.IsPaused.Not()
				: processingManager.IsPaused;

			Point from = default;
			MouseLeftButtonUp += (o, e) => from = default;
			MouseLeftButtonDown += (o, e) => from = e.GetPosition(this);
			MagnitudeCanvas.MouseMove += MouseMove;
			PhaseCanvas.MouseMove += MouseMove;
			PianoCanvas.MouseMove += MouseMove;
			FrameCanvas.MouseMove += MouseMove;

			void MouseMove(object o, MouseEventArgs e)
			{
				if (e.LeftButton.IsNot(MouseButtonState.Pressed))
					return;

				var till = e.GetPosition(this);
				if (from.Is(default))
				{
					from = till;
					return;
				}

				var deltaX = from.X - till.X;
				var deltaY = from.Y - till.Y;
				var isHorizontalMove = deltaX * deltaX > deltaY * deltaY;

				var control = (FrameworkElement)o;
				var bandwidth = o.Is(FrameCanvas)
					? musicalPresenter.Frame.Offset
					: musicalPresenter.Spectrum.Frequency;

				if (isHorizontalMove)
				{
					var originalScaler = MusicalPresenter.GetScaleTransformer(bandwidth, control.ActualWidth);
					var originalFromOffset = originalScaler.GetLogicalOffset(from.X);
					var originalTillOffset = originalScaler.GetLogicalOffset(till.X);

					bandwidth.ShiftThreshold(originalFromOffset, originalTillOffset);
				}
				else
				{
					var center = control.ActualWidth / 2;

					var basicScaler = MusicalPresenter.GetScaleTransformer(bandwidth, control.ActualHeight);
					var basicFromOffset = basicScaler.GetLogicalOffset(from.Y);
					var basicTillOffset = basicScaler.GetLogicalOffset(till.Y);

					var alignScaler = MusicalPresenter.GetScaleTransformer(bandwidth, control.ActualWidth);
					var alignFromOffset = alignScaler.GetLogicalOffset(from.X);
					var alignTillOffset = alignScaler.GetLogicalOffset(center);

					bandwidth.ShiftThreshold(alignFromOffset, alignTillOffset);

					bandwidth.ScaleThreshold(basicFromOffset, basicTillOffset);

					var finalScaler = MusicalPresenter.GetScaleTransformer(bandwidth, control.ActualWidth);
					var finalFromOffset = finalScaler.GetLogicalOffset(center);
					var finalTillOffset = finalScaler.GetLogicalOffset(from.X);

					bandwidth.ShiftThreshold(finalFromOffset, finalTillOffset);
				}

				from = till;

				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
				{
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						bandwidth.Limit.Lower = bandwidth.Threshold.Lower;
						bandwidth.Limit.Upper = bandwidth.Threshold.Upper;
					}

					return;
				}

				bandwidth.LimitThreshold();
			};

			MouseWheel += (o, e) =>
			{
				var lowerDirection = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? +1 : -1;
				var upperDirection = +1;

				Shift(musicalPresenter.Spectrum.Frequency, e.Delta / 16, lowerDirection, upperDirection);
			};

			PreviewKeyDown += (o, e) =>
			{
				var upperDirection = e.Key switch
				{
					Key.Left or Key.Up => -1,
					Key.Right or Key.Down => +1,
					_ => 0,
				};

				var lowerDirection = e.Key switch
				{
					Key.Left or Key.Down => -1,
					Key.Right or Key.Up => +1,
					_ => 0,
				};

				e.Handled = lowerDirection.IsNot(0) && upperDirection.IsNot(0);
				if (e.Handled.Not()) return;

				Shift(musicalPresenter.Spectrum.Frequency, + 1, lowerDirection, upperDirection);
			};

			var timer = new DispatcherTimer();
			timer.Tick += (o, e) =>
			{
				var spectrum = processingManager.Spectrum;
				var spectrumInterpolated = processingManager.SpectrumBetter;
				if (spectrum.IsNot()) return;

				PianoCanvas.Children.Clear();

				PhaseCanvas.Children.Clear();
				MagnitudeCanvas.Children.Clear();

				PhaseCanvas.Children.Setup(c => new(c)
				{
					Items =
					{
						Polyline_Phase_FFT,
						Polyline_Phase_PMI,
					}
				}).OfType<Polyline>().ForEach(p => p.Points.Clear());
				MagnitudeCanvas.Children.Setup(c => new(c)
				{
					Items =
					{
						Polyline_Magnitude_FFT,
						Polyline_Magnitude_PMI,
						//Polyline_Flower
					}
				}).OfType<Polyline>().ForEach(p => p.Points.Clear());
				FrameCanvas.Children.OfType<Polyline>().ForEach(p => p.Points.Clear());

				static bool IsVisible(UIElement element) => element.Visibility.IsNot(Visibility.Collapsed);

				var width = FrameCanvas.ActualWidth;
				var height = FrameCanvas.ActualHeight;
				if (height > 0 && width > 0)
				{
					if (IsVisible(Polyline_Frame_Direct))
						musicalPresenter.DrawFrame(processingManager.OuterFrame, width, height).
						Use(Polyline_Frame_Direct.Points.AppendRange);

					if (IsVisible(Polyline_Frame_Window))
						musicalPresenter.DrawFrame(processingManager.InnerFrame, width, height).
						Use(Polyline_Frame_Window.Points.AppendRange);
				}

				width = PhaseCanvas.ActualWidth;
				height = PhaseCanvas.ActualHeight;
				if (height > 0 && width > 0)
				{
					if (IsVisible(Polyline_Phase_FFT))
						musicalPresenter.DrawPhase(spectrum, PhaseCanvas.ActualWidth, PhaseCanvas.ActualHeight).
						Use(Polyline_Phase_FFT.Points.AppendRange);

					if (IsVisible(Polyline_Phase_PMI))
						musicalPresenter.DrawPhase(spectrumInterpolated, PhaseCanvas.ActualWidth, PhaseCanvas.ActualHeight).
						Use(Polyline_Phase_PMI.Points.AppendRange);
				}

				width = MagnitudeCanvas.ActualWidth;
				height = MagnitudeCanvas.ActualHeight;
				if (height > 0 && width > 0)
				{
					if (IsVisible(Polyline_Magnitude_FFT))
						musicalPresenter.DrawMagnitude(spectrum, width, height).
						Use(Polyline_Magnitude_FFT.Points.AppendRange);

					if (IsVisible(Polyline_Magnitude_PMI))
						musicalPresenter.DrawMagnitude(spectrumInterpolated, width, height).
						Use(Polyline_Magnitude_PMI.Points.AppendRange);

					FFTHistogramCanvas.Children.Clear();
					PMIHistogramCanvas.Children.Clear();

					if (IsVisible(FFTHistogramCanvas))
						musicalPresenter.DrawHistogram(spectrum, width, height).
							ForEach(FFTHistogramCanvas.Children.Add);

					if (IsVisible(PMIHistogramCanvas))
						musicalPresenter.DrawHistogram(spectrumInterpolated, width, height).
							ForEach(PMIHistogramCanvas.Children.Add);
				}

				MagnitudeCanvas.Children.Add(FFTHistogramCanvas);
				MagnitudeCanvas.Children.Add(PMIHistogramCanvas);

				var discreteStep = processingManager.ActiveProfile.SampleRate / processingManager.ActiveProfile.FrameSize;

				var resources = App.Current.Resources;

				var vA = resources["Visibility.FrequencyDiscreteGrid"];
				var zIndexA = (int)resources["ZIndex.FrequencyDiscreteGrid"];
				if (vA.Is(Visibility.Visible))
				{
					musicalPresenter.DrawMarkers(PhaseCanvas.Children, width, height,
						AppPalette.ButterflyGridBrush, AppPalette.NoteGridBrush,
						musicalPresenter.EnumerateGrid(discreteStep), zIndexA);

					musicalPresenter.DrawMarkers(MagnitudeCanvas.Children, width, height,
						AppPalette.ButterflyGridBrush, AppPalette.NoteGridBrush,
						musicalPresenter.EnumerateGrid(discreteStep), zIndexA);
				}

				var vB = resources["Visibility.FrequencyNotesGrid"];
				var zIndexB = (int)resources["ZIndex.FrequencyNotesGrid"];
				if (vB.Is(Visibility.Visible))
				{
					musicalPresenter.DrawMarkers(PhaseCanvas.Children, width, height,
						AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
						musicalPresenter.EnumerateNotes(), zIndexB);

					musicalPresenter.DrawMarkers(MagnitudeCanvas.Children, width, height,
						AppPalette.ButterflyGridBrush, AppPalette.NoteGridBrush,
						musicalPresenter.EnumerateNotes(), zIndexB);
				}

				var peakKeys = musicalPresenter.DrawPiano(PianoCanvas.Children, spectrumInterpolated, PianoCanvas.ActualWidth, PianoCanvas.ActualHeight, out var peaks);
				var vC = resources["Visibility.Peak"];
				if (vC.Is(Visibility.Visible) && musicalPresenter.VisualProfile.PeakProfiles.Any(p => p.Value.IsVisible))
					musicalPresenter.DrawPeakLabels(peakKeys, width, height)
					.ForEach(p =>
					{
						MagnitudeCanvas.Children.Add(p);
						p.UpdateLayout();
						p.Margin = new(p.Margin.Left - p.ActualWidth / 2d, p.Margin.Top - p.ActualHeight / 8d, 0d, 0d);
					});

				var activeProfile = processingManager.ActiveProfile;
				AppView.GeometryView.Draw(peaks, activeProfile.SampleSize, activeProfile.SampleRate);

				musicalPresenter.DrawMarkers(PhaseCanvas.Children, PhaseCanvas.ActualWidth, PhaseCanvas.ActualHeight,
					AppPalette.GetBrush("PhasePeakBrush"), default, peaks.Select(p => p.Frequency), zIndexA);

				musicalPresenter.DrawMarkers(MagnitudeCanvas.Children, width, height,
					AppPalette.GetBrush("MagnitudePeakBrush"), default, peaks.Select(p => p.Frequency), zIndexA);

				appViewModel.Harmonics = peakKeys;

				if (processingManager.IsPaused)
					return;

				var w = SpectrogramCanvas.ActualWidth;
				var h = SpectrogramCanvas.ActualHeight;

				var actualBand = musicalPresenter.Spectrum.Frequency;

				var count = 127;
				if (SpectrogramCanvas.Children.Count > count)
					SpectrogramCanvas.Children.RemoveAt(count);

				var magnitudeProjection = musicalPresenter.Spectrum.Magnitude.VisualScaleFunc;
				SpectrogramCanvas.Children.Insert(0, new Rectangle 
				{
					Tag = spectrumInterpolated,
					Fill = GetSpectrogramLineBrush(spectrumInterpolated, actualBand, w, h, magnitudeProjection),
				});

				var hh = SpectrogramFrame.ActualHeight / count;
				SpectrogramCanvas.Children.OfType<Rectangle>().ForEach(r => r.Height = hh);

				void FullSpectrogramRefresh() => SpectrogramCanvas.Children
					.OfType<Rectangle>()
					.ForEach(r => r.Fill = GetSpectrogramLineBrush((IList<Bin>)r.Tag, actualBand, w, h, magnitudeProjection));

				async void RequestFullSpectrogramRefresh(int delay)
				{
					_requestsCount++;
					await Task.Delay(delay);
					_requestsCount--;
					if (_requestsCount > 0)
						return;

					FullSpectrogramRefresh();
				}

				var threshold = actualBand.Threshold;
				if (IsStateChanged(actualBand.VisualScaleFunc, threshold).Not())
					return;

				KeepState(actualBand.VisualScaleFunc, threshold);
				RequestFullSpectrogramRefresh(500);
			};

			timer.Start();
		}

		int _requestsCount = 0;
		Projection _previousScaleFunc;
		double _previousLower, _previousUpper;

		bool IsStateChanged(Projection actualScaleFunc, SmartRange threshold) =>
			threshold.Lower.IsNot(_previousLower) ||
			threshold.Upper.IsNot(_previousUpper) ||
			actualScaleFunc.IsNot(_previousScaleFunc);

		void KeepState(Projection actualScaleFunc, SmartRange threshold)
		{
			_previousLower = threshold.Lower;
			_previousUpper = threshold.Upper;
			_previousScaleFunc = actualScaleFunc;
		}

		private static LinearGradientBrush GetSpectrogramLineBrush(IList<Bin> bins, Bandwidth bandwidth, double w, double h, Projection magnitudeProjection)
		{
			var transformer = MusicalPresenter.GetScaleTransformer(bandwidth, w);

			var from = transformer.GetLogicalOffset(0);
			var till = transformer.GetLogicalOffset(w);

			var stops = bins.Where(p => from <= p.Frequency && p.Frequency <= till)
				.Select(p => new GradientStop(Color.FromArgb(
					(byte)(magnitudeProjection(p.Magnitude) * 255), 255, 255, 255), transformer.GetVisualOffset(p.Frequency) / w)).ToList();
			return new(new(stops), 0d);
		}
	}
}
