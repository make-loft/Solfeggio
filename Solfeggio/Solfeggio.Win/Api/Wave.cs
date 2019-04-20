using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public partial class Wave
	{
		public abstract class ADeviceInfo<TCapabilities>
			where TCapabilities : new()
		{
			protected int _number;
			public int Number => _number;
			public ADeviceInfo(int number) => _number = number;
			public abstract ref TCapabilities Fill(ref TCapabilities capabilities);
			public TCapabilities GetCapabilities()
			{
				var capabilities = new TCapabilities();
				Fill(ref capabilities);
				return capabilities;
			}
		}

		public abstract class ASession
		{
			protected IntPtr handle;
			protected int deviceNumber;
			public IntPtr Handle => handle;
			public WaveFormat WaveFormat { get; set; } = new WaveFormat(16000, 16, 1);

			public abstract MmResult Wake();
			public abstract MmResult Lull();

			public abstract MmResult Close();
			public abstract MmResult Reset();
			public abstract MmResult Open(WaveInterop.WaveCallback callback);

			public abstract MmResult GetPosition(out MmTime time, int size);

			public long GetPosition()
			{
				// request results in bytes, TODO: perhaps make this a little more flexible and support the other types?
				var mmTime = new MmTime { wType = MmTime.TIME_BYTES };
				GetPosition(out mmTime, Marshal.SizeOf(mmTime)).Verify();

				if (mmTime.wType != MmTime.TIME_BYTES)
					throw new Exception(string.Format("waveInGetPosition: wType -> Expected {0}, Received {1}", MmTime.TIME_BYTES, mmTime.wType));

				return mmTime.cb;
			}

			public abstract MmResult PrepareHeader(WaveHeader header);
			public abstract MmResult UnprepareHeader(WaveHeader header);
			public abstract MmResult MarkAsProcessed(WaveHeader header);
		}
	}
}
