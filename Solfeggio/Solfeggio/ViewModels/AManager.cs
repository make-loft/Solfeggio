using Ace;

using System.Collections.Generic;
using System.Linq;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public abstract class AManager<TProfile> : ContextObject, IExposable
		where TProfile : Models.AProfile, new()
	{
		[DataMember]
		public SmartSet<TProfile> Profiles { get; set; } = new();

		[DataMember]
		public TProfile ActiveProfile
		{
			get => Get(() => ActiveProfile, GetDefault());
			set => Set(() => ActiveProfile, value);
		}

		public virtual void Expose()
		{
			if (Profiles.Count.Is(0))
			{
				CreateDefaultProfiles()?.ForEach(Profiles.Add);
				Profiles.ForEach(p => p.IsDefault = true);
			}

			this[Context.Set.Create].Executed += (o, e) => Create().Use(Profiles.Add);
			this[Context.Set.Delete].Executed += (o, e) => e.Parameter.To<TProfile>().Use(Profiles.Remove);
			this[Context.Set.Delete].CanExecute += (o, e) => e.CanExecute = e.Parameter.Is(out TProfile p) && p.IsDefault.Not();

			//this[() => ActiveProfile].Changed += (o, e) => Context.Set.Delete.EvokeCanExecuteChanged();

			Profiles.CollectionChanged += (o, e) => ActiveProfile = Profiles.LastOrDefault();
		}

		public virtual TProfile GetDefault() => Profiles.FirstOrDefault();

		public virtual IEnumerable<TProfile> CreateDefaultProfiles() => default;

		public virtual TProfile Create() => new() { Title = "~~~" };
	}
}