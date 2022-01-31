using Ace;

namespace Solfeggio.Models
{
	[DataContract]
	public class AProfile : ContextObject
	{
		[DataMember] public string Title
		{
			get => Get(() => Title, "~~~");
			set => Set(() => Title, value);
		}

		[DataMember] public bool IsDefault { get; set; }
	}
}
