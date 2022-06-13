using Ace;

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Solfeggio.Headers
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class WaveHeader
	{
		/* ChunkID		  Contains the letters "RIFF" in ASCII form */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		private char[] chunkID = "RIFF".ToCharArray();

		/* ChunkSize		36 + SubChunk2Size */
		[MarshalAs(UnmanagedType.U4, SizeConst = 4)]
		private int chunkSize = 0;

		/* Format		   The "WAVE" format name */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		private char[] format = "WAVE".ToCharArray();

		/* Subchunk1ID	  Contains the letters "fmt " */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		private char[] subchunk1ID = new char[] { 'f', 'm', 't', ' ' };

		/* Subchunk1Size	16 for PCM */
		[MarshalAs(UnmanagedType.U4, SizeConst = 4)]
		private int subchunk1Size = 16;

		/* AudioFormat	  PCM = 1 (i.e. Linear quantization) */
		[MarshalAs(UnmanagedType.U2, SizeConst = 2)]
		private short audioFormat = 1;

		/* NumChannels	  Mono = 1, Stereo = 2, etc. */
		[MarshalAs(UnmanagedType.U2, SizeConst = 2)]
		private short numChannels = 1;
		public short NumChannels { get => numChannels; set => numChannels = value; }

		/* SampleRate	   8000, 44100, etc. */
		[MarshalAs(UnmanagedType.U4, SizeConst = 4)]
		private int sampleRate = 44100;
		public int SampleRate { get => sampleRate; set => sampleRate = value; }

		/* ByteRate		 == SampleRate * NumChannels * BitsPerSample/8 */
		[MarshalAs(UnmanagedType.U4, SizeConst = 4)]
		private int byteRate = 0;

		/* BlockAlign	   == NumChannels * BitsPerSample/8 */
		[MarshalAs(UnmanagedType.U2, SizeConst = 2)]
		private short blockAlign = 0;

		/* BitsPerSample	8 bits = 8, 16 bits = 16, etc. */
		[MarshalAs(UnmanagedType.U2, SizeConst = 2)]
		private short bitsPerSample = 16;
		public short BitsPerSample { get => bitsPerSample; set => bitsPerSample = value; }

		/* Subchunk2ID	  Contains the letters "data" */
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		private char[] subchunk2ID = new char[] { 'd', 'a', 't', 'a' };

		/* Subchunk2Size	== NumSamples * NumChannels * BitsPerSample/8 */
		[MarshalAs(UnmanagedType.U4, SizeConst = 4)]
		public int subchunk2Size = 0;

		public WaveHeader() : this(1) { }

		public WaveHeader(short numChannels = 2, int sampleRate = 44100, short bitsPerSample = 16)
		{
			NumChannels = numChannels;
			SampleRate = sampleRate;
			BitsPerSample = bitsPerSample;
		}

		public void CalculateSizes(long length)
		{
			subchunk2Size = (int)length;
			blockAlign = (short)(NumChannels * BitsPerSample / 8);
			byteRate = SampleRate * NumChannels * BitsPerSample / 8;
			chunkSize = 36 + subchunk2Size;
		}

		public byte[] ToBytes()
		{
			int headerSize = Marshal.SizeOf(this);
			var headerPtr = Marshal.AllocHGlobal(headerSize);
			Marshal.StructureToPtr(this, headerPtr, false);
			var rawData = new byte[headerSize];
			Marshal.Copy(headerPtr, rawData, 0, headerSize);
			Marshal.FreeHGlobal(headerPtr);
			return rawData;
		}
	}

	public class WaveContainer
	{
		public WaveHeader Header { get; set; }
		public byte[] Data { get; }

		public string Path { get; }
		public Stream GetDataStream()
		{
			var stream = File.OpenRead(Path);
			stream.Seek(44, SeekOrigin.Begin);
			return stream;
		}

		public WaveContainer(string path)
		{
			Path = path;
			using var stream = File.OpenRead(path);
			using var reader = new BinaryReader(stream);
			var headerBytes = reader.ReadBytes(44);
			Header = headerBytes.To<WaveHeader>();
			var hBytes = Header.ToBytes();
		}

		public WaveContainer(WaveHeader header, byte[] data)
		{
			Header = header;
			Data = data;
		}

		public byte[] ToBytes()
		{
			Header.CalculateSizes(Data.Length);
			int headerSize = Marshal.SizeOf(Header);
			var headerPtr = Marshal.AllocHGlobal(headerSize);
			Marshal.StructureToPtr(this, headerPtr, false);
			var rawData = new byte[headerSize + Data.Length];
			Marshal.Copy(headerPtr, rawData, 0, headerSize);
			Marshal.FreeHGlobal(headerPtr);
			Array.Copy(Data, 0, rawData, 44, Data.Length);
			return rawData;
		}
	}
}
