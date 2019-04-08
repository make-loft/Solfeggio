using Ace;
using Rainbow;

namespace Solfeggio.Presenters
{
	[DataContract]
	public class SmartRange<T> : SmartObject where T : struct
	{
		T _lower, _upper;

		[DataMember]
		public T Upper
		{
			get => _upper;
			set => value.To(out _upper).Notify(this).Notify(this, nameof(Length));
		}

		[DataMember]
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

	[DataContract]
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
			var logicalLength = (uLogicalOffset - lLogicalOffset);
			logicalLength = logicalLength == 0d ? 1d : logicalLength;

			visualLowerDecrementOffset = visualLength * lLogicalOffset / logicalLength;
			visualUpperDecrementOffset = visualLength * uLogicalOffset / logicalLength;
			visualLengthStretchFactor = uLogicalOffset == 0d
				? visualUpperDecrementOffset
				: visualUpperDecrementOffset / uLogicalOffset;
		}
	}

	[DataContract]
	public class Bandwidth
	{
		public static readonly ScaleFunc[] AllScaleFuncs = new ScaleFunc[]
		{ ScaleFuncs.Lineal, ScaleFuncs.Log2, ScaleFuncs.Log, ScaleFuncs.Exp, ScaleFuncs._20Log10 };

		[DataMember] public SmartRange Limit { get; set; }
		[DataMember] public SmartRange Threshold { get; set; }
		[DataMember] public ScaleFunc VisualScaleFunc { get; set; } = ScaleFuncs.Lineal;
		[DataMember] public ScaleFunc[] VisualScaleFuncs { get; set; } = AllScaleFuncs;
		[DataMember] public bool IsVisible { get; set; }
		[DataMember] public string NumericFormat { get; set; } = "F2";
	}

	[DataContract]
	public class VisualStates
	{
		[DataMember] public bool Wave { get; set; } = false;
		[DataMember] public bool Dominants { get; set; } = true;
		[DataMember] public bool ActualFrequncy { get; set; } = true;
		[DataMember] public bool ActualMagnitude { get; set; } = true;
		[DataMember] public bool EthalonFrequncy { get; set; } = true;
		[DataMember] public bool Notes { get; set; } = true;
		[DataMember] public bool NotesGrid { get; set; } = false;
	}
}
