using System.Runtime.Serialization;
using Ace;
using Rainbow;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class SpectralViewModel : ContextObject, IExposable
    {
		public IAudioInputDevice AudioInputDevice { get; } = Store.Get<IAudioInputDevice>();

	    public delegate double ApodizationFunc(double binIndex, double frameSize);

		public ApodizationFunc ActiveWindow
		{
			get => Get(() => ActiveWindow, Windowing.Gausse);
			set => Set(() => ActiveWindow, value);
	    }

	    public ApodizationFunc[] Windows { get; set; } =
	    {
		    Windowing.BlackmanHarris,
			Windowing.DOGWavelet,
			Windowing.Gausse,
			Windowing.Hamming,
			Windowing.Hann,
		    Windowing.Rectangle,
	    };

	    [DataMember]
	    public int FramePow
	    {
		    get { return Get(() => FramePow, 12); }
		    set { Set(() => FramePow, value); }
	    }

		public Complex[] CurrentSpectrum { get; set; }

		public void Expose()
	    {
			AudioInputDevice.DataReady += (sender, args) =>
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

			AudioInputDevice.Start();
		}
    }
}
