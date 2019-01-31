using System.Windows;
using Ace;

namespace Solfeggio
{
	public partial class App
	{
		private void App_OnStartup(object sender, StartupEventArgs e) =>
			Store.Set<IAudioInputDevice>(Microphone.Default);

		private void App_OnExit(object sender, ExitEventArgs e) =>
			Store.Snapshot();
	}
}
