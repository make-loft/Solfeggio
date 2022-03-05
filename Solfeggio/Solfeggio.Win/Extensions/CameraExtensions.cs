using Ace.Zest.Extensions;

using System;
using System.Windows.Input;
using System.Windows.Media.Media3D;

using static System.Windows.Input.Key;
using static System.Windows.Input.Keyboard;

namespace Solfeggio.Extensions
{
	public static class CameraExtensions
	{
		public static TCamera MoveBy<TCamera>(this TCamera camera, double step) where TCamera : ProjectionCamera =>
			camera.Move(camera.GetDirectionAxis(), step);

		public static Vector3D GetDirectionAxis<TCamera>(this TCamera camera) where TCamera : ProjectionCamera =>
			IsKeysDown(W, A, D) ? camera.GetYawAxis() :
			IsKeysDown(S, A, D) ? -camera.GetYawAxis() :
			IsKeysDown(A, D) || IsKeysDown(W, S) ? new() :
			IsKeysDown(W, A) ? (camera.GetRollAxis() + camera.GetPitchAxis()) / Math.Sqrt(2) :
			IsKeysDown(W, D) ? (camera.GetRollAxis() - camera.GetPitchAxis()) / Math.Sqrt(2) :
			IsKeysDown(S, A) ? -(camera.GetRollAxis() - camera.GetPitchAxis()) / Math.Sqrt(2) :
			IsKeysDown(S, D) ? -(camera.GetRollAxis() + camera.GetPitchAxis()) / Math.Sqrt(2) :
			IsKeysDown(A) ? camera.GetPitchAxis() :
			IsKeysDown(D) ? -camera.GetPitchAxis() :
			IsKeysDown(W) ? camera.GetRollAxis() :
			IsKeysDown(S) ? -camera.GetRollAxis() :
			new();

		static bool IsKeysDown(Key a, Key b, Key c) => IsKeysDown(a, b) && IsKeyDown(c);
		static bool IsKeysDown(Key a, Key b) => IsKeyDown(a) && IsKeyDown(b);
		static bool IsKeysDown(Key a) => IsKeyDown(a);

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
