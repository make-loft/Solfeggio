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
			var spectralViewModel = Store.Get<SpectralViewModel>();
			var generator = spectralViewModel.Devices[1].To<Generator>();
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
				

				var step = spectralViewModel.SampleRate / spectralViewModel.FrameSize;

				if (presenter.Spectrum.Frequency.IsVisible)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.ButterflyGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateGrid(step));

				//if (presenter.Spectrum.Frequency.IsVisible)
				//	presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
				//		AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
				//		generator.Frequency.ToEnumerable(), 0.92d);

				if (presenter.Show.NotesGrid)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateNotes());

				if (presenter.Spectrum.Phase.IsVisible)
					presenter.DrawPhase(spectrum, width, height).
					Use(PhasePolyline.Points.AppendRange);

				if (presenter.Frame.Level.IsVisible)
					presenter.DrawFrame(spectralViewModel.OuterFrame, width, height).
					Use(WaveInPolyline.Points.AppendRange);

				if (presenter.Show.Wave && spectralViewModel.ActiveWindow.IsNot(Rainbow.Windowing.Rectangle))
					presenter.DrawFrame(spectralViewModel.InnerFrame, width, height).
					Use(WaveOutPolyline.Points.AppendRange);

				var dominanats = presenter.DrawPiano(PianoCanvas.Children, spectrum, PianoCanvas.ActualWidth, PianoCanvas.ActualHeight);
				if (presenter.Show.ActualFrequncy || presenter.Show.ActualMagnitude || presenter.Show.EthalonFrequncy || presenter.Show.Notes)
					presenter.DrawTops(dominanats, width, height,
						presenter.Show.ActualFrequncy,
						presenter.Show.ActualMagnitude,
						presenter.Show.EthalonFrequncy,
						presenter.Show.Notes).
						ForEach(p =>
						{
							SpectrumCanvas.Children.Add(p);
							p.UpdateLayout();
							p.Margin = new Thickness(p.Margin.Left - p.ActualWidth / 2d, p.Margin.Top - p.ActualHeight / 2d, 0d, 0d);
						});

				appViewModel.Dominants = dominanats.OrderBy(k => Math.Abs(k.DeltaFrequency)).ToArray();
			};

			timer.Start();
		}
	}
}
