using Ace;
using System;
using System.Collections.Generic;

namespace Solfeggio.Api
{
	public class WaveInCapabilities
	{ }
	public static class Wave
	{
		public static float[] StretchArray(this float[] data, float value)
		{
			if (value.Is(1f)) return data;

			for (var i = 0; i < data.Length; i++)
				data[i] *= value;

			return data;
		}

		public static class In
		{
			public static IEnumerable<DeviceInfo> EnumerateDevices()
			{
				yield return new MicrophoneDeviceInfo();
			}


			public class MicrophoneDeviceInfo : DeviceInfo
			{
				public delegate IProcessor CreateProcessorDelegate(WaveFormat waveFormat, int sampleSize, int buffersCount);

				public static CreateProcessorDelegate ProvideDevice { get; set; }

				public MicrophoneDeviceInfo() : base(0) { }
				public override string ProductName => "Microphone";

				public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
					ProvideDevice(waveFormat, sampleSize, buffersCount);
			}

			public abstract class DeviceInfo
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
				yield return new SpeakerDeviceInfo();
			}


			public class SpeakerDeviceInfo : DeviceInfo
			{
				public delegate IProcessor CreateProcessorDelegate(WaveFormat waveFormat, int sampleSize, int buffersCount, IProcessor inputProcessor);

				public static CreateProcessorDelegate ProvideDevice { get; set; }

				public SpeakerDeviceInfo() : base(0) { }
				public override string ProductName => "Speaker";

				public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount, IProcessor inputProcessor) =>
					ProvideDevice(waveFormat, sampleSize, buffersCount, inputProcessor);
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