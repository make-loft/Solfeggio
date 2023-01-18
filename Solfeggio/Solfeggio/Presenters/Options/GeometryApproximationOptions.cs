using Ace;

namespace Solfeggio.Presenters.Options
{
	[DataContract]
	public class GeometryApproximationOptions : ContextObject
	{
		[DataMember]
		public double FlowerApproximationLevel
		{
			get => Get(() => FlowerApproximationLevel, 1d);
			set => Set(() => FlowerApproximationLevel, value);
		}

		[DataMember]
		public double SpiralApproximationLevel
		{
			get => Get(() => SpiralApproximationLevel, 1d);
			set => Set(() => SpiralApproximationLevel, value);
		}
	}
}
