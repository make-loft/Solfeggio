using Ace;

using Solfeggio.Api;
using Solfeggio.Extensions;
using Solfeggio.ViewModels;

using System;

namespace Solfeggio.Processors
{
	class SoftwareSignalGenerator : ASoftwareSignalProcessor
	{
		public class DeviceInfo : Wave.In.DeviceInfo
		{
			public DeviceInfo() : base(int.MinValue) { }

			public override string ProductName => "Generator";

			public override WaveInCapabilities GetCapabilities() => throw new NotImplementedException();

			public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
				new SoftwareSignalGenerator(waveFormat.SampleRate, sampleSize);
		}

		public SoftwareSignalGenerator(int sampleRate, int sampleSize)
			: base(sampleRate, sampleSize) { }

		private readonly HarmonicManager _manager = Store.Get<HarmonicManager>();

		public override float[] Next()
		{
			var sample = NextSample();
			EvokeDataAvailable(sample);
			return sample;
		}

		public float[] NextSample() => _manager.ActiveProfile
			.GenerateSignalSample(SampleSize, SampleRate, false)
			.StretchArray(Level * Boost);
	}
}
