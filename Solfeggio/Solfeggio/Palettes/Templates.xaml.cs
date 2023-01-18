using Ace;
using Ace.Markup.Patterns;

using Solfeggio.ViewModels;

using Xamarin.Forms.Xaml;

namespace Solfeggio.Palettes
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Templates
	{
		public Templates() => InitializeComponent();

		ProcessingManager ProcessingManager = Store.Get<ProcessingManager>();

		object LevelToPointsCount(ConvertArgs args) =>
			(int) (ProcessingManager.ActiveProfile.FrameSize / args.Value.To<double>());
	}
}
