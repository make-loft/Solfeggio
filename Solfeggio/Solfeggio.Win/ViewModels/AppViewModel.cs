using Ace;
using Solfeggio.Languages;
using Solfeggio.Presenters;
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

			EvokePropertyChanged(nameof(ActiveLanguage));
		}
	}
}
