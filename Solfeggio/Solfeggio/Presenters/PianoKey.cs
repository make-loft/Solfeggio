using Ace;
using System;
using Rainbow;
using System.Collections.Generic;

namespace Solfeggio.Models
{
	public class PianoKey
	{
		public int NoteNumber { get; private set; }
		public string NoteName { get; private set; }
		public double LowerFrequency { get; private set; }
		public double UpperFrequency { get; private set; }
		public double EthalonFrequency { get; private set; }
		public double OffsetFrequency => Harmonic.Frequency - EthalonFrequency;
		public double LowerValue { get; private set; }
		public double UpperValue { get; private set; }
		public double EthalonValue { get; private set; }
		public double OffsetValue => Math.Log(Harmonic.Frequency, 2d) - EthalonValue;
		public double RelativeOffset => 2d * OffsetValue / (UpperValue - LowerValue);

		public double Magnitude { get; set; }
		public Bin Harmonic { get; set; }
		public List<Bin> Peaks { get; set; } = new();

		public double RelativeOpacity { get; set; } = 1d;

		public override string ToString() => $"{EthalonFrequency:F2} Hz • {NoteName}";

		public static PianoKey Construct(double[] oktaveNotes, int noteNumber, int oktaveNumber, string note)
		{
			var lowNoteNumber = 0;
			var topNoteNumber = oktaveNotes.Length - 1;

			var ethalonFrequency = oktaveNotes[noteNumber];
			var lowerNote = noteNumber.Is(lowNoteNumber) ? oktaveNotes[topNoteNumber] / 2 : oktaveNotes[noteNumber - 1];
			var upperNote = noteNumber.Is(topNoteNumber) ? oktaveNotes[lowNoteNumber] * 2 : oktaveNotes[noteNumber + 1];

			var ethalonValue = Math.Log(ethalonFrequency, 2d);
			var lowerValue = (Math.Log(lowerNote, 2d) + ethalonValue) / 2d;
			var upperValue = (ethalonValue + Math.Log(upperNote, 2d)) / 2d;

			var lowerFrequency = Math.Pow(2d, lowerValue);
			var upperFrequency = Math.Pow(2d, upperValue);

			return new()
			{
				NoteNumber = noteNumber,
				NoteName = note + oktaveNumber,
				LowerFrequency = lowerFrequency,
				UpperFrequency = upperFrequency,
				EthalonFrequency = ethalonFrequency,

				LowerValue = lowerValue,
				UpperValue = upperValue,
				EthalonValue = ethalonValue,
			};
		}
	}
}
