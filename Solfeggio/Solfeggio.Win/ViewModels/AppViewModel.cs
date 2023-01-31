using Ace;

using Solfeggio.Models;
using Solfeggio.Views;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class AppViewModel : ContextObject, IExposable
	{
		public LanguageCodes[] Languages { get; } = Enum.GetValues(TypeOf<LanguageCodes>.Raw).Cast<LanguageCodes>().Take(2).ToArray();

		[DataMember]
		public LanguageCodes ActiveLanguage
		{
			get => Get(() => ActiveLanguage, LanguageCodes.English);
			set => Set(() => ActiveLanguage, value);
		}

		public IList<PianoKey> Harmonics
		{
			get => Get<IList<PianoKey>>(nameof(Harmonics));
			set => Set(nameof(Harmonics), value);
		}

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

			this[Context.Navigate].Executed += (o, e) => System.Diagnostics.Process.Start(e.Parameter.To<string>());
			this[Context.Navigate].Executed += (o, e) => Yandex.Metrica.YandexMetrica.ReportEvent($"{e.Parameter}");

			this[Context.Get("LoadActiveFrame")].Executed += (o, e) => SolfeggioView.LoadActiveFrame();
			this[Context.Get("SaveActiveFrame")].Executed += (o, e) => SolfeggioView.SaveActiveFrame();

			Tape ??= new();
			Flower ??= new();

			EvokePropertyChanged(nameof(ActiveLanguage));
		}
	}
}
