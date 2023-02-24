using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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

	public class ResourceDictionary : System.Windows.ResourceDictionary { }

	public class ContentView : UserControl
	{
	}

	public class Pivot : TabControl
	{
		public HorizontalAlignment HorizontalOptions { get; set; }
	}

	public class ListView : System.Windows.Controls.ListView
	{
		public HorizontalAlignment HorizontalOptions { get; set; }
	}

	public class Item : TabItem
	{
		public HorizontalAlignment HorizontalOptions { get; set; }
	}

	public class Stack : StackPanel
	{
		public HorizontalAlignment HorizontalOptions
		{
			get => HorizontalAlignment;
			set => HorizontalAlignment = value;
		}
	}


    public class ItemsView : ItemsControl
	{
		public ItemsView()
		{
			DataContextChanged += (o, e) =>
			{
				foreach (var item in Items.OfType<FrameworkElement>())
					item.DataContext = DataContext;
			};
		}

		public static readonly DependencyProperty BindingContextProperty =
			DependencyProperty.Register(nameof(BindingContext), typeof(object), typeof(ItemsView), new PropertyMetadata((o, e)=>
			{
				if (o is ItemsView control)	control.SetValue(DataContextProperty, e.NewValue);
			}));

		public object BindingContext
		{
			get => GetValue(BindingContextProperty);
			set => SetValue(BindingContextProperty, value);
		}

		protected override DependencyObject GetContainerForItemOverride() => new ContentControl();

		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			if (item is FrameworkElement e && e.DataContext is null) e.DataContext = DataContext;
			return false; // wrap always
		}
	}


	public class Entry : TextBox
	{
		public static readonly List<WeakReference<Entry>> Entres = new();
		readonly WeakReference<Entry> _this;

		public Entry()
		{
			Entres.Add(_this = new(this));

			KeyDown += (o, e) =>
			{
				if (e.Key != System.Windows.Input.Key.Enter)
					return;

				GetBindingExpression(Entry.TextProperty).UpdateSource();
			};
		}

		~Entry() => Entres.Remove(_this);

		public static void GlobalTextBindingRefresh() => Entres.ForEach(w =>
		{
			if (w.TryGetTarget(out var e))
				e.GetBindingExpression(Entry.TextProperty).UpdateTarget();
		});
	}

	public class Label : TextBlock
    {
		public Brush TextColor { get; set; }
    }

	public class Frame : Border
	{
	}

	public class StaticResource : StaticResourceExtension
	{
		public StaticResource(object key) : base(key) { }
	}

    public class RackSplitter : System.Windows.Controls.GridSplitter
    {
    }

	public class GridSplitter : System.Windows.Controls.GridSplitter
	{
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

	public class RelativeSource : System.Windows.Data.RelativeSource
	{
	}

	public class Slider : System.Windows.Controls.Slider
	{
		public Slider()
		{
			PreviewKeyDown += (o, e) =>
			{
				if (Value == Minimum)
					if (e.Key is Key.Left || e.Key is Key.Down)
						Value = Maximum;
				if (Value == Maximum)
					if (e.Key is Key.Right || e.Key is Key.Up)
						Value = Minimum;
			};

			MouseWheel += (o, e) =>
			{
				var delta = (Maximum - Minimum) / 256;
				Value += e.Delta < 0 ? +delta : e.Delta > 0 ? -delta : 0;
			};
		}
	}

	public class Picker : ComboBox
	{
		public Binding ItemDisplayBinding
		{
			set => DisplayMemberPath = value.Path?.Path ?? "Method.Name";
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

	//public static class Colors
	//{
	//	public static Color Transparent { get; } = Color.FromArgb(0, 0, 0, 0);
	//}
}
