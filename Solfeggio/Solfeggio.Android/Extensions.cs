using Android.App;
using Android.Content;

using System.Threading.Tasks;

namespace Solfeggio.Droid
{
	static class Extensions
	{
		public static async Task<bool?> ShowAlertDialogAsync(this Context context,
			string title, string message,
			string positive = default,
			string negative = default,
			string neutral = default)
		{
			var source = new TaskCompletionSource<bool?>();

			new AlertDialog.Builder(context).SetTitle(title).SetMessage(message)
				.SetPositiveButton(positive, (sender, args) => source.SetResult(true))
				.SetNegativeButton(negative, (sender, args) => source.SetResult(false))
				.SetNeutralButton(neutral, (sender, args) => source.SetResult(default))
				.Show();

			return await source.Task;
		}
	}
}