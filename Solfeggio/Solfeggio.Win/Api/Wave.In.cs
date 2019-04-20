using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public partial class Wave
    {
		public static class In
		{
			public class DeviceInfo : ADeviceInfo<WaveInCapabilities>
			{
				public DeviceInfo(int number) : base(number) { }

				public override ref WaveInCapabilities Fill(ref WaveInCapabilities capabilities)
				{
					WaveInterop.waveInGetDevCaps((IntPtr)_number, out capabilities, Marshal.SizeOf(capabilities)).Verify();
					return ref capabilities;
				}

				public Session CreateSession() => new Session(this);
			}

			public static int GetDevicesCount() => WaveInterop.waveInGetNumDevs();

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

				public override MmResult Lull() => WaveInterop.waveInStop(handle).Verify(); /* waveOutPause */
				public override MmResult Wake() => WaveInterop.waveInStart(handle).Verify(); /* waveOutRestart */

				public override MmResult Close() => WaveInterop.waveInClose(handle).Verify();
				public override MmResult Reset() => WaveInterop.waveInReset(handle).Verify();
				public override MmResult Open(WaveInterop.WaveCallback callback) =>
					WaveInterop.waveInOpen(out handle, (IntPtr)deviceNumber, WaveFormat, callback, IntPtr.Zero, WaveOpenFlags.CallbackFunction).Verify();

				public override MmResult GetPosition(out MmTime time, int size) =>
					 WaveInterop.waveInGetPosition(handle, out time, size).Verify();

				public override MmResult PrepareHeader(WaveHeader header) => WaveInterop.waveInPrepareHeader(handle, header, Marshal.SizeOf(header)).Verify();
				public override MmResult UnprepareHeader(WaveHeader header) => WaveInterop.waveInUnprepareHeader(handle, header, Marshal.SizeOf(header)).Verify();
				public override MmResult MarkAsProcessed(WaveHeader header) =>
					WaveInterop.waveInAddBuffer(handle, header, Marshal.SizeOf(header)).Verify();
			}

			public class Processor : Processor<DeviceInfo>
			{
				public Processor(Session session) : base(session) { }
			}
		}
	}
}
