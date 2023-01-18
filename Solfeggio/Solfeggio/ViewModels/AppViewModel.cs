using Ace;

using Solfeggio.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Xamarin.Essentials;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class AppViewModel : ContextObject, IExposable
	{
		public LanguageCodes[] Languages { get; } = { LanguageCodes.English, LanguageCodes.Russian };

		[DataMember]
		public LanguageCodes ActiveLanguage
		{
			get => Get(() => ActiveLanguage);
			set => Set(() => ActiveLanguage, value);
		}

		public IList<PianoKey> Harmonics
		{
			get => Get<IList<PianoKey>>(nameof(Harmonics));
			set => Set(nameof(Harmonics), value);
		}

		public void Expose()
		{
			this[() => ActiveLanguage].Changed += (o, e) =>
				LocalizationSource.Wrap.ActiveManager = new LanguageManager(ActiveLanguage);

			this[() => ActiveLanguage].Changed += (o, e) => Store.Get<ProcessingManager>()
				.Profiles
				.Where(p => p.IsDefault)
				.ForEach(p => p.RefreshTitle());

			this[() => ActiveLanguage].Changed += (o, e) => Store.Get<HarmonicManager>()
				.Profiles
				.Where(p => p.IsDefault)
				.ForEach(p => p.RefreshTitle());

			this[Context.Get("Navigate")].Executed += async (o, e) =>
			{
				try
				{
#if WPF
					var uri = e.Parameter?.ToString();
					await System.Diagnostics.Process.Start(uri).ToAsync();
#else
					var uri = e.Parameter?.ToString().Format(AppInfo.PackageName);
					await Browser.OpenAsync(uri);
#endif
				}
				catch (Exception exception)
				{
					//Yandex.Metrica.YandexMetrica.ReportError(exception.Message, exception);
				}
			};
			//this[Context.Navigate].Executed += (o, e) => Yandex.Metrica.YandexMetrica.ReportEvent($"{e.Parameter}");

			//this[Context.Get("LoadActiveFrame")].Executed += (o, e) => SolfeggioView.LoadActiveFrame();
			//this[Context.Get("SaveActiveFrame")].Executed += (o, e) => SolfeggioView.SaveActiveFrame();

			ActiveLanguage = ActiveLanguage.Is(LanguageCodes.Default)
				? new[] { "ru", "be" }.Contains(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
					? LanguageCodes.Russian
					: LanguageCodes.English
				: ActiveLanguage;
		}
	}
}
