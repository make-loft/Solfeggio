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

			var spectralViewModel = Store.Get<SpectralViewModel>();
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

				//var max = spectrum.Values.Max() * 0.7;

				var width = SpectrumCanvas.ActualWidth;
				var height = SpectrumCanvas.ActualHeight;
				presenter.DrawSpectrum(MagnitudePolyline.Points, spectrum, width, height);
				presenter.DrawWave(WaveInPolyline.Points, spectralViewModel.WaveInData, width, height);
				presenter.DrawWave(WaveOutPolyline.Points, spectralViewModel.WaveOutData, width, height);
				var tops = presenter.DrawPiano(PianoCanvas, spectrum);
				presenter.DrawTops(SpectrumCanvas, tops);
			};

			timer.Start();
		}
	}
}
