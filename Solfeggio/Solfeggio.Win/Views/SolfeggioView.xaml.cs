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

				PianoCanvas.Children.Clear();
				SpectrumCanvas.Children.Clear();

				var max = spectrum.Values.Max() * 0.7;

				presenter.DrawSpectrum(SpectrumCanvas, spectrum, MagnitudePolyline);
				var waveCorrectionMargin = presenter.UseHorizontalLogScale ? SpectrumCanvas.Margin : new Thickness();
				presenter.DrawWave(SpectrumCanvas, spectralViewModel.WaveInData, WaveInPolyline, waveCorrectionMargin);
				presenter.DrawWave(SpectrumCanvas, spectralViewModel.WaveOutData, WaveOutPolyline, waveCorrectionMargin);
				var tops = presenter.DrawPiano(PianoCanvas, spectrum);
				presenter.DrawTops(SpectrumCanvas, tops);
			};

			timer.Start();
		}
	}
}
