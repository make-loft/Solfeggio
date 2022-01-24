using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Ace;
using Solfeggio.ViewModels;
using Yandex.Metrica;

using static System.Environment;
using static Solfeggio.Editions;

namespace Solfeggio
{
	public partial class App
	{
		public static Editions Edition { get; } = Developer;

		public static Dictionary<Editions, string> YandexMetricaKeys = new()
		{
			{ Developer, "4722c611-c016-4e44-943b-05f9c56968d6" },
			{ Portable, "d1e987f6-1930-473c-8f45-78cd96bb5fc0" },
			{ Education, "37e2b088-6a25-4987-992c-e12b7e020e85" },
			{ Scientific, "e23fdc0c-166e-4885-9f18-47cc7b60f867" },
		};

		public static readonly string Location = Assembly.GetEntryAssembly().Location;
		public static readonly string EntryFolder = Path.GetDirectoryName(Location);

		private static bool CheckWriteAccess(string path)
		{
			try
			{
				var directory = Path.Combine(path, "tmp");
				while (Directory.Exists(directory)) directory += "_";
				Directory.CreateDirectory(directory).Delete();

				var file = Path.Combine(path, "tmp");
				while (File.Exists(file)) file += "_";
				File.Create(file).Dispose();
				File.Delete(file);

				return true;
			}
			catch
			{
				return false;
			}
		}

		private DateTime _startupTimestamp;

		private void App_OnStartup(object sender, StartupEventArgs args)
		{
			var settingsFolder = CheckWriteAccess(EntryFolder)
				? Path.Combine(EntryFolder, "Settings", Location.GetHashCode().ToString())
				: Path.Combine(GetFolderPath(SpecialFolder.MyDocuments), "Solfeggio", Location.GetHashCode().ToString());

			try
			{
				if (Directory.Exists(settingsFolder).Not()) Directory.CreateDirectory(settingsFolder);
				Store.ActiveBox = new Memory(new Ace.Specific.KeyFileStorage(), Path.Combine(settingsFolder, "{0}.json"));
				SetHandlers(Store.ActiveBox);

				var metricaFolder = Path.Combine(settingsFolder, "Metric");

				YandexMetricaFolder.SetCurrent(metricaFolder);
				YandexMetrica.Activate(YandexMetricaKeys[Edition]);
				AgreementManager.CheckExpirationStatus(Edition);

				//Current.Resources.MergedDictionaries[3].Values.OfType<Brush>().ForEach(b => b.Freeze());
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.ToString());
			}

			DispatcherUnhandledException += (o, e) =>
			{
				if (Debugger.IsAttached) return;
				YandexMetrica.ReportError(e.Exception.Message, e.Exception);
				MessageBox.Show(e.ToString());
				e.Handled = true;
			};

			Store.Get<AppViewModel>();
			_startupTimestamp = DateTime.Now;


			var theme = Store.ActiveBox.Revive<Dictionary<string, object>>("Theme");
			//var brushes = Resources.MergedDictionaries[2];
			theme.ForEach(p => Resources[p.Key] = p.Value);
		}

		private void App_OnExit(object sender, ExitEventArgs e)
		{
			var keysForSave =
				Resources.MergedDictionaries[2].Keys.Cast<string>()
				.Concat(Resources.MergedDictionaries[1].Keys.Cast<string>()).ToList();
			var forSave = keysForSave.ToDictionary(k => k, k => Resources[k]);

			Store.ActiveBox.TryKeep(forSave, "Theme");
			Store.Snapshot();

			AgreementManager.CheckSessionDuration(Edition, _startupTimestamp);
		}

		private void SetHandlers(Memory memoryBox)
		{
			memoryBox.EncodeFailed += (key, item, exception) =>
			{
				if (Debugger.IsAttached)
					MessageBox.Show($"{key} : {item}\n{exception}");
				YandexMetrica.ReportError($"{key} : {item}", exception);
			};

			memoryBox.DecodeFailed += (key, type, exception) =>
			{
				if (Debugger.IsAttached)
					MessageBox.Show($"{key} - {type}\n{exception}");
				YandexMetrica.ReportError($"{key} - {type}", exception);
			};
		}

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
