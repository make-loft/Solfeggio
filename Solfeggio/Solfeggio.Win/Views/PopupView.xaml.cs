using Ace;

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

		public PopupView()
		{
			InitializeComponent();

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
		}
	}
}
