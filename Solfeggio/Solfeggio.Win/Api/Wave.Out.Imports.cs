using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public partial class Wave
	{
		public static partial class Out
		{
			public enum Message
			{
				Close = 0x3BC,
				Done = 0x3BD,
				Open = 0x3BB
			}

			//public delegate void Callback(IntPtr hWaveOut, Message message, IntPtr dwInstance, Wave.Header wavhdr, IntPtr dwReserved);

			[DllImport("winmm.dll")] public static extern Int32 waveOutGetNumDevs();
			[DllImport("winmm.dll")] public static extern MmResult waveOutPrepareHeader(IntPtr hWaveOut, Wave.Header lpWaveOutHdr, int uSize);
			[DllImport("winmm.dll")] public static extern MmResult waveOutUnprepareHeader(IntPtr hWaveOut, Wave.Header lpWaveOutHdr, int uSize);
			[DllImport("winmm.dll")] public static extern MmResult waveOutWrite(IntPtr hWaveOut, Wave.Header lpWaveOutHdr, int uSize);
			[DllImport("winmm.dll")] public static extern MmResult waveOutOpen(out IntPtr hWaveOut, IntPtr uDeviceID, WaveFormat lpFormat, Callback dwCallback, IntPtr dwInstance, OpenFlags dwFlags);
			[DllImport("winmm.dll", EntryPoint = "waveOutOpen")] public static extern MmResult waveOutOpenWindow(out IntPtr hWaveOut, IntPtr uDeviceID, WaveFormat lpFormat, IntPtr callbackWindowHandle, IntPtr dwInstance, OpenFlags dwFlags);
			[DllImport("winmm.dll")] public static extern MmResult waveOutReset(IntPtr hWaveOut);
			[DllImport("winmm.dll")] public static extern MmResult waveOutClose(IntPtr hWaveOut);
			[DllImport("winmm.dll")] public static extern MmResult waveOutPause(IntPtr hWaveOut);
			[DllImport("winmm.dll")] public static extern MmResult waveOutRestart(IntPtr hWaveOut);
			[DllImport("winmm.dll")] public static extern MmResult waveOutGetPosition(IntPtr hWaveOut, out MmTime mmTime, int uSize);
			[DllImport("winmm.dll")] public static extern MmResult waveOutGetVolume(IntPtr hWaveOut, out int dwVolume);
			[DllImport("winmm.dll")] public static extern MmResult waveOutSetVolume(IntPtr hWaveOut, int dwVolume);
			[DllImport("winmm.dll", CharSet = CharSet.Auto)] public static extern MmResult waveOutGetDevCaps(IntPtr deviceID, out WaveOutCapabilities waveOutCaps, int waveOutCapsSize);
		}
	}
}
