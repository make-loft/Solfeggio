using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Runtime;

using System;

using Solfeggio.Api;

namespace Solfeggio.Droid
{
	[Activity(
		Label = "Solfeggio", /* 🎶🎵 */
		Icon = "@drawable/logo",
		Theme = "@style/MainTheme",
		ScreenOrientation = ScreenOrientation.Landscape,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
		MainLauncher = false,
		NoHistory = true
		)]
	public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			AndroidEnvironment.UnhandledExceptionRaiser += (o, args) => ProcessUnhandledException(args.Exception);
			AppDomain.CurrentDomain.UnhandledException += (o, args) => ProcessUnhandledException(args.ExceptionObject as Exception);

			RequestedOrientation = ScreenOrientation.Landscape;
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			base.OnCreate(bundle);

			Wave.In.MicrophoneDeviceInfo.ProvideDevice += (waveFormat, sampleSize, buffersCount) =>
			{
				var device = Microphone.Default;
				device.SampleRate = waveFormat.SampleRate;
				device.DesiredFrameSize = sampleSize;
				return device;
			};

			Wave.Out.SpeakerDeviceInfo.ProvideDevice += (waveFormat, sampleSize, buffersCount, source) =>
			{
				var device = Speaker.Default;
				device.SampleRate = waveFormat.SampleRate;
				device.DesiredFrameSize = sampleSize;
				device.Source = source;
				return device;
			};

			Xamarin.Forms.Forms.Init(this, bundle);
			LoadApplication(new App());
		}

		async void ProcessUnhandledException(Exception exception)
		{
			//for (var e = exception; e.Is(); e = e.InnerException)
			//	YandexMetrica.ReportUnhandledException(e);

			await this.ShowAlertDialogAsync("Exception", exception.Message, neutral: "Ok");
		}
	}
}