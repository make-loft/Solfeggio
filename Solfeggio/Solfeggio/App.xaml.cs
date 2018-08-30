using Ace;
using Ace.Specific;
using Rainbow;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace Solfeggio
{
	public partial class App
	{
	    public static Complex[] CurrentSpectrum = new Complex[0];
	    public static Complex Max;

		public App ()
		{
			InitializeComponent();

			MainPage = new MainPage();
		}

		protected override void OnStart ()
		{
		    Memory.ActiveBox = new Memory(new KeyFileStorage());
        }

		protected override void OnSleep ()
		{
            Store.Snapshot();
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
