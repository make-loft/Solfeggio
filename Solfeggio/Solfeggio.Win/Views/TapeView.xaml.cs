using Ace;
using Ace.Zest.Extensions;

using Solfeggio.Extensions;
using Solfeggio.ViewModels;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Solfeggio.Views
{
	public partial class TapeView
	{
		public TapeView() => InitializeComponent();

		public void Draw(IList<Point> geometry)
		{
			geo.TriangleIndices.Clear();
			geo.Positions.Clear();

			wave.TriangleIndices.Clear();
			wave.Positions.Clear();

			var radius = RadiusSlider.Value;
			var depth = DepthSlider.Value;
			var thin = ThinSlider.Value;
			int k = 0;
			for (var n = 0; n < geometry.Count; n++)
			{
				var x = radius * geometry[n].X;
				var y = radius * geometry[n].Y;
				var z = depth * n / geometry.Count;

				geo.Positions.Add(new(x, y, z + thin));
				geo.Positions.Add(new(x, y, z - thin));

				wave.Positions.Add(new(x, 0d, z + thin));
				wave.Positions.Add(new(x, 0d, z - thin));

				AddTriangle(geo, k);
				AddTriangle(wave, k);

				k++;
			}
		}

		static void AddTriangle(MeshGeometry3D geometry, int k)
		{
			if (k > 0)
			{
				geometry.TriangleIndices.Add(k);
				geometry.TriangleIndices.Add(k - 1);
				geometry.TriangleIndices.Add(k + 1);
				geometry.TriangleIndices.Add(k);
			}

			geometry.TriangleIndices.Add(k);
			geometry.TriangleIndices.Add(k + 1);
		}

		private void SwitchPause() => Store.Get<ProcessingManager>().To(out var m).IsPaused = m.IsPaused.Not();

		readonly Key[] HandleKeys = { Key.Space, Key.W, Key.S, Key.D, Key.A, Key.Left, Key.Right, Key.Up, Key.Down };
		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var key = e.Key;
			e.Handled = HandleKeys.Contains(key);
			if (e.Key is Key.Space)
				SwitchPause();
		}

		private async void Window_Activated(object sender, System.EventArgs e)
		{
			while (IsActive)
			{
				var step = PerspectiveCamera.FieldOfView / 360d;
				var angle = PerspectiveCamera.FieldOfView / 45d;

				NavigationKeys.Where(Keyboard.IsKeyDown)
					.ForEach(key => PerspectiveCamera.MoveBy(step).RotateBy(key, angle));

				NavigationKeys.Where(Keyboard.IsKeyDown)
					.ForEach(key => OrthographicCamera.MoveBy(step).RotateBy(key, angle));

				var scale =
					Keyboard.IsKeyDown(Key.S) ? 1 * 1.05 :
					Keyboard.IsKeyDown(Key.W) ? 1 / 1.05 :
					1d;

				if (scale != 1d)
				{
					ScaleTransform.ScaleX *= scale;
					ScaleTransform.ScaleY *= scale;
					ScaleTransform.ScaleZ *= scale;
				}

				await Task.Delay(16);
			}
		}

		readonly Key[] NavigationKeys = { Key.Down, Key.Up, Key.Left, Key.Right, Key.W, Key.S, Key.A, Key.D };

		private void Window_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			SwitchPause();
		}

		Point from;
		private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			var till = e.GetPosition(sender as IInputElement);
			double dx = till.X - from.X;
			double dy = till.Y - from.Y;
			from = till;

			var distance = dx * dx + dy * dy;
			if (distance <= 0)
				return;

			if (e.MouseDevice.LeftButton is MouseButtonState.Pressed)
			{
				var angle = (distance / PerspectiveCamera.FieldOfView) % 45;
				PerspectiveCamera.Rotate(new(dy, -dx, 0d), angle);
				OrthographicCamera.Rotate(new(dy, -dx, 0d), angle);
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e) => MessageBox.Show
		(
			"Move:\n" +
			"W,S,D,A and combination\n\n" +
			"Rise:\n" +
			"W+D+A\n\n" +
			"Fall:\n" +
			"S+D+A\n\n" +
			"Orientation:\n" +
			"mouse or Up,Down,Left,Right Arrows\n",
			"3D FLY!"
		);
	}
}
