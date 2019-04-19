using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
    [Flags]
    enum WaveOutSupport
    {
        /// <summary>supports pitch control (WAVECAPS_PITCH)</summary>
        Pitch = 0x0001,
        /// <summary>supports playback rate control (WAVECAPS_PLAYBACKRATE)</summary>
        PlaybackRate = 0x0002,
        /// <summary>supports volume control (WAVECAPS_VOLUME)</summary>
        Volume = 0x0004,
        /// <summary>supports separate left-right volume control (WAVECAPS_LRVOLUME)</summary>
        LRVolume = 0x0008,
        /// <summary>(WAVECAPS_SYNC)</summary>
        Sync = 0x0010,
        /// <summary>(WAVECAPS_SAMPLEACCURATE)</summary>
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