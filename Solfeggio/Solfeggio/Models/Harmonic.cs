using System;
using System.Collections.Generic;
using Ace;
using Rainbow;

namespace Solfeggio.Models
{
	[DataContract]
	public partial class Harmonic
	{
		public delegate double Basis(double v);
		[DataMember] public Basis[] BasisFuncs { get; } = { Math.Sin, Math.Cos, Math.Tan, Math.Sqrt };

		[DataMember] public Basis BasisFunc { get; set; } = Math.Sin;
		[DataMember] public double Magnitude { get; set; } = 0.3d;
		[DataMember] public double Frequency { get; set; } = 440d;
		[DataMember] public double PhaseShift { get; set; } = 0d;
		[DataMember] public bool IsEnabled { get; set; } = true;
		[DataMember] public bool IsStatic { get; set; } = false;

		private double offset;

		public IEnumerable<double> EnumerateBins(double sampleRate, bool isStatic)
		{
			var step = Frequency * Pi.Double / sampleRate;
			for (offset = IsStatic || isStatic ? 0d : offset; ; offset += step)
			{
				yield return 2d * Magnitude * BasisFunc(offset + PhaseShift);
			}
		}
	}
}
