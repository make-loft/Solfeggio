using System;
using System.Collections.Generic;
using Ace;
using Rainbow;

namespace Solfeggio.Models
{
	public enum PhaseMode
	{
		Flow, Loop
	}

	[DataContract]
	public partial class Harmonic
	{
		public delegate double Basis(double v);
		[DataMember] public Basis[] BasisFuncs { get; } = { Math.Sin, Math.Cos, Math.Tan, Math.Sqrt };

		[DataMember] public Basis BasisFunc { get; set; } = Math.Sin;
		[DataMember] public double Magnitude { get; set; } = 0.3d;
		[DataMember] public double Frequency { get; set; } = 440d;
		[DataMember] public double PhaseShift { get; set; } = 0d;
		[DataMember] public PhaseMode PhaseMode { get; set; } = PhaseMode.Flow;
		[DataMember] public bool IsEnabled { get; set; } = true;

		private double offset;

		public IEnumerable<double> EnumerateBins(double sampleRate, bool globalLoop = false)
		{
			var step = Frequency * Pi.Double / sampleRate;
			offset = PhaseMode.Is(PhaseMode.Loop) || globalLoop ? -step : offset;
			while (true)
			{
				offset += step;
				yield return 2d * Magnitude * BasisFunc(offset + PhaseShift);
			}
		}
	}
}
