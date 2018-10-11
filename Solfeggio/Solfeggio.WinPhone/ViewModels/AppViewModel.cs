using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using Aero;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Pitch;
using Rainbow;
using Solfeggio.Assistents;

namespace Solfeggio.ViewModels
{
    public class AppViewModel : ContextObject
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

        public int FramePow
        {
            get { return Get(() => FramePow, 11); }
            set { Set(() => FramePow, value); }
        }

        public int ShiftsPerFrame
        {
            get { return 16; }
        }

        public Canvas SpectrumCanvas { get; set; }

        public Polyline SpectrumPolyline { get; set; }

        public Canvas PianoCanvas { get; set; }

        public bool IsPaused
        {
            get { return Get(() => IsPaused); }
            set { Set(() => IsPaused, value); }
        }

        public bool UseAliasing
        {
            get { return Get(() => UseAliasing, true); }
            set { Set(() => UseAliasing, value); }
        }

        public AppViewModel()
        {
            Initialize();
        }

        [OnDeserializing]
        public new void Initialize(StreamingContext context = default(StreamingContext))
        {
            Complex[] frame0 = null;
            Complex[] frame1 = null;
            byte[] sampleBytes = null;

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
                sampleBytes = new byte[2*BufferSize];
                RaisePropertyChanged(() => BufferSize);
                RaisePropertyChanged(() => FrameSize);
                RaisePropertyChanged(() => BufferDuration);
                RaisePropertyChanged(() => FrameDuration);
                FrameworkDispatcher.Update();
                Device.Start();
            };

            RaisePropertyChanged(() => FramePow);

            var pitch = new PitchTracker {SampleRate = Device.SampleRate};
            pitch.PitchDetected += (sender, record) =>
            {
                var r = pitch.CurrentPitchRecord;
                r = r;
            };
            var spectrum = new Dictionary<double, double>();
            Device.BufferReady += async (sender, args) =>
            {
                try
                {
                    if (IsPaused) return;
                    await TaskEx.Run(() =>
                    {
                        var frameSize = FrameSize;
                        var sampleSizeInBytes = Device.GetData(sampleBytes);
                        var sampleSize = sampleSizeInBytes/2;
                        if (sampleSize < frameSize*(1d + 1d/ShiftsPerFrame)) return;
                        var sample = new double[sampleSize];
                        for (var i = 0; i < sampleSize; i++)
                        {
                            sample[i] = Convert.ToDouble(BitConverter.ToInt16(sampleBytes, 2*i));
                        }

                        pitch.ProcessBuffer(sample.Select(t=>(float)t).ToArray());

                        var offset = frameSize/ShiftsPerFrame;
                        for (var i = 0; i < frameSize; i++)
                        {
                            var bin0 = sample[i];
                            var bin1 = sample[i + offset];
                            bin0 *= Window.Gausse(i, frameSize);
                            bin1 *= Window.Gausse(i, frameSize);
                            frame0[i] = bin0;
                            frame1[i] = bin1;
                        }

                        var spectrum0 = Butterfly.DecimationInTime(frame0, true);
                        var spectrum1 = Butterfly.DecimationInTime(frame1, true);
                        for (var i = 0; i < frameSize; i++)
                        {
                            spectrum0[i] /= frameSize;
                            spectrum1[i] /= frameSize;
                        }

                        spectrum = Filters.GetJoinedSpectrum(spectrum0, spectrum1, ShiftsPerFrame, Device.SampleRate);
                        if (UseAliasing) spectrum = Filters.Antialiasing(spectrum);
                    });

                    presenter.DrawSpectrum(SpectrumCanvas, spectrum, SpectrumPolyline);
                    var tops = presenter.DrawPiano(PianoCanvas, spectrum);
                    presenter.DrawTops(SpectrumCanvas, tops);
                }
                catch
                {
                    FrameworkDispatcher.Update();
                }
            };
        }
    }
}