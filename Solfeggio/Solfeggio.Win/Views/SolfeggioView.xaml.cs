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
using Ace.Extensions;

using Microsoft.Win32;

using Rainbow;

using Solfeggio.Presenters;
using Solfeggio.Presenters.Options;
using Solfeggio.ViewModels;

using static Ace.Extensions.Colority;

namespace Solfeggio.Views
{
	public partial class SolfeggioView
	{
		AppViewModel appViewModel = Store.Get<AppViewModel>();
		MusicalPresenter musicalPresenter = Store.Get<MusicalPresenter>();
		ProcessingManager processingManager = Store.Get<ProcessingManager>();

		public SolfeggioView()
		{
			InitializeComponent();

			Loaded += (o, e) => Focus();
			SizeChanged += (o, e) => _sizeChanged = true;

			MouseLeftButtonUp += (o, e) => Mouse.Capture(default);
			MouseLeftButtonDown += (o, e) => Mouse.Capture(e.OriginalSource as Canvas);
			PreviewKeyDown += (o, e) => processingManager.IsPaused = e.Key.Is(Key.Space)
				? processingManager.IsPaused.Not()
				: processingManager.IsPaused;

			Point from = default;
			MouseLeftButtonUp += (o, e) => from = default;
			MouseLeftButtonDown += (o, e) => from = e.GetPosition(this);
			SpectrogramCanvas.MouseMove += MouseMove;
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
				var flip = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && isHorizontalMove.Not();
				var bandwidth = o.Is(FrameCanvas)
					? flip ? musicalPresenter.Frame.Level : musicalPresenter.Frame.Offset
					: flip ? musicalPresenter.Spectrum.Magnitude : musicalPresenter.Spectrum.Frequency;

				if (isHorizontalMove)
				{
					var originalScaler = MusicalPresenter.GetScaleTransformer(bandwidth, control.ActualWidth);
					var originalFromOffset = originalScaler.GetLogicalOffset(from.X);
					var originalTillOffset = originalScaler.GetLogicalOffset(till.X);

					bandwidth.ShiftThreshold(originalFromOffset, originalTillOffset);
				}
				else
				{
					bandwidth.TransformRelative(control.ActualWidth, control.ActualHeight, from, till);
				}

				from = till;

				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
				{
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
					{
						bandwidth.Scope.From = bandwidth.Window.From;
						bandwidth.Scope.Till = bandwidth.Window.Till;
					}

					return;
				}

				bandwidth.LimitThreshold();
			};

			MouseWheel += (o, e) =>
			{
				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
				{
					var offset = e.Delta * ActualWidth / 32d;
					musicalPresenter.Spectrum.Frequency.Transform(ActualWidth, offset, true);
					return;
				}

				var control = o as FrameworkElement;
				//var control = Mouse.DirectlyOver.As<DependencyObject>()
				//	?.EnumerateSelfAndVisualAncestors().OfType<Canvas>().FirstOrDefault()
				//	?? Mouse.DirectlyOver.As<FrameworkElement>();

				var bandwidth = o.Is(FrameCanvas)
					? musicalPresenter.Frame.Offset
					: musicalPresenter.Spectrum.Frequency;

				var from = Mouse.GetPosition(control);
				var till = new Point { X = from.X, Y = from.Y + e.Delta / 8d };

				if (control.Is())
					bandwidth.TransformRelative(control.ActualWidth, control.ActualHeight, from, till);
			};

			var navigationKeys = new[] { Key.Left, Key.Up, Key.Right, Key.Down };

			PreviewKeyDown += async (o, e) =>
			{
				var control = Mouse.DirectlyOver.As<DependencyObject>()
					?.EnumerateSelfAndVisualAncestors().OfType<Canvas>().FirstOrDefault()
					?? Mouse.DirectlyOver.As<object>();

				var pressedKeys = navigationKeys.Where(Keyboard.IsKeyDown).ToList();
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

				var shift = lowerDirection == upperDirection;
				var delta = shift
					? lowerDirection
					: lowerDirection < upperDirection ? -1 : +1;
				var offset = delta * ActualWidth / 32d;

				var flip = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
				var bandwidth = control.Is(FrameCanvas)
					? flip ? musicalPresenter.Frame.Level : musicalPresenter.Frame.Offset
					: flip ? musicalPresenter.Spectrum.Magnitude : musicalPresenter.Spectrum.Frequency;

				var stepsCount = 16;
				for (var step = 0; step < stepsCount; step++)
				{
					bandwidth.Transform(ActualWidth, offset / stepsCount, shift);
					await Task.Delay(8);
				}
			};

