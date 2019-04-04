using System.Windows;

namespace Solfeggio.Controls
{
	public partial class RangeBar
	{
		public RangeBar() => InitializeComponent();

		bool _isRangeMoving, _isRangeChanging;

		private void RangeMoved(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_isRangeMoving || _isRangeChanging) return;
			_isRangeMoving = true;
			var delta = e.NewValue - e.OldValue;
			var newSelectionEnd = SelectionEnd += delta;
			var newSelectionStart = SelectionStart += delta;
			SelectionEnd = newSelectionEnd < Maximum ? newSelectionEnd : Maximum;
			SelectionStart = newSelectionStart > Minimum ? newSelectionStart : Minimum;
			Value = (SelectionStart + SelectionEnd) / 2d;
			_isRangeMoving = false;
		}

		private void RangeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (_isRangeChanging || _isRangeMoving) return;
			_isRangeChanging = true;
			Value += (e.NewValue - e.OldValue) / 2d;
			_isRangeChanging = false;
		}

		private void RangeBarLoaded(object sender, RoutedEventArgs e)
		{
			_isRangeMoving = true;
			Value = (SelectionStart + SelectionEnd) / 2d;
			_isRangeMoving = false;
		}
	}
}
