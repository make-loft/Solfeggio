using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public static partial class Wave
	{
		public enum Message
		{
			WaveInOpen = 0x3BE,
			WaveInClose = 0x3BF,
			WaveInData = 0x3C0,

			WaveOutOpen = 0x3BB,
			WaveOutClose = 0x3BC,
			WaveOutDone = 0x3BD,
		}

		[Flags]
		public enum OpenFlags
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

		public delegate void Callback(IntPtr hWaveOut, Message message, IntPtr dwInstance, Wave.Header wavhdr, IntPtr dwReserved);

		public abstract class ADeviceInfo<TCapabilities>
			where TCapabilities : IDeviceCapabilities, new()
		{
			protected int _number;
			public ADeviceInfo(int number) => _number = number;
			public int Number => _number;
			public virtual string ProductName => GetCapabilities().ProductName;
			public abstract TCapabilities GetCapabilities();
		}

		public abstract class ASession
		{
			protected IntPtr handle;
			protected int deviceNumber;
			public IntPtr Handle => handle;
			public WaveFormat WaveFormat { get; protected set; }

			public abstract MmResult Wake();
			public abstract MmResult Lull();

			public abstract MmResult Close();
			public abstract MmResult Reset();
			public abstract MmResult Open(Callback callback);

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

			public abstract MmResult PrepareHeader(Header header);
			public abstract MmResult UnprepareHeader(Header header);
			public abstract MmResult MarkForProcessing(Header header);

			public abstract float GetVolume();
			public abstract void SetVolume(float value);
		}
	}
}
