using System;
using System.Linq;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Ace;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Solfeggio.Api;
using Android.Runtime;

namespace Solfeggio.Droid
{
    [Activity(
        Label = "Solfeggio",
		Icon = "@drawable/logo",
		Theme = "@style/MainTheme",
        MainLauncher = false,
        NoHistory = true,
        ScreenOrientation = ScreenOrientation.Landscape,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private static readonly string[] RequriedPermissions = {Manifest.Permission.RecordAudio};

	    protected override void OnCreate(Bundle bundle)
	    {
			AndroidEnvironment.UnhandledExceptionRaiser += (o, args) => ProcessUnhandledException(args.Exception);
			AppDomain.CurrentDomain.UnhandledException += (o, args) => ProcessUnhandledException(args.ExceptionObject as Exception);

			RequestedOrientation = ScreenOrientation.Landscape;
		    Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			try
		    {
			    if (RequriedPermissions.Any(p => CheckSelfPermission(p).IsNot(Permission.Granted)))
			    {
				    ActivityCompat.RequestPermissions(this, RequriedPermissions, RequestPermissionCode);
			    }

				Store.Set<IProcessor>(Microphone.Default);
			}
		    catch (Exception e)
		    {
			    Console.WriteLine(e);
		    }

			base.OnCreate(bundle);

			Xamarin.Forms.Forms.Init(this, bundle);
			LoadApplication(new App());
		}

	    private const int RequestPermissionCode = 1000;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            for (var i = 0; i < grantResults.Length; i++)
            {
                Toast.MakeText(this, $"{permissions[i]} : {grantResults[i]}", ToastLength.Short);
            }
        }

		void ShowExceptionAlert(Exception exception) => new AlertDialog.Builder(this)
			.SetPositiveButton("Ok", (sender, args) =>
			{
				if (App.Current.Is(out var app).Not()) return;
				//app.Quit();
			})
			.SetMessage(exception.Message)
			.SetTitle("Error")
			.Show();

		void ShowAlert(string message) => new AlertDialog.Builder(this)
			.SetPositiveButton("Ok", (sender, args) =>
			{
				if (App.Current.Is(out var app).Not()) return;
				//app.Quit();
			})
			.SetMessage(message)
			.SetTitle("Error")
			.Show();

		void ProcessUnhandledException(Exception exception)
		{
			//for (var e = exception; e.Is(); e = e.InnerException)
			//	YandexMetrica.ReportUnhandledException(e);

			if (exception.Is())
				ShowExceptionAlert(exception);
		}
	}
}