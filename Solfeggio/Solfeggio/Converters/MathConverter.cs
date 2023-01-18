using Ace;
using Ace.Markup.Patterns;

using System;

using Rainbow;
using System.Globalization;

namespace Solfeggio.Converters
{
	using static Operations;

	public enum Operations
	{
		Identity, Negation, Increment, Decrement, Stretch, Squeeze, Log, Pow
	}

	public class MathConverter : AValueConverter
	{
		public MathConverter() => Parameter = Math.E;

		public double Parameter { get; set; }
		public Operations Operation { get; set; }

		public override object Convert(object value, object parameter) =>
			TryCast(value, out var v) && TryCast(parameter ?? Parameter, out var b)
				? Apply(Operation, v, b)
				: value;

		public override object ConvertBack(object value, object parameter) =>
			TryCast(value, out var v) && TryCast(parameter ?? Parameter, out var b)
				? Apply(Back(Operation), b, v)
				: value;

		private double Apply(Operations code, double value, double parameter) =>
			code.Is(Identity) ? value.Identity() :
			code.Is(Negation) ? value.Negation() :
			code.Is(Increment) ? value.Increment(parameter) :
			code.Is(Decrement) ? value.Decrement(parameter) :
			code.Is(Stretch) ? value.Stretch(parameter) :
			code.Is(Squeeze) ? value.Squeeze(parameter) :
			code.Is(Pow) ? Math.Pow(value, parameter) :
			code.Is(Log) ? Math.Log(value, parameter) :
			throw new ArgumentException();

		Operations Back(Operations code) =>
			code.Is(Identity) ? Identity :
			code.Is(Negation) ? Negation :
			code.Is(Increment) ? Decrement :
			code.Is(Decrement) ? Increment :
			code.Is(Stretch) ? Squeeze :
			code.Is(Squeeze) ? Stretch :
			code.Is(Pow) ? Log :
			code.Is(Log) ? Pow :
			throw new ArgumentException();

		bool TryCast(object value, out double v) =>
			Cast(value).To(out v).IsNot(double.NaN) ||
			value.To<string>().TryParse(out v, NumberFormatInfo.CurrentInfo);

		double Cast(object value) => value switch
		{
			double v => v,
			float v => v,
			ulong v => v,
			long v => v,
			ushort v => v,
			short v => v,
			uint v => v,
			int v => v,
			_ => double.NaN
		};
	}
}
