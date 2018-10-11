using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Ace;
using Ace.Patterns;
using Ace.Specific;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.GamerServices;
using Solfeggio.Presenters;
using Solfeggio.ViewModels;

namespace Solfeggio
{
	public partial class MainPage
	{
		private static void ShowTrialBox()
		{
			var title = LocalizationSource.Wrap["TrialTitle"];
			var message = LocalizationSource.Wrap["TrialMessage"];
			Guide.BeginShowMessageBox(
				title.Length > 255 ? title.Substring(0, 255) : title,
				message.Length > 255 ? message.Substring(0, 255) : message,
				new[] { LocalizationSource.Wrap["Skip"], LocalizationSource.Wrap["Buy"] }, 0,
				new MessageBoxIcon(),
				result =>
				{
					var pressedButton = Guide.EndShowMessageBox(result);
					if (pressedButton != 1) return;
					new MarketplaceDetailTask
					{
						ContentType = MarketplaceContentType.Applications,
						ContentIdentifier = "29ad20cb-edd9-424b-87c8-23ed9f089a29",
					}.Show();
				}, null);
		}

		public MainPage()
		{
			InitializeComponent();		   

			//AdControl.Tap += (sender, args) => AdToggleButton.IsChecked = true;
			//AdToggleButton.Click += (sender, args) =>
			//{
			//	ShowTrialBox();
			//	AdToggleButton.IsChecked = true;
			//};
		}
	}
}