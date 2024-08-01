using Ace;

using Rainbow;

using Solfeggio.Models;
#if !NETSTANDARD
using Solfeggio.Views;
#endif

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

		public Segregator<IDictionary<Bin, PianoKey>> Harmonics { get; } = new();

		[DataMember] public TapeViewModel Tape { get; set; }
		[DataMember] public FlowerViewModel Flower { get; set; }

		public void Expose()
		{
			this[() => ActiveLanguage].Changed += args =>
				LocalizationSource.Wrap.ActiveManager = new LanguageManager(ActiveLanguage);

			this[() => ActiveLanguage].Changed += args => Store.Get<ProcessingManager>()
				.Profiles
				.Where(p => p.IsDefault)
				.ForEach(p => p.RefreshTitle());

			this[() => ActiveLanguage].Changed += args => Store.Get<HarmonicManager>()
				.Profiles
				.Where(p => p.IsDefault)
				.ForEach(p => p.RefreshTitle());

			this[Context.Get("Navigate")].Executed += async args =>
			{
				try
				{
#if NETSTANDARD
					var packageName = Xamarin.Essentials.AppInfo.PackageName;
#else
					var packageName = App.Current.GetType().Assembly.GetName();
#endif
					var uri = args.Parameter?.ToString().Format(packageName);

					if (uri.Contains("gitlab"))
					{
						this["OptionsIsVisible"] = false;
						this["CarePageVisit"] = true;
					}
#if NETSTANDARD
					await Xamarin.Essentials.Browser.OpenAsync(uri);
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
			this[Context.Get("LoadActiveFrame")].Executed += args => SolfeggioView.LoadActiveFrame();
			this[Context.Get("SaveActiveFrame")].Executed += args => SolfeggioView.SaveActiveFrame();

			Tape ??= new();
			Flower ??= new();
#endif
		}
	}
}
