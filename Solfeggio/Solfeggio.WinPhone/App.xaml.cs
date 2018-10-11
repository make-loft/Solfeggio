using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Ace;
using Ace.Specific;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Solfeggio.Languages;
using System;
using Mic = Microsoft.Xna.Framework.Audio.Microphone;

namespace Solfeggio
{
	public partial class App
	{
		public App()
		{
			UnhandledException += (sender, args) => MessageBox.Show(args.ExceptionObject.Message);

			Memory.ActiveBox = new Memory(new KeyFileStorage());
			Store.Set<IAudioInputDevice>(Microphone.Default);
			LocalizationSource.Wrap.ActiveManager = English.ResourceManager;

			InitializeComponent();
			InitializePhoneApplication();
		}

		private void Application_Launching(object sender, LaunchingEventArgs e) { }
		private void Application_Activated(object sender, ActivatedEventArgs e) { }
		private void Application_Deactivated(object sender, DeactivatedEventArgs e) { }
		private void Application_Closing(object sender, ClosingEventArgs e) { }

		private void InitializePhoneApplication()
		{
			RootVisual = new PhoneApplicationFrame();

			Current.Host.Settings.EnableFrameRateCounter = Debugger.IsAttached;
			Current.Host.Settings.EnableCacheVisualization = Debugger.IsAttached;
			PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			// Debugger.IsAttached ? IdleDetectionMode.Disabled : IdleDetectionMode.Enabled;
		}
	}
}