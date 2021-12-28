using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public partial class Wave
	{
		public static partial class In
		{
			public class DeviceInfo : ADeviceInfo<WaveInCapabilities>
			{
				public DeviceInfo(int number) : base(number) { }

				public override WaveInCapabilities GetCapabilities()
				{
					waveInGetDevCaps((IntPtr)_number, out var capabilities, Marshal.SizeOf(typeof(WaveInCapabilities))).Verify();
					return capabilities;
				}

				public Session CreateSession(WaveFormat format) => new Session(_number, format);

				public virtual IProcessor CreateProcessor(WaveFormat waveFormat, int sampleSize, int buffersCount) =>
					new Processor(CreateSession(waveFormat), sampleSize, buffersCount);
			}

			public static int GetDevicesCount() => waveInGetNumDevs();

			public static IEnumerable<DeviceInfo> EnumerateDevices()
			{
				var devicesCount = GetDevicesCount();
				for (var i = 0; i < devicesCount; i++)
					yield return new DeviceInfo(i);
			}

			public static readonly DeviceInfo DefaultDevice = new(-1);

			public class Session : ASession
			{
				public Session(int deviceNumber, WaveFormat format)
				{
					this.deviceNumber = deviceNumber;
					WaveFormat = format;
				}

				public override MmResult Lull() => waveInStop(handle).Verify(); /* waveOutPause */
				public override MmResult Wake() => waveInStart(handle).Verify(); /* waveOutRestart */

				public override MmResult Close() => waveInClose(handle).Verify();
				public override MmResult Reset() => waveInReset(handle).Verify();
				public override MmResult Open(Callback callback) =>
					waveInOpen(out handle, (IntPtr)deviceNumber, WaveFormat, callback, IntPtr.Zero, OpenFlags.CallbackFunction).Verify();

				public override MmResult GetPosition(out MmTime time, int size) => waveInGetPosition(handle, out time, size).Verify();

				public override MmResult PrepareHeader(Header header) => waveInPrepareHeader(handle, header, Marshal.SizeOf(header)).Verify();
				public override MmResult UnprepareHeader(Header header) => waveInUnprepareHeader(handle, header, Marshal.SizeOf(header)).Verify();
				public override MmResult MarkForProcessing(Header header) =>	waveInAddBuffer(handle, header, Marshal.SizeOf(header)).Verify();

				public override float GetVolume() => default;
				public override void SetVolume(double value) { }
			}

			public class Processor : Processor<DeviceInfo>
			{
				public Processor(Session session, int bufferSize, int buffersCount) : 
					base(session, bufferSize, buffersCount) { }
			}
		}
	}
}
