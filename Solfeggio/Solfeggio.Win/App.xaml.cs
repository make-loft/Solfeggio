using System;
using System.IO;
using System.Windows;
using Ace;
using Yandex.Metrica;

namespace Solfeggio
{
	public partial class App
	{
		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var metricaFolder = Path.Combine(appDataFolder, "Solfeggio", "Metric");

			YandexMetricaFolder.SetCurrent(metricaFolder);
			YandexMetrica.Activate("4722c611-c016-4e44-943b-05f9c56968d6");

			Store.Get<ViewModels.AppViewModel>();
			Store.Set<IAudioInputDevice>(Microphone.Default);
		}

		private void App_OnExit(object sender, ExitEventArgs e) =>
			Store.Snapshot();
	}
}
