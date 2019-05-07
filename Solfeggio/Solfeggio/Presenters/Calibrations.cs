using Ace;
using Rainbow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

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

		public static SmartRange Create(double lower, double upper) => new SmartRange
		{
			Lower = lower,
			Upper = upper
		};
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

	[DataContract]
	public class SpectralOptions
	{
		[DataMember]
		public Bandwidth Frequency { get; set; } = new Bandwidth
		{
			Limit = SmartRange.Create(10d, 22000d),
			Threshold = SmartRange.Create(20d, 3000d),
			VisualScaleFunc = ScaleFuncs.Log2,
			NumericFormat = "F0",
		};

		[DataMember]
		public Bandwidth Magnitude { get; set; } = new Bandwidth
		{
			Limit = SmartRange.Create(0.00d, 1d),
			Threshold = SmartRange.Create(0.00d, 1d),
			IsVisible = true
		};

		[DataMember]
		public Bandwidth Phase { get; set; } = new Bandwidth
		{
			Limit = SmartRange.Create(-Pi.Single, +Pi.Single),
			Threshold = SmartRange.Create(-Pi.Single, +Pi.Single),
		};
	}

	[DataContract]
	public class FrameOptions
	{
		[DataMember]
		public Bandwidth Level { get; set; } = new Bandwidth
		{
			Limit = SmartRange.Create(-1d, +1d),
			Threshold = SmartRange.Create(-1d, +1d),
			VisualScaleFunc = ScaleFuncs.Lineal,
		};

		[DataMember]
		public Bandwidth Offset { get; set; } = new Bandwidth
		{
			Limit = SmartRange.Create(+0d, +1d),
			Threshold = SmartRange.Create(+0d, +1d),
			VisualScaleFunc = ScaleFuncs.Lineal,
		};
	}

	[DataContract]
	public class MusicalOptions : ContextObject
	{

		[DataMember] public double[] PitchStandards { get; } = new[] { 415d, 420d, 432d, 435d, 440d, 444d };
		public static double DefaultPitchStandard = 440d;

		[DataMember]
		public double ActivePitchStandard
		{
			get => Get(() => ActivePitchStandard, DefaultPitchStandard);
			set
			{
				Set(() => ActivePitchStandard, value);
				BaseOktaveFrequencySet = GetBaseOktaveFrequencySet(value);
			}
		}

		public Dictionary<string, string[]> Notations { get; set; } = new Dictionary<string, string[]>
		{
			{ "Dies", Notes.Select(n => n.Split('|')[0]).ToArray() },
			{ "Bemole", Notes.Select(n => n.Split('|')[1]).ToArray() },
			{ "Combined", Notes }
		};

		[DataMember] public KeyValuePair<string, string[]> ActiveNotation { get; set; }

		protected static readonly string[] Notes =
			{"C |С ","C♯|D♭","D |D ","D♯|E♭","E |E ","F |F ","F♯|G♭","G |G ","G♯|A♭","A |A ","A♯|B♭","B |B "};

		public static readonly bool[] Tones = Notes.Select(n => n.Contains("♯|").Not()).ToArray();
		public static readonly Brush[] OktaveBrushes =
			Tones.Select(t => t ? AppPalette.FullToneKeyBrush : AppPalette.HalfToneKeyBrush).Cast<Brush>().ToArray();

		public static double HalfTonesCount => Notes.Length;
		private static double GetBaseFrequency(double pitchStandard) => pitchStandard / 16d;
		private static double GetHalftoneStep(double halfTonesCount) => Math.Pow(2d, 1d / halfTonesCount);

		private static double[] GetBaseOktaveFrequencySet(double pitchStandard) =>
			GetBaseOktaveFrequencySet(GetBaseFrequency(pitchStandard), GetHalftoneStep(HalfTonesCount));

		private static double[] GetBaseOktaveFrequencySet(double baseFrequency, double halftoneStep) =>
			Enumerable.Range(-9, 12).Select(dt => baseFrequency * Math.Pow(halftoneStep, dt)).ToArray();

		public double[] BaseOktaveFrequencySet = GetBaseOktaveFrequencySet(DefaultPitchStandard);
	}
}
