using Ace;

using Solfeggio.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Solfeggio.Views
{
	public partial class FlowerView
	{
		public FlowerView() => InitializeComponent();

		public void Draw(IList<Point> geometry)
		{
			var centerX = PolylineCanvas.ActualWidth / 2d;
			var centerY = PolylineCanvas.ActualHeight / 2d;
			var radius = Math.Max(Math.Min(centerX, centerY), 1d);
			var polyline = Polyline_Geometry;
			polyline.Points.Clear();
			geometry.ForEach(p => polyline.Points.Add(new(p.X * radius + centerX, p.Y * radius + centerY)));
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
				//var angle = (distance / PerspectiveCamera.FieldOfView) % 45;
				//PerspectiveCamera.Rotate(new(dy, -dx, 0d), angle);
				//OrthographicCamera.Rotate(new(dy, -dx, 0d), angle);
			}
		}
	}
}
