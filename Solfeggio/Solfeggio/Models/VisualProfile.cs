using Ace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Solfeggio.Presenters;
using System.Windows.Controls;
using Ace.Zest.Extensions;
using System.Windows;
#if NETSTANDARD
using Xamarin.Forms;
using Color = Xamarin.Forms.Color;
using Colors = Xamarin.Forms.Color;
using FontFamily = System.String;
using static Xamarin.Forms.Color;
#else
using System.Windows.Media;
using static System.Windows.Media.Colors;
#endif

namespace Solfeggio.Models
{
	[DataContract]
	public class TopProfile
	{
		public Brush Brush { get; set; }
		[DataMember] public string FontFamilyName { get; set; }
		[DataMember] public double FontSize { get; set; }
		[DataMember] public string StringFormat { get; set; }
		[DataMember] public bool IsVisible { get; set; }
	}

	[DataContract]
	public class VisualProfile : ContextObject
	{
		public SmartSet<Brush> Brushes { get; } = EnumerateColors().Select(c => (Brush)new SolidColorBrush(c)).ToSet();

		public SmartSet<double> FontSizes { get; } = new[] { 5d, 6d, 7d, 8d, 9d, 10d, 11d, 12d, 14d, 16d, 18d }.ToSet();

		private static readonly Type ColorType = typeof(Color);

		public static IEnumerable<Color> EnumerateColors() => typeof(Colors)
			.GetProperties()
			.Where(p => p.PropertyType.Is(ColorType))
			.Select(p => (Color)p.GetValue(null));

		public Brush TopBrush = new SolidColorBrush(BurlyWood);

		[DataMember] public bool Harmonics
		{
			get => Get(() => Harmonics);
			set => Set(() => Harmonics, value);
		}

		private static TopProfile CreateProfile(
			Color color,
			double fontSize,
			string stringFormat = default,
			bool isVisible = true) => new()
			{
				Brush = color.Is(Transparent) ? default : new SolidColorBrush(color).DoFreeze(),
				FontFamilyName = "Consolas",
				StringFormat = stringFormat,
				IsVisible = isVisible,
				FontSize = fontSize,
			};

		public TopProfile ActualMagnitude => GetProfile();
		public TopProfile ActualFrequancy => GetProfile();
		public TopProfile DeltaFrequancy => GetProfile();
		public TopProfile EthalonFrequncy => GetProfile();
		public TopProfile NoteName => GetProfile();

		static readonly Window RainbowView = new()
		{
			Width = 120,
			Height = 120,
			WindowStyle = WindowStyle.None,
			ResizeMode = ResizeMode.NoResize,
			WindowStartupLocation = WindowStartupLocation.CenterScreen,
			Background = (Brush)App.Current.Resources["RainbowBrush.H"]
		};

		static VisualProfile()
		{
			RainbowView.Show();
			Rainbow = Enumerable.Range(0, PointsCount)
				.Select(i => RainbowView.GetPixelColor(new(RainbowView.ActualWidth * i / PointsCount, 0)))
				.ToArray();
			RainbowView.Close();
		}

		static readonly int PointsCount = 12;
		public static Color[] Rainbow;

		public Brush[] NoteBrushes = Rainbow.Select(c => new SolidColorBrush(c).DoFreeze()).ToArray();
		public Brush[] NoteTextBrushes = Rainbow.Select(c => new SolidColorBrush(c).DoFreeze()).ToArray();

		private TopProfile GetProfile([CallerMemberName]string key = default) => TopProfiles[key];

		//[DataMember]
		public Dictionary<string, TopProfile> TopProfiles { get; set; } = new()
		{
			{ nameof(ActualMagnitude), CreateProfile(White, 12d) },
			{ nameof(ActualFrequancy), CreateProfile(White, 14d) },
			{ nameof(DeltaFrequancy), CreateProfile(Color.FromRgb(32, 33, 36), 16d, "+0.0;-0.0; 0.0") },
			{ nameof(EthalonFrequncy), CreateProfile(White, 14d) },
			{ nameof(NoteName), CreateProfile(White, 16d, default) },
		};
	}
}
