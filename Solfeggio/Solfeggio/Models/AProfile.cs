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

		[DataMember] public string DefaultTitleFormat { get; set; }
		[DataMember] public string DefaultTitle { get; set; }
		[DataMember] public bool IsDefault { get; set; }

		public void RefreshTitle() => Title = DefaultTitleFormat.Is(out var format)
			? format.Format(DefaultTitle?.Localize())
			: DefaultTitle?.Localize();
	}
}
