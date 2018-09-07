using System;
using System.Linq;
using System.Runtime.Serialization;
using Ace;
using Rainbow;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class SpectralViewModel : ContextObject, IExposable
    {
		public IAudioInputDevice ActiveDevice
		{
			get => Get(() => ActiveDevice, Devices.FirstOrDefault());
			set => Set(() => ActiveDevice, value);
		}

		public IAudioInputDevice[] Devices { get; } = {Store.Get<IAudioInputDevice>()};

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

        public int MinFramePow { get; set; }

        public int FrameSize => (int) Math.Pow(2.0d, FramePow);

        public Complex[] CurrentSpectrum { get; set; }

        public double SampleRate
        {
            get => ActiveDevice.SampleRate;
            set
            {
                ActiveDevice.StartWith(value, FrameSize);
                RaisePropertyChanged(() => SampleRate);
            }
        }

        public double MinSampleRate => ActiveDevice.SampleRates.Min();
        public double MaxSampleRate => ActiveDevice.SampleRates.Max();

        public void Expose()
		{
			this[() => FramePow].PropertyChanged += (sender, args) =>
			{
				ActiveDevice.StartWith(ActiveDevice.SampleRate, FrameSize);
			};

		    void OnActiveDeviceOnDataReady(object sender, AudioInputEventArgs args)
		    {
			    if (args.Frame.Length < FrameSize) return;
			    var spectrum = args.Frame.DecimationInTime(true);
			    CurrentSpectrum = spectrum;
		    }

			this[() => ActiveDevice].PropertyChanging += (sender, args) =>
			{
				ActiveDevice.Stop();
				ActiveDevice.DataReady -= OnActiveDeviceOnDataReady;
			};

			this[() => ActiveDevice].PropertyChanged += (sender, args) =>
			{
				ActiveDevice.DataReady += OnActiveDeviceOnDataReady;
				ActiveDevice.StartWith();
			};

			//RaisePropertyChanged(() => FramePow);
			RaisePropertyChanged(() => ActiveDevice);
		}
    }
}
