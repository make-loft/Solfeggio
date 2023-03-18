using Ace.Extensions;

using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Media3D;

using static System.Windows.Input.Key;

namespace Solfeggio.Extensions
{
	public static class CameraExtensions
	{
		public static TCamera MoveBy<TCamera>(this TCamera camera, IList<Key> pattern, double step) where TCamera : ProjectionCamera =>
			camera.Move(camera.GetDirectionAxis(pattern), step);

		public static Vector3D GetDirectionAxis<TCamera>(this TCamera camera, IList<Key> pattern) where TCamera : ProjectionCamera =>
			pattern.Contains(W, A, D) ? camera.GetYawAxis() :
			pattern.Contains(S, A, D) ? -camera.GetYawAxis() :
			pattern.Contains(A, D) || pattern.Contains(W, S) ? new() :
			pattern.Contains(W, A) ? (camera.GetRollAxis() + camera.GetPitchAxis()) / Math.Sqrt(2) :
			pattern.Contains(S, A) ? -(camera.GetRollAxis() - camera.GetPitchAxis()) / Math.Sqrt(2) :
			pattern.Contains(W, D) ? (camera.GetRollAxis() - camera.GetPitchAxis()) / Math.Sqrt(2) :
			pattern.Contains(S, D) ? -(camera.GetRollAxis() + camera.GetPitchAxis()) / Math.Sqrt(2) :
			pattern.Contains(A) ? camera.GetPitchAxis() :
			pattern.Contains(D) ? -camera.GetPitchAxis() :
			pattern.Contains(W) ? camera.GetRollAxis() :
			pattern.Contains(S) ? -camera.GetRollAxis() :
			new();

		public static bool Contains(this IList<Key> pattern, Key a, Key b, Key c) => pattern.Contains(a, b) && pattern.Contains(c);
		public static bool Contains(this IList<Key> pattern, Key a, Key b) => pattern.Contains(a) && pattern.Contains(b);

		public static TCamera RotateBy<TCamera>(this TCamera camera, Key key, double angle) where TCamera : ProjectionCamera => key switch
		{
			Left => camera.Rotate(camera.GetYawAxis(), +angle),
			Right => camera.Rotate(camera.GetYawAxis(), -angle),
			Down => camera.Rotate(camera.GetPitchAxis(), +angle),
			Up => camera.Rotate(camera.GetPitchAxis(), -angle),

			_ => camera
		};
	}
}
