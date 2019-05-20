using Ace;
using Solfeggio.Models;
using System;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class HarmonicManager : AManager<Harmonic.Profile>
	{
		public override Harmonic.Profile Create() => new Harmonic.Profile { Title = DateTime.Now.ToShortDateString() };

		public override void Expose()
		{
			base.Expose();
			if (Profiles.Count.Is(0))
			{
				Profiles.Add(Create());
				Profiles.Add(Create());
			}
		}
	}
}
