using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

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

			LoadApplication(new App());
		}
	}
}