using Ace;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;

using System;

using Xamarin.Essentials;

namespace Solfeggio.Droid
{
	[Activity(
		Label = "Solfeggio", /* 🎶🎵 */
		Icon = "@drawable/logo",
		Theme = "@style/SplashTheme",
		ScreenOrientation = ScreenOrientation.Landscape,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
		MainLauncher = true,
		NoHistory = true
		)]
	public class SplashActivity : Activity
	{
		static SplashActivity()
		{
			AppDomain.CurrentDomain.UnhandledException += (o, args) =>
			{
				for (var e = args.ExceptionObject as Exception; e is object; e = e.InnerException)
					Log.Error("Unhandled", e.ToString());
			};
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		protected override async void OnCreate(Bundle bundle)
		{
			RequestedOrientation = ScreenOrientation.Landscape;
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			base.OnCreate(bundle);

			Platform.Init(this, bundle);

			var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
			for (var i = 0; i < 5; i++)
			{
				if (status.Is(PermissionStatus.Granted))
					break;

				status = await Permissions.RequestAsync<Permissions.Microphone>();
			}

			if (status.Is(PermissionStatus.Granted))
			{
				StartActivity(new Intent(Application.Context, typeof(MainActivity)));
				return;
			}

			Store.Get<ViewModels.AppViewModel>(); // initialize for localization

			await this.ShowAlertDialogAsync
			(
				"Oops".Localize() + "!",
				"MicrophoneAccessErrorMessage".Localize(),
				neutral: "Ok".Localize()
			);

			FinishAffinity();
		}
	}
}