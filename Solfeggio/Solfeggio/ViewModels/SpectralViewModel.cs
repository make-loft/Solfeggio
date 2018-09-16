using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;
using Ace;
using Rainbow;
using Solfeggio.Presenters;
using Xamarin.Forms;

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
            get => Get(() => FramePow, 12);
	        set => Set(() => FramePow, value);
        }

	    [DataMember]
	    public bool ShowSettings
	    {
		    get => Get(() => ShowSettings);
		    set => Set(() => ShowSettings, value);
	    }

		public int MinFramePow { get; set; }

        public int FrameSize => (int) Math.Pow(2.0d, FramePow);

        public Dictionary<double, double> CurrentSpectrum { get; set; }

	    public Dictionary<double, double> WaveInData { get; set; }

	    public Dictionary<double, double> WaveOutData { get; set; }

		public double SampleRate
        {
            get => ActiveDevice.SampleRate;
            set
            {
                if (value <= 0) return;
                SampleRates.Remove(value);
                ActiveDevice.StartWith(value, FrameSize);
                SampleRates.Insert(0, value);
                RaisePropertyChanged(() => SampleRate);
            }
        }

        public SmartSet<double> SampleRates { get; private set; }

        public double MinSampleRate => ActiveDevice.SampleRates.Min();
        public double MaxSampleRate => ActiveDevice.SampleRates.Max();

		public bool UseAliasing { get; set; }
	    public double ShiftsPerFrame { get; set; } = 16;

	    public void Expose()
	    {
			this[() => FramePow].PropertyChanged += (sender, args) =>
			{
				ActiveDevice.StartWith(ActiveDevice.SampleRate, (int)(FrameSize * (1d + 1d / ShiftsPerFrame)));
			};

		    void OnActiveDeviceOnDataReady(object sender, AudioInputEventArgs args)
		    {
			    var frameSize = FrameSize;
				if (args.Frame.Length < frameSize) return;
			    var spectrum0 = args.Frame.DecimationInTime(true);
			    var spectrum1 = spectrum0;
			    

				for (var i = 0; i < frameSize; i++)
			    {
				    spectrum0[i] /= frameSize;
				    spectrum1[i] /= frameSize;
			    }

			    var x = 0d;
			    WaveInData = args.Frame.ToDictionary(c => x++, c => c.Real);

			    var y = 0d;
			    var outSample = spectrum0.DecimationInTime(false);
			    WaveOutData = outSample.ToDictionary(c => y++, c => c.Real);

			    var spectrum = Filtering.GetJoinedSpectrum(spectrum0, spectrum1, ShiftsPerFrame, SampleRate);
			    if (UseAliasing) spectrum = Filtering.Antialiasing(spectrum);

			    CurrentSpectrum = spectrum;
		    }

			this[() => ActiveDevice].PropertyChanging += (sender, args) =>
			{
				ActiveDevice.Stop();
				ActiveDevice.DataReady -= OnActiveDeviceOnDataReady;
			};

			this[() => ActiveDevice].PropertyChanged += (sender, args) =>
			{
			    SampleRates = ActiveDevice.SampleRates.ToSet();

                ActiveDevice.DataReady += OnActiveDeviceOnDataReady;
				ActiveDevice.StartWith();
			};

			RaisePropertyChanged(() => ActiveDevice);
		}
	}
}
