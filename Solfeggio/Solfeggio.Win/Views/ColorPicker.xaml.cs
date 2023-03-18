using Ace;
using Ace.Extensions;

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using static System.Reflection.BindingFlags;

namespace Solfeggio.Views
{
	public partial class ColorPicker : INotifyPropertyChanged
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(Color), typeof(ColorPicker),
				new((s, a) => s.To<ColorPicker>().InvokePropertyChanged(PropertyNames)));

		public Color Value
		{
			get => (Color)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		static readonly string[] PropertyNames = { nameof(Value), nameof(A), nameof(R), nameof(G), nameof(B) };
		readonly LinearGradientBrush AlphaBrush = new()
		{
			EndPoint = new(1,0),
			StartPoint = new(0,0),
			GradientStops =
			{
				new(Colors.Transparent, 0.04),
				new(Colors.Transparent, 0.48),
				new(Colors.Transparent, 0.52),
				new(Colors.Transparent, 0.96),
			}
		};

		public void InvokePropertyChanged(params string[] names) => names.ForEach(InvokePropertyChanged);
		public void InvokePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


		public event PropertyChangedEventHandler PropertyChanged;

		public NamedColor _activeNamedColor;
		public NamedColor ActiveNamedColor
		{
			get => _activeNamedColor;
			set => Value = (_activeNamedColor = value).Color;
		}

		public class NamedColor
		{
			public Color Color { get; set; }
			public Brush Brush => new SolidColorBrush(Color);
			public string Name { get; set; }
		}

		public SmartSet<NamedColor> NamedColors { get; } = typeof(Colors).GetProperties(Public | Static)
			.Select(i => new NamedColor { Name = i.Name, Color = (Color)i.GetValue(default) }).ToSet();

		public ColorPicker()
		{
			InitializeComponent();
			Opacity.Background = AlphaBrush;

			var skipOpacity = false;
			PropertyChanged += (o, e) =>
			{
				if (e.PropertyName.IsNot(nameof(Value))) return;
				var value = Value;
				_activeNamedColor = NamedColors.FirstOrDefault(p => p.Color.Is(value));
				InvokePropertyChanged(nameof(ActiveNamedColor));

				ActiveColor.Background = ActiveColorA.Background = new SolidColorBrush(value);
				var stops = AlphaBrush.GradientStops;
				stops.First().Color = stops.Last().Color = Color.FromRgb(value.R, value.G, value.B);

				if (skipOpacity) return;
				Spectrum.Opacity = Value.A / 255d;
			};

			Spectrum.MouseLeftButtonDown += (o, e) => Spectrum.Opacity = 1.0;
			Spectrum.MouseLeftButtonUp += (o, e) => Spectrum.Opacity = Value.A / 255d;

			Spectrum.MouseLeftButtonDown += (o, e) =>
			{
				skipOpacity = true;
				Value = PickColor(Spectrum, Mouse.GetPosition(Spectrum));
				skipOpacity = false;
			};

			Spectrum.MouseMove += (o, e) =>
			{
				if (Mouse.LeftButton.Is(MouseButtonState.Released))
					return;

				skipOpacity = true;
				Value = PickColor(Spectrum, Mouse.GetPosition(Spectrum));
				skipOpacity = false;
			};

			Spectrum.MouseWheel += (o, e) => Spectrum.Opacity += e.Delta / 2000;

			Opacity.MouseLeftButtonDown += (o, e) =>
				A = PickColor(Opacity, Mouse.GetPosition(Opacity), true).A;

			Opacity.MouseMove += (o, e) =>
			{
				if (Mouse.LeftButton.Is(MouseButtonState.Released))
					return;
				A = PickColor(Opacity, Mouse.GetPosition(Opacity), true).A;
			};
		}

		public Color PickColor(FrameworkElement element, Point position, bool pickA = false)
		{
			var bytes = element.GetPixelBytes(position);
			var color = Color.FromArgb(pickA ? bytes[3] : Value.A, bytes[2], bytes[1], bytes[0]);

			return color;
		}

		public byte A
		{
			get => Value.A;
			set
			{
				var v = Value;
				Value = Color.FromArgb(value, v.R, v.G, v.B);
			}
		}

		public byte R
		{
			get => Value.R;
			set
			{
				var v = Value;
				Value = Color.FromArgb(v.A, value, v.G, v.B);
			}
		}

		public byte G
		{
			get => Value.G;
			set
			{
				var v = Value;
				Value = Color.FromArgb(v.A, v.R, value, v.B);
			}
		}

		public byte B
		{
			get => Value.B;
			set
			{
				var v = Value;
				Value = Color.FromArgb(v.A, v.R, v.G, value);
			}
		}
	}
}
