using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
    [Flags]
    enum WaveOutSupport
    {
        Pitch = 0x0001,
        PlaybackRate = 0x0002,
        Volume = 0x0004,
        LRVolume = 0x0008,
        Sync = 0x0010,
        SampleAccurate = 0x0020,
    }
    
	public interface IDeviceCapabilities
	{
		string ProductName { get; }
	}


	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct WaveOutCapabilities : IDeviceCapabilities
	{
		private short manufacturerId;
		private short productId;
		private int driverVersion;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		private string productName;
		private SupportedWaveFormat supportedFormats;
		private short channels;
		private short reserved;
		private WaveOutSupport support; // = new WaveOutSupport();

		// extra WAVEOUTCAPS2 members

		private Guid manufacturerGuid;
		private Guid productGuid;
		private Guid nameGuid;

		public int Channels => channels;
		public bool SupportsPlaybackRateControl => (support & WaveOutSupport.PlaybackRate) == WaveOutSupport.PlaybackRate;

		public string ProductName => productName;
		public bool SupportsWaveFormat(SupportedWaveFormat waveFormat) => (supportedFormats & waveFormat) == waveFormat;
		public Guid NameGuid => nameGuid;
		public Guid ProductGuid => productGuid;
		public Guid ManufacturerGuid => manufacturerGuid;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct WaveInCapabilities : IDeviceCapabilities
	{
		private short manufacturerId;
		private short productId;
		private int driverVersion;
		private SupportedWaveFormat supportedFormats;
		private short channels;
		private short reserved;

		// extra WAVEINCAPS2 members

		public int Channels => channels;

		[field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string ProductName { get; }

		public Guid NameGuid { get; }
		public Guid ProductGuid { get; }
		public Guid ManufacturerGuid { get; }

		public bool SupportsWaveFormat(SupportedWaveFormat waveFormat) => (supportedFormats & waveFormat) == waveFormat;
	}
}