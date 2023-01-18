using Ace;

namespace Solfeggio.Presenters.Options
{
	[DataContract]
	public class FormatOptions : ContextObject
	{
		[DataMember] public string[] NumericFormats { get; set; } = { "F0", "F1", "F2", "F3", "F4", "F5" };

		[DataMember]
		public string MonitorNumericFormat
		{
			get => Get(() => MonitorNumericFormat, "F2");
			set => Set(() => MonitorNumericFormat, value);
		}

		[DataMember]
		public string ScreenNumericFormat
		{
			get => Get(() => ScreenNumericFormat, "F1");
			set => Set(() => ScreenNumericFormat, value);
		}
	}
}
