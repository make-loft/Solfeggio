using Ace;

using Rainbow;

using System;
using System.Collections.Generic;
using System.Windows;

namespace Solfeggio.Presenters
{
	public class GeometryPresenter
	{
		public static IEnumerable<Point> Draw(
			IList<Bin> peaks, int sampleSize, double sampleRate,
			double approximation = 1d, double spiralStep = 0d, double phaseAngle = 0d
			)
		{
			if (peaks.Count.Is(0))
				yield break;

			var spiralScale = 1d;
			var pointsCount = (int)(sampleSize / approximation);

			for (var i = 0; i < pointsCount; i++, spiralScale -= spiralStep)
			{
				var a = 0d;
				var b = 0d;

				for (var j = 0; j < peaks.Count; j++)
				{
					var peak = peaks[j];
					var w = Pi.Double * peak.Frequency;
					var t = 2d * i / sampleRate * approximation;
					var phase = peak.Phase + w * t + phaseAngle;
					var magnitude = peak.Magnitude * spiralScale;

					a += magnitude * Math.Cos(phase);
					b += magnitude * Math.Sin(phase);
				}

				yield return new(a, b);
			}
		}
	}
}
