namespace Solfeggio.Extensions
{
	public static class SignalExtensions
	{
		public static float[] Stretch(this float[] data, float volume)
		{
			if (volume == 1f) return data;

			for (var i = 0; i < data.Length; i++)
				data[i] *= volume;

			return data;
		}

		public static float[] StretchArray(this float[] data, float value)
		{
			if (value == 1f) return data;

			for (var i = 0; i < data.Length; i++)
				data[i] *= value;

			return data;
		}

		public static float[] Add(this float[] signal, float[] signalB)
		{
			for (var i = 0; i < signal.Length; i++)
				signal[i] = signal[i] + signalB[i];
			
			return signal;
		}

		public static float[] Raise(this float[] signal, float fromOffset = 0f, float tillOffset = 1f)
		{
			var from = (int)(signal.Length * fromOffset);
			var till = (int)(signal.Length * tillOffset);
			var length = (float)(till - from);

			for (var i = from; i < till; i++)
				signal[i] *= (i - from) / length;

			for (var i = 0; i < from; i++)
				signal[i] = 0f;

			return signal;
		}

		public static float[] Fade(this float[] signal, float fromOffset = 0f, float tillOffset = 1f)
		{
			var from = (int)(signal.Length * fromOffset);
			var till = (int)(signal.Length * tillOffset);
			var length = (float)(till - from);

			for (var i = from; i < till; i++)
				signal[i] *= 1f - (i - from) / length;

			for (var i = till; i < signal.Length; i++)
				signal[i] = 0f;

			return signal;
		}
	}
}
