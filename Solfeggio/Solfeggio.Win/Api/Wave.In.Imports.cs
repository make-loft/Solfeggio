using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public partial class Wave
	{
		public static partial class In
		{
			public enum Message
			{
				Open = 0x3BE,
				Close = 0x3BF,
				Data = 0x3C0,
			}

			//public delegate void Callback(IntPtr hWaveOut, Message message, IntPtr dwInstance, Wave.Header wavhdr, IntPtr dwReserved);

			[DllImport("winmm.dll")] public static extern Int32 waveInGetNumDevs();
			[DllImport("winmm.dll", CharSet = CharSet.Auto)] public static extern MmResult waveInGetDevCaps(IntPtr deviceID, out WaveInCapabilities waveInCaps, int waveInCapsSize);
			[DllImport("winmm.dll")] public static extern MmResult waveInAddBuffer(IntPtr hWaveIn, Wave.Header pwh, int cbwh);
			[DllImport("winmm.dll")] public static extern MmResult waveInClose(IntPtr hWaveIn);
			[DllImport("winmm.dll")] public static extern MmResult waveInOpen(out IntPtr hWaveIn, IntPtr uDeviceID, WaveFormat lpFormat, Callback dwCallback, IntPtr dwInstance, OpenFlags dwFlags);
			[DllImport("winmm.dll", EntryPoint = "waveInOpen")] public static extern MmResult waveInOpenWindow(out IntPtr hWaveIn, IntPtr uDeviceID, WaveFormat lpFormat, IntPtr callbackWindowHandle, IntPtr dwInstance, OpenFlags dwFlags);
			[DllImport("winmm.dll")] public static extern MmResult waveInPrepareHeader(IntPtr hWaveIn, Wave.Header lpWaveInHdr, int uSize);
			[DllImport("winmm.dll")] public static extern MmResult waveInUnprepareHeader(IntPtr hWaveIn, Wave.Header lpWaveInHdr, int uSize);
			[DllImport("winmm.dll")] public static extern MmResult waveInReset(IntPtr hWaveIn);
			[DllImport("winmm.dll")] public static extern MmResult waveInStart(IntPtr hWaveIn);
			[DllImport("winmm.dll")] public static extern MmResult waveInStop(IntPtr hWaveIn);
			[DllImport("winmm.dll")] public static extern MmResult waveInGetPosition(IntPtr hWaveIn, out MmTime mmTime, int uSize);
		}
	}
}
