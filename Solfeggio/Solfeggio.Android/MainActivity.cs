using System;
using System.Linq;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Ace;
using Android.Support.V4.App;
using Android.Widget;

namespace Solfeggio.Droid
{
    [Activity(
        Label = "Solfeggio",
        Icon = "@mipmap/icon",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private static readonly string[] RequriedPermissions = {Manifest.Permission.RecordAudio};

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;

                base.OnCreate(bundle);

                Xamarin.Forms.Forms.Init(this, bundle);
                LoadApplication(new App());

                if (RequriedPermissions.Any(p => CheckSelfPermission(p).IsNot(Permission.Granted)))
                {
                    ActivityCompat.RequestPermissions(this, RequriedPermissions, RequestPermissionCode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private const int RequestPermissionCode = 1000;

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            for (var i = 0; i < grantResults.Length; i++)
            {
                Toast.MakeText(this, $"{permissions[i]} : {grantResults[i]}", ToastLength.Short);
            }
        }
    }
}