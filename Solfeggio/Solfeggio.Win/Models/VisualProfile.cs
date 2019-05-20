using Ace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Solfeggio.Models
{
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

		[DataMember]
		public Dictionary<string, Segregator<Brush>> AppBrushes { get; set; } = new Dictionary<string, Segregator<Brush>>
		{
			{"TopBrush",  new Segregator<Brush> { Value = new SolidColorBrush(Colors.WhiteSmoke) { Opacity = 0.1d } } },

			{"ActualMagnitude",  new Segregator<Brush> { Value = new SolidColorBrush(Colors.White) } },
			{"ActualFrequancy",  new Segregator<Brush> { Value = new SolidColorBrush(Colors.White) } },
			{"EthalonFrequncy",  new Segregator<Brush> { Value = new SolidColorBrush(Colors.GreenYellow) } },
			{"NoteName",  new Segregator<Brush> { Value = new SolidColorBrush(Colors.GreenYellow) } },
		};

		[DataMember]
		public Dictionary<string, Segregator<double>> AppFontSizes { get; set; } = new Dictionary<string, Segregator<double>>
		{
			{"ActualMagnitude",  new Segregator<double> { Value = 8d } },
			{"ActualFrequancy",  new Segregator<double> { Value = 10d } },
			{"EthalonFrequncy",  new Segregator<double> { Value = 10d } },
			{"NoteName",  new Segregator<double> { Value = 10d } },
		};
	}
}
