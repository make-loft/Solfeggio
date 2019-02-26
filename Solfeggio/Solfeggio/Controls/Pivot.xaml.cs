using System.Linq;
using Ace;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Solfeggio.Controls
{
	public class PivotItem : ContentView
	{
		public static readonly BindableProperty HeaderProperty =
			BindableProperty.Create(nameof(Header), typeof(string), typeof(PivotItem));

		public string Header
		{
			get => GetValue(HeaderProperty).To<string>();
			set => SetValue(HeaderProperty, value);
		}
	}

	[ContentProperty("Items")]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Pivot
	{
		public Pivot()
		{
			InitializeComponent();
			Content.To(out Grid grid).With
			(
				grid.Children[0].To(out ListView listView).ItemsSource = Items,
				grid.Children[1].To(out ScrollView scrollView).Content.To(out ContentPresenter presenter)
			);

			Items.CollectionChanged += (o, e) => listView.SelectedItem = listView.SelectedItem ?? Items.FirstOrDefault();
			listView.ItemSelected += async (o, e) => await Invoke.Delay(32, () => listView.SelectedItem.To(out PivotItem item).With
			(
				scrollView.Orientation = ScrollOrientation.Both,
				presenter.BindingContext = item.BindingContext,
				presenter.Content = item.Content,
				scrollView.Orientation = ScrollOrientation.Vertical
			));
		}

		public SmartSet<PivotItem> Items { get; } = new SmartSet<PivotItem>();
	}
}