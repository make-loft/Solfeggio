using System.Windows;
using Ace;

namespace Solfeggio
{
	public partial class AppView
	{
		public AppView()
		{
			InitializeComponent();
			new MonitorView().To(out var optionsView).Show();

			Closed += (o, e) =>
			{
				optionsView.Close();
				Application.Current.Shutdown();
			};
		}
	}
}
