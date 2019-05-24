using Ace;
using Solfeggio.Models;
using System;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class HarmonicManager : AManager<Harmonic.Profile>
	{
		public override Harmonic.Profile Create() => new Harmonic.Profile { Title = DateTime.Now.Millisecond.ToString() };

		public override void Expose()
		{
			if (Profiles.Count.Is(0))
			{
				var p = default(Harmonic.Profile);

				Create().To(out p).With
				(
					p.Title = "Two Waves Resonance",
					p.Harmonics[2].IsEnabled = false
				).Use(Profiles.Add);

				Create().To(out p).With
				(
					p.Title = "Three Waves Resonance"
				).Use(Profiles.Add);

				Create().To(out p).With
				(
					p.Title = "Sin Wave",
					p.Harmonics[1].IsEnabled = false,
					p.Harmonics[2].IsEnabled = false
				).Use(Profiles.Add);
			}

			base.Expose();
		}
	}
}
