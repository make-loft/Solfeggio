using Ace;
using System;
using System.Runtime.InteropServices;
using static Solfeggio.Api.Wave.Header.Flags;

namespace Solfeggio.Api
{
	public partial class Wave
	{
		public class Buffer : Buffer<short>
		{
			public Buffer(ASession session, int binsCount) :
				base(session, binsCount) { }
		}

		public class Buffer<TBin> : IDisposable, IExposable where TBin : struct
		{
			private static readonly int SizeOfBin = Marshal.SizeOf(typeof(TBin));

			private GCHandle _bufferHandle;
			private GCHandle _headerHandle; // we need to pin the header structure
			private GCHandle _thisHandle; // for the user callback
			private Header _header;
			private readonly ASession _session;

			public TBin[] Data { get; }

			public bool IsDone => (_header.flags & Done) == Done;
			public bool IsInQueue => (_header.flags & InQueue) == InQueue;
			public int BinsCount => _header.bytesRecorded / SizeOfBin;

			public Buffer(ASession session, int binsCount)
			{
				_session = session;
				Data = new TBin[binsCount];

				Expose();
			}

			~Buffer() => Dispose();

			public void Expose()
			{
				_thisHandle = GCHandle.Alloc(this);
				_bufferHandle = GCHandle.Alloc(Data, GCHandleType.Pinned);
				_header = new Header
				{
					dataBuffer = _bufferHandle.AddrOfPinnedObject(),
					bufferLength = SizeOfBin * Data.Length,
					userData = (IntPtr)_thisHandle,
					loops = 1
				};
				_headerHandle = GCHandle.Alloc(_header, GCHandleType.Pinned);

				_session.PrepareHeader(_header).Verify();
			}

			public void Dispose()
			{
				GC.SuppressFinalize(this);

				_session.UnprepareHeader(_header).Verify();

				if (_headerHandle.IsAllocated) _headerHandle.Free();
				if (_bufferHandle.IsAllocated) _bufferHandle.Free();
				if (_thisHandle.IsAllocated) _thisHandle.Free();

				_header = default;
			}

			public MmResult MarkAsProcessed() => _session.MarkAsProcessed(_header).Verify();
		}
	}
}
