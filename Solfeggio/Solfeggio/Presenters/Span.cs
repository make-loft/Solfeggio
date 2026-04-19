using Ace;

using Rainbow;

using System;

#if NETSTANDARD
using Xamarin.Forms;
#else
using System.Windows;
#endif

using static Rainbow.ScaleFuncs;

namespace Solfeggio.Presenters;

public enum OffsetMode { Shift, Scale }

[DataContract]
public class Span
{
	public static readonly Projection[] ScaleFuncs
		= [ Lineal, Log2, Log, Exp, Sqrt ];

	public static readonly Projection[] BackScaleFuncs
		= [ Lineal, _2Pow, Exp, Log, Pow2 ];

	[DataMember] public string Units { get; set; }
	[DataMember] public SmartRange Scope { get; set; }
	[DataMember] public SmartRange Window { get; set; }
	[DataMember] public Projection[] VisualScaleFuncs { get; set; } = ScaleFuncs;
	[DataMember] public Projection VisualScaleFunc { get; set; } = Lineal;
	public Projection LogicalScaleFunc => BackScaleFuncs[Array.IndexOf(ScaleFuncs, VisualScaleFunc)];

	public void LimitWindow()
	{
		var scope = Scope;
		var window = Window;

		if (window.From < scope.From) window.From = scope.From;
		if (window.From > scope.Till) window.From = scope.Till;

		if (window.Till > scope.Till) window.Till = scope.Till;
		if (window.Till < scope.From) window.Till = scope.From;
	}

	public void TransformWindow(double fromOffset, double tillOffset, OffsetMode mode)
	{
		if (VisualScaleFunc.Is(Lineal))
		{
			var isShiftMode = mode.Is(OffsetMode.Shift);
			var valueOffset = isShiftMode ? fromOffset - tillOffset : tillOffset - fromOffset;

			Window.From += isShiftMode ? +valueOffset : -valueOffset;
			Window.Till += valueOffset;
		}
		else
		{
			var isScaleMode = mode.Is(OffsetMode.Scale);
			var scale = isScaleMode ? tillOffset / fromOffset : fromOffset / tillOffset;

			Window.From *= isScaleMode ? (1 / scale) : scale;
			Window.Till *= scale;
		}

		if (Window.From < Scope.From)
			Window.From = Scope.From;

		if (Window.Till > Scope.Till)
			Window.Till = Scope.Till;

		if (Window.Length < Scope.Length / 1024d && Scope.From < Scope.Till)
		{
			Window.From = Scope.From;
			Window.Till = Scope.Till;
		}
	}
}

public static class SpanExtensions
{
	public static void Transform(this Span span, double width, double offset, bool shift)
	{
		var center = width / 2d;
		var finalTransformer = MusicalPresenter.GetScaleTransformer(span, width);
		var finalFromOffset = finalTransformer.GetLogicalOffset(center);
		var finalTillOffset = finalTransformer.GetLogicalOffset(center + offset);
		var offsetMode = shift ? OffsetMode.Shift : OffsetMode.Scale;
		span.TransformWindow(finalFromOffset, finalTillOffset, offsetMode);
	}

	public static void TransformRelative(this Span span, double width, double height, Point from, Point till)
	{
		var center = width / 2d;
		var alignTransformer = MusicalPresenter.GetScaleTransformer(span, width);
		var alignFromOffset = alignTransformer.GetLogicalOffset(from.X);
		var alignTillOffset = alignTransformer.GetLogicalOffset(center);
		span.TransformWindow(alignFromOffset, alignTillOffset, OffsetMode.Shift);

		var basicTransformer = MusicalPresenter.GetScaleTransformer(span, height);
		var centerY = height / 2d;
		var offsetY = from.Y - till.Y;
		var basicFromOffset = basicTransformer.GetLogicalOffset(centerY);
		var basicTillOffset = basicTransformer.GetLogicalOffset(centerY + offsetY);
		span.TransformWindow(basicFromOffset, basicTillOffset, OffsetMode.Scale);

		var finalTransformer = MusicalPresenter.GetScaleTransformer(span, width);
		var finalFromOffset = finalTransformer.GetLogicalOffset(center);
		var finalTillOffset = finalTransformer.GetLogicalOffset(from.X);
		span.TransformWindow(finalFromOffset, finalTillOffset, OffsetMode.Shift);
	}
}
