using System;
using System.Collections.Generic;
using Ace;
using Rainbow;

namespace Solfeggio.Models
{
	[DataContract]
	public class Harmonic
	{
		[DataMember] public Func<double, double> Signal { get; set; } = v => Math.Sin(v);
		[DataMember] public double Magnitude { get; set; } = 0.3d;
		[DataMember] public double Frequency { get; set; } = 440d;
		[DataMember] public double Phase { get; set; } = 0d;
		[DataMember] public bool IsEnabled { get; set; } = true;
		[DataMember] public bool IsStatic { get; set; } = false;

		private double offset;

		public IEnumerable<double> EnumerateBins(double sampleRate, bool isStatic)
		{
			var step = Frequency * Pi.Double / sampleRate;
			for (offset = IsStatic || isStatic ? 0d : offset; ; offset += step)
			{
				yield return 2d * Magnitude * Signal(offset + Phase);
			}
		}
	}
}
