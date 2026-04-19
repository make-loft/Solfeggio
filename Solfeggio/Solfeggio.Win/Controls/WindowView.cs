using System.Threading.Tasks;
using System.Windows;

using static System.Windows.Visibility;
using static System.Windows.WindowState;

namespace Solfeggio.Controls;

public class WindowView : Window
{
	public WindowView() : base()
	{
		Closing += async (o, e) =>
		{
			try
			{
				e.Cancel = true;

				await Task.Delay(128);

				Visibility = Collapsed;
			}
			catch { }
		};

		IsVisibleChanged += async (o, e) =>
		{
			var state = Visibility is Visible && WindowState is Minimized
				? Normal
				: WindowState;

			await Task.Delay(256);

			WindowState = state;
		};
	}
}
