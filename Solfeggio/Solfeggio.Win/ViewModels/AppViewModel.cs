using Ace;
using Solfeggio.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solfeggio.ViewModels
{
	[DataContract]
	class AppViewModel : ContextObject, IExposable
	{
		public LanguageCodes[] Languages { get; } = Enum.GetValues(TypeOf<LanguageCodes>.Raw).Cast<LanguageCodes>().ToArray();

		[DataMember]
		public LanguageCodes ActiveLanguage
		{
			get => Get(() => ActiveLanguage);
			set => Set(() => ActiveLanguage, value);
		}

		public IList<PianoKey> Dominants
		{
			get => Get<IList<PianoKey>>(nameof(Dominants));
			set => Set(nameof(Dominants), value);
		}

		public void Expose()
		{
			this[() => ActiveLanguage].PropertyChanged += (o, e) =>
				LocalizationSource.Wrap.ActiveManager = new LanguageManager(ActiveLanguage);

			this[Context.Navigate].Executed += (o, e) => System.Diagnostics.Process.Start(e.Parameter.ToStr());
			this[Context.Navigate].Executed += (o, e) => Yandex.Metrica.YandexMetrica.ReportEvent($"{e.Parameter}");

			EvokePropertyChanged(nameof(ActiveLanguage));
		}
	}
}
