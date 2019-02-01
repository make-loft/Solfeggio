using Ace;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Skip)]
namespace Solfeggio
{
	public partial class App
	{
		public App()
		{
			InitializeComponent();
			MainPage = new Views.SolfeggioView();
		}

		protected override void OnStart() { }
		protected override void OnSleep() => Store.Snapshot();
		protected override void OnResume() { }
	}
}
