namespace Solfeggio.Extensions
{
	static class SignalExtensions
	{
		public static double[] Add(this double[] signal, double[] signalB)
		{
			for (var i = 0; i < signal.Length; i++) signal[i] = signal[i] + signalB[i];
			return signal;
		}

		public static double[] Raise(this double[] signal, double fromOffset = 0d, double tillOffset = 1d)
		{
			var from = (int)(signal.Length * fromOffset);
			var till = (int)(signal.Length * tillOffset);
			var length = (double)(till - from);

			for (var i = from; i < till; i++)
				signal[i] *= (i - from) / length;

			for (var i = 0; i < from; i++)
				signal[i] = 0d;

			return signal;
		}

		public static double[] Fade(this double[] signal, double fromOffset = 0d, double tillOffset = 1d)
		{
			var from = (int)(signal.Length * fromOffset);
			var till = (int)(signal.Length * tillOffset);
			var length = (double)(till - from);

			for (var i = from; i < till; i++)
				signal[i] *= (1d - (i - from) / length);

			for (var i = till; i < signal.Length; i++)
				signal[i] = 0d;

			return signal;
		}
	}
}