			var timer = new DispatcherTimer();
			timer.Tick += (o, e) =>
			{
				var spectrum = processingManager.Spectrum;
				var spectrumInterpolated = processingManager.SpectrumBetter;
				var activeProcessingProfile = processingManager.ActiveProfile;

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
					}
				}).OfType<Polyline>().ForEach(p => p.Points.Clear());
				FrameCanvas.Children.OfType<Polyline>().ForEach(p => p.Points.Clear());

				if (spectrum.IsNot()) return;
				if (activeProcessingProfile.IsNot()) return;

				static bool IsVisible(UIElement element) => element.Visibility.IsNot(Visibility.Collapsed);

				var width = FrameCanvas.ActualWidth;
				var height = FrameCanvas.ActualHeight;
				if (height > 0 && width > 0)
				{
					if (IsVisible(Polyline_Frame_Direct))
						musicalPresenter.DrawFrame(processingManager.OuterFrame, width, height).
						Use(Polyline_Frame_Direct.Points.AddRange);

					if (IsVisible(Polyline_Frame_Window))
						musicalPresenter.DrawFrame(processingManager.InnerFrame, width, height).
						Use(Polyline_Frame_Window.Points.AddRange);
				}

				width = PhaseCanvas.ActualWidth;
				height = PhaseCanvas.ActualHeight;
				if (height > 0 && width > 0)
				{
					if (IsVisible(Polyline_Phase_FFT))
						musicalPresenter.DrawPhase(spectrum, PhaseCanvas.ActualWidth, PhaseCanvas.ActualHeight).
						Use(Polyline_Phase_FFT.Points.AddRange);

					if (IsVisible(Polyline_Phase_PMI))
						musicalPresenter.DrawPhase(spectrumInterpolated, PhaseCanvas.ActualWidth, PhaseCanvas.ActualHeight).
						Use(Polyline_Phase_PMI.Points.AddRange);
				}

				width = MagnitudeCanvas.ActualWidth;
				height = MagnitudeCanvas.ActualHeight;
				if (height > 0 && width > 0)
				{
					if (IsVisible(Polyline_Magnitude_FFT))
						musicalPresenter.DrawMagnitude(spectrum, width, height).
						Use(Polyline_Magnitude_FFT.Points.AddRange);

					if (IsVisible(Polyline_Magnitude_PMI))
						musicalPresenter.DrawMagnitude(spectrumInterpolated, width, height).
						Use(Polyline_Magnitude_PMI.Points.AddRange);

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

				var discreteStep = activeProcessingProfile.SampleRate / activeProcessingProfile.FrameSize;

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

				var peaks = processingManager.Peaks;
				var pianoKeys = musicalPresenter.DrawPiano(PianoCanvas.Children, spectrumInterpolated, PianoCanvas.ActualWidth, PianoCanvas.ActualHeight, peaks);

				var activeProfile = processingManager.ActiveProfile;
				var geometryFill = MusicalPresenter.DrawGeometry(processingManager.Peaks, activeProfile.FrameSize, activeProfile.SampleRate,
					musicalPresenter.Geometry.SpiralApproximationLevel, 1d / activeProfile.FrameSize, Pi.Half);
				var geometryStroke = MusicalPresenter.DrawGeometry(processingManager.Peaks, activeProfile.FrameSize, activeProfile.SampleRate,
					musicalPresenter.Geometry.FlowerApproximationLevel, 0d, Pi.Half);

				var centerX = FrameCanvas.ActualHeight / 2d;
				var centerY = FrameCanvas.ActualHeight / 2d;
				var radius = Math.Max(Math.Min(centerX, centerY), 1d);

				FlowerStrokeCanvas.Width = FrameCanvas.ActualHeight;
				SpiralStrokeCanvas.Width = FrameCanvas.ActualHeight;

				SpiralStrokePolyline.Points.Clear();
				FlowerStrokePolyline.Points.Clear();
				geometryFill.ForEach(p => SpiralStrokePolyline.Points.Add(new(centerY - p.X * radius, centerY - p.Y * radius)));
				geometryStroke.ForEach(p => FlowerStrokePolyline.Points.Add(new(centerY - p.X * radius, centerY - p.Y * radius)));


				if (pianoKeys.Is() && pianoKeys.Any())
				{
					var maxMagnitude = pianoKeys.Max(k => k.Magnitude);
					var minOpacity = 0.632d;
					var topOpacity = 1 - minOpacity;
					pianoKeys.ForEach(k => k.RelativeOpacity = minOpacity + topOpacity * k.Magnitude / maxMagnitude);
				}

				var vC = resources["Visibility.Peak"];
				if (vC.Is(Visibility.Visible) && musicalPresenter.VisualProfile.PeakProfiles.Any(p => p.Value.IsVisible))
					musicalPresenter.DrawPeakTitles(pianoKeys, width, height)
					.ForEach(p =>
					{
						MagnitudeCanvas.Children.Add(p);
						p.UpdateLayout();
						p.Margin = new(p.Margin.Left - p.ActualWidth / 2d, p.Margin.Top - p.ActualHeight / 8d, 0d, 0d);
					});

				Draw(peaks, activeProfile.SampleSize, activeProfile.SampleRate);

				musicalPresenter.DrawMarkers(PhaseCanvas.Children, PhaseCanvas.ActualWidth, PhaseCanvas.ActualHeight,
					AppPalette.GetBrush("PhasePeakBrush"), default, pianoKeys.Select(k => k.Harmonic.Frequency), zIndexA);

				musicalPresenter.DrawMarkers(MagnitudeCanvas.Children, width, height,
					AppPalette.GetBrush("MagnitudePeakBrush"), default, pianoKeys.Select(k => k.Harmonic.Frequency), zIndexA);

				if (App.Current.MainWindow.IsNot())
					return;

				appViewModel.Harmonics.Value = pianoKeys;

				var w = SpectrogramCanvas.ActualWidth;
				var h = SpectrogramCanvas.ActualHeight;

				var actualBand = musicalPresenter.Spectrum.Frequency;
				var transformer = MusicalPresenter.GetScaleTransformer(actualBand, w);
				var magnitudeProjection = musicalPresenter.Spectrum.Magnitude.VisualScaleFunc;

				var count = 127;
				if (SpectrogramCanvas.Children.Count > count)
					SpectrogramCanvas.Children.RemoveAt(count);

				if (processingManager.IsPaused.Not())
				{
					SpectrogramCanvas.Children.Insert(0, DrawKeys(new Grid
					{
						Tag = spectrumInterpolated,
						DataContext = pianoKeys,
						Background = GetSpectrogramLineBrush(spectrumInterpolated, transformer, w, magnitudeProjection),
					}));
				}

				Grid DrawKeys(Grid grid)
				{
					var pressedHalfToneKeyColor = (Color)App.Current.Resources["PressedHalfToneKeyColor"];
					var pressedFullToneKeyColor = (Color)App.Current.Resources["PressedFullToneKeyColor"];

					grid.Children.Clear();
					grid.DataContext.To<IList<Models.PianoKey>>().Select(k => new Rectangle
					{
						Tag = (MusicalOptions.Tones[k.NoteNumber] ? pressedFullToneKeyColor : pressedHalfToneKeyColor).To(out var color),
						Fill = new SolidColorBrush(color.Mix(Channel.A, magnitudeProjection(k.Magnitude))),
						Width = transformer.GetVisualOffset(k.UpperFrequency) - transformer.GetVisualOffset(k.LowerFrequency),
						Margin = new(transformer.GetVisualOffset(k.LowerFrequency), 0, 0, 0),
						HorizontalAlignment = HorizontalAlignment.Left,
					})
					.ForEach(grid.Children.Add);

					grid.DataContext.To<IList<Models.PianoKey>>().Select(k => new Rectangle
					{
						Tag = (MusicalOptions.Tones[k.NoteNumber] ? pressedFullToneKeyColor : pressedHalfToneKeyColor).To(out var color),
						Fill = new SolidColorBrush(Palettes.Converters.GetOffsetColor(k, magnitudeProjection)),
						Width = (0.2d * (transformer.GetVisualOffset(k.UpperFrequency) - transformer.GetVisualOffset(k.LowerFrequency))).To(out var w),
						Margin = new(transformer.GetVisualOffset(k.Harmonic.Frequency) - w / 2d, 0, 0, 0),
						HorizontalAlignment = HorizontalAlignment.Left,
					})
					.ForEach(grid.Children.Add);

					return grid;
				}

				var hh = SpectrogramFrame.ActualHeight / count;
				SpectrogramCanvas.Children.OfType<Grid>().ForEach(r => r.Height = hh);

				void FullSpectrogramRefresh() => SpectrogramCanvas.Children
					.OfType<Grid>()
					.ForEach(g => DrawKeys(g).Background = GetSpectrogramLineBrush((IList<Bin>)g.Tag, transformer, w, magnitudeProjection));

				async void RequestFullSpectrogramRefresh(int delay)
				{
					_requestsCount++;
					await Task.Delay(delay);
					_requestsCount--;
					if (_requestsCount > 0)
						return;

					FullSpectrogramRefresh();
				}

				var window = actualBand.Window;
				if (IsStateChanged(actualBand.VisualScaleFunc, window).Not())
					return;

				KeepState(actualBand.VisualScaleFunc, window);
				RequestFullSpectrogramRefresh(500);
			};

			timer.Start();
		}

		int _requestsCount = 0;
		Projection _previousScaleFunc;
		double _previousWindowFrom, _previousWindowTill;

		IList<Bin> _peaks;
		int _sampleSize;
		double _sampleRate;

		void Draw(IList<Bin> peaks, int sampleSize, double sampleRate)
		{
			if (processingManager.IsPaused is false)
			{
				_peaks = peaks;
				_sampleSize = sampleSize;
				_sampleRate = sampleRate;
			}

			var approximation = AppView.TapeView.TapeViewModel.Approximation;
			var geometry = MusicalPresenter.DrawGeometry(_peaks, _sampleSize, _sampleRate, approximation).ToList();

			if (AppView.FlowerView.To(out var flowerView).IsVisible) flowerView.Draw(geometry);
			if (AppView.TapeView.To(out var tapeView).IsVisible) tapeView.Draw(geometry);

			Peaks = peaks;
		}

		static IList<Bin> Peaks;
		public static void SaveActiveFrame()
		{
			var sample = Peaks.Aggregate("", (s, b) => s += $"{b}|");

			var dialog = new SaveFileDialog
			{
				Filter = "Frame files (*.frame.txt)|*.frame.txt|All files (*.*)|*.*"
			};

			if (dialog.ShowDialog() is false)
				return;

			try
			{
				System.IO.File.WriteAllText(dialog.FileName, sample);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString());
			}
		}

		public static void LoadActiveFrame()
		{
			var dialog = new OpenFileDialog
			{
				Filter = "Frame files (*.frame.txt)|*.frame.txt|All files (*.*)|*.*"
			};

			if (dialog.ShowDialog() is false)
				return;

			try
			{
				var text = System.IO.File.ReadAllText(dialog.FileName);
				var peaks = text.SplitByChars("|\n").Select(l => l.SplitByChars(" ")).Select(l => new Bin
				{
					Magnitude = double.Parse(l[0]),
					Frequency = double.Parse(l[1]),
					Phase = double.Parse(l[2]),
				});

				var harmonicManager = Store.Get<HarmonicManager>();
				harmonicManager.Profiles.Add(new Models.Harmonic.Profile
				{
					Title = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName),
					Harmonics = new(peaks.Select(b => new Models.Harmonic
					{
						Magnitude = b.Magnitude,
						Frequency = b.Frequency,
						Phase = b.Phase,
						IsEnabled = true
					}))
				});
				harmonicManager.ActiveProfile = harmonicManager.Profiles.Last();

				var processingManager = Store.Get<ProcessingManager>();
				processingManager.ActiveProfile = processingManager.Profiles[0];
				processingManager.IsPaused = false;
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString());
			}
		}

		bool _sizeChanged;

		bool IsStateChanged(Projection actualScaleFunc, SmartRange window) =>
			_sizeChanged ||
			window.From.IsNot(_previousWindowFrom) ||
			window.Till.IsNot(_previousWindowTill) ||
			actualScaleFunc.IsNot(_previousScaleFunc);

		void KeepState(Projection actualScaleFunc, SmartRange window)
		{
			_sizeChanged = false;
			_previousWindowFrom = window.From;
			_previousWindowTill = window.Till;
			_previousScaleFunc = actualScaleFunc;
		}

		static LinearGradientBrush GetSpectrogramLineBrush(IList<Bin> bins, ScaleTransformer transformer, double width, Projection magnitudeProjection)
		{
			var from = transformer.GetLogicalOffset(0);
			var till = transformer.GetLogicalOffset(width);

			var color = (Color)App.Current.Resources["ColorD"];
			var stops = bins.Where(p => from <= p.Frequency && p.Frequency <= till)
				.Select(p => new GradientStop(color.Mix(Channel.A, magnitudeProjection(p.Magnitude)), transformer.GetVisualOffset(p.Frequency) / width));
			return new(new(stops));
		}
	}
}
