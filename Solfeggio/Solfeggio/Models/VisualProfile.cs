using Ace;
using Ace.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Solfeggio.Presenters;
#if NETSTANDARD
using Xamarin.Forms;
using Color = Xamarin.Forms.Color;
using Colors = Xamarin.Forms.Color;
using static Xamarin.Forms.Color;
#else
using System.Windows.Media;
using static System.Windows.Media.Colors;
#endif

namespace Solfeggio.Models
{
	[DataContract]
	public class PeakProfile
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

		private static PeakProfile CreateProfile(
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

		public PeakProfile ActualMagnitude => GetProfile();
		public PeakProfile ActualFrequency => GetProfile();
		public PeakProfile OffsetFrequency => GetProfile();
		public PeakProfile EthalonFrequency => GetProfile();
		public PeakProfile NoteName => GetProfile();

		static readonly int PointsCount = 12;
		public static Color[] Rainbow = Enumerable.Range(0, PointsCount).Select(i => Colority.FromHSVA((double)i / PointsCount, 1d, 1d)).ToArray();

		public Brush[] NoteBrushes = Rainbow.Select(c => new SolidColorBrush(c).DoFreeze()).ToArray();
		public Brush[] NoteTextBrushes = Rainbow.Select(c => new SolidColorBrush(c).DoFreeze()).ToArray();

		private PeakProfile GetProfile([CallerMemberName]string key = default) => PeakProfiles[key];

		//[DataMember]
		public Dictionary<string, PeakProfile> PeakProfiles { get; set; } = new()
		{
			{ nameof(ActualMagnitude), CreateProfile(Colority.FromHEX("#60FFFFFF"), 12d) },
			{ nameof(ActualFrequency), CreateProfile(Colority.FromHEX("#A0FFFFFF"), 14d) },
			{ nameof(OffsetFrequency), CreateProfile(Colority.FromHEX("#FFFFFFFF"), 16d, "+0.0;−0.0;•0.0") },
			{ nameof(EthalonFrequency), CreateProfile(Colority.FromHEX("#A0FFFFFF"), 14d) },
			{ nameof(NoteName), CreateProfile(Colority.FromHEX("#FFFFFFFF"), 16d, default) },
		};
	}
}
