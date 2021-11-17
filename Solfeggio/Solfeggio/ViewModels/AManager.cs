using Ace;
using System.Linq;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public abstract class AManager<TProfile> : ContextObject, IExposable
	{
		[DataMember]
		public SmartSet<TProfile> Profiles { get; set; } = new SmartSet<TProfile>();

		[DataMember]
		public TProfile ActiveProfile
		{
			get => Get(() => ActiveProfile, Profiles.FirstOrDefault());
			set => Set(() => ActiveProfile, value);
		}

		public virtual void Expose()
		{
			this[Context.Set.Create].Executed += (o, e) => Create().Use(Profiles.Add);
			this[Context.Set.Delete].Executed += (o, e) => e.Parameter.To<TProfile>().Use(Profiles.Remove);
			this[Context.Set.Delete].CanExecute += (o, e) => e.CanExecute = Profiles.IndexOf(e.Parameter.To<TProfile>()) > 2;

			//this[() => ActiveProfile].PropertyChanged += (o, e) => Context.Set.Delete.EvokeCanExecuteChanged();

			Profiles.CollectionChanged += (o, e) => ActiveProfile = Profiles.LastOrDefault();
		}

		public abstract TProfile Create();
	}
}