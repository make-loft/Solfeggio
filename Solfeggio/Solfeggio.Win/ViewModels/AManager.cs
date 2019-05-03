using Ace;
using Solfeggio.Models;
using System;
using System.Linq;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class HarmonicManager : Manager<Harmonic.Profile>
	{
		public override Harmonic.Profile Create() => new Harmonic.Profile { Title = DateTime.Now.ToShortDateString() };

		public override void Expose()
		{
			base.Expose();
			Profiles.Add(Create());
			Profiles.Add(Create());
		}
	}

	[DataContract]
	public abstract class Manager<TProfile> : ContextObject, IExposable
	{
		[DataMember]
		public SmartSet<TProfile> Profiles { get; set; } = new SmartSet<TProfile>();

		[DataMember]
		public TProfile ActiveProfile
		{
			get => Get(() => ActiveProfile, Profiles.LastOrDefault());
			set => Set(() => ActiveProfile, value);
		}

		public virtual void Expose()
		{
			this[Context.Set.Create].Executed += (o, e) => Create().Use(Profiles.Add);
			this[Context.Set.Delete].Executed += (o, e) => e.Parameter.To<TProfile>().Use(Profiles.Remove);

			Profiles.CollectionChanged += (o, e) => ActiveProfile = Profiles.LastOrDefault();
		}

		public abstract TProfile Create();
	}
}