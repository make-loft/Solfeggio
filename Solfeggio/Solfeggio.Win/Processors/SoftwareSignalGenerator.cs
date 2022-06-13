using Ace;

using Solfeggio.Api;
using Solfeggio.ViewModels;

using System;
using System.Linq;

namespace Solfeggio.Processors
{
	class SoftwareSignalGenerator : ASoftwareSignalProcessor
	{
		public class DeviceInfo : Wave.In.DeviceInfo
		{
			public DeviceInfo() : base(int.MinValue) { }

			public override string ProductName => "Software Signal Generator";

			public override WaveInCapabilities GetCapabilities() => throw new NotImplementedException();

			public override IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
				new SoftwareSignalGenerator(waveFormat.SampleRate, sampleSize, buffersCount);
		}

		public SoftwareSignalGenerator(int sampleRate, int sampleSize, int buffersCount)
			: base(sampleRate, sampleSize, buffersCount) { }

		private readonly HarmonicManager _manager = Store.Get<HarmonicManager>();

		public override short[] Next()
		{
			var signal = _manager.ActiveProfile.GenerateSignalSample(_sampleSize, _sampleRate, false);
			var bins = signal.Stretch(Level).Select(d => (short)(d * short.MaxValue / 2d)).ToArray();
			return bins.Scale(Boost);
		}
	}
}
