using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Ace;
using Solfeggio.Presenters;
using Solfeggio.ViewModels;

namespace Solfeggio.Views
{
	public partial class SolfeggioView
	{
		public SolfeggioView()
		{
			InitializeComponent();

			var appViewModel = Store.Get<AppViewModel>();
			var spectralViewModel = Store.Get<ProcessingManager>();
			var presenter = Store.Get<MusicalPresenter>();
			var timer = new DispatcherTimer();
			timer.Tick += (o, e) =>
			{
				var spectrum = spectralViewModel.Spectrum;
				if (spectrum.IsNot()) return;

				MagnitudePolyline.Points.Clear();
				PhasePolyline.Points.Clear();
				WaveOutPolyline.Points.Clear();
				WaveInPolyline.Points.Clear();

				PianoCanvas.Children.Clear();
				SpectrumCanvas.Children.Clear();

				SpectrumCanvas.Children.Add(MagnitudePolyline);
				SpectrumCanvas.Children.Add(PhasePolyline);
				SpectrumCanvas.Children.Add(WaveInPolyline);
				SpectrumCanvas.Children.Add(WaveOutPolyline);

				var width = SpectrumCanvas.ActualWidth;
				var height = SpectrumCanvas.ActualHeight;

				if (presenter.Spectrum.Magnitude.IsVisible)
					presenter.DrawMagnitude(spectrum, width, height).
					Use(MagnitudePolyline.Points.AppendRange);
				

				var step = spectralViewModel.ActiveProfile.SampleRate / spectralViewModel.ActiveProfile.FrameSize;

				if (presenter.Spectrum.Frequency.IsVisible)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.ButterflyGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateGrid(step));

				//if (presenter.Spectrum.Frequency.IsVisible)
				//	presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
				//		AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
				//		generator.Frequency.ToEnumerable(), 0.92d);

				if (presenter.VisualProfile.NotesGrid)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateNotes());

				if (presenter.Spectrum.Phase.IsVisible)
					presenter.DrawPhase(spectrum, width, height).
					Use(PhasePolyline.Points.AppendRange);

				if (presenter.Frame.Level.IsVisible)
					presenter.DrawFrame(spectralViewModel.OuterFrame, width, height).
					Use(WaveInPolyline.Points.AppendRange);

				if (presenter.Frame.Level.IsVisible && spectralViewModel.ActiveProfile.ActiveWindow.IsNot(Rainbow.Windowing.Rectangle))
					presenter.DrawFrame(spectralViewModel.InnerFrame, width, height).
					Use(WaveOutPolyline.Points.AppendRange);

				var dominanats = presenter.DrawPiano(PianoCanvas.Children, spectrum, PianoCanvas.ActualWidth, PianoCanvas.ActualHeight);
				if (presenter.VisualProfile.TopProfiles.Any(p => p.Value.IsVisible))
					presenter.DrawTops(dominanats, width, height).
						ForEach(p =>
						{
							SpectrumCanvas.Children.Add(p);
							p.UpdateLayout();
							p.Margin = new Thickness(p.Margin.Left - p.ActualWidth / 2d, p.Margin.Top - p.ActualHeight / 2d, 0d, 0d);
						});

				appViewModel.Harmonics = dominanats.OrderBy(k => Math.Abs(k.DeltaFrequency)).ToArray();
			};

			timer.Start();
		}
	}
}
