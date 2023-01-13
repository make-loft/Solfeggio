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
				Title = "Camertone".Localize(),
				Harmonics =
				{
					new() { IsEnabled = true, Frequency = 440d },
				}
			};

			yield return new()
			{
				Title = "Resonance".Localize(),
				Harmonics =
				{
					new() { IsEnabled = true, Frequency = 442d },
					new() { IsEnabled = true, Frequency = 440d },
					new() { IsEnabled = false, Frequency = 438d },
				}
			};

			yield return new()
			{
				Title = "Harmony".Localize(),
				Harmonics =
				{
					new() { Magnitude = 0.43d, Frequency = 258d },
					new() { Magnitude = 0.30d, Frequency = 645d },
				}
			};

			yield return new()
			{
				Title = "Fantasy".Localize(),
				Harmonics =
				{
					new() { Magnitude = 0.43d, Frequency = 150d },
					new() { Magnitude = 0.30d, Frequency = 600.5d },
					new() { Magnitude = 0.10d, Frequency = 900d },
				}
			};

			yield return new()
			{
				Title = "Relax".Localize(),
				Harmonics =
				{
					new() { Magnitude = 0.43d, Frequency = 170 },
					new() { Magnitude = 0.30d, Frequency = 210d },
					new() { Magnitude = 0.10d, Frequency = 220d },
				}
			};
		}
	}
}
