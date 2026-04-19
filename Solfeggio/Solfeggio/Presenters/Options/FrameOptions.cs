using Ace;

using static Rainbow.ScaleFuncs;

namespace Solfeggio.Presenters.Options;

[DataContract]
public class FrameOptions
{
	[DataMember]
	public Span Level { get; set; } = new()
	{
		Scope = SmartRange.Create(-1d, +1d),
		Window = SmartRange.Create(-1d, +1d),
		VisualScaleFunc = Lineal,
	};

	[DataMember]
	public Span Offset { get; set; } = new()
	{
		Scope = SmartRange.Create(+0d, +1d),
		Window = SmartRange.Create(+0d, +1d),
		VisualScaleFunc = Lineal,
	};
}
