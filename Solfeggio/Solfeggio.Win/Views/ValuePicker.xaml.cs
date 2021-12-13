using System.Windows;

namespace Solfeggio.Views
{
	public partial class ValuePicker
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(double), typeof(ValuePicker));

		public double Value
		{
			get => (double)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		public ValuePicker()
		{
			InitializeComponent();
		}
	}
}
