using System;
using Ace;
using Rainbow;

namespace Solfeggio.ViewModels
{
    public class CoreViewModel : ContextObject, IExposable
    {
		public Complex[] CurrentSpectrum { get; set; }

	    public IMicrophone Microphone { get; } = Store.Get<IMicrophone>();

	    public Func<double, double, double> Window
		{
			get => Get(() => Window) ?? Windowing.Gausse;
			set => Set(() => Window, value);
	    }

	    public SmartSet<Func<double, double, double>> Windows { get; set; }

		public void Expose()
	    {
			Microphone.DataReady += (sender, args) =>
			{
				var buffer = args.Buffer;		
				var frame = new Complex[4096];
				for (var i = 0; i < frame.Length; i++)
				{
					frame[i] = buffer[i];
				}

				var spectrum = frame.DecimationInTime(true);
				CurrentSpectrum = spectrum;
			};

			Microphone.Start();
		}
    }
}
