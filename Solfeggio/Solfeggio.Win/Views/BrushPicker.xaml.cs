using Ace;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Xamarin.Forms;

namespace Solfeggio.Views
{
	public partial class BrushPicker
	{
		SolidColorBrush solidColorBrush = new(Colors.Gray);
		LinearGradientBrush linearGradientBrush = new()
		{
			StartPoint = new(0, 0),
			EndPoint = new(0, 1),
			GradientStops =
			{
				new(Colors.Transparent, 0),
				new(Colors.Gray, 1)
			}
		};
		RadialGradientBrush radialGradientBrush = new()
		{
			Center = new(0, 0),
			RadiusX = 1,
			RadiusY = 1,
			GradientStops =
			{
				new(Colors.Transparent, 0),
				new(Colors.Gray, 1)
			}
		};

		public BrushPicker()
		{
			InitializeComponent();

			bool isRefresh = false;
			void CloneToValue(object o, EventArgs e)
			{
				isRefresh = true;
				Value = UnfrozenValue?.Clone();
				isRefresh = false;
			}

			SelectionChanged += (o, e) =>
			{
				var _ = Value switch
				{
					SolidColorBrush b => solidColorBrush = b,
					LinearGradientBrush b => linearGradientBrush = b,
					RadialGradientBrush b => radialGradientBrush = b,
					_ => Value
				};

				Value = SelectedIndex switch
				{
					0 => Value as SolidColorBrush ?? solidColorBrush,
					1 => Value as LinearGradientBrush ?? linearGradientBrush,
					2 => Value as RadialGradientBrush ?? radialGradientBrush,
					_ => Value
				};
			};

			DataContextChanged += (o, e) =>
			{
				if (isRefresh)
					return;

				if (UnfrozenValue.Is(out var oldBrush))
					oldBrush.Changed -= CloneToValue;

				var newBrush = Value?.Clone();
				if (newBrush.Is())
					newBrush.Changed += CloneToValue;

				UnfrozenValue = newBrush;

				SelectedIndex = Value switch
				{
					SolidColorBrush b => 0,
					LinearGradientBrush b => 1,
					RadialGradientBrush b => 2,
					_ => 0
				};
			};
		}

		public static readonly DependencyProperty UnfrozenValueProperty =
			DependencyProperty.Register(nameof(UnfrozenValue), typeof(Brush), typeof(BrushPicker));

		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(Brush), typeof(BrushPicker));

		public Brush UnfrozenValue
		{
			get => (Brush)GetValue(UnfrozenValueProperty);
			set => SetValue(UnfrozenValueProperty, value);
		}

		public Brush Value
		{
			get => (Brush)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		SmartSet<GradientStop> _activeGradientStopCollection;
		GradientStopCollection _stops;

		private void RemoveGradientStop_Button_Click(object sender, RoutedEventArgs e) =>
			_activeGradientStopCollection.Remove(sender.To<Button>().DataContext.To<GradientStop>());
		private void AddGradientStop_Button_Click(object sender, RoutedEventArgs e) =>
			_activeGradientStopCollection.Add(new(Colors.Gray, 0.5));

		private object Converter_Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (_stops.Is(value))
				return _activeGradientStopCollection;

			_activeGradientStopCollection = value.To(out _stops).ToSet();
			_activeGradientStopCollection.CollectionChangeCompleted += (o, e) =>
			{
				_stops.Clear();
				_activeGradientStopCollection.ForEach(_stops.Add);
			};

			return _activeGradientStopCollection;
		}
	}
}
