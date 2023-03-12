using Ace;

using Rainbow;

using System;

#if NETSTANDARD
using Xamarin.Forms;
#else
using System.Windows;
#endif

using static Rainbow.ScaleFuncs;

namespace Solfeggio.Presenters
{
	[DataContract]
	public class Bandwidth
	{
		public static readonly Projection[] ScaleFuncs =
			{ Lineal, Log2, Log, Exp, Sqrt };

		public static readonly Projection[] BackScaleFuncs =
			{ Lineal, _2Pow, Exp, Log, Pow2 };

		[DataMember] public string Units { get; set; }
		[DataMember] public SmartRange Scope { get; set; }
		[DataMember] public SmartRange Window { get; set; }
		[DataMember] public Projection[] VisualScaleFuncs { get; set; } = ScaleFuncs;
		[DataMember] public Projection VisualScaleFunc { get; set; } = Lineal;
		public Projection LogicalScaleFunc => BackScaleFuncs[Array.IndexOf(ScaleFuncs, VisualScaleFunc)];

		public void LimitThreshold()
		{
			var l = Scope;
			var t = Window;

			if (t.From < l.From) t.From = l.From;
			if (t.From > l.Till) t.From = l.Till;

			if (t.Till > l.Till) t.Till = l.Till;
			if (t.Till < l.From) t.Till = l.From;
		}

		public void ShiftThreshold(double from, double till) =>
			TransformThreshold(from, till, true);
		public void ScaleThreshold(double from, double till) =>
			TransformThreshold(from, till, false);

		public void TransformThreshold(double from, double till, bool shift)
		{
			if (VisualScaleFunc.Is(Lineal))
			{
				var offset = shift ? from - till : till - from;

				Window.From += shift ? +offset : -offset;
				Window.Till += offset;
			}
			else
			{
				var scale = shift ? from / till : till / from;

				Window.From *= shift ? scale : (1 / scale);
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

	public static class BandwidthExtensions
	{
		public static void Transform(this Bandwidth bandwidth, double width, double offset, bool shift)
		{
			var center = width / 2d;
			var basicScaler = MusicalPresenter.GetScaleTransformer(bandwidth, width);
			var from = basicScaler.GetLogicalOffset(center);
			var till = basicScaler.GetLogicalOffset(center + offset);
			bandwidth.TransformThreshold(from, till, shift);
		}

		public static void TransformRelative(this Bandwidth bandwidth, double width, double height, Point from, Point till)
		{
			var center = width / 2d;

			var alignScaler = MusicalPresenter.GetScaleTransformer(bandwidth, width);
			var alignFromOffset = alignScaler.GetLogicalOffset(from.X);
			var alignTillOffset = alignScaler.GetLogicalOffset(center);
			bandwidth.ShiftThreshold(alignFromOffset, alignTillOffset);

			var basicScaler = MusicalPresenter.GetScaleTransformer(bandwidth, height);
			var centerY = height / 2d;
			var offsetY = from.Y - till.Y;
			var basicFromOffset = basicScaler.GetLogicalOffset(centerY);
			var basicTillOffset = basicScaler.GetLogicalOffset(centerY + offsetY);
			bandwidth.ScaleThreshold(basicFromOffset, basicTillOffset);

			var finalScaler = MusicalPresenter.GetScaleTransformer(bandwidth, width);
			var finalFromOffset = finalScaler.GetLogicalOffset(center);
			var finalTillOffset = finalScaler.GetLogicalOffset(from.X);
			bandwidth.ShiftThreshold(finalFromOffset, finalTillOffset);
		}
	}
}
