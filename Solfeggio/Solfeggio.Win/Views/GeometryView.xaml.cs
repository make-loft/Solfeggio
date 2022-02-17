using Ace.Zest.Extensions;

using Rainbow;

using Solfeggio.Presenters;

using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Solfeggio.Views
{
	public partial class GeometryView
	{
		public GeometryView() => InitializeComponent();

		public bool IsPaused { get; set; }

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

		public void Draw(Bin[] peaks, int sampleSize, double sampleRate)
		{
			if (IsPaused is false)
			{
				_peaks = peaks;
				_sampleSize = sampleSize;
				_sampleRate = sampleRate;
			}

			if (TabControl.SelectedIndex == 0)
				DrawFlower(_peaks, _sampleSize, _sampleRate);

			if (TabControl.SelectedIndex == 1)
				DrawStripe(_peaks, _sampleSize, _sampleRate);
		}
		public void DrawStripe(Bin[] peaks, int sampleSize, double sampleRate)
		{
			var geometry = MusicalPresenter.DrawGeometry(peaks, sampleSize, sampleRate).ToList();
			geo.TriangleIndices.Clear();
			geo.Positions.Clear();

			int k = 0;
			for (var n = 0; n < geometry.Count; n++)
			{
				var x = geometry[n].X;
				var y = geometry[n].Y;
				var z = 32d * n / geometry.Count;
				var thin = 0.05d / 2;

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

		private void SwitchPause() => IsPaused = !IsPaused;

	

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;
			if (e.Key is Key.Space)
				SwitchPause();

			Camera.MoveBy(e.Key).RotateBy(e.Key);
		}

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
				var angle = (distance / Camera.FieldOfView) % 45;
				Camera.Rotate(new(dy, -dx, 0d), angle);
			}
		}
	}
}
