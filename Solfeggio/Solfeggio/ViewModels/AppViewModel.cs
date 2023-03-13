using Ace;

using Solfeggio.Models;
#if !NETSTANDARD
using Solfeggio.Views;
#endif

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

		public Segregator<IList<PianoKey>> Harmonics { get; } = new();

		[DataMember] public TapeViewModel Tape { get; set; }
		[DataMember] public FlowerViewModel Flower { get; set; }

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
					var uri = e.Parameter?.ToString().Format(AppInfo.PackageName);

					if (uri.Contains("gitlab"))
					{
						this["OptionsIsVisible"] = false;
						this["CarePageVisit"] = true;
					}
#if NETSTANDARD
					await Browser.OpenAsync(uri);
#else
					await System.Diagnostics.Process.Start(uri).ToAsync();
#endif
				}
				catch (Exception exception)
				{
					Yandex.Metrica.YandexMetrica.ReportError(exception.Message, exception);
				}
			};

			if (this["CarePageVisit"].IsNot(true))
			{
				this["OptionsIsVisible"] = true;
				this["Options.ActiveItemOffset"] = 4; /* agreement item */
			}

			ActiveLanguage = ActiveLanguage.Is(LanguageCodes.Default)
				? new[] { "ru", "be" }.Contains(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
					? LanguageCodes.Russian
					: LanguageCodes.English
				: ActiveLanguage;
#if !NETSTANDARD
			this[Context.Get("LoadActiveFrame")].Executed += (o, e) => SolfeggioView.LoadActiveFrame();
			this[Context.Get("SaveActiveFrame")].Executed += (o, e) => SolfeggioView.SaveActiveFrame();

			Tape ??= new();
			Flower ??= new();
#endif
		}
	}
}
