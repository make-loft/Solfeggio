using Ace;
using Ace.Controls;
using Ace.Markup.Patterns;

using Solfeggio.ViewModels;

using System.Linq;

using Xamarin.Forms.Xaml;

namespace Solfeggio.Views
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class OptionsView
	{
		public OptionsView()
		{
			InitializeComponent();

			this.ContextChanged(args =>
			{
				if (Store.Get<AppViewModel>()["CarePageVisit"].IsNot(true))
					Content.To(out Pivot pivot).ActiveItem = pivot.Items.LastOrDefault();
			});
		}

		object SkipTelemetry_Convert(ConvertArgs args) =>
			IsVisible && Content.Is(out Pivot pivot) && pivot.ActiveItemOffset.Is(3) ? args.Value : default;
	}
}