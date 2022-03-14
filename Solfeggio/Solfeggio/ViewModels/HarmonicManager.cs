using Ace;
using Solfeggio.Models;
using System.Collections.Generic;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class HarmonicManager : AManager<Harmonic.Profile>
	{
		public override IEnumerable<Harmonic.Profile> CreateDefaultProfiles()
		{
			yield return new()
			{
				Title = "Two Waves Resonance",
				Harmonics =
				{
					[2] = { IsEnabled = false }
				}
			};

			yield return new()
			{
				Title = "Three Waves Resonance",
			};

			yield return new()
			{
				Title = "Cos Wave",
				Harmonics = 
				{
					[1] = { IsEnabled = false },
					[1] = { IsEnabled = false },
				}
			};
		}
	}
}
