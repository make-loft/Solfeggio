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

namespace Solfeggio
{
	public partial class App
	{
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

		public void CheckExpiration()
		{
			var versionAge = DateTime.Now - new DateTime(2019, 5, 12);
			if (versionAge > TimeSpan.FromDays(32))
			{
				YandexMetrica.ReportEvent("Expiration", versionAge);
				var activeLanguage = Store.Get<AppViewModel>().ActiveLanguage;
				MessageBox.Show(Localizator.ExpirationMessage[activeLanguage]);
				Process.Start(Localizator.HomeLink[activeLanguage]);
			}
		}

		private void App_OnStartup(object sender, StartupEventArgs args)
		{
			var settingsFolder = CheckWriteAccess(EntryFolder)
				? Path.Combine(EntryFolder, "Settings", Location.GetHashCode().ToString())
				: Path.Combine(GetFolderPath(SpecialFolder.MyDocuments), "Solfeggio", Location.GetHashCode().ToString());

			try
			{
				if (Directory.Exists(settingsFolder).Not()) Directory.CreateDirectory(settingsFolder);
				Store.ActiveBox = new Memory(new Ace.Specific.KeyFileStorage(), Path.Combine(settingsFolder, "{0}.json"));
				var metricaFolder = Path.Combine(settingsFolder, "Metric");

				YandexMetricaFolder.SetCurrent(metricaFolder);
				YandexMetrica.Activate("4722c611-c016-4e44-943b-05f9c56968d6");
				CheckExpiration();
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
		}

		private DateTime _startupTimestamp;
		private static readonly TimeSpan LongSessionDuation = TimeSpan.FromMinutes(4);

		private void App_OnExit(object sender, ExitEventArgs e)
		{
			Store.Snapshot();
			var sessionDuration = DateTime.Now - _startupTimestamp;
			if (sessionDuration > LongSessionDuation)
			{
				YandexMetrica.ReportEvent("LongSession", sessionDuration);
				var activeLanguage = Store.Get<AppViewModel>().ActiveLanguage;
				Process.Start(Localizator.HomeLink[activeLanguage]);
			}
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
