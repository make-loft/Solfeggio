using Ace;

using Solfeggio.Models;

using System;
using System.Collections.Generic;
using System.Linq;
#if NETSTANDARD
using Xamarin.Forms;
#else
using System.Windows.Media;
#endif

namespace Solfeggio.Presenters.Options
{
	[DataContract]
	public class MusicalOptions : ContextObject, IExposable
	{
		public MusicalOptions() => Expose();

		public void Expose()
		{
			this[() => ActivePitchStandard].Changed += (o, e) =>
			{
				BaseOktaveFrequencySet = GetBaseOktaveFrequencySet(ActivePitchStandard);
				PianoKeys = EnumeratePianoKeys().ToList();
				EvokePropertyChanged(nameof(PianoKeys));
			};

			EvokePropertyChanged(nameof(ActivePitchStandard));
		}

		public static double DefaultPitchStandard = 440d;
		[DataMember]
		public double[] PitchStandards { get; } =
			{ 415d, 420d, 432d, 435d, 440d, 444d };

		[DataMember]
		public double ActivePitchStandard
		{
			get => Get(() => ActivePitchStandard, DefaultPitchStandard);
			set => Set(() => ActivePitchStandard, value);
		}

		public Dictionary<string, string[]> NotationToNotes { get; set; } = new()
		{
			{ "Dies", Notes.Select(n => n.Split('|')[0]).ToArray() },
			{ "Bemole", Notes.Select(n => n.Split('|')[1]).ToArray() },
			{ "Combined", Notes }
		};

		public string[] Notations => NotationToNotes.Keys.ToArray();
		[DataMember] public string ActiveNotation
		{
			get => Get(() => ActiveNotation);
			set => Set(() => ActiveNotation, value);
		}

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
			var noteNames = NotationToNotes.TryGetValue(ActiveNotation ?? "", out var names)
				? names
				: NotationToNotes[ActiveNotation = NotationToNotes.First().Key];

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
