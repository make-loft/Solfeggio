using Ace;

using Rainbow;

namespace Solfeggio.Presenters
{
	public struct ScaleTransformer
	{
		public readonly Projection UnscaleFunc;
		public readonly Projection InscaleFunc;
		public readonly Projection Correction;

		public readonly double VisualStretchFactor;
		public readonly double VisualLength;

		public readonly double LogicalLength;
		public readonly double LogicalLower;
		public readonly double LogicalUpper;

		public readonly double ScaledLength;
		public readonly double ScaledLower;
		public readonly double ScaledUpper;

		public ScaleTransformer(
			Projection scaleFunc, double visualLength,
			double logicalLower, double logicalUpper,
			Projection correction = default)
		{
			VisualLength = visualLength;
			InscaleFunc = scaleFunc;
			Correction = correction;
			LogicalLower = logicalLower;
			LogicalUpper = logicalUpper;
			LogicalLength = logicalUpper - logicalLower;
			ScaledLower = scaleFunc(logicalLower);
			ScaledUpper = scaleFunc(logicalUpper);
			ScaledLength = ScaledUpper - ScaledLower;
			VisualStretchFactor = visualLength / ScaledLength;

			UnscaleFunc =
				InscaleFunc.Is(ScaleFuncs.Log) ? ScaleFuncs.Exp :
				InscaleFunc.Is(ScaleFuncs.Log2) ? ScaleFuncs._2Pow :
				InscaleFunc.Is(ScaleFuncs.Sqrt) ? ScaleFuncs.Pow2 :
				ScaleFuncs.Lineal;
		}

		public double GetLogicalOffset(double visualOffset) => visualOffset
			.Squeeze(VisualStretchFactor)
			.Increment(ScaledLower)
			.Project(UnscaleFunc);

		public double GetVisualOffset(double logicalOffset) => logicalOffset
			.Project(InscaleFunc)
			.Decrement(ScaledLower)
			.Stretch(VisualStretchFactor)
			.Project(Correction);

		//((hVisualScaleFunc(hActiveValue) - scaled.Lower) *hLength / scaled.Length)
	}
}
