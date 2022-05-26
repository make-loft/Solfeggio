using Ace;
using Ace.Zest.Extensions;

using Solfeggio.Extensions;
using Solfeggio.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Solfeggio.Views
{
	public partial class TapeView
	{
		public TapeView() => InitializeComponent();

		public TapeViewModel TapeViewModel => (TapeViewModel)DataContext;

		public void Draw(IList<Point> geometry)
		{
			geo.TriangleIndices.Clear();
			geo.Positions.Clear();

			wave.TriangleIndices.Clear();
			wave.Positions.Clear();

			var tapeViewModel = (TapeViewModel)DataContext;
			var radius = tapeViewModel.Radius;
			var depth = tapeViewModel.Depth;
			var thin = tapeViewModel.Thin;
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
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.FocusedElement.IsNot(e.OriginalSource))
				return;

			var key = e.Key;
			e.Handled = HandleKeys.Contains(key);
			if (e.Key is Key.Space)
				SwitchPause();
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e) => Focus();

		private async void Window_Activated(object sender, EventArgs e)
		{
			Focus();

			while (IsActive)
			{
				await Task.Delay(16);

				var element = Keyboard.FocusedElement;
				if (element.Is<TextBox>() || element.Is<Slider>() || element.Is<ComboBox>())
					continue;

				var fieldOfView = TapeViewModel.Camera is PerspectiveCamera c ? c.FieldOfView : 45;
				var step = fieldOfView / 360d;
				var angle = fieldOfView / 45d;

				var pattern = NavigationKeys.Where(Keyboard.IsKeyDown).ToList();
				var modifiers = Keyboard.Modifiers;

				if (Mouse.LeftButton is MouseButtonState.Pressed)
				{
					var b = Mouse.DirectlyOver.As<DependencyObject>()?
						.EnumerateSelfAndVisualAncestors().OfType<Button>().FirstOrDefault();

					if (b.IsNot())
						goto Skip;

					if (Enum.TryParse(b.Name.ToStr(), out Key k))
					{
						pattern.Add(k);
					}
					else
					{
						if (b.Is(WAD))
						{
							pattern.Add(Key.W);
							pattern.Add(Key.A);
							pattern.Add(Key.D);
						}
						if (b.Is(SDA))
						{
							pattern.Add(Key.S);
							pattern.Add(Key.A);
							pattern.Add(Key.D);
						}

						if (b.Is(WA) || b.Is(WD)) pattern.Add(Key.W);
						if (b.Is(SA) || b.Is(SD)) pattern.Add(Key.S);
						if (b.Is(WD) || b.Is(SD)) pattern.Add(Key.D);
						if (b.Is(WA) || b.Is(SA)) pattern.Add(Key.A);

						if (b.Is(UpLeft) || b.Is(UpRight)) pattern.Add(Key.Up);
						if (b.Is(DownLeft) || b.Is(DownRight)) pattern.Add(Key.Down);
						if (b.Is(UpRight) || b.Is(DownRight)) pattern.Add(Key.Right);
						if (b.Is(UpLeft) || b.Is(DownLeft)) pattern.Add(Key.Left);

						if (b.Is(AngleInc))
						{
							pattern.Add(Key.Up);
							pattern.Add(Key.Left);
							pattern.Add(Key.Right);
						}

						if (b.Is(AngleDec))
						{
							pattern.Add(Key.Down);
							pattern.Add(Key.Left);
							pattern.Add(Key.Right);
						}
					}
				}
				Skip:

				static double Inc(double value, double direction) => value + value * 0.1 * direction;
				var directionR =
					pattern.Contains(Key.Up) ? +1d :
					pattern.Contains(Key.Down) ? -1d :
					0d;

				var directionL =
					pattern.Contains(Key.W) ? +1d :
					pattern.Contains(Key.S) ? -1d :
					0d;

				if (Alt.IsChecked is true) modifiers |= ModifierKeys.Alt;
				if (Shift.IsChecked is true) modifiers |= ModifierKeys.Shift;
				if (Control.IsChecked is true) modifiers |= ModifierKeys.Control;

				if (modifiers.Is(ModifierKeys.None))
				{
					if (pattern.Contains(Key.Left, Key.Right))
					{
						if (TapeViewModel.Camera is PerspectiveCamera perspectiveCamera)
							perspectiveCamera.FieldOfView = Inc(perspectiveCamera.FieldOfView, directionR);
					}
					else
					{
						pattern.ForEach(key => TapeViewModel.Camera.MoveBy(pattern, step).RotateBy(key, angle));
					}
				}
				else
				{
					if (modifiers.HasFlag(ModifierKeys.Shift))
					{
						TapeViewModel.Radius += Inc(TapeViewModel.Radius, directionR);
					}

					if (modifiers.HasFlag(ModifierKeys.Control))
					{
						TapeViewModel.Thin = Inc(TapeViewModel.Thin, directionL);
						TapeViewModel.Depth = Inc(TapeViewModel.Depth, directionR);
					}

					if (modifiers.HasFlag(ModifierKeys.Alt))
					{
						TapeViewModel.Approximation = Inc(TapeViewModel.Approximation, directionL);
						if (TapeViewModel.Camera is PerspectiveCamera perspectiveCamera)
							perspectiveCamera.FieldOfView = Inc(perspectiveCamera.FieldOfView, directionR);
					}
				}

				if (TapeViewModel.Camera.Is() && TapeViewModel.Camera.Transform.Is(out ScaleTransform3D scaleTransform))
				{
					var scale =
						Keyboard.IsKeyDown(Key.S) ? 1 * 1.05 :
						Keyboard.IsKeyDown(Key.W) ? 1 / 1.05 :
						1d;

					scaleTransform.ScaleX *= scale;
					scaleTransform.ScaleY *= scale;
					scaleTransform.ScaleZ *= scale;
				}

				var modifierButtons = new[] { Alt, Control, Shift };
				var navigationButtons = new[] { W, A, S, D, Up, Down, Left, Right };

				modifierButtons.ForEach(m => m.Opacity = Enum.TryParse(m.Name.ToStr(), out ModifierKeys k) && modifiers.HasFlag(k)
					? 1.0
					: 0.5);
				navigationButtons.ForEach(b => b.Opacity = Enum.TryParse(b.Name.ToStr(), out Key k) && pattern.Contains(k)
					? 1.0
					: 0.5);
			}
		}

		readonly Key[] NavigationKeys = { Key.Down, Key.Up, Key.Left, Key.Right, Key.W, Key.S, Key.A, Key.D };

		private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e) => SwitchPause();

		Point from;
		private void Window_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Handled)
				return;

			var till = e.GetPosition(sender as IInputElement);
			double dx = till.X - from.X;
			double dy = till.Y - from.Y;
			from = till;

			var distance = dx * dx + dy * dy;
			if (distance <= 0)
				return;

			if (e.MouseDevice.LeftButton is MouseButtonState.Pressed)
			{
				var fieldOfView = TapeViewModel.Camera is PerspectiveCamera c ? c.FieldOfView : 45;
				var angle = (distance / fieldOfView) % 45;
				TapeViewModel.Camera.Rotate(new(dy, -dx, 0d), angle);
			}
		}

		private void Expander_Expanded(object sender, RoutedEventArgs e) => 
			Expander.Visibility = Visibility.Visible;
	}
}
