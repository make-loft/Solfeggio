using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Xamarin.Forms
{
	namespace Xaml
	{
		public enum XamlCompilationOptions { Skip, Compile }
	}

	public class XamlCompilationAttribute : System.Attribute
	{
		public XamlCompilationAttribute(Xaml.XamlCompilationOptions options) { }
	}

	public class ContentView : UserControl
	{
	}

	public class StackLayout : StackPanel
	{
		public static readonly DependencyProperty BindingContextProperty =
			DependencyProperty.Register(nameof(BindingContext), typeof(object), typeof(StackLayout), new PropertyMetadata((o, e)=>
			{
				if (o is Control control) control.SetValue(DataContextProperty, e.NewValue);
			}));

		public object BindingContext
		{
			get => GetValue(BindingContextProperty);
			set => SetValue(BindingContextProperty, value);
		}
	}

	public class Label : TextBlock
    {
		public Brush TextColor { get; set; }
    }

	public class StaticResource : StaticResourceExtension
	{
		public StaticResource(object key) : base(key) { }
	}

	public class Grid : System.Windows.Controls.Grid
	{
		public static readonly DependencyProperty BindingContextProperty =
			DependencyProperty.Register(nameof(BindingContext), typeof(object), typeof(Grid), new PropertyMetadata((o, e)=>
			{
				if (o is Control control) control.SetValue(DataContextProperty, e.NewValue);
			}));

		public object BindingContext
		{
			get => GetValue(BindingContextProperty);
			set => SetValue(BindingContextProperty, value);
		}
	}

	public class Binding : System.Windows.Data.Binding
	{
		public Binding() : base() { }
		public Binding(string path) : base(path) { }
	}

	public class Slider : System.Windows.Controls.Slider
	{
	}

	public class Picker : ComboBox
	{
		public Binding ItemDisplayBinding
		{
			set => DisplayMemberPath = "Method.Name";
		}
	}

	public class Switch : ToggleButton
	{
		public Switch()
		{
			Checked += StateChanged;
			Unchecked += StateChanged;
		}

		private void StateChanged(object sender, RoutedEventArgs e) => IsToggled = IsChecked;

		public static readonly DependencyProperty IsToggledProperty =
			DependencyProperty.Register(nameof(IsToggled), typeof(bool?), typeof(Control), new PropertyMetadata((o, e) =>
			{
				if (o is ToggleButton toggleButton) toggleButton.SetValue(IsCheckedProperty, e.NewValue);
			}));

		public bool? IsToggled
		{
			get => (bool?) GetValue(IsToggledProperty);
			set => SetValue(IsToggledProperty, value);
		}
	}
}
