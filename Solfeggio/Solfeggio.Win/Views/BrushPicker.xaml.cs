using Ace;
using Ace.Markup.Converters;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Xamarin.Forms;

namespace Solfeggio.Views
{
	public partial class BrushPicker
	{
		SolidColorBrush solidColorBrush;
		LinearGradientBrush linearGradientBrush = new()
		{
			StartPoint = new(0, 0),
			EndPoint = new(0, 1),
			GradientStops = default
		};
		RadialGradientBrush radialGradientBrush = new()
		{
			Center = new(0.5, 0.0),
			RadiusX = 1,
			RadiusY = 1,
			GradientStops = default
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

				GradientStopCollection CreateDefaultGradientStops() => new()
				{
					new(Colors.Transparent, 0),
					new(Colors.Gray, 1)
				};

				radialGradientBrush.GradientStops ??= linearGradientBrush.GradientStops;
				radialGradientBrush.GradientStops ??= CreateDefaultGradientStops();

				linearGradientBrush.GradientStops ??= radialGradientBrush.GradientStops;
				radialGradientBrush.GradientStops ??= CreateDefaultGradientStops();

				solidColorBrush ??= new SolidColorBrush(linearGradientBrush.GradientStops.LastOrDefault()?.Color ?? Colors.Gray);

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

		private object Converter_Convert(ConvertArgs args)
		{
			if (_stops.Is(args.Value))
				return _activeGradientStopCollection;

			_activeGradientStopCollection = args.Value.To(out _stops).ToSet();
			_activeGradientStopCollection.CollectionChangeCompleted += (o, e) =>
			{
				_stops.Clear();
				_activeGradientStopCollection.ForEach(_stops.Add);
			};

			return _activeGradientStopCollection;
		}
	}
}
