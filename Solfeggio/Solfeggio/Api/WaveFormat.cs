using System;
using System.Collections.Generic;
using System.Text;

namespace Solfeggio.Api
{
	public struct WaveFormat
	{
		public int SampleRate { get; set; }
		public WaveFormat(int sampleRate, int a, int channels)
		{
			SampleRate = sampleRate;
		}
	}
}
