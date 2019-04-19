using Ace;
using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public class WaveBuffer : WaveBuffer<short>
	{
		public WaveBuffer(DirectionKind kind, IntPtr waveHandle, int binsCount) :
			base(kind, waveHandle, binsCount)
		{ }
	}

	public class WaveBuffer<T> where T : struct
	{
		private static readonly int SizeOfBin = Marshal.SizeOf(typeof(T));

		protected IntPtr hWave;
		protected GCHandle hBuffer;
		protected GCHandle hHeader; // we need to pin the header structure
		protected GCHandle hThis; // for the user callback
		protected WaveHeader header;

		public T[] Data { get; }

		public DirectionKind Kind { get; }

		public WaveBuffer(DirectionKind kind, IntPtr waveHandle, int binsCount)
		{
			Kind = kind;
			Data = new T[binsCount];

			hWave = waveHandle;
			hThis = GCHandle.Alloc(this);
			hBuffer = GCHandle.Alloc(Data, GCHandleType.Pinned);
			hHeader = GCHandle.Alloc(header, GCHandleType.Pinned);

			header = new WaveHeader
			{
				dataBuffer = hBuffer.AddrOfPinnedObject(),
				bufferLength = SizeOfBin * binsCount,
				userData = (IntPtr)hThis,
				loops = 1
			};

			if (Kind.Is(DirectionKind.In))
				WaveInterop.waveInPrepareHeader(hWave, header, Marshal.SizeOf(header)).Verify();

			if (Kind.Is(DirectionKind.Out))
				WaveInterop.waveOutPrepareHeader(hWave, header, Marshal.SizeOf(header)).Verify();
		}

		public bool IsDone => (header.flags & WaveHeaderFlags.Done) == WaveHeaderFlags.Done;
		public bool InQueue => (header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue;
		public int BinsCount => header.bytesRecorded / SizeOfBin;

		#region Dispose Pattern

		~WaveBuffer()
		{
			Dispose();
			System.Diagnostics.Debug.Assert(true, "WaveInBuffer was not disposed");
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);

			if (hWave != IntPtr.Zero)
			{
				if (Kind.Is(DirectionKind.In))
					WaveInterop.waveInUnprepareHeader(hWave, header, Marshal.SizeOf(header)).Verify();

				if (Kind.Is(DirectionKind.Out))
					WaveInterop.waveOutUnprepareHeader(hWave, header, Marshal.SizeOf(header)).Verify();

				hWave = IntPtr.Zero;
			}

			if (hHeader.IsAllocated) hHeader.Free();
			if (hBuffer.IsAllocated) hBuffer.Free();
			if (hThis.IsAllocated) hThis.Free();
		}

		#endregion

		// to avoid deadlocks in Function callback mode,
		// we lock round this whole thing, which will include the
		// reading from the stream.
		// this protects us from calling waveOutReset on another 
		// thread while a WaveOutWrite is in progress

		// this is called by the WAVE callback and should be used to refill the buffer
		internal bool WriteAsync(IWaveProvider<T> waveStream)
		{
			int bytes;

			lock (waveStream)
			{
				bytes = waveStream.Read(Data, 0, Data.Length);
			}

			if (bytes == 0) return false;

			for (int n = bytes; n < Data.Length; n++)
			{
				Data[n] = default;
			}

			WaveInterop.waveOutWrite(hWave, header, Marshal.SizeOf(header)).Verify();
			return true;
		}

		public void ReadAsync()
		{
			// TEST: we might not actually need to bother unpreparing and repreparing
			//WaveInterop.waveInUnprepareHeader(hWave, header, Marshal.SizeOf(header)).Verify();
			//WaveInterop.waveInPrepareHeader(hWave, header, Marshal.SizeOf(header)).Verify();
			//System.Diagnostics.Debug.Assert(header.bytesRecorded == 0, "bytes recorded was not reset properly");
			WaveInterop.waveInAddBuffer(hWave, header, Marshal.SizeOf(header)).Verify();
		}
	}
}
