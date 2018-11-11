using System.Windows;
using Ace;
using Ace.Specific;

namespace Solfeggio
{
	public partial class App
	{
		private void App_OnStartup(object sender, StartupEventArgs e) =>
			Store.Set<IAudioInputDevice>(Solfeggio.Microphone.Default);

		private void App_OnExit(object sender, ExitEventArgs e) =>
			Store.Snapshot();
	}
}
