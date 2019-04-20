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
    
	public interface IWaveCapabilities { }


	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WaveOutCapabilities : IWaveCapabilities
    {
        private short manufacturerId;
        private short productId;
        private int driverVersion;
        private SupportedWaveFormat supportedFormats;
        private short channels;
        private short reserved;
        /// <summary>
        /// Optional functionality supported by the device
        /// </summary>
        private WaveOutSupport support; // = new WaveOutSupport();

        // extra WAVEOUTCAPS2 members

        private const int MaxProductNameLength = 32;

        public int Channels => channels;
        public bool SupportsPlaybackRateControl => (support & WaveOutSupport.PlaybackRate) == WaveOutSupport.PlaybackRate;

        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxProductNameLength)] public string ProductName { get; }

		public bool SupportsWaveFormat(SupportedWaveFormat waveFormat) => (supportedFormats & waveFormat) == waveFormat;

		public Guid NameGuid { get; }
        public Guid ProductGuid { get; }
        public Guid ManufacturerGuid { get; }
    }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct WaveInCapabilities : IWaveCapabilities
	{
		private short manufacturerId;
		private short productId;
		private int driverVersion;
		private SupportedWaveFormat supportedFormats;
		private short channels;
		private short reserved;

		// extra WAVEINCAPS2 members

		private const int MaxProductNameLength = 32;

		public int Channels => channels;

		[field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxProductNameLength)] public string ProductName { get; }

		public Guid NameGuid { get; }
		public Guid ProductGuid { get; }
		public Guid ManufacturerGuid { get; }

		public bool SupportsWaveFormat(SupportedWaveFormat waveFormat) => (supportedFormats & waveFormat) == waveFormat;
	}
}