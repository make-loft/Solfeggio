﻿using Ace;
using Solfeggio.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using Yandex.Metrica;
using static Solfeggio.Editions;

namespace Solfeggio
{
	internal static class PolicyManager
	{
		private static readonly TimeSpan LongSessionDuation = TimeSpan.FromMinutes(4);

		public static void CheckExpirationStatus(Editions edition)
		{
			if (edition.Is(Scientific)) return;
			var versionAge = DateTime.Now - new DateTime(2019, 5, 12);
			if (versionAge > TimeSpan.FromDays(32))
			{
				YandexMetrica.ReportEvent("Expiration", versionAge);
				var activeLanguage = Store.Get<AppViewModel>().ActiveLanguage;
				MessageBox.Show(Localizator.ExpirationMessage[activeLanguage]);
				Process.Start(Localizator.HomeLink[activeLanguage]);
			}
		}

		public static void CheckSessionDuration(Editions edition, DateTime startupTimestamp)
		{
			if (edition.Is(Scientific)) return;
			var sessionDuration = DateTime.Now - startupTimestamp;
			if (sessionDuration > LongSessionDuation)
			{
				YandexMetrica.ReportEvent("LongSession", sessionDuration);
				var activeLanguage = Store.Get<AppViewModel>().ActiveLanguage;
				Process.Start(Localizator.HomeLink[activeLanguage]);
			}
		}
	}
}