using Ace;
using Rainbow;

using Solfeggio.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using static Rainbow.ScaleFuncs;

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

		public static SmartRange Create(double lower, double upper) => new()
		{
			Lower = lower,
			Upper = upper
		};

		static double Limit(double value, double lowerLimit, double upperLimit) =>
			value < lowerLimit ? lowerLimit :
			value > upperLimit ? upperLimit :
			value;
	}

	[DataContract]
	public class Bandwidth
	{
		public static readonly Projection[] AllScaleFuncs =
			{ Lineal, Log2, Log, Exp, Sqrt };

		[DataMember] public SmartRange Limit { get; set; }
		[DataMember] public SmartRange Threshold { get; set; }
		[DataMember] public Projection[] VisualScaleFuncs { get; set; } = AllScaleFuncs;
		[DataMember] public Projection VisualScaleFunc { get; set; } = Lineal;

		public void LimitThreshold()
		{
			var l = Limit;
			var t = Threshold;

			if (t.Lower < l.Lower) t.Lower = l.Lower;
			if (t.Lower > l.Upper) t.Lower = l.Upper;

			if (t.Upper > l.Upper) t.Upper = l.Upper;
			if (t.Upper < l.Lower) t.Upper = l.Lower;
		}

		public void ShiftThreshold(double from, double till) =>
			TransformThreshold(from, till, true);
		public void ScaleThreshold(double from, double till) =>
			TransformThreshold(from, till, false);

		public void TransformThreshold(double from, double till, bool shift)
		{
			if (VisualScaleFunc.Is(Lineal))
			{
				var offset = shift ? from - till : till - from;

				Threshold.Lower += shift ? +offset : -offset;
				Threshold.Upper += offset;
			}
			else
			{
				var scale = shift ? from / till : till / from;

				Threshold.Lower *= shift ? scale : (1 / scale);
				Threshold.Upper *= scale;
			}

			if (Threshold.Lower < Limit.Lower)
				Threshold.Lower = Limit.Lower;

			if (Threshold.Upper > Limit.Upper)
				Threshold.Upper = Limit.Upper;

			if (Threshold.Length < Limit.Length / 1024d && Limit.Lower < Limit.Upper)
			{
				Threshold.Lower = Limit.Lower;
				Threshold.Upper = Limit.Upper;
			}
		}
	}

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

	[DataContract]
	public class MusicalOptions : ContextObject
	{

		[DataMember] public double[] PitchStandards { get; } = { 415d, 420d, 432d, 435d, 440d, 444d };
		public static double DefaultPitchStandard = 440d;

		[DataMember]
		public double ActivePitchStandard
		{
			get => Get(() => ActivePitchStandard, DefaultPitchStandard);
			set
			{
				Set(() => ActivePitchStandard, value);
				BaseOktaveFrequencySet = GetBaseOktaveFrequencySet(value);
				PianoKeys = EnumeratePianoKeys().ToList();
				EvokePropertyChanged(nameof(PianoKeys));
			}
		}

		public Dictionary<string, string[]> Notations { get; set; } = new()
		{
			{ "Dies", Notes.Select(n => n.Split('|')[0]).ToArray() },
			{ "Bemole", Notes.Select(n => n.Split('|')[1]).ToArray() },
			{ "Combined", Notes }
		};

		[DataMember] public string ActiveNotation { get; set; }

		protected static readonly string[] Notes =
			{"C |С ","C♯|D♭","D |D ","D♯|E♭","E |E ","F |F ","F♯|G♭","G |G ","G♯|A♭","A |A ","A♯|B♭","B |B "};

		public static readonly bool[] Tones = Notes.Select(n => n.Contains("♯|").Not()).ToArray();
		public static Brush[] OktaveBrushes() =>
			Tones.Select(t => t ? AppPalette.FullToneKeyBrush : AppPalette.HalfToneKeyBrush).Cast<Brush>().ToArray();

		public static double HalfTonesCount => Notes.Length;
		private static double GetBaseFrequency(double pitchStandard) => pitchStandard / 16d;
		private static double GetHalftoneStep(double halfTonesCount) => Math.Pow(2d, 1d / halfTonesCount);

		private static double[] GetBaseOktaveFrequencySet(double pitchStandard) =>
			GetBaseOktaveFrequencySet(GetBaseFrequency(pitchStandard), GetHalftoneStep(HalfTonesCount));

		private static double[] GetBaseOktaveFrequencySet(double baseFrequency, double halftoneStep) =>
			Enumerable.Range(-9, 12).Select(dt => baseFrequency * Math.Pow(halftoneStep, dt)).ToArray();

		public double[] BaseOktaveFrequencySet = GetBaseOktaveFrequencySet(DefaultPitchStandard);

		public List<PianoKey> PianoKeys { get; private set; }

		public IEnumerable<PianoKey> EnumeratePianoKeys()
		{
			var noteNames = Notations.TryGetValue(ActiveNotation ?? "", out var names)
				? names
				: Notations[ActiveNotation = Notations.First().Key];

			var oktavesCount = HalfTonesCount + 1;

			for (var oktaveNumber = 1; oktaveNumber < oktavesCount; oktaveNumber++)
			{
				var oktaveNotes = BaseOktaveFrequencySet.Select(d => d * Math.Pow(2, oktaveNumber)).ToArray();
				var notesCount = oktaveNotes.Length;
				for (var noteIndex = 0; noteIndex < notesCount; noteIndex++)
					yield return PianoKey.Construct(oktaveNotes, noteIndex, oktaveNumber, noteNames[noteIndex]);
			}
		}
	}
}
