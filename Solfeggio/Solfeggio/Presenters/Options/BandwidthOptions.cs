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
			Limit = SmartRange.Create(10d, AudioInputDevice.DefaultSampleRate / 2),
			Threshold = SmartRange.Create(20d, 2870d),
			VisualScaleFunc = Log2,
		};

		[DataMember]
		public Bandwidth Magnitude { get; set; } = new()
		{
			Limit = SmartRange.Create(0.00d, 1d),
			Threshold = SmartRange.Create(0.00d, 1d),
			VisualScaleFunc = Sqrt,
		};

		[DataMember]
		public Bandwidth Phase { get; set; } = new()
		{
			Limit = SmartRange.Create(-Pi.Single, +Pi.Single),
			Threshold = SmartRange.Create(-Pi.Single, +Pi.Single),
		};
	}

	[DataContract]
	public class FrameOptions
	{
		[DataMember]
		public Bandwidth Level { get; set; } = new()
		{
			Limit = SmartRange.Create(-1d, +1d),
			Threshold = SmartRange.Create(-1d, +1d),
			VisualScaleFunc = Lineal,
		};

		[DataMember]
		public Bandwidth Offset { get; set; } = new()
		{
			Limit = SmartRange.Create(+0d, +1d),
			Threshold = SmartRange.Create(+0d, +1d),
			VisualScaleFunc = Lineal,
		};
	}
}
