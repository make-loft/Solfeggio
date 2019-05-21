﻿using Ace;
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

		public Brush TopBrush = new SolidColorBrush(BurlyWood) { Opacity = 0.1d };

		[DataMember] public bool Wave { get; set; } = false;
		[DataMember] public bool Dominants { get; set; } = true;
		[DataMember] public bool NotesGrid { get; set; } = false;

		private static TopProfile CreateProfile(
			Color color,
			double fontSize,
			string stringFormat = "{0:0.0;-0.0;0.0}",
			bool isVisible = true) => new TopProfile
			{
				Brush = color.Is(Transparent) ? default : new SolidColorBrush(color),
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
			Red,
			Color.Multiply(SkyBlue, 0.5f),
			Orange,
			Color.Multiply(Blue, 0.5f),
			Yellow,

			Green,
			Color.Multiply(Red, 0.5f),
			SkyBlue,
			Color.Multiply(Orange, 0.5f),
			Blue,
			Color.Multiply(Yellow, 0.5f),
			Violet
		};

		public Brush[] NoteBrushes = Rainbow.Reverse()
			.Select(c => new SolidColorBrush(Color.Subtract(White, c)) { Opacity = 0.1d }).ToArray();
		public Brush[] NoteTextBrushes = Rainbow.Select(c => new SolidColorBrush(c) { Opacity = 1d }).ToArray();

		private TopProfile GetProfile([CallerMemberName]string key = default) => TopProfiles[key];

		[DataMember]
		public Dictionary<string, TopProfile> TopProfiles { get; set; } = new Dictionary<string, TopProfile>
		{
			{ nameof(ActualMagnitude), CreateProfile(White, 10d) },
			{ nameof(ActualFrequancy), CreateProfile(White, 12d) },
			{ nameof(DeltaFrequancy), CreateProfile(LightBlue, 12d, "{0:+0.0;-0.0; 0.0}") },
			{ nameof(EthalonFrequncy), CreateProfile(Transparent, 12d) },
			{ nameof(NoteName), CreateProfile(Transparent, 14d, default) },
		};
	}
}
