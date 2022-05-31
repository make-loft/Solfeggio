using Ace;

namespace Solfeggio.ViewModels
{
	[DataContract]
	public class FlowerViewModel : ContextObject
	{
		[DataMember]
		public double Radius
		{
			get => Get(() => Radius, 1d);
			set => Set(() => Radius, value);
		}
	}
}
