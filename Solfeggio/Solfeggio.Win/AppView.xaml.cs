using System.Linq;
using System.Windows;

using Solfeggio.Views;

namespace Solfeggio
{
	public partial class AppView
	{
		public static MonitorView MonitorView { get; set; }
		public static FlowerView FlowerView { get; set; }
		public static TapeView TapeView { get; set; }
		public static EncoderView EncoderView { get; set; }

		public static bool IsShutdown { get; private set; }

		public AppView()
		{
			InitializeComponent();
			Closing += (o, e) => IsShutdown = true;

			MonitorView = new MonitorView();
			FlowerView = new FlowerView();
			TapeView = new TapeView();

			//EncoderView = new EncoderView();
			//EncoderView.Show();

			static void ShowVisible(params Window[] windows) =>
				windows.Where(w => w.Visibility is Visibility.Visible).ForEach(w => w.Show());

			ShowVisible(MonitorView, FlowerView, TapeView);

			Closed += (o, e) => Application.Current.Shutdown();
		}
	}
}
