using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Ace;
using Solfeggio.Processors;
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
			Store.Set<IAudioInputDevice>(Store.Get<Microphone>());
		}

		private void App_OnExit(object sender, ExitEventArgs e) => Store.Snapshot();

		#region Launching
		/* should not directly use nested assemblies into this region */

		private static readonly Type ArrayOfBytesType = typeof(byte[]);

		public static IEnumerable<byte[]> EnumerateNestedRawAssemblies() => typeof(Properties.Assemblies)
				.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(p => p.PropertyType == ArrayOfBytesType)
				.Select(p => p.GetValue(null, null))
				.Cast<byte[]>();
		#endregion
	}
}
