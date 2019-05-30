using Ace;
using Solfeggio.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using static System.Windows.Media.Colors;

namespace Solfeggio.Models
{
	[DataContract]
	public class TopProfile
	{
		public Brush Brush { get; set; }
		[DataMember] public FontFamily FontFamily { get; set; }
		[DataMember] public double FontSize { get; set; }
		[DataMember] public string StringFormat { get; set; }
		[DataMember] public bool IsVisible { get; set; }
	}

	//[DataContract]
	public class VisualProfile : ContextObject
	{
		public SmartSet<Brush> Brushes { get; } = EnumerateColors().Select(c => (Brush)new SolidColorBrush(c)).ToSet();

		public SmartSet<double> FontSizes { get; } = new[] { 5d, 6d, 7d, 8d, 9d, 10d, 11d, 12d, 14d, 16d, 18d }.ToSet();

		private static readonly Type ColorType = typeof(Color);

		public static IEnumerable<Color> EnumerateColors() => typeof(Colors)
			.GetProperties()
			.Where(p => p.PropertyType.Is(ColorType))
			.Select(p => (Color)p.GetValue(null));

		public Brush TopBrush = new SolidColorBrush(BurlyWood) { Opacity = 0.1d };

		public Color FirstColor
		{
			get => App.Current.Resources["SpectumBackgroundBrush"].To<LinearGradientBrush>().GradientStops[0].Color;
			set => App.Current.Resources["SpectumBackgroundBrush"].To<LinearGradientBrush>().GradientStops[0].Color = value;
		}
		public Color SecondColor
		{
			get => App.Current.Resources["SpectumBackgroundBrush"].To<LinearGradientBrush>().GradientStops[1].Color;
			set => App.Current.Resources["SpectumBackgroundBrush"].To<LinearGradientBrush>().GradientStops[1].Color = value;
		}

		[DataMember] public bool Harmonics
		{
			get => Get(() => Harmonics);
			set => Set(() => Harmonics, value);
		}

		[DataMember] public bool NotesGrid
		{
			get => Get(() => NotesGrid);
			set => Set(() => NotesGrid, value);
		}

		private static TopProfile CreateProfile(
			Color color,
			double fontSize,
			string stringFormat = "{0:0.0;-0.0;0.0}",
			bool isVisible = true) => new TopProfile
			{
				Brush = color.Is(Transparent) ? default : new SolidColorBrush(color).DoFreeze(),
				FontFamily = new FontFamily("Consolas"),
				StringFormat = stringFormat,
				IsVisible = isVisible,
				FontSize = fontSize,
			};

		public TopProfile ActualMagnitude => GetProfile();
		public TopProfile ActualFrequancy => GetProfile();
		public TopProfile DeltaFrequancy => GetProfile();
		public TopProfile EthalonFrequncy => GetProfile();
		public TopProfile NoteName => GetProfile();

		public static Color[] Rainbow =	new[]
		{
			PaleVioletRed,
			SkyBlue, //Color.Multiply(SkyBlue, 0.5f),
			Orange,
			Blue, //Color.Multiply(Blue, 0.5f),
			Yellow,

			YellowGreen,
			PaleVioletRed, //Color.Multiply(Red, 0.5f),
			SkyBlue,
			Orange, //Color.Multiply(Orange, 0.5f),
			Blue,
			Yellow, //Color.Multiply(Yellow, 0.5f),
			BlueViolet
		};

		public Brush[] NoteBrushes = Rainbow.Select(c => new SolidColorBrush(c) { Opacity = 0.1d }.DoFreeze()).ToArray();
		public Brush[] NoteTextBrushes = Rainbow.Select(c => new SolidColorBrush(c) { Opacity = 1d }.DoFreeze()).ToArray();

		private TopProfile GetProfile([CallerMemberName]string key = default) => TopProfiles[key];

		[DataMember]
		public Dictionary<string, TopProfile> TopProfiles { get; set; } = new Dictionary<string, TopProfile>
		{
			{ nameof(ActualMagnitude), CreateProfile(White, 12d) },
			{ nameof(ActualFrequancy), CreateProfile(White, 14d) },
			{ nameof(DeltaFrequancy), CreateProfile(Color.FromRgb(32, 33, 36), 16d, "{0:+0.0;-0.0; 0.0}") },
			{ nameof(EthalonFrequncy), CreateProfile(White, 14d) },
			{ nameof(NoteName), CreateProfile(White, 16d, default) },
		};
	}
}
