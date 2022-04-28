using System.Threading.Tasks;
using System.Windows;

using static System.Windows.Visibility;
using static System.Windows.WindowState;

namespace Solfeggio.Controls
{
	public class WindowView : Window
	{
		public WindowView() : base()
		{
			Closing += (o, e) =>
			{
				if (AppView.IsShutdown) return;
				Visibility = Collapsed;
				e.Cancel = true;
			};

			IsVisibleChanged += async (o, e) =>
			{
				var state = Visibility is Visible && WindowState is Minimized
					? Normal
					: WindowState;

				await Task.Delay(400);

				WindowState = state;
			};
		}
	}
}
