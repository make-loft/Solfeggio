using Ace;
using Solfeggio.Palettes.Languages;
using System.Resources;

namespace Solfeggio.ViewModels
{
	class AppViewModel : ContextObject, IExposable
	{
		public ResourceManager[] Languages { get; } = new ResourceManager[] { English.ResourceManager };

		public ResourceManager ActiveLanguage
		{
			get => Get(() => ActiveLanguage);
			set => Set(() => ActiveLanguage, value);
		}

		public void Expose()
		{
			this[() => ActiveLanguage].PropertyChanged += (o, e) => LocalizationSource.Wrap.ActiveManager = ActiveLanguage;
			ActiveLanguage = Languages[0];
		}
	}
}
