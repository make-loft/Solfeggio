using Ace;
using Ace.Markup;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Solfeggio.Models
{
	[DataContract]
	public class VisualizationProfile : AProfile, IExposable
	{
		public static string VisualizationsDirectoryName = "Visualizations";

		[DataMember] public int Id { get; set; }
		[DataMember] public string Palette { get; set; }
		public string FileName => $"{VisualizationsDirectoryName}\\{Title}.{Id}";

		public bool IsBusy
		{
			get => Get(() => IsBusy, false);
			set => Set(() => IsBusy, value);
		}

		public VisualizationProfile() => Expose();

		public void Expose()
		{
			var oldFileName = Ace.Store.ActiveBox.KeyFormat.Format(FileName);
			var newFileName = Ace.Store.ActiveBox.KeyFormat.Format(FileName);
			var oldTitle = Title;

			this[() => Title].PropertyChanging += (o, e) =>
			{
				oldTitle = Title;
				oldFileName = Ace.Store.ActiveBox.KeyFormat.Format(FileName);
			};

			this[() => Title].PropertyChanged += (o, e) =>
			{
				newFileName = Ace.Store.ActiveBox.KeyFormat.Format(FileName);
				try
				{
					if (File.Exists(oldFileName))
						File.Move(oldFileName, newFileName);
				}
				catch
				{
					if (Title.IsNot(oldTitle))
						Title = oldTitle;
				}
			};
		}

		public static IEnumerable<object> EnumerateAllKeys(ResourceDictionary dictionary)
		{
			foreach (var d in dictionary.MergedDictionaries)
			{
				var keys = EnumerateAllKeys(d);
				foreach (var key in keys)
					yield return key;
			}

			foreach (var key in dictionary.Keys)
				yield return key;
		}

		const int AsyncDelay = 64;

		public async void Keep(bool asyncDelay = true)
		{
			var resources = AppPalette.Resources;
			var valueKeys = AppPalette.Values.Keys.OfType<string>();
			var colorsKeys = EnumerateAllKeys(AppPalette.Colors).OfType<string>().Distinct();
			var brushKeys = AppPalette.Brushes.Keys.OfType<string>();

			var theme =
				colorsKeys.OrderBy()
				.Concat(valueKeys.Concat(brushKeys).OrderBy())
				.ToDictionary(k => k, k => resources[k]);

			if (asyncDelay)
				await Task.Delay(AsyncDelay);

			Ace.Store.ActiveBox.TryKeep(theme, FileName);
		}

		public async void Load(bool asyncDelay = true)
		{
			IsBusy = true;

			if (asyncDelay)
				await Task.Delay(AsyncDelay);

			Reset();

			if (Ace.Store.ActiveBox.Check<object>(FileName).Not())
			{
				IsBusy = false;
				Keep();
				return;
			}

			var resources = AppPalette.Resources;
			var values = AppPalette.Values;
			var colors = new Map();
			var theme = Ace.Store.ActiveBox.Revive<Dictionary<string, object>>(FileName);

			theme.ForEach(p =>
			{
				if (p.Value is Color)
					colors[p.Key] = p.Value;
				else resources[p.Key] = p.Value;
			});

			resources.MergedDictionaries[3] = colors;
			resources.EvokePropertyChanged();

			IsBusy = false;
		}

		public void Reset(string paletteKey = default)
		{
			Palette = paletteKey ?? Palette ?? "Nature";
			var resources = AppPalette.Resources;
			var brushes = AppPalette.Brushes;
			var palettes = AppPalette.ColorPalettes;
			resources.MergedDictionaries[3] = (Map)palettes[Palette];
			foreach (var key in brushes.Keys)
			{
				if (brushes[key].Is(out Brush b))
					resources[key] = b.Clone();
			}

			resources.EvokePropertyChanged();
		}
	}
}
