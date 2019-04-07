using System.Linq;
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

				if (presenter.Magnitude.IsVisible)
					presenter.DrawSpectrum(MagnitudePolyline.Points, spectrum, width, height);

				var step = spectralViewModel.SampleRate / spectralViewModel.FrameSize;

				if (presenter.Frequency.IsVisible)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.ButterflyGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateGrid(step));

				if (presenter.Frequency.IsVisible)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
						generator.Frequency.ToEnumerable(), 0.92d);

				if (presenter.Show.NotesGrid)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateNotes());

				if (presenter.Phase.IsVisible)
					presenter.DrawPhase(PhasePolyline.Points, spectralViewModel.Spectrum, width, height);

				if (presenter.Wave.IsVisible)
					presenter.DrawWave(WaveInPolyline.Points, spectralViewModel.WaveInData, width, height);

				if (presenter.Show.Wave && spectralViewModel.ActiveWindow.IsNot(Rainbow.Windowing.Rectangle))
					presenter.DrawWave(WaveOutPolyline.Points, spectralViewModel.WaveOutData, width, height);

				var dominanats = presenter.DrawPiano(PianoCanvas.Children, spectrum, PianoCanvas.ActualWidth, PianoCanvas.ActualHeight);
				presenter.DrawTops(SpectrumCanvas.Children, dominanats, width, height,
					presenter.Show.ActualFrequncy,
					presenter.Show.ActualMagnitude,
					presenter.Show.EthalonFrequncy,
					presenter.Show.Notes);

				appViewModel.Dominants = dominanats.OrderByDescending(k => k.Magnitude).ThenBy(k => k.NoteName).ToArray();
			};

			timer.Start();
		}
	}
}
