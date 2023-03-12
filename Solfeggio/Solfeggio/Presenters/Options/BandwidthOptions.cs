using Ace;

using Rainbow;

using static Rainbow.ScaleFuncs;

namespace Solfeggio.Presenters.Options
{
	[DataContract]
	public class SpectralOptions
	{
		[DataMember]
		public Bandwidth Frequency { get; set; } = new()
		{
			Scope = SmartRange.Create(10d, Api.AudioInputDevice.DefaultSampleRate / 2),
			Window = SmartRange.Create(20d, 2870d),
			VisualScaleFunc = Log2,
			Units = "Hz",
		};

		[DataMember]
		public Bandwidth Magnitude { get; set; } = new()
		{
			Scope = SmartRange.Create(0.00d, 1d),
			Window = SmartRange.Create(0.00d, 1d),
			VisualScaleFunc = Sqrt,
		};

		[DataMember]
		public Bandwidth Phase { get; set; } = new()
		{
			Scope = SmartRange.Create(-Pi.Single, +Pi.Single),
			Window = SmartRange.Create(-Pi.Single, +Pi.Single),
			Units = "Rad",
		};
	}

	[DataContract]
	public class FrameOptions
	{
		[DataMember]
		public Bandwidth Level { get; set; } = new()
		{
			Scope = SmartRange.Create(-1d, +1d),
			Window = SmartRange.Create(-1d, +1d),
			VisualScaleFunc = Lineal,
		};

		[DataMember]
		public Bandwidth Offset { get; set; } = new()
		{
			Scope = SmartRange.Create(+0d, +1d),
			Window = SmartRange.Create(+0d, +1d),
			VisualScaleFunc = Lineal,
		};
	}
}
