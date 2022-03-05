using Ace;
using Ace.Zest.Extensions;

using Rainbow;

using Solfeggio.Extensions;
using Solfeggio.Presenters;
using Solfeggio.ViewModels;

using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Solfeggio.Views
{
	public partial class GeometryView
	{
		public GeometryView() => InitializeComponent();

		Bin[] _peaks;
		int _sampleSize;
		double _sampleRate;

		public void DrawFlower(Bin[] peaks, int sampleSize, double sampleRate)
		{
			var geometry = MusicalPresenter.DrawGeometry(peaks, sampleSize, sampleRate, ActualWidth / 2d, ActualHeight / 2d).ToList();
			var polyline = Polyline_Geometry;
			polyline.Points.Clear();
			geometry.ForEach(polyline.Points.Add);
		}

		ProcessingManager processingManager = Store.Get<ProcessingManager>();

		public void Draw(Bin[] peaks, int sampleSize, double sampleRate)
		{
			if (processingManager.IsPaused is false)
			{
				_peaks = peaks;
				_sampleSize = sampleSize;
				_sampleRate = sampleRate;
			}

			if (TabControl.SelectedIndex == 0)
				DrawFlower(_peaks, _sampleSize, _sampleRate);

			if (TabControl.SelectedIndex == 1)
				DrawTape(_peaks, _sampleSize, _sampleRate);
		}
		public void DrawTape(Bin[] peaks, int sampleSize, double sampleRate)
		{
			var geometry = MusicalPresenter.DrawGeometry(peaks, sampleSize, sampleRate).ToList();
			geo.TriangleIndices.Clear();
			geo.Positions.Clear();

			var depth = DepthSlider.Value;
			var thin = ThinSlider.Value / 128;
			int k = 0;
			for (var n = 0; n < geometry.Count; n++)
			{
				var x = geometry[n].X;
				var y = geometry[n].Y;
				var z = depth * n / geometry.Count;

				geo.Positions.Add(new(x, y, z + thin));
				geo.Positions.Add(new(x, y, z - thin));

				if (k > 0)
				{
					geo.TriangleIndices.Add(k);
					geo.TriangleIndices.Add(k - 1);
					geo.TriangleIndices.Add(k + 1);
					geo.TriangleIndices.Add(k);
				}

				geo.TriangleIndices.Add(k);
				geo.TriangleIndices.Add(k + 1);

				k++;
			}
		}

		private void SwitchPause() => processingManager.IsPaused = processingManager.IsPaused.Not();

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
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
	}
}
