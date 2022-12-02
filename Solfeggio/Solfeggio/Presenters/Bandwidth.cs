using Ace;

using Rainbow;

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

		[DataMember] public SmartRange Limit { get; set; }
		[DataMember] public SmartRange Threshold { get; set; }
		[DataMember] public Projection[] VisualScaleFuncs { get; set; } = ScaleFuncs;
		[DataMember] public Projection VisualScaleFunc { get; set; } = Lineal;

		public void LimitThreshold()
		{
			var l = Limit;
			var t = Threshold;

			if (t.Lower < l.Lower) t.Lower = l.Lower;
			if (t.Lower > l.Upper) t.Lower = l.Upper;

			if (t.Upper > l.Upper) t.Upper = l.Upper;
			if (t.Upper < l.Lower) t.Upper = l.Lower;
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

				Threshold.Lower += shift ? +offset : -offset;
				Threshold.Upper += offset;
			}
			else
			{
				var scale = shift ? from / till : till / from;

				Threshold.Lower *= shift ? scale : (1 / scale);
				Threshold.Upper *= scale;
			}

			if (Threshold.Lower < Limit.Lower)
				Threshold.Lower = Limit.Lower;

			if (Threshold.Upper > Limit.Upper)
				Threshold.Upper = Limit.Upper;

			if (Threshold.Length < Limit.Length / 1024d && Limit.Lower < Limit.Upper)
			{
				Threshold.Lower = Limit.Lower;
				Threshold.Upper = Limit.Upper;
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
