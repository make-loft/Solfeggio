using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	class WaveOutBuffer : IDisposable
	{
		private readonly WaveHeader header;
		private readonly byte[] buffer;
		private readonly IWaveProvider waveStream;
		private readonly object waveOutLock;
		private GCHandle hBuffer;
		private IntPtr hWaveOut;
		private GCHandle hHeader; // we need to pin the header structure
		private GCHandle hThis; // for the user callback

		public WaveOutBuffer(IntPtr hWaveOut, int bufferSize, IWaveProvider bufferFillStream, object waveOutLock)
		{
			BufferSize = bufferSize;
			buffer = new byte[bufferSize];
			hBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			this.hWaveOut = hWaveOut;
			this.waveOutLock = waveOutLock;
			waveStream = bufferFillStream;

			header = new WaveHeader();
			hHeader = GCHandle.Alloc(header, GCHandleType.Pinned);
			header.dataBuffer = hBuffer.AddrOfPinnedObject();
			header.bufferLength = bufferSize;
			header.loops = 1;
			hThis = GCHandle.Alloc(this);
			header.userData = (IntPtr)hThis;

			lock (waveOutLock)
			{
				MmException.Try(WaveInterop.waveOutPrepareHeader(hWaveOut, header, Marshal.SizeOf(header)), "waveOutPrepareHeader");
			}
		}

		#region Dispose Pattern

		~WaveOutBuffer()
		{
			Dispose(false);
			System.Diagnostics.Debug.Assert(true, "WaveBuffer was not disposed");
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				// free managed resources
			}

			// free unmanaged resources

			if (hHeader.IsAllocated)
				hHeader.Free();

			if (hBuffer.IsAllocated)
				hBuffer.Free();

			if (hThis.IsAllocated)
				hThis.Free();

			if (hWaveOut != IntPtr.Zero)
			{
				lock (waveOutLock)
				{
					WaveInterop.waveOutUnprepareHeader(hWaveOut, header, Marshal.SizeOf(header));
				}

				hWaveOut = IntPtr.Zero;
			}
		}

		#endregion

		/// this is called by the WAVE callback and should be used to refill the buffer
		internal bool OnDone()
		{
			int bytes;

			lock (waveStream)
			{
				bytes = waveStream.Read(buffer, 0, buffer.Length);
			}

			if (bytes == 0) return false;

			for (int n = bytes; n < buffer.Length; n++)
			{
				buffer[n] = 0;
			}

			WriteToWaveOut();
			return true;
		}

		public bool InQueue => (header.flags & WaveHeaderFlags.InQueue) == WaveHeaderFlags.InQueue;
		public int BufferSize { get; }

		private void WriteToWaveOut()
		{
			//lock (waveOutLock)
			{
				var result = WaveInterop.waveOutWrite(hWaveOut, header, Marshal.SizeOf(header));
				if (result != MmResult.NoError)	throw new MmException(result, "waveOutWrite");
			}

			GC.KeepAlive(this);
		}
	}
}
