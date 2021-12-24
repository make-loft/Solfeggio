using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ace;
using Solfeggio.Presenters;
using Solfeggio.ViewModels;

namespace Solfeggio.Views
{
	public struct Setup<T>
	{
		public Setup(T value) => Setters = Items = value;
		public T Setters { get; set; }
		public T Items { get; set; }

		//public static implicit operator T(Setup<T> d) => d.Setters;
	}

	public static class Test
	{
		public static T Setup<T>(this T o, Func<T, Setup<T>> block) => block(o).Setters;
	}

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
			var spectralViewModel = Store.Get<ProcessingManager>();
			var presenter = Store.Get<MusicalPresenter>();

			Point d = default;
			PreviewMouseLeftButtonUp += (o, e) => d = default;
			PreviewMouseLeftButtonDown += (o, e) => d = e.GetPosition(this);
			PreviewMouseMove += (o, e) =>
			{
				if (e.LeftButton.IsNot(MouseButtonState.Pressed) || e.OriginalSource is GridSplitter)
					return;

				var p = e.GetPosition(this);
				if (d.Is(default))
				{
					d = p;
					return;
				}

				var deltaX = d.X - p.X;
				var deltaY = d.Y - p.Y;
				var isHorizontalMove = deltaX * deltaX > deltaY * deltaY;
				var delta = isHorizontalMove ? deltaX : deltaY;

				d = p;

				var lowerDirection = isHorizontalMove ? +1 : -1;
				var upperDirection = +1;

				Shift(presenter.Spectrum.Frequency, delta / 16, lowerDirection, upperDirection);
			};

			PreviewMouseWheel += (o, e) =>
			{
				var lowerDirection = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? +1 : -1;
				var upperDirection = +1;

				Shift(presenter.Spectrum.Frequency, e.Delta / 16, lowerDirection, upperDirection);
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

				Shift(presenter.Spectrum.Frequency, + 1, lowerDirection, upperDirection);
			};

			var timer = new DispatcherTimer();
			timer.Tick += (o, e) =>
			{
				var spectrum = spectralViewModel.Spectrum;
				var spectrumInterpolated = spectralViewModel.SpectrumBetter;
				if (spectrum.IsNot()) return;

				PianoCanvas.Children.Clear();
				SpectrumCanvas.Children.Clear();
				SpectrumCanvas.Children.Setup(c => new(c)
				{
					Items =
					{
						Polyline_Spectrum_Magnitude_FFT,
						Polyline_Spectrum_Magnitude_PMI,
						Polyline_Spectrum_Phase_FFT,
						Polyline_Spectrum_Phase_PMI,
						Polyline_Frame_Direct,
						Polyline_Frame_Window,
					}
				}).OfType<Polyline>().ForEach(p => p.Points.Clear());

				var width = SpectrumCanvas.ActualWidth;
				var height = SpectrumCanvas.ActualHeight;

				static bool IsVisible(UIElement element) => element.Visibility.IsNot(Visibility.Collapsed);

				if (IsVisible(Polyline_Spectrum_Magnitude_FFT))
					presenter.DrawMagnitude(spectrum, width, height).
					Use(Polyline_Spectrum_Magnitude_FFT.Points.AppendRange);

				if (IsVisible(Polyline_Spectrum_Magnitude_PMI))
					presenter.DrawMagnitude(spectrumInterpolated, width, height).
					Use(Polyline_Spectrum_Magnitude_PMI.Points.AppendRange);

				if (IsVisible(Polyline_Spectrum_Phase_FFT))
					presenter.DrawPhase(spectrum, width, height).
					Use(Polyline_Spectrum_Phase_FFT.Points.AppendRange);

				if (IsVisible(Polyline_Spectrum_Phase_PMI))
					presenter.DrawPhase(spectrumInterpolated, width, height).
					Use(Polyline_Spectrum_Phase_PMI.Points.AppendRange);

				if (IsVisible(Polyline_Frame_Direct))
					presenter.DrawFrame(spectralViewModel.OuterFrame, width, height).
					Use(Polyline_Frame_Direct.Points.AppendRange);

				if (IsVisible(Polyline_Frame_Window))
					presenter.DrawFrame(spectralViewModel.InnerFrame, width, height).
					Use(Polyline_Frame_Window.Points.AppendRange);

				var discreteStep = spectralViewModel.ActiveProfile.SampleRate / spectralViewModel.ActiveProfile.FrameSize;

				var resources = App.Current.Resources;

				var vA = resources["Visibility.FrequencyDiscreteGrid"];
				if (vA.Is(Visibility.Visible))
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.ButterflyGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateGrid(discreteStep));

				var vB = resources["Visibility.FrequencyNotesGrid"];
				if (vB.Is(Visibility.Visible))
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateNotes());

				var dominanats = presenter.DrawPiano(PianoCanvas.Children, spectrumInterpolated, PianoCanvas.ActualWidth, PianoCanvas.ActualHeight, out var peaks);
				if (presenter.VisualProfile.TopProfiles.Any(p => p.Value.IsVisible))
					presenter.DrawTops(dominanats, width, height).
						ForEach(p =>
						{
							SpectrumCanvas.Children.Add(p);
							p.UpdateLayout();
							p.Margin = new Thickness(p.Margin.Left - p.ActualWidth / 2d, p.Margin.Top - p.ActualHeight / 2d, 0d, 0d);
						});

				appViewModel.Harmonics = peaks;
			};

			timer.Start();
		}
	}
}
