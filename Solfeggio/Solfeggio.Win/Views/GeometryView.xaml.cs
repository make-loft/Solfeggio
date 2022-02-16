using Rainbow;

using Solfeggio.Presenters;

using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Solfeggio.Views
{
	public partial class GeometryView
	{
		public GeometryView() => InitializeComponent();

		public bool IsPaused { get; set; }

		Bin[] _peaks;
		int _sampleSize;
		double _sampleRate;

		public void Draw(Bin[] peaks, int sampleSize, double sampleRate)
		{
			if (IsPaused is false)
			{
				_peaks = peaks;
				_sampleSize = sampleSize;
				_sampleRate = sampleRate;
			}

			var geometry = MusicalPresenter.DrawGeometry(_peaks, _sampleSize, _sampleRate, ActualWidth / 2d, ActualHeight / 2d).ToList();
			//var polyline = Polyline_Geometry;
			//polyline.Points.Clear();
			//geometry.ForEach(polyline.Points.Add);

			geo.TriangleIndices.Clear();
			geo.Positions.Clear();

			int k = 0;
			for (var n = 0; n < geometry.Count; n++)
			{
				//calculate the current Point3D based on the equations of the curve
				var x = geometry[n].X - ActualWidth / 2d;
				var y = geometry[n].Y - ActualHeight / 2d;
				var z = 1000 * n / geometry.Count;
				//the current Point3D
				var p = new Point3D(x, y, z);
				//here is where we make it thin, 
				//you can replace .1 with such as 10 to enlarge the strip
				var u = new Point3D(x, y, z - 5d);
				geo.Positions.Add(p);
				geo.Positions.Add(u);
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
			if (e.Key is Key.Space)
				SwitchPause();

			var step = 10;
			var direction = Camera.Position;
			switch (e.Key)
			{
				case Key.W:
					direction.Z += step;
					break;
				case Key.S:
					direction.Z -= step;
					break;
				case Key.A:
					direction.X += step;
					break;
				case Key.D:
					direction.X -= step;
					break;
			}

			Camera.Position = direction;
		}

		private void Window_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			SwitchPause();
		}
	}
}
