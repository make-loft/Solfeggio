using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;

using System;

namespace Solfeggio.Droid
{
	[Activity(
		Label = "Solfeggio 🎶",
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

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestedOrientation = ScreenOrientation.Landscape;
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			base.OnCreate(savedInstanceState);

			StartActivity(new Intent(Application.Context, typeof(MainActivity)));
		}
	}
}