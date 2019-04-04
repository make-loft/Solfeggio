using Ace;
using Rainbow;

namespace Solfeggio.Presenters
{
	public class SmartRange<T> : SmartObject where T : struct
	{
		T _lower, _upper;
		public T Upper
		{
			get => _upper;
			set => value.To(out _upper).Notify(this).Notify(this, nameof(Length));
		}

		public T Lower
		{
			get => _lower;
			set => value.To(out _lower).Notify(this).Notify(this, nameof(Length));
		}

		public virtual T Length { get; }

		public void Deconstruct(out T lower, out T upper)
		{
			lower = _lower;
			upper = _upper;
		}
	}

	public class SmartRange : SmartRange<double>
	{
		public override double Length => Upper - Lower;

		public void Deconstruct(double visualLength, ScaleFunc visualScaleFunc,
			out double logicalLowerValue, out double logicalUpperValue,
			out double visualLowerDecrementOffset, out double visualUpperDecrementOffset,
			out double visualLengthStretchFactor)
		{
			Deconstruct(out logicalLowerValue, out logicalUpperValue);

			var lLogicalOffset = visualScaleFunc(logicalLowerValue);
			var uLogicalOffset = visualScaleFunc(logicalUpperValue);

			visualLowerDecrementOffset = visualLength * lLogicalOffset / (uLogicalOffset - lLogicalOffset);
			visualUpperDecrementOffset = visualLength * uLogicalOffset / (uLogicalOffset - lLogicalOffset);
			visualLengthStretchFactor = visualUpperDecrementOffset / uLogicalOffset;
		}
	}

	[DataContract]
	public class Bandwidth
	{
		[DataMember] public SmartRange Limit { get; set; }
		[DataMember] public SmartRange Threshold { get; set; }
		[DataMember] public ScaleFunc VisualScaleFunc { get; set; } = ScaleFuncs.Lineal;
		[DataMember] public ScaleFunc[] VisualScaleFuncs { get; set; }
	}

	public class NumericFormatting
	{
		[DataMember] public string Frequancy { get; set; } = "F0";
		[DataMember] public string Magnitude { get; set; } = "F2";
		[DataMember] public string Phase { get; set; } = "F0";
	}

	[DataContract]
	public class VisualStates
	{
		[DataMember] public bool Wave { get; set; } = false;
		[DataMember] public bool PhaseSpectrum { get; set; } = false;
		[DataMember] public bool MagnitudeSpectrum { get; set; } = true;
		[DataMember] public bool Dominants { get; set; } = true;
		[DataMember] public bool ActualFrequncy { get; set; } = true;
		[DataMember] public bool ActualMagnitude { get; set; } = true;
		[DataMember] public bool EthalonFrequncy { get; set; } = true;
		[DataMember] public bool Notes { get; set; } = true;
		[DataMember] public bool NotesGrid { get; set; } = false;
		[DataMember] public bool DiscreteGrid { get; set; } = false;
	}
}
