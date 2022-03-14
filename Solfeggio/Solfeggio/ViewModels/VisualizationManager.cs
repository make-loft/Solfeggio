using Ace;

using Solfeggio.Models;

using System.Collections.Generic;
using System.Linq;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class VisualizationManager : AManager<VisualizationProfile>
	{
		public override VisualizationProfile Create() => new()
		{
			Id = Profiles.Select(p => p.Id).Max() + 1,
			Title = ActiveProfile?.Title + " ~",
			Palette = ActiveProfile?.Palette ?? "Nature",
		};

		public override IEnumerable<VisualizationProfile> CreateDefaultProfiles() => AppPalette.ColorPalettes.Keys.OfType<string>()
			.Let(out int i).Select(k => new VisualizationProfile
			{
				Id = i++,
				Title = k,
				Palette = k,
			});

		public override VisualizationProfile GetDefault() => Profiles.FirstOrDefault(p => p.Palette.Is("Nature"));

		public override void Expose()
		{
			base.Expose();

			this[() => ActiveProfile].PropertyChanging += (o, e) => ActiveProfile?.Keep();
			this[() => ActiveProfile].PropertyChanged += (o, e) => ActiveProfile?.Load();

			ActiveProfile?.Load();
		}
	}
}
