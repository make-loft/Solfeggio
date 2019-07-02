using Ace;
using System;
using System.Collections.Generic;

namespace Solfeggio.Api
{
	public class WaveInCapabilities
	{ }
	public static class Wave
	{
		public static short[] Scale(this short[] data, double boost)
		{
			if (boost.Is(1d)) return data;

			for (var i = 0; i < data.Length; i++)
			{
				var normalizedValue = (double)data[i] / short.MaxValue;
				data[i] = (short)(short.MaxValue * Math.Pow(normalizedValue, 1d / boost));
			}

			return data;
		}

		public static double[] Stretch(this double[] data, double volume)
		{
			if (volume.Is(1d)) return data;

			for (var i = 0; i < data.Length; i++)
				data[i] *= volume;

			return data;
		}

		public static class In
		{
			public static IEnumerable<DeviceInfo> EnumerateDevices()
			{
				yield break;
			}

			public class DeviceInfo
			{
				public DeviceInfo(int number) { }
				public virtual string ProductName { get; }
				public virtual IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
					default;

				public virtual WaveInCapabilities GetCapabilities() => default;
			}
		}

		public static class Out
		{
			public static IEnumerable<DeviceInfo> EnumerateDevices()
			{
				yield break;
			}

			public class DeviceInfo
			{
				public DeviceInfo(int number) { }
				public virtual string ProductName { get; }

				public virtual IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount, IProcessor inputProcessor) =>
					default;

				public virtual WaveInCapabilities GetCapabilities() => default;
			}
		}
	}
}