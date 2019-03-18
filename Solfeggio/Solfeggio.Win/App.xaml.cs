using System.Windows;
using Ace;

namespace Solfeggio
{
	public partial class App
	{
		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			Store.Get<ViewModels.AppViewModel>();
			Store.Set<IAudioInputDevice>(Microphone.Default);
		}

		private void App_OnExit(object sender, ExitEventArgs e) =>
			Store.Snapshot();
	}
}
