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
				Title = "Harmony",
				Harmonics =
				{
					[0] = { Magnitude = 0.43d, Frequency = 258d },
					[1] = { Magnitude = 0.30d, Frequency = 645d },
					[2] = { IsEnabled = false },
				}
			};

			yield return new()
			{
				Title = "Fantasy",
				Harmonics =
				{
					[0] = { Magnitude = 0.43d, Frequency = 150d },
					[1] = { Magnitude = 0.30d, Frequency = 600.5d },
					[2] = { Magnitude = 0.10d, Frequency = 900d },
				}
			};

			yield return new()
			{
				Title = "Cos Wave",
				Harmonics =
				{
					[1] = { IsEnabled = false },
					[2] = { IsEnabled = false },
				}
			};

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
		}
	}
}
