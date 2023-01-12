using Ace;

using Solfeggio.Models;

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

		public IList<PianoKey> Harmonics
		{
			get => Get<IList<PianoKey>>(nameof(Harmonics));
			set => Set(nameof(Harmonics), value);
		}

		public void Expose()
		{
			this[() => ActiveLanguage].Changed += (o, e) =>
				LocalizationSource.Wrap.ActiveManager = new LanguageManager(ActiveLanguage);

			this[Context.Navigate].Executed += (o, e) => System.Diagnostics.Process.Start(e.Parameter.ToStr());
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
