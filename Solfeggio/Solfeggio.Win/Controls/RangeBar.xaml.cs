using System.Windows;

namespace Solfeggio.Controls;

public partial class RangeBar
{
	public RangeBar() => InitializeComponent();

	bool _isRangeMoving, _isRangeChanging;

	private void RangeMoved(object sender, RoutedPropertyChangedEventArgs<double> args)
	{
		if (_isRangeMoving || _isRangeChanging) return;
		_isRangeMoving = true;
		var delta = args.NewValue - args.OldValue;
		var newSelectionEnd = SelectionEnd += delta;
		var newSelectionStart = SelectionStart += delta;
		SelectionEnd = newSelectionEnd < Maximum ? newSelectionEnd : Maximum;
		SelectionStart = newSelectionStart > Minimum ? newSelectionStart : Minimum;
		Value = (SelectionStart + SelectionEnd) / 2d;
		_isRangeMoving = false;
	}

	private void RangeChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
	{
		if (_isRangeChanging || _isRangeMoving) return;
		_isRangeChanging = true;
		Value += (args.NewValue - args.OldValue) / 2d;
		_isRangeChanging = false;
	}

	private void RangeBarLoaded(object sender, RoutedEventArgs args)
	{
		_isRangeMoving = true;
		Value = (SelectionStart + SelectionEnd) / 2d;
		_isRangeMoving = false;
	}
}
