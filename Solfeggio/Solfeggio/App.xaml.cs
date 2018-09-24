using Ace;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace Solfeggio
{
	public partial class App
	{
		public App ()
		{
			InitializeComponent();
			MainPage = new MainPage();
		}

		protected override void OnStart ()
		{
        }

		protected override void OnSleep () => Store.Snapshot();

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
