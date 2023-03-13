using Ace;
using System;
using Rainbow;
using System.Collections.Generic;

namespace Solfeggio.Models
{
	public class PianoKey
	{
		public int NoteNumber { get; set; }
		public string NoteName { get; set; }
		public double Magnitude { get; set; }
		public double LowerFrequency { get; set; }
		public double UpperFrequency { get; set; }
		public double EthalonFrequency { get; set; }
		public double OffsetFrequency => Harmonic.Frequency - EthalonFrequency;
		public Bin Harmonic { get; set; }
		public List<Bin> Peaks { get; set; } = new();

		public double RelativeOpacity { get; set; } = 1d;

		public override string ToString() => $"{EthalonFrequency:F2} Hz • {NoteName}";

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

			return new()
			{
				NoteNumber = noteNumber,
				NoteName = note + oktaveNumber,
				LowerFrequency = lowFrequency,
				UpperFrequency = topFrequency,
				EthalonFrequency = ethalonFrequency,
			};
		}
	}
}
