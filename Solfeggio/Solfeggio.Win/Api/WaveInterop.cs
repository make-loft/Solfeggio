using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	// http://msdn.microsoft.com/en-us/library/dd757347(v=VS.85).aspx
	[StructLayout(LayoutKind.Explicit)]
	public struct MmTime
	{
		public const int TIME_MS = 0x0001;
		public const int TIME_SAMPLES = 0x0002;
		public const int TIME_BYTES = 0x0004;

		[FieldOffset(0)]
		public UInt32 wType;
		[FieldOffset(4)]
		public UInt32 ms;
		[FieldOffset(4)]
		public UInt32 sample;
		[FieldOffset(4)]
		public UInt32 cb;
		[FieldOffset(4)]
		public UInt32 ticks;
		[FieldOffset(4)]
		public Byte smpteHour;
		[FieldOffset(5)]
		public Byte smpteMin;
		[FieldOffset(6)]
		public Byte smpteSec;
		[FieldOffset(7)]
		public Byte smpteFrame;
		[FieldOffset(8)]
		public Byte smpteFps;
		[FieldOffset(9)]
		public Byte smpteDummy;
		[FieldOffset(10)]
		public Byte smptePad0;
		[FieldOffset(11)]
		public Byte smptePad1;
		[FieldOffset(4)]
		public UInt32 midiSongPtrPos;
	}

	public enum SupportedWaveFormat
	{
		/// <summary>
		/// 11.025 kHz, Mono,   8-bit
		/// </summary>
		WAVE_FORMAT_1M08 = 0x00000001,
		/// <summary>
		/// 11.025 kHz, Stereo, 8-bit
		/// </summary>
		WAVE_FORMAT_1S08 = 0x00000002,
		/// <summary>
		/// 11.025 kHz, Mono,   16-bit
		/// </summary>
		WAVE_FORMAT_1M16 = 0x00000004,
		/// <summary>
		/// 11.025 kHz, Stereo, 16-bit
		/// </summary>
		WAVE_FORMAT_1S16 = 0x00000008,
		/// <summary>
		/// 22.05  kHz, Mono,   8-bit
		/// </summary>
		WAVE_FORMAT_2M08 = 0x00000010,
		/// <summary>
		/// 22.05  kHz, Stereo, 8-bit 
		/// </summary>
		WAVE_FORMAT_2S08 = 0x00000020,
		/// <summary>
		/// 22.05  kHz, Mono,   16-bit
		/// </summary>
		WAVE_FORMAT_2M16 = 0x00000040,
		/// <summary>
		/// 22.05  kHz, Stereo, 16-bit
		/// </summary>
		WAVE_FORMAT_2S16 = 0x00000080,
		/// <summary>
		/// 44.1   kHz, Mono,   8-bit 
		/// </summary>
		WAVE_FORMAT_4M08 = 0x00000100,
		/// <summary>
		/// 44.1   kHz, Stereo, 8-bit 
		/// </summary>
		WAVE_FORMAT_4S08 = 0x00000200,
		/// <summary>
		/// 44.1   kHz, Mono,   16-bit
		/// </summary>
		WAVE_FORMAT_4M16 = 0x00000400,
		/// <summary>
		///  44.1   kHz, Stereo, 16-bit
		/// </summary>
		WAVE_FORMAT_4S16 = 0x00000800,

		/// <summary>
		/// 44.1   kHz, Mono,   8-bit 
		/// </summary>
		WAVE_FORMAT_44M08 = 0x00000100,
		/// <summary>
		/// 44.1   kHz, Stereo, 8-bit 
		/// </summary>
		WAVE_FORMAT_44S08 = 0x00000200,
		/// <summary>
		/// 44.1   kHz, Mono,   16-bit
		/// </summary>
		WAVE_FORMAT_44M16 = 0x00000400,
		/// <summary>
		/// 44.1   kHz, Stereo, 16-bit
		/// </summary>
		WAVE_FORMAT_44S16 = 0x00000800,
		/// <summary>
		/// 48     kHz, Mono,   8-bit 
		/// </summary>
		WAVE_FORMAT_48M08 = 0x00001000,
		/// <summary>
		///  48     kHz, Stereo, 8-bit
		/// </summary>
		WAVE_FORMAT_48S08 = 0x00002000,
		/// <summary>
		/// 48     kHz, Mono,   16-bit
		/// </summary>
		WAVE_FORMAT_48M16 = 0x00004000,
		/// <summary>
		/// 48     kHz, Stereo, 16-bit
		/// </summary>
		WAVE_FORMAT_48S16 = 0x00008000,
		/// <summary>
		/// 96     kHz, Mono,   8-bit 
		/// </summary>
		WAVE_FORMAT_96M08 = 0x00010000,
		/// <summary>
		/// 96     kHz, Stereo, 8-bit
		/// </summary>
		WAVE_FORMAT_96S08 = 0x00020000,
		/// <summary>
		/// 96     kHz, Mono,   16-bit
		/// </summary>
		WAVE_FORMAT_96M16 = 0x00040000,
		/// <summary>
		/// 96     kHz, Stereo, 16-bit
		/// </summary>
		WAVE_FORMAT_96S16 = 0x00080000,

	}

	[StructLayout(LayoutKind.Sequential)]
	public class WaveHeader
	{
		/// <summary>pointer to locked data buffer (lpData)</summary>
		public IntPtr dataBuffer;
		/// <summary>length of data buffer (dwBufferLength)</summary>
		public int bufferLength;
		/// <summary>used for input only (dwBytesRecorded)</summary>
		public int bytesRecorded;
		/// <summary>for client's use (dwUser)</summary>
		public IntPtr userData;
		/// <summary>assorted flags (dwFlags)</summary>
		public WaveHeaderFlags flags;
		/// <summary>loop control counter (dwLoops)</summary>
		public int loops;
		/// <summary>PWaveHdr, reserved for driver (lpNext)</summary>
		public IntPtr next;
		/// <summary>reserved for driver</summary>
		public IntPtr reserved;
	}

	[Flags]
	public enum WaveHeaderFlags
	{
		/// <summary>
		/// WHDR_BEGINLOOP
		/// This buffer is the first buffer in a loop.  This flag is used only with output buffers.
		/// </summary>
		BeginLoop = 0x00000004,
		/// <summary>
		/// WHDR_DONE
		/// Set by the device driver to indicate that it is finished with the buffer and is returning it to the application.
		/// </summary>
		Done = 0x00000001,
		/// <summary>
		/// WHDR_ENDLOOP
		/// This buffer is the last buffer in a loop.  This flag is used only with output buffers.
		/// </summary>
		EndLoop = 0x00000008,
		/// <summary>
		/// WHDR_INQUEUE
		/// Set by Windows to indicate that the buffer is queued for playback.
		/// </summary>
		InQueue = 0x00000010,
		/// <summary>
		/// WHDR_PREPARED
		/// Set by Windows to indicate that the buffer has been prepared with the waveInPrepareHeader or waveOutPrepareHeader function.
		/// </summary>
		Prepared = 0x00000002
	}

	[Flags]
	public enum WaveOpenFlags
	{
		CallbackNull = 0,
		CallbackFunction = 0x30000,
		CallbackEvent = 0x50000,
		CallbackWindow = 0x10000,
		CallbackThread = 0x20000,
		/*
		WAVE_FORMAT_QUERY = 1,
		WAVE_MAPPED = 4,
		WAVE_FORMAT_DIRECT = 8*/
	}

	public class WaveInterop
	{

		//public const int TIME_MS = 0x0001;  // time in milliseconds 
		//public const int TIME_SAMPLES = 0x0002;  // number of wave samples 
		//public const int TIME_BYTES = 0x0004;  // current byte offset 

		public enum WaveMessage
		{
			WaveInOpen = 0x3BE,
			WaveInClose = 0x3BF,
			WaveInData = 0x3C0,
			WaveOutClose = 0x3BC,
			WaveOutDone = 0x3BD,
			WaveOutOpen = 0x3BB
		}

		// use the userdata as a reference
		// WaveOutProc http://msdn.microsoft.com/en-us/library/dd743869%28VS.85%29.aspx
		// WaveInProc http://msdn.microsoft.com/en-us/library/dd743849%28VS.85%29.aspx
		public delegate void WaveCallback(IntPtr hWaveOut, WaveMessage message, IntPtr dwInstance, WaveHeader wavhdr, IntPtr dwReserved);

		[DllImport("winmm.dll")] public static extern Int32 mmioStringToFOURCC([MarshalAs(UnmanagedType.LPStr)] String s, int flags);
		[DllImport("winmm.dll")] public static extern Int32 waveOutGetNumDevs();
		[DllImport("winmm.dll")] public static extern MmResult waveOutPrepareHeader(IntPtr hWaveOut, WaveHeader lpWaveOutHdr, int uSize);
		[DllImport("winmm.dll")] public static extern MmResult waveOutUnprepareHeader(IntPtr hWaveOut, WaveHeader lpWaveOutHdr, int uSize);
		[DllImport("winmm.dll")] public static extern MmResult waveOutWrite(IntPtr hWaveOut, WaveHeader lpWaveOutHdr, int uSize);

		// http://msdn.microsoft.com/en-us/library/dd743866%28VS.85%29.aspx
		[DllImport("winmm.dll")] public static extern MmResult waveOutOpen(out IntPtr hWaveOut, IntPtr uDeviceID, WaveFormat lpFormat, WaveCallback dwCallback, IntPtr dwInstance, WaveOpenFlags dwFlags);
		[DllImport("winmm.dll", EntryPoint = "waveOutOpen")]
		public static extern MmResult waveOutOpenWindow(out IntPtr hWaveOut, IntPtr uDeviceID, WaveFormat lpFormat, IntPtr callbackWindowHandle, IntPtr dwInstance, WaveOpenFlags dwFlags);

		[DllImport("winmm.dll")] public static extern MmResult waveOutReset(IntPtr hWaveOut);
		[DllImport("winmm.dll")] public static extern MmResult waveOutClose(IntPtr hWaveOut);
		[DllImport("winmm.dll")] public static extern MmResult waveOutPause(IntPtr hWaveOut);
		[DllImport("winmm.dll")] public static extern MmResult waveOutRestart(IntPtr hWaveOut);

		// http://msdn.microsoft.com/en-us/library/dd743863%28VS.85%29.aspx
		[DllImport("winmm.dll")] public static extern MmResult waveOutGetPosition(IntPtr hWaveOut, out MmTime mmTime, int uSize);

		// http://msdn.microsoft.com/en-us/library/dd743874%28VS.85%29.aspx
		[DllImport("winmm.dll")] public static extern MmResult waveOutGetVolume(IntPtr hWaveOut, out int dwVolume);
		[DllImport("winmm.dll")] public static extern MmResult waveOutSetVolume(IntPtr hWaveOut, int dwVolume);

		// http://msdn.microsoft.com/en-us/library/dd743857%28VS.85%29.aspx
		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		public static extern MmResult waveOutGetDevCaps(IntPtr deviceID, out WaveOutCapabilities waveOutCaps, int waveOutCapsSize);

		[DllImport("winmm.dll")] public static extern Int32 waveInGetNumDevs();

		// http://msdn.microsoft.com/en-us/library/dd743841%28VS.85%29.aspx
		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		public static extern MmResult waveInGetDevCaps(IntPtr deviceID, out WaveInCapabilities waveInCaps, int waveInCapsSize);

		// http://msdn.microsoft.com/en-us/library/dd743838%28VS.85%29.aspx
		[DllImport("winmm.dll")] public static extern MmResult waveInAddBuffer(IntPtr hWaveIn, WaveHeader pwh, int cbwh);
		[DllImport("winmm.dll")] public static extern MmResult waveInClose(IntPtr hWaveIn);

		// http://msdn.microsoft.com/en-us/library/dd743847%28VS.85%29.aspx
		[DllImport("winmm.dll")]
		public static extern MmResult waveInOpen(out IntPtr hWaveIn, IntPtr uDeviceID, WaveFormat lpFormat, WaveCallback dwCallback, IntPtr dwInstance, WaveOpenFlags dwFlags);
		[DllImport("winmm.dll", EntryPoint = "waveInOpen")]
		public static extern MmResult waveInOpenWindow(out IntPtr hWaveIn, IntPtr uDeviceID, WaveFormat lpFormat, IntPtr callbackWindowHandle, IntPtr dwInstance, WaveOpenFlags dwFlags);

		// http://msdn.microsoft.com/en-us/library/dd743848%28VS.85%29.aspx
		[DllImport("winmm.dll")] public static extern MmResult waveInPrepareHeader(IntPtr hWaveIn, WaveHeader lpWaveInHdr, int uSize);
		[DllImport("winmm.dll")] public static extern MmResult waveInUnprepareHeader(IntPtr hWaveIn, WaveHeader lpWaveInHdr, int uSize);

		// http://msdn.microsoft.com/en-us/library/dd743850%28VS.85%29.aspx
		[DllImport("winmm.dll")] public static extern MmResult waveInReset(IntPtr hWaveIn);

		// http://msdn.microsoft.com/en-us/library/dd743851%28VS.85%29.aspx
		[DllImport("winmm.dll")] public static extern MmResult waveInStart(IntPtr hWaveIn);

		// http://msdn.microsoft.com/en-us/library/dd743852%28VS.85%29.aspx
		[DllImport("winmm.dll")] public static extern MmResult waveInStop(IntPtr hWaveIn);

		// https://msdn.microsoft.com/en-us/library/Dd743845(v=VS.85).aspx
		[DllImport("winmm.dll")] public static extern MmResult waveInGetPosition(IntPtr hWaveIn, out MmTime mmTime, int uSize);
	}
}