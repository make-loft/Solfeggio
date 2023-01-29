using Ace;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;

using System;
using System.IO;
using System.Threading;

using Xamarin.Essentials;

using Yandex.Metrica;

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
		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		protected override async void OnCreate(Bundle bundle)
		{
			AndroidEnvironment.UnhandledExceptionRaiser += (o, args) => ProcessUnhandledException(args.Exception);
			AppDomain.CurrentDomain.UnhandledException += (o, args) => ProcessUnhandledException(args.ExceptionObject as Exception);
			async void ProcessUnhandledException(Exception exception)
			{
				for (var e = exception; e.Is(); e = e.InnerException)
					YandexMetrica.ReportUnhandledException(e);

				await this.ShowAlertDialogAsync
				(
					"Oops".Localize() + "!",
					exception.Message,
					neutral: "Ok".Localize()
				);
			}

			RequestedOrientation = ScreenOrientation.Landscape;
			Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			base.OnCreate(bundle);

			ThreadPool.QueueUserWorkItem(RunAnalitics);

			Platform.Init(this, bundle);
			Xamarin.Forms.Forms.Init(this, bundle);

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

		private void RunAnalitics(object o)
		{
			var (androidId, advertisingId) = default(string);

			try { androidId = Android.Provider.Settings.Secure.GetString(ContentResolver, Android.Provider.Settings.Secure.AndroidId); }
			catch { }
			//try { advertisingId = AdvertisingIdClient.GetAdvertisingIdInfo(ApplicationContext).Id; }
			//catch { }
			try
			{
				var appDataFolderPath = System.Environment.SpecialFolder.ApplicationData.GetPath();
				var metricaFolderPath = Path.Combine(appDataFolderPath, "Yandex.Metrica");

				YandexMetricaFolder.SetCurrent(metricaFolderPath);
				YandexMetrica.Activate(App.YandexMetricaKeys[App.Edition], androidId, advertisingId);
			}
			catch (Exception exception)
			{
				Log.Error("Yandex.Metrica", exception.ToString());
			}
		}
	}
}