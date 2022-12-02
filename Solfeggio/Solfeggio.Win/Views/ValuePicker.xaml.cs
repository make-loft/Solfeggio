using Ace;
using Ace.Markup.Patterns;

using System;

namespace Solfeggio.Views
{
	public partial class ValuePicker
	{
		public ValuePicker() => InitializeComponent();

		object LogBase2(ConvertArgs args) => args.Value.To(out double d) > 0d ? Math.Log(d, 2d) : -16d;
		object PowBase2(ConvertArgs args) => Math.Pow(2d, (double)args.Value);
		object ExpandRange(ConvertArgs args) => ExpandRange((double)args.Value);
		double ExpandRange(double value)
		{
			if (Maximum < value) Maximum = value;
			if (Minimum > value) Minimum = value;
			return value;
		}
	}
}
