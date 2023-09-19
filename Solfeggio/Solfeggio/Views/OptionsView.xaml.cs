using Ace;
using Ace.Controls;
using Ace.Markup.Patterns;

using Xamarin.Forms.Xaml;

namespace Solfeggio.Views
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class OptionsView
	{
		public OptionsView() => InitializeComponent();

		object SkipTelemetry_Convert(ConvertArgs args) =>
			IsVisible && Content.Is(out Pivot pivot) && pivot.ActiveItemOffset.Is(2) ? args.Value : default;
	}
}