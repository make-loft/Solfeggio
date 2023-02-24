using Ace;
using Ace.Controls;
using Ace.Markup.Patterns;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Solfeggio.Views
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class OptionsView
	{
		public OptionsView() => InitializeComponent();

		object SkipTelemetry_Convert(ConvertArgs args) =>
			Content.Is(out Pivot pivot) && pivot.ActiveItemOffset.Is(3) ? args.Value : default;
	}
}