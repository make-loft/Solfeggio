using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	class WaveInBuffer : WaveInBuffer<short>
	{
		public WaveInBuffer(IntPtr waveInHandle, int binsCount) : base(waveInHandle, binsCount) { }
	}

	class WaveInBuffer<T> : IDisposable where T : struct
    {
		private static readonly int SizeOfBin = Marshal.SizeOf(typeof(T));
		private readonly WaveHeader header;
		private readonly GCHandle hBuffer;
        private readonly GCHandle hHeader; // we need to pin the header structure
        private readonly GCHandle hThis; // for the user callback
		private IntPtr waveInHandle;

		public WaveInBuffer(IntPtr waveInHandle, int binsCount)
        {
            this.Data = new T[binsCount];
            this.hBuffer = GCHandle.Alloc(Data, GCHandleType.Pinned);
            this.waveInHandle = waveInHandle;

            header = new WaveHeader();
            hHeader = GCHandle.Alloc(header, GCHandleType.Pinned);
            header.dataBuffer = hBuffer.AddrOfPinnedObject();
            header.bufferLength = SizeOfBin * binsCount;
            header.loops = 1;
            hThis = GCHandle.Alloc(this);
            header.userData = (IntPtr)hThis;

            MmException.Try(WaveInterop.waveInPrepareHeader(waveInHandle, header, Marshal.SizeOf(header)), "waveInPrepareHeader");
            //MmException.Try(WaveInterop.waveInAddBuffer(waveInHandle, header, Marshal.SizeOf(header)), "waveInAddBuffer");
        }

        public void Reuse()
        {
            // TEST: we might not actually need to bother unpreparing and repreparing
            MmException.Try(WaveInterop.waveInUnprepareHeader(waveInHandle, header, Marshal.SizeOf(header)), "waveUnprepareHeader");
            MmException.Try(WaveInterop.waveInPrepareHeader(waveInHandle, header, Marshal.SizeOf(header)), "waveInPrepareHeader");
            //System.Diagnostics.Debug.Assert(header.bytesRecorded == 0, "bytes recorded was not reset properly");
            MmException.Try(WaveInterop.waveInAddBuffer(waveInHandle, header, Marshal.SizeOf(header)), "waveInAddBuffer");
        }

        #region Dispose Pattern

        ~WaveInBuffer()
        {
            Dispose();
            System.Diagnostics.Debug.Assert(true, "WaveInBuffer was not disposed");
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

			if (waveInHandle != IntPtr.Zero)
			{
				WaveInterop.waveInUnprepareHeader(waveInHandle, header, Marshal.SizeOf(header));
				waveInHandle = IntPtr.Zero;
			}

			if (hHeader.IsAllocated) hHeader.Free();
			if (hBuffer.IsAllocated) hBuffer.Free();
			if (hThis.IsAllocated) hThis.Free();
		}

		#endregion

		public T[] Data { get; }
		public bool Done => (header.flags & WaveHeaderFlags.Done) == WaveHeaderFlags.Done;
		public bool InQueue => (header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue;
		public int BinsCount => header.bytesRecorded / SizeOfBin;
	}
}