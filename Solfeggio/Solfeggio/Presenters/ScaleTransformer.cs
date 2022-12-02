using Ace;

using Rainbow;

using static Rainbow.ScaleFuncs;

namespace Solfeggio.Presenters
{
	public struct ScaleTransformer
	{
		public readonly Projection
			Correction, UnscaleFunc, InscaleFunc;

		public readonly double
			LogicalLength, LogicalLower, LogicalUpper,
			ScaledLength, ScaledLower, ScaledUpper,
			VisualLength, VisualStretchFactor;

		public ScaleTransformer(
			Projection scaleFunc, double visualLength,
			double logicalLower, double logicalUpper,
			Projection correction = default)
		{
			LogicalLower = logicalLower;
			LogicalUpper = logicalUpper;
			LogicalLength = LogicalUpper - LogicalLower;

			ScaledLower = scaleFunc(logicalLower);
			ScaledUpper = scaleFunc(logicalUpper);
			ScaledLength = ScaledUpper - ScaledLower;

			VisualLength = visualLength;
			VisualStretchFactor = VisualLength / ScaledLength;

			Correction = correction;
			InscaleFunc = scaleFunc;
			UnscaleFunc =
				InscaleFunc.Is(Log) ? Exp :
				InscaleFunc.Is(Log2) ? _2Pow :
				InscaleFunc.Is(Sqrt) ? Pow2 :
				Lineal;
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
	}
}
