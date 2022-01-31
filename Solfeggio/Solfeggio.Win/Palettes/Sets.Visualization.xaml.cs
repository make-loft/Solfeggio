using Ace;

using System.Collections;

using Xamarin.Forms;

namespace Solfeggio.Palettes
{
	partial class Sets_Visualization
	{
		void ResetPicker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) =>
			Store.Get<ViewModels.VisualizationManager>().ActiveProfile?
			.Reset(sender.To<Picker>().SelectedItem.To<DictionaryEntry>().Key.ToStr());

		void ResetButton_Click(object sender, System.Windows.RoutedEventArgs e) =>
			Store.Get<ViewModels.VisualizationManager>().ActiveProfile?
			.Reset();
	}
}
