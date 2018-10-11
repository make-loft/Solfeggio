using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ace;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Solfeggio.Presenters;

namespace Solfeggio.ViewModels
{
    [DataContract]
    public class SpectralViewModel : ContextObject, IExposable
    {
        public Microphone Device { get; private set; }

        public TimeSpan BufferDuration
        {
            get { return Device.BufferDuration; }
        }

        public TimeSpan FrameDuration
        {
            get { return TimeSpan.FromSeconds((double) FrameSize/Device.SampleRate); }
        }

        public int BufferSize { get; private set; }

        public int FrameSize { get; private set; }

        [DataMember]
        public int FramePow
        {
            get { return Get(() => FramePow, 12); }
            set { Set(() => FramePow, value); }
        }

        public int ShiftsPerFrame
        {
            get { return 16; }
        }

        public Canvas WaveCanvas { get; set; }

        public Canvas SpectrumCanvas { get; set; }

        public Polyline SpectrumPolyline { get; set; }

        public Canvas PianoCanvas { get; set; }

        public Polyline WaveInLine { get; set; }

        public Polyline WaveOutLine { get; set; }

        public bool IsPaused
        {
            get { return Get(() => IsPaused); }
            set { Set(() => IsPaused, value); }
        }

        [DataMember]
        public bool UseAliasing
        {
            get { return Get(() => UseAliasing, true); }
            set { Set(() => UseAliasing, value); }
        }

        public Func<double, double, double> Window
        {
            get { return Get(() => Window) ?? Windowing.Gausse; }
            set { Set(() => Window, value); }
        }

        public List<Func<double, double, double>> Windows { get; set; }

        public void Expose()
        {
            Windows = new List<Func<double, double, double>>
            {
                Windowing.Rectangle,
                Windowing.Gausse,
                Windowing.BlackmanHarris,
                Windowing.Hamming,
                Windowing.Hann
            };

            var sample = new double[0];
            Complex[] frame0 = null;
            Complex[] frame1 = null;
            byte[] sampleBytes = null;

            var waveInData = new Dictionary<double, double>();
            var waveOutData = new Dictionary<double, double>();

            Device = Microphone.Default;
            var presenter = Store.Get<Presenter>();
            presenter.TopFrequency = Device.SampleRate;
            presenter.LimitFrequency = Device.SampleRate/2.0;

            var timer = new DispatcherTimer();
            timer.Tick += (sender, args) => FrameworkDispatcher.Update();
            timer.Start();

            this[() => FramePow].PropertyChanged += (sender, args) =>
            {
                Device.Stop();
                FrameSize = (int) Math.Pow(2, FramePow);
                frame0 = new Complex[FrameSize];
                frame1 = new Complex[FrameSize];
                var miliseconds = FrameDuration.TotalMilliseconds*(1d + 1d/ShiftsPerFrame);
                miliseconds = Math.Ceiling(miliseconds/100)*100;
                Device.BufferDuration = TimeSpan.FromMilliseconds(miliseconds);
                BufferSize = Device.GetSampleSizeInBytes(Device.BufferDuration)/2;
                sampleBytes = new byte[4*BufferSize];
                RaisePropertyChanged(() => BufferSize);
                RaisePropertyChanged(() => FrameSize);
                RaisePropertyChanged(() => BufferDuration);
                RaisePropertyChanged(() => FrameDuration);
                FrameworkDispatcher.Update();
                Device.Start();
            };

            RaisePropertyChanged(() => FramePow);

            //var pitch = new PitchTracker {SampleRate = Device.SampleRate};
            //pitch.PitchDetected += (sender, record) =>
            //{
            //    var r = pitch.CurrentPitchRecord;
            //    r = r;
            //};

            var spectrum = new Dictionary<double, double>();
            Device.BufferReady += async (sender, args) =>
            {
                try
                {
                    await Task.Run(() =>
                    {
                        var frameSize = FrameSize;

                        if (!IsPaused)
                        {
                            var sampleSizeInBytes = Device.GetData(sampleBytes);
                            var sampleSize = sampleSizeInBytes/2;
                            if (sampleSize < frameSize*(1d + 1d/ShiftsPerFrame)) return;
                            sample = new double[sampleSize];
                            for (var i = 0; i < sampleSize; i++)
                            {
                                sample[i] = Convert.ToDouble(BitConverter.ToInt16(sampleBytes, 2*i));
                            }
                        }

                        //pitch.ProcessBuffer(sample.Select(t=>(float)t).ToArray());

                        var tmp = new double[frameSize];
                        var offset = frameSize/ShiftsPerFrame;
                        for (var i = 0; i < frameSize; i++)
                        {
                            tmp[i] = sample[i];
                            var bin0 = sample[i];
                            var bin1 = sample[i + offset];
                            bin0 *= Window(i, frameSize);
                            bin1 *= Window(i, frameSize);
                            frame0[i] = bin0;
                            frame1[i] = bin1;
                        }

                        var spectrum0 = frame0.DecimationInTime(true);
                        var spectrum1 = frame1.DecimationInTime(true);
                        for (var i = 0; i < frameSize; i++)
                        {
                            spectrum0[i] /= frameSize;
                            spectrum1[i] /= frameSize;
                        }

                        var x = 0d;
                        waveInData = tmp.ToDictionary(c => x++, c => c);

                        var y = 0d;
                        var outSample = spectrum0.DecimationInTime(false);
                        waveOutData = outSample.ToDictionary(c => y++, c => c.Real);

                        spectrum = Filtering.GetJoinedSpectrum(spectrum0, spectrum1, ShiftsPerFrame, Device.SampleRate);
                        if (UseAliasing) spectrum = Filtering.Antialiasing(spectrum);
                    });

                    Redraw(presenter, spectrum, waveInData, waveOutData);
                }
                catch
                {
                    FrameworkDispatcher.Update();
                }
            };
        }

        private void Redraw(
            Presenter presenter, 
            Dictionary<double, double> spectrum, 
            Dictionary<double, double> waveInData,
            Dictionary<double, double> waveOutData)
        {
            SpectrumCanvas.Children.Clear();
            presenter.DrawSpectrum(SpectrumCanvas, spectrum, SpectrumPolyline);
            var waveCorrectionMargin = presenter.UseHorizontalLogScale ? WaveCanvas.Margin : new Thickness();
            presenter.DrawWave(WaveCanvas, waveInData, WaveInLine, waveCorrectionMargin);
            presenter.DrawWave(WaveCanvas, waveOutData, WaveOutLine, waveCorrectionMargin);
            var tops = presenter.DrawPiano(PianoCanvas, spectrum);
            presenter.DrawTops(SpectrumCanvas, tops);
        }
    }
}