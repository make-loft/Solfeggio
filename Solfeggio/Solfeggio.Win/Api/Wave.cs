using Ace;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Solfeggio.Api.ProcessingState;
using static Solfeggio.Api.DirectionKind;

namespace Solfeggio.Api
{
	public class Wave : IWavePosition, IDisposable, IWaveState<short>
	{
		public DirectionKind Kind { get; protected set; }

		protected IntPtr handle;
		protected WaveBuffer[] buffers;
		protected WaveInterop.WaveCallback callback;

		public event EventHandler<WaveInEventArgs> DataAvailable;

		public WaveFormat WaveFormat { get; set; }

		public int BufferMilliseconds { get; set; }
		public int NumberOfBuffers { get; set; }
		public int DeviceNumber { get; set; }

		public Wave(DirectionKind kind)
		{
			Kind = kind;
			WaveFormat = new WaveFormat(16000, 16, 1);
			BufferMilliseconds = 100;
			NumberOfBuffers = 3;
			callback = Callback;
		}

		public int BufferSize { get; set; }

		protected void CreateBuffers()
		{
			// Default to three buffers of 100ms each
			int bufferSize = BufferSize.Is(default) ? BufferMilliseconds * WaveFormat.AverageBytesPerSecond / 1000 : BufferSize;
			if (bufferSize % WaveFormat.BlockAlign != 0)
			{
				bufferSize -= bufferSize % WaveFormat.BlockAlign;
			}

			buffers = new WaveBuffer[NumberOfBuffers];
			for (int n = 0; n < buffers.Length; n++)
			{
				buffers[n] = new WaveBuffer(Kind, handle, bufferSize);
			}
		}

		public long GetPosition()
		{
			// request results in bytes, TODO: perhaps make this a little more flexible and support the other types?
			var mmTime = new MmTime	{ wType = MmTime.TIME_BYTES };
			WaveInterop.waveInGetPosition(handle, out mmTime, Marshal.SizeOf(mmTime)).Verify();

			if (mmTime.wType != MmTime.TIME_BYTES)
				throw new Exception(string.Format("waveInGetPosition: wType -> Expected {0}, Received {1}", MmTime.TIME_BYTES, mmTime.wType));

			return mmTime.cb;
		}

		internal MmResult Open(out IntPtr handle, int deviceNumber, WaveFormat waveFormat, WaveInterop.WaveCallback callback) =>
			Kind.Is(Out) ? WaveInterop.waveOutOpen(out handle, (IntPtr)deviceNumber, waveFormat, callback, IntPtr.Zero,	WaveInterop.WaveInOutOpenFlags.CallbackFunction) :
			Kind.Is(In) ? WaveInterop.waveInOpen(out handle, (IntPtr)deviceNumber, waveFormat, callback, IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackFunction) :
			throw new ArgumentException();

		internal MmResult Reset(IntPtr handle) =>
			Kind.Is(Out) ? WaveInterop.waveOutReset(handle) :
			Kind.Is(In) ? WaveInterop.waveInReset(handle) :
			throw new ArgumentException();

		internal MmResult Close(IntPtr handle) =>
			Kind.Is(Out) ? WaveInterop.waveOutClose(handle) :
			Kind.Is(In) ? WaveInterop.waveInClose(handle) :
			throw new ArgumentException();

		internal int GetDeviceCount() =>
			Kind.Is(Out) ? WaveInterop.waveOutGetNumDevs() :
			Kind.Is(In) ? WaveInterop.waveInGetNumDevs() :
			throw new ArgumentException();

		public IWaveCapabilities GetCapabilities(int devNumber)
		{
			if (Kind.Is(DirectionKind.Out))
			{
				var capabilities = new WaveOutCapabilities();
				int structSize = Marshal.SizeOf(capabilities);
				WaveInterop.waveOutGetDevCaps((IntPtr)devNumber, out capabilities, structSize).Verify();
				return capabilities;
			}

			if (Kind.Is(DirectionKind.In))
			{
				var capabilities = new WaveInCapabilities();
				int structSize = Marshal.SizeOf(capabilities);
				WaveInterop.waveInGetDevCaps((IntPtr)devNumber, out capabilities, structSize).Verify();
				return capabilities;
			}

			throw new ArgumentException();
		}

		~Wave()
		{
			Debug.Assert(false, "WaveOut device was not closed");
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (State.IsNot(ProcessingState.Hibernation))
				Free();

			if (!disposing) return;
			CloseWaveInDevice();
		}

		protected void CloseWaveInDevice()
		{
			if (handle == IntPtr.Zero) return;
			// Some drivers need the reset to properly release buffers
			Reset(handle);
			if (buffers != null)
			{
				for (int n = 0; n < buffers.Length; n++)
				{
					buffers[n].Dispose();
				}
			}

			Close(handle).Verify();
			handle = IntPtr.Zero;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		private int lastReturnedBufferIndex;

		private void RaiseDataAvailable(WaveBuffer buffer) =>
			DataAvailable?.Invoke(this, new WaveInEventArgs(buffer.Data, buffer.BinsCount));

		private void EnqueueBuffers()
		{
			foreach (var buffer in buffers)
			{
				if (!buffer.InQueue)
				{
					if (Kind.Is(In))
						buffer.ReadAsync();

					if (Kind.Is(Out))
						buffer.WriteAsync(waveProvider);
				}
			}
		}

		IWaveProvider<short> waveProvider;

		private void Callback(IntPtr waveHandle, WaveInterop.WaveMessage message, IntPtr userData, WaveHeader waveHeader, IntPtr reserved)
		{
			if (message.Is(WaveInterop.WaveMessage.WaveInData))
			{
				GCHandle hBuffer = (GCHandle)waveHeader.userData;
				WaveBuffer buffer = (WaveBuffer)hBuffer.Target;

				lastReturnedBufferIndex = Array.IndexOf(buffers, buffer);
				RaiseDataAvailable(buffer);
				try
				{
					buffer.ReadAsync();
				}
				catch (Exception e)
				{

				}
			}

			if (message.Is(WaveInterop.WaveMessage.WaveOutDone))
			{
				GCHandle hBuffer = (GCHandle)waveHeader.userData;
				WaveBuffer buffer = (WaveBuffer)hBuffer.Target;
				buffer.WriteAsync(waveProvider);
			}
		}

		public void Init(IWaveProvider<short> waveProvider, int bufferSize)
		{
			this.waveProvider = waveProvider;
			WaveFormat = waveProvider.WaveFormat;
			BufferSize = bufferSize;

			Wake();
		}

		public void Wake()
		{
			if (State.IsNot(Processing))
			{
				if (handle.Is()) Close(handle);

				Open(out handle, DeviceNumber, WaveFormat, callback);
				CreateBuffers();
				EnqueueBuffers();

				if (Kind.Is(In))
					WaveInterop.waveInStart(handle).Verify();

				if (Kind.Is(Out))
					WaveInterop.waveOutRestart(handle).Verify();
			}

			State = Processing;
		}

		public void Lull()
		{
			if (State.IsNot(Suspending))
			{
				if (Kind.Is(Out))
					WaveInterop.waveOutPause(handle).Verify();

				if (Kind.Is(In))
					WaveInterop.waveInStop(handle).Verify();
			}

			State = Suspending;
		}

		public void Free()
		{
			if (State.IsNot(Hibernation))
			{
				if (Kind.Is(Out))
				{
					WaveInterop.waveOutReset(handle).Verify();
				}


				if (Kind.Is(In))
				{
					Console.Beep();
					WaveInterop.waveInStop(handle).Verify();
					// report the last buffers, sometimes more than one, so taking care to report them in the right order
					for (int n = 0; n < buffers.Length; n++)
					{
						int index = (n + lastReturnedBufferIndex + 1) % buffers.Length;
						var buffer = buffers[index];
						if (buffer.IsDone)
						{
							RaiseDataAvailable(buffer);
						}
					}
					//MmException.Try(WaveInterop.waveInReset(waveInHandle), "waveInReset");      
					// Don't actually close yet so we get the last buffer
				}
			}

			State = Hibernation;
		}

		public ProcessingState State { get; set; }

		internal static float GetWaveOutVolume(IntPtr hWaveOut)
		{
			//lock (lockObject)
			{
				WaveInterop.waveOutGetVolume(hWaveOut, out var stereoVolume).Verify();
				return (stereoVolume & 0xFFFF) / (float)0xFFFF;
			}
		}

		internal static void SetWaveOutVolume(float value, IntPtr hWaveOut)
		{
			if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");
			if (value > 1) throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");

			float left = value;
			float right = value;

			int stereoVolume = (int)(left * 0xFFFF) + ((int)(right * 0xFFFF) << 16);
			WaveInterop.waveOutSetVolume(hWaveOut, stereoVolume).Verify();
		}
	}
}
