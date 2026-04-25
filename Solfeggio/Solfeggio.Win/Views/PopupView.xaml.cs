using Ace;

using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Solfeggio.Views;

[ContentProperty(nameof(Content))]
public partial class PopupView
{
	public object Content
	{
		get => ContentControl.Content;
		set => ContentControl.Content = value;
	}

	private static readonly List<PopupView> Popups = [];

	public PopupView()
	{
		InitializeComponent();

		Opened += (sender, args) => Popups.Add(this);
		Closed += (sender, args) => Popups.Remove(this);

		Opened += (sender, args) => Child.MoveFocus(new(FocusNavigationDirection.Next));
		Closed += (sender, args) => Popups.LastOrDefault()?.Child.MoveFocus(new(FocusNavigationDirection.Next));

		MouseMove += (sender, args) => args.Handled = true;

		PreviewMouseLeftButtonDown += (sender, args) =>
		{
			if (args.Source.Is(ContentControl))
				Mouse.Capture(DragMoveThumb);
		};

		DragMoveThumb.DragDelta += (sender, args) =>
		{
			HorizontalOffset += args.HorizontalChange;
			VerticalOffset += args.VerticalChange;
		};

		KeyDown += (sender, args) =>
		{
			if (args.Key.IsNot(Key.Escape) || args.Handled.IsTrue())
				return;

			args.Handled = true;
			IsOpen = false;
		};
	}
}
