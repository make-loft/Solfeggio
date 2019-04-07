using Ace;
using System;
using Rainbow;

namespace Solfeggio.Presenters
{
	public class PianoKey
	{
		public int NoteNumber { get; set; }
		public string NoteName { get; set; }
		public double Magnitude { get; set; }
		public double LowerFrequancy { get; set; }
		public double UpperFrequancy { get; set; }
		public double EthalonFrequancy { get; set; }
		public double DeltaFrequancy => Peak.Frequancy - EthalonFrequancy;
		public Bin Peak { get; set; }
		public int Hits { get; set; }

		public static PianoKey Construct(double[] oktaveNotes, int noteNumber, int oktaveNumber, string note)
		{
			var lowNoteNumber = 0;
			var topNoteNumber = oktaveNotes.Length - 1;

			var activeNote = oktaveNotes[noteNumber];
			var lowNote = noteNumber.Is(lowNoteNumber) ? oktaveNotes[topNoteNumber] / 2 : oktaveNotes[noteNumber - 1];
			var topNote = noteNumber.Is(topNoteNumber) ? oktaveNotes[lowNoteNumber] * 2 : oktaveNotes[noteNumber + 1];

			var ethalonOffset = Math.Log(activeNote, 2d);
			var lowOffset = (Math.Log(lowNote, 2d) + ethalonOffset) / 2d;
			var topOffset = (ethalonOffset + Math.Log(topNote, 2d)) / 2d;

			var lowFrequency = Math.Pow(2d, lowOffset);
			var topFrequency = Math.Pow(2d, topOffset);
			var ethalonFrequency = activeNote; // Math.Pow(2d, ethalonOffset)

			return new PianoKey
			{
				NoteNumber = noteNumber,
				NoteName = note + oktaveNumber,
				LowerFrequancy = lowFrequency,
				UpperFrequancy = topFrequency,
				EthalonFrequancy = ethalonFrequency,
			};
		}
	}
}
