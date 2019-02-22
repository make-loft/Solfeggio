using System.Linq;
using Ace;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Solfeggio.Controls
{
	public class PivotItem : ContentView
	{
		public string Header { get; set; }
	}

	[ContentProperty("Items")]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Pivot
	{
		public Pivot()
		{
			InitializeComponent();
			Content.To<Grid>().Children[0].To(out ListView listView).ItemsSource = Items;
			Content.To<Grid>().Children[1].To<ScrollView>().Content.To<StackLayout>().Children[0].To(out ContentPresenter presenter);
			listView.ItemSelected += (o, e) => presenter.Content = listView.SelectedItem.To<PivotItem>().Content;
			Items.CollectionChanged += (o, e) => listView.SelectedItem = listView.SelectedItem ?? Items.FirstOrDefault();
		}

		public SmartSet<PivotItem> Items { get; } = new SmartSet<PivotItem>();
	}
}