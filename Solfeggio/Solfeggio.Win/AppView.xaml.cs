using System.Windows;
using Ace;

using Solfeggio.Views;

namespace Solfeggio
{
	public partial class AppView
	{
		public static MonitorView MonitorView;
		public static GeometryView GeometryView;

		public AppView()
		{
			InitializeComponent();

			new MonitorView().To(out MonitorView).Show();
			new GeometryView().To(out GeometryView).Show();

			Closed += (o, e) =>
			{
				MonitorView.Close();
				GeometryView.Close();

				Application.Current.Shutdown();
			};
		}
	}
}
