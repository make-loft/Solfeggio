using Ace;

using Rainbow;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Solfeggio.ViewModels
{
	[DataContract]
	class AppViewModel : ContextObject, IExposable
	{
		public LanguageCodes[] Languages { get; } = Enum.GetValues(TypeOf<LanguageCodes>.Raw).Cast<LanguageCodes>().Take(2).ToArray();

		[DataMember]
		public LanguageCodes ActiveLanguage
		{
			get => Get(() => ActiveLanguage);
			set => Set(() => ActiveLanguage, value);
		}

		public IList<Bin> Harmonics
		{
			get => Get<IList<Bin>>(nameof(Harmonics));
			set => Set(nameof(Harmonics), value);
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
