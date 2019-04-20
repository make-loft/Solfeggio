using Ace;
using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public partial class Wave
	{
		public class Buffer : Buffer<short>
		{
			public Buffer(ASession session, int binsCount) :
				base(session, binsCount) { }
		}

		public class Buffer<T> : IDisposable, IExposable where T : struct
		{
			private static readonly int SizeOfBin = Marshal.SizeOf(typeof(T));

			protected GCHandle hBuffer;
			protected GCHandle hHeader; // we need to pin the header structure
			protected GCHandle hThis; // for the user callback
			protected Header header;

			public T[] Data { get; }

			public ASession session { get; }
			public bool IsDone => (header.flags & Header.Flags.Done) == Header.Flags.Done;
			public bool InQueue => (header.flags & Header.Flags.InQueue) == Header.Flags.InQueue;
			public int BinsCount => header.bytesRecorded / SizeOfBin;

			public Buffer(ASession session, int binsCount)
			{
				this.session = session;
				Data = new T[binsCount];

				Expose();
			}

			~Buffer()
			{
				Dispose();
			}

			public void Expose()
			{
				hThis = GCHandle.Alloc(this);
				hBuffer = GCHandle.Alloc(Data, GCHandleType.Pinned);
				header = new Header
				{
					dataBuffer = hBuffer.AddrOfPinnedObject(),
					bufferLength = SizeOfBin * Data.Length,
					userData = (IntPtr)hThis,
					loops = 1
				};
				hHeader = GCHandle.Alloc(header, GCHandleType.Pinned);

				session.PrepareHeader(header);
			}

			public void Dispose()
			{
				GC.SuppressFinalize(this);

				session.UnprepareHeader(header);

				if (hHeader.IsAllocated) hHeader.Free();
				if (hBuffer.IsAllocated) hBuffer.Free();
				if (hThis.IsAllocated) hThis.Free();

				header = default;
			}

			public void MarkAsProcessed() => session.MarkAsProcessed(header);
		}
	}
}
