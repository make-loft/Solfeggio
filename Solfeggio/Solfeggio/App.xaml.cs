using Ace;

using Solfeggio.Presenters;
using Solfeggio.ViewModels;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Xamarin.Forms.Xaml;

using Yandex.Metrica;

using static Solfeggio.Editions;

[assembly: XamlCompilation(XamlCompilationOptions.Skip)]
namespace Solfeggio
{
	public partial class App
	{
#if DEBUG
		public static Editions Edition { get; } = Developer;
#else
		public static Editions Edition { get; } = Education;
#endif
		public static Dictionary<Editions, string> YandexMetricaKeys = new()
		{
			{ Developer, "4722c611-c016-4e44-943b-05f9c56968d6" },
			{ Portable, "d1e987f6-1930-473c-8f45-78cd96bb5fc0" },
			{ Education, "37e2b088-6a25-4987-992c-e12b7e020e85" },
			{ Gratitude, "e23fdc0c-166e-4885-9f18-47cc7b60f867" },
		};

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

		protected override void OnStart()
		{
			YandexMetrica.Wake();
		}
		protected override void OnSleep()
		{
			Store.Get<ProcessingManager>().ActiveProfile?.Dispose();

			Store.Snapshot();

			YandexMetrica.Lull();
		}

		protected override void OnResume()
		{
			YandexMetrica.Wake();

			Store.Get<ProcessingManager>().ActiveProfile?.Expose();
		}
	}
}
