using System.Threading.Tasks;
using System.Windows;

using static System.Windows.Visibility;
using static System.Windows.WindowState;

namespace Solfeggio.Controls;

public class WindowView : Window
{
	public WindowView() : base()
	{
		Closing += async (sender, args) =>
		{
			try
			{
				args.Cancel = true;

				await Task.Delay(128);

				Visibility = Collapsed;
			}
			catch { }
		};

		IsVisibleChanged += async (sender, args) =>
		{
			var state = Visibility is Visible && WindowState is Minimized
				? Normal
				: WindowState;

			await Task.Delay(256);

			WindowState = state;
		};
	}
}
