using Ace;

using Solfeggio.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class TapeViewModel
	{
		public ProjectionCamera Camera { get; set; }
		[DataMember] public double Radius { get; set; } = 1d;
		[DataMember] public double Depth { get; set; } = 32d;
		[DataMember] public double Thin { get; set; } = 0.04d;
		[DataMember] public double Approximation { get; set; } = 1d;
		[DataMember] public bool Joystick { get; set; } = true;
	}

	[DataContract]
	public class AppViewModel : ContextObject, IExposable
	{
		public LanguageCodes[] Languages { get; } = Enum.GetValues(TypeOf<LanguageCodes>.Raw).Cast<LanguageCodes>().Take(2).ToArray();

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

		[DataMember]
		public TapeViewModel Tape { get; set; }

		public void Expose()
		{
			this[() => ActiveLanguage].PropertyChanged += (o, e) =>
				LocalizationSource.Wrap.ActiveManager = new LanguageManager(ActiveLanguage);

			this[Context.Navigate].Executed += (o, e) => System.Diagnostics.Process.Start(e.Parameter.ToStr());
			this[Context.Navigate].Executed += (o, e) => Yandex.Metrica.YandexMetrica.ReportEvent($"{e.Parameter}");

			Tape ??= new();
			EvokePropertyChanged(nameof(ActiveLanguage));
		}
	}
}
