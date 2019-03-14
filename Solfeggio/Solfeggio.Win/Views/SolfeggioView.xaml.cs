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

			var spectralViewModel = Store.Get<SpectralViewModel>();
			var generator = spectralViewModel.Devices[1].To<Generator>();
			var presenter = Store.Get<MusicalPresenter>();
			var timer = new DispatcherTimer();
			timer.Tick += (o, e) =>
			{
				var spectrum = spectralViewModel.CurrentSpectrum;
				if (spectrum.IsNot()) return;

				MagnitudePolyline.Points.Clear();
				WaveOutPolyline.Points.Clear();
				WaveInPolyline.Points.Clear();

				PianoCanvas.Children.Clear();
				SpectrumCanvas.Children.Clear();

				SpectrumCanvas.Children.Add(MagnitudePolyline);
				SpectrumCanvas.Children.Add(WaveInPolyline);
				SpectrumCanvas.Children.Add(WaveOutPolyline);

				var width = SpectrumCanvas.ActualWidth;
				var height = SpectrumCanvas.ActualHeight;

				if (presenter.Show.Spectrum)
					presenter.DrawSpectrum(MagnitudePolyline.Points, spectrum, width, height);

				var step = spectralViewModel.SampleRate / spectralViewModel.FrameSize;

				if (presenter.Show.DiscreteFourierGrid)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.ButterflyGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateGrid(step));

				if (presenter.Show.DiscreteFourierGrid)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
						generator.Frequancy.ToEnumerable(), 0.92d);

				if (presenter.Show.NotesGrid)
					presenter.DrawMarkers(SpectrumCanvas.Children, width, height,
						AppPalette.NoteGridBrush, AppPalette.NoteGridBrush,
						presenter.EnumerateNotes());

				if (presenter.Show.Wave)
					presenter.DrawWave(WaveInPolyline.Points, spectralViewModel.WaveInData, width, height);

				if (presenter.Show.Wave && spectralViewModel.ActiveWindow.IsNot(Rainbow.Windowing.Rectangle))
					presenter.DrawWave(WaveOutPolyline.Points, spectralViewModel.WaveOutData, width, height);

				var tops = presenter.DrawPiano(PianoCanvas.Children, spectrum, PianoCanvas.ActualWidth, PianoCanvas.ActualHeight);
				presenter.DrawTops(SpectrumCanvas.Children, tops, width, height,
					presenter.Show.ActualFrequncy, presenter.Show.EthalonFrequncy, presenter.Show.Notes);
			};

			timer.Start();
		}
	}
}
