using Ace;

using Solfeggio.Presenters;
using Solfeggio.ViewModels;

using System;
using System.Globalization;
using System.IO;

using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Skip)]
namespace Solfeggio
{
	public partial class App
	{
		public static readonly string AppDataFolderPath =
			Environment.SpecialFolder.ApplicationData.GetPath();

		static App()
		{
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

			Store.ActiveBox.KeyFormat = Path.Combine(AppDataFolderPath, "{0}.json");
		}

		public App()
		{
			Store.Get<AppViewModel>();
			Store.Get<MusicalPresenter>();

			InitializeComponent();
			MainPage = new Views.SolfeggioView();
		}

		protected override void OnStart() { }
		protected override void OnSleep() => Store.Snapshot();
		protected override void OnResume() { }
	}
}
