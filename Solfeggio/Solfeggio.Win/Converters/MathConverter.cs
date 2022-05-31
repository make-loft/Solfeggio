using System;
using Ace;
using Ace.Converters.Patterns;

using Rainbow;

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

		private double GetBase(object parameter) =>
			parameter.Is(out double value) || double.TryParse(parameter.ToStr(), out value) ? value : Parameter;

		public override object Convert(object value, object parameter) =>
			Apply(Operation, (double)value, GetBase(parameter));

		public override object ConvertBack(object value, object parameter) =>
			Apply(Back(Operation), GetBase(parameter), (double)value);

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

		private Operations Back(Operations code) =>
			code.Is(Identity) ? Identity :
			code.Is(Negation) ? Negation :
			code.Is(Increment) ? Decrement :
			code.Is(Decrement) ? Increment :
			code.Is(Stretch) ? Squeeze :
			code.Is(Squeeze) ? Stretch :
			code.Is(Pow) ? Log :
			code.Is(Log) ? Pow :
			throw new ArgumentException();
	}
}
