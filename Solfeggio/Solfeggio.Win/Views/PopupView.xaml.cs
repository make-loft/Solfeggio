using Ace;

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Solfeggio.Views
{
	[ContentProperty(nameof(Content))]
	public partial class PopupView
	{
		public object Content
		{
			get => ContentControl.Content;
			set => ContentControl.Content = value;
		}

		private static readonly List<PopupView> Popups = new();

		public PopupView()
		{
			InitializeComponent();

			Opened += (o, e) => Popups.Add(this);
			Closed += (o, e) => Popups.Remove(this);

			Opened += (o, e) => Child.MoveFocus(new(FocusNavigationDirection.Next));
			Closed += (o, e) => Popups.LastOrDefault()?.Child.MoveFocus(new(FocusNavigationDirection.Next));

			MouseMove += (o, e) => e.Handled = true;

			PreviewMouseLeftButtonDown += (o, e) =>
			{
				if (e.Source.Is(ContentControl))
					Mouse.Capture(DragMoveThumb);
			};

			DragMoveThumb.DragDelta += (o, e) =>
			{
				HorizontalOffset += e.HorizontalChange;
				VerticalOffset += e.VerticalChange;
			};

			KeyDown += (o, e) =>
			{
				if (e.Key.IsNot(Key.Escape) || e.Handled.IsTrue())
					return;

				e.Handled = true;
				IsOpen = false;
			};
		}
	}
}
