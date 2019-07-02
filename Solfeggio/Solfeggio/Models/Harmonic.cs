using System;
using System.Collections.Generic;
using Ace;
using Rainbow;
using static System.Math;
using static Rainbow.HarmonicFuncs;

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
		[DataMember] public Basis[] BasisFuncs { get; } = { Sin, Triangle, Sawtooth, Rectangle };

		[DataMember] public Basis BasisFunc { get; set; } = Sin;
		[DataMember] public double Magnitude { get; set; } = 0.3d;
		[DataMember] public double Frequency { get; set; } = 440d;
		[DataMember] public double PhaseShift { get; set; } = 0d;
		[DataMember] public double Gap { get; set; } = 0d;
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
				var value = offset + PhaseShift;
				if (Gap.Is(0d))
					yield return 2d * Magnitude * BasisFunc(value);
				else
				{
					var hit = (int)(Abs(value / Pi.Double) % Gap) == 0d;
					yield return hit ^ Gap > 0d ? 0d : 2d * Magnitude * BasisFunc(value);
				}
			}
		}
	}
}
