using System;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	public partial class Wave
    {
		[StructLayout(LayoutKind.Sequential)]
		public class Header
		{
			[Flags]
			public enum Flags
			{
				Done = 0x00000001,
				Prepared = 0x00000002,
				BeginLoop = 0x00000004,
				EndLoop = 0x00000008,
				InQueue = 0x00000010,
			}

			public IntPtr dataBuffer;
			public int bufferLength;
			public int bytesRecorded;
			public IntPtr userData;
			public Flags flags;
			public int loops;
			public IntPtr next;
			public IntPtr reserved;
		}
	}
}
