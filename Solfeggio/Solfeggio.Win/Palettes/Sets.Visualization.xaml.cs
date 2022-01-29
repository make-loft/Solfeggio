using Ace;

using System.Windows.Media;

namespace Solfeggio.Palettes
{
	partial class Sets_Visualization
	{
		private void Picker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			var resources = App.Current.Resources.To<Ace.Markup.Map>();
			var brushes = resources.MergedDictionaries[4];
			foreach (var key in brushes.Keys)
			{
				if (brushes[key].Is(out Brush b))
					resources[key] = b.Clone();
			}

			resources.EvokePropertyChanged();
		}
	}
}
