using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ace;
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

			Point d = default;
			MouseLeftButtonUp += (o, e) => d = default;
			MouseLeftButtonDown += (o, e) => d = e.GetPosition(this);
			SpectrumCanvas.MouseMove += MouseMove;
			PianoCanvas.MouseMove += MouseMove;
			FrameCanvas.MouseMove += MouseMove;

			void MouseMove(object o, MouseEventArgs e)
			{
				if (e.LeftButton.IsNot(MouseButtonState.Pressed))
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

				var bandwidth = o.Is(FrameCanvas)
					? musicalPresenter.Frame.Offset
					: musicalPresenter.Spectrum.Frequency;

				var factor = bandwidth.Threshold.Length / bandwidth.Limit.Length;
				Shift(bandwidth, delta * factor / (o.Is(FrameCanvas) ? 32 : 2), lowerDirection, upperDirection);
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

				Shift(musicalPresenter.Spectrum.Frequency, + 1, lowerDirection, upperDirection);
			};

			var timer = new DispatcherTimer();
			timer.Tick += (o, e) =>
			{
				var spectrum = processingManager.Spectrum;
				var spectrumInterpolated = processingManager.SpectrumBetter;
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
					}
				}).OfType<Polyline>().ForEach(p => p.Points.Clear());
				FrameCanvas.Children.OfType<Polyline>().ForEach(p => p.Points.Clear());

				static bool IsVisible(UIElement element) => element.Visibility.IsNot(Visibility.Collapsed);

				var width = FrameCanvas.ActualWidth;
				var height = FrameCanvas.ActualHeight;

				if (IsVisible(Polyline_Frame_Direct))
					musicalPresenter.DrawFrame(processingManager.OuterFrame, width, height).
					Use(Polyline_Frame_Direct.Points.AppendRange);

				if (IsVisible(Polyline_Frame_Window))
					musicalPresenter.DrawFrame(processingManager.InnerFrame, width, height).
					Use(Polyline_Frame_Window.Points.AppendRange);

				width = SpectrumCanvas.ActualWidth;
				height = SpectrumCanvas.ActualHeight;

				if (IsVisible(Polyline_Spectrum_Magnitude_FFT))
					musicalPresenter.DrawMagnitude(spectrum, width, height).
					Use(Polyline_Spectrum_Magnitude_FFT.Points.AppendRange);

				if (IsVisible(Polyline_Spectrum_Magnitude_PMI))
					musicalPresenter.DrawMagnitude(spectrumInterpolated, width, height).
					Use(Polyline_Spectrum_Magnitude_PMI.Points.AppendRange);

				if (IsVisible(Polyline_Spectrum_Phase_FFT))
					musicalPresenter.DrawPhase(spectrum, width, height).
					Use(Polyline_Spectrum_Phase_FFT.Points.AppendRange);

				if (IsVisible(Polyline_Spectrum_Phase_PMI))
					musicalPresenter.DrawPhase(spectrumInterpolated, width, height).
					Use(Polyline_Spectrum_Phase_PMI.Points.AppendRange);

				var discreteStep = processingManager.ActiveProfile.SampleRate / processingManager.ActiveProfile.FrameSize;

				var resources = App.Current.Resources;

				var vA = resources["Visibility.FrequencyDiscreteGrid"];
				if (vA.Is(Visibility.Visible))
					musicalPresenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.ButterflyGridBrush, AppPalette.NoteGridBrush,
						musicalPresenter.EnumerateGrid(discreteStep));

				var vB = resources["Visibility.FrequencyNotesGrid"];
				if (vB.Is(Visibility.Visible))
					musicalPresenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
						musicalPresenter.EnumerateNotes());

				var dominanats = musicalPresenter.DrawPiano(PianoCanvas.Children, spectrumInterpolated, PianoCanvas.ActualWidth, PianoCanvas.ActualHeight, out var peaks);
				if (musicalPresenter.VisualProfile.TopProfiles.Any(p => p.Value.IsVisible))
					musicalPresenter.DrawTops(dominanats, width, height)
					.ForEach(p =>
					{
						SpectrumCanvas.Children.Add(p);
						p.UpdateLayout();
						p.Margin = new(p.Margin.Left - p.ActualWidth / 2d, p.Margin.Top - p.ActualHeight / 8d, 0d, 0d);
					});

				appViewModel.Harmonics = dominanats;

				var w = SpectrumCanvas.ActualWidth;
				var h = SpectrumCanvas.ActualHeight;
				var stops = Polyline_Spectrum_Magnitude_PMI.Points
					.Select(p => new GradientStop(Color.FromArgb((byte)
					(512d * (h - p.Y)/ h), 255, 0, 0), p.X / w));
				var brush = new LinearGradientBrush(new(stops), 0d);

				var count = 255;
				if (SpectrogramCanvas.Children.Count > count)
					SpectrogramCanvas.Children.RemoveAt(count);
				SpectrogramCanvas.Children.Insert(0, new Rectangle { Fill = brush, Height = 4 });
				var hh = SpectrogramFrame.ActualHeight / count;
				SpectrogramCanvas.Children.Cast<Rectangle>().ForEach(c => c.Height = hh);
			};

			timer.Start();
		}
	}
}
