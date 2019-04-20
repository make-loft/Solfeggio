using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public partial class Wave
    {
		public static class Out
		{
			public class DeviceInfo : ADeviceInfo<WaveOutCapabilities>
			{
				public DeviceInfo(int number) : base(number) { }

				public override ref WaveOutCapabilities Fill(ref WaveOutCapabilities capabilities)
				{
					WaveInterop.waveOutGetDevCaps((IntPtr)_number, out capabilities, Marshal.SizeOf(capabilities)).Verify();
					return ref capabilities;
				}

				public Session CreateSession() => new Session(this);
			}

			public static int GetDevicesCount() => WaveInterop.waveOutGetNumDevs();

			public static IEnumerable<DeviceInfo> EnumerateDevices()
			{
				var devicesCount = GetDevicesCount();
				for (var i = 0; i < devicesCount; i++)
					yield return new DeviceInfo(i);
			}

			public static readonly DeviceInfo DefaultDevice = new DeviceInfo(-1);

			public class Session : ASession
			{
				public Session(DeviceInfo deviceInfo) => deviceNumber = deviceInfo.Number;

				public override MmResult Lull() => WaveInterop.waveOutPause(handle).Verify(); /* waveInStop */
				public override MmResult Wake() => WaveInterop.waveOutRestart(handle).Verify(); /* waveInStart */

				public override MmResult Close() => WaveInterop.waveOutClose(handle).Verify();
				public override MmResult Reset() => WaveInterop.waveOutReset(handle).Verify();
				public override MmResult Open(WaveInterop.WaveCallback callback) =>
					WaveInterop.waveOutOpen(out handle, (IntPtr)deviceNumber, WaveFormat, callback, IntPtr.Zero, WaveOpenFlags.CallbackFunction).Verify();

				public override MmResult GetPosition(out MmTime time, int size) =>
					WaveInterop.waveOutGetPosition(handle, out time, size).Verify();

				public override MmResult PrepareHeader(WaveHeader header) => WaveInterop.waveOutPrepareHeader(handle, header, Marshal.SizeOf(header)).Verify();
				public override MmResult UnprepareHeader(WaveHeader header) => WaveInterop.waveOutUnprepareHeader(handle, header, Marshal.SizeOf(header)).Verify();
				public override MmResult MarkAsProcessed(WaveHeader header) =>
					WaveInterop.waveOutWrite(handle, header, Marshal.SizeOf(header)).Verify();

				internal static float GetVolume(IntPtr hWaveOut)
				{
					WaveInterop.waveOutGetVolume(hWaveOut, out var stereoVolume).Verify();
					return (stereoVolume & 0xFFFF) / (float)0xFFFF;
				}

				internal static void SetVolume(float value, IntPtr hWaveOut)
				{
					if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");
					if (value > 1) throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");

					float left = value;
					float right = value;

					var stereoVolume = (int)(left * 0xFFFF) + ((int)(right * 0xFFFF) << 16);
					WaveInterop.waveOutSetVolume(hWaveOut, stereoVolume).Verify();
				}
			}

			public class Processor : Processor<DeviceInfo>
			{
				public Processor(Session session, IDataSource<short> source) : base(session, source) { }
			}
		}
	}
}
