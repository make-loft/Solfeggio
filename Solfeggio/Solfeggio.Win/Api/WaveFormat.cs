using Ace;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Solfeggio.Api
{
	class AudioMediaSubtypes
	{
		// https://msdn.microsoft.com/en-us/library/windows/desktop/dd317599(v=vs.85).aspx
		public static readonly Guid MEDIASUBTYPE_PCM = new Guid("00000001-0000-0010-8000-00AA00389B71"); // PCM audio. 
		public static readonly Guid MEDIASUBTYPE_PCMAudioObsolete = new Guid("e436eb8a-524f-11ce-9f53-0020af0ba770"); // Obsolete. Do not use. 
		public static readonly Guid MEDIASUBTYPE_MPEG1Packet = new Guid("e436eb80-524f-11ce-9f53-0020af0ba770"); // MPEG1 Audio packet. 
		public static readonly Guid MEDIASUBTYPE_MPEG1Payload = new Guid("e436eb81-524f-11ce-9f53-0020af0ba770"); // MPEG1 Audio Payload. 
		public static readonly Guid MEDIASUBTYPE_MPEG2_AUDIO = new Guid("e06d802b-db46-11cf-b4d1-00805f6cbbea"); // MPEG-2 audio data  
		public static readonly Guid MEDIASUBTYPE_DVD_LPCM_AUDIO = new Guid("e06d8032-db46-11cf-b4d1-00805f6cbbea"); // DVD audio data  
		public static readonly Guid MEDIASUBTYPE_DRM_Audio = new Guid("00000009-0000-0010-8000-00aa00389b71"); // Corresponds to WAVE_FORMAT_DRM. 
		public static readonly Guid MEDIASUBTYPE_IEEE_FLOAT = new Guid("00000003-0000-0010-8000-00aa00389b71"); // Corresponds to WAVE_FORMAT_IEEE_FLOAT 
		public static readonly Guid MEDIASUBTYPE_DOLBY_AC3 = new Guid("e06d802c-db46-11cf-b4d1-00805f6cbbea"); // Dolby data  
		public static readonly Guid MEDIASUBTYPE_DOLBY_AC3_SPDIF = new Guid("00000092-0000-0010-8000-00aa00389b71"); // Dolby AC3 over SPDIF.  
		public static readonly Guid MEDIASUBTYPE_RAW_SPORT = new Guid("00000240-0000-0010-8000-00aa00389b71"); // Equivalent to MEDIASUBTYPE_DOLBY_AC3_SPDIF. 
		public static readonly Guid MEDIASUBTYPE_SPDIF_TAG_241h = new Guid("00000241-0000-0010-8000-00aa00389b71"); // Equivalent to MEDIASUBTYPE_DOLBY_AC3_SPDIF. 


		// http://msdn.microsoft.com/en-us/library/dd757532%28VS.85%29.aspx
		public static readonly Guid MEDIASUBTYPE_I420 = new Guid("30323449-0000-0010-8000-00AA00389B71");
		public static readonly Guid MEDIASUBTYPE_IYUV = new Guid("56555949-0000-0010-8000-00AA00389B71");
		public static readonly Guid MEDIASUBTYPE_RGB1 = new Guid("e436eb78-524f-11ce-9f53-0020af0ba770");
		public static readonly Guid MEDIASUBTYPE_RGB24 = new Guid("e436eb7d-524f-11ce-9f53-0020af0ba770");
		public static readonly Guid MEDIASUBTYPE_RGB32 = new Guid("e436eb7e-524f-11ce-9f53-0020af0ba770");
		public static readonly Guid MEDIASUBTYPE_RGB4 = new Guid("e436eb79-524f-11ce-9f53-0020af0ba770");
		public static readonly Guid MEDIASUBTYPE_RGB555 = new Guid("e436eb7c-524f-11ce-9f53-0020af0ba770");
		public static readonly Guid MEDIASUBTYPE_RGB565 = new Guid("e436eb7b-524f-11ce-9f53-0020af0ba770");
		public static readonly Guid MEDIASUBTYPE_RGB8 = new Guid("e436eb7a-524f-11ce-9f53-0020af0ba770");
		public static readonly Guid MEDIASUBTYPE_UYVY = new Guid("59565955-0000-0010-8000-00AA00389B71");
		public static readonly Guid MEDIASUBTYPE_VIDEOIMAGE = new Guid("1d4a45f2-e5f6-4b44-8388-f0ae5c0e0c37");
		public static readonly Guid MEDIASUBTYPE_YUY2 = new Guid("32595559-0000-0010-8000-00AA00389B71");
		public static readonly Guid MEDIASUBTYPE_YV12 = new Guid("31313259-0000-0010-8000-00AA00389B71");
		public static readonly Guid MEDIASUBTYPE_YVU9 = new Guid("39555659-0000-0010-8000-00AA00389B71");
		public static readonly Guid MEDIASUBTYPE_YVYU = new Guid("55595659-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMFORMAT_MPEG2Video = new Guid("e06d80e3-db46-11cf-b4d1-00805f6cbbea");
		public static readonly Guid WMFORMAT_Script = new Guid("5C8510F2-DEBE-4ca7-BBA5-F07A104F8DFF");
		public static readonly Guid WMFORMAT_VideoInfo = new Guid("05589f80-c356-11ce-bf01-00aa0055595a");
		public static readonly Guid WMFORMAT_WaveFormatEx = new Guid("05589f81-c356-11ce-bf01-00aa0055595a");
		public static readonly Guid WMFORMAT_WebStream = new Guid("da1e6b13-8359-4050-b398-388e965bf00c");
		public static readonly Guid WMMEDIASUBTYPE_ACELPnet = new Guid("00000130-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_Base = new Guid("00000000-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_DRM = new Guid("00000009-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_MP3 = new Guid("00000055-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_MP43 = new Guid("3334504D-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_MP4S = new Guid("5334504D-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_M4S2 = new Guid("3253344D-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_P422 = new Guid("32323450-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_MPEG2_VIDEO = new Guid("e06d8026-db46-11cf-b4d1-00805f6cbbea");
		public static readonly Guid WMMEDIASUBTYPE_MSS1 = new Guid("3153534D-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_MSS2 = new Guid("3253534D-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_PCM = new Guid("00000001-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WebStream = new Guid("776257d4-c627-41cb-8f81-7ac7ff1c40cc");
		public static readonly Guid WMMEDIASUBTYPE_WMAudio_Lossless = new Guid("00000163-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMAudioV2 = new Guid("00000161-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMAudioV7 = new Guid("00000161-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMAudioV8 = new Guid("00000161-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMAudioV9 = new Guid("00000162-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMSP1 = new Guid("0000000A-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMV1 = new Guid("31564D57-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMV2 = new Guid("32564D57-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMV3 = new Guid("33564D57-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMVA = new Guid("41564D57-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WMVP = new Guid("50564D57-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIASUBTYPE_WVP2 = new Guid("32505657-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIATYPE_Audio = new Guid("73647561-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIATYPE_FileTransfer = new Guid("D9E47579-930E-4427-ADFC-AD80F290E470");
		public static readonly Guid WMMEDIATYPE_Image = new Guid("34A50FD8-8AA5-4386-81FE-A0EFE0488E31");
		public static readonly Guid WMMEDIATYPE_Script = new Guid("73636d64-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMMEDIATYPE_Text = new Guid("9BBA1EA7-5AB2-4829-BA57-0940209BCF3E");
		public static readonly Guid WMMEDIATYPE_Video = new Guid("73646976-0000-0010-8000-00AA00389B71");
		public static readonly Guid WMSCRIPTTYPE_TwoStrings = new Guid("82f38a70-c29f-11d1-97ad-00a0c95ea850");


		// others?
		public static readonly Guid MEDIASUBTYPE_WAVE = new Guid("e436eb8b-524f-11ce-9f53-0020af0ba770");
		public static readonly Guid MEDIASUBTYPE_AU = new Guid("e436eb8c-524f-11ce-9f53-0020af0ba770");
		public static readonly Guid MEDIASUBTYPE_AIFF = new Guid("e436eb8d-524f-11ce-9f53-0020af0ba770");

		public static readonly Guid[] AudioSubTypes = {
			MEDIASUBTYPE_PCM,
			MEDIASUBTYPE_PCMAudioObsolete,
			MEDIASUBTYPE_MPEG1Packet,
			MEDIASUBTYPE_MPEG1Payload,
			MEDIASUBTYPE_MPEG2_AUDIO,
			MEDIASUBTYPE_DVD_LPCM_AUDIO,
			MEDIASUBTYPE_DRM_Audio,
			MEDIASUBTYPE_IEEE_FLOAT,
			MEDIASUBTYPE_DOLBY_AC3,
			MEDIASUBTYPE_DOLBY_AC3_SPDIF,
			MEDIASUBTYPE_RAW_SPORT,
			MEDIASUBTYPE_SPDIF_TAG_241h,
			WMMEDIASUBTYPE_MP3,
		};

		public static readonly string[] AudioSubTypeNames = {
			"PCM",
			"PCM Obsolete",
			"MPEG1Packet",
			"MPEG1Payload",
			"MPEG2_AUDIO",
			"DVD_LPCM_AUDIO",
			"DRM_Audio",
			"IEEE_FLOAT",
			"DOLBY_AC3",
			"DOLBY_AC3_SPDIF",
			"RAW_SPORT",
			"SPDIF_TAG_241h",
			"MP3"
		};
		public static string GetAudioSubtypeName(Guid subType)
		{
			for (int index = 0; index < AudioSubTypes.Length; index++)
			{
				if (subType == AudioSubTypes[index])
				{
					return AudioSubTypeNames[index];
				}
			}
			return subType.ToString();
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class WaveFormatExtensible : WaveFormat
	{
		short wValidBitsPerSample; // bits of precision, or is wSamplesPerBlock if wBitsPerSample==0
		int dwChannelMask; // which channels are present in stream
		Guid subFormat;

		/// <summary>
		/// Parameterless constructor for marshalling
		/// </summary>
		WaveFormatExtensible()
		{
		}

		/// <summary>
		/// Creates a new WaveFormatExtensible for PCM or IEEE
		/// </summary>
		public WaveFormatExtensible(int rate, int bits, int channels)
			: base(rate, bits, channels)
		{
			waveFormatTag = WaveFormatEncoding.Extensible;
			extraSize = 22;
			wValidBitsPerSample = (short)bits;
			for (int n = 0; n < channels; n++)
			{
				dwChannelMask |= (1 << n);
			}
			if (bits == 32)
			{
				// KSDATAFORMAT_SUBTYPE_IEEE_FLOAT
				subFormat = AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT;
			}
			else
			{
				// KSDATAFORMAT_SUBTYPE_PCM
				subFormat = AudioMediaSubtypes.MEDIASUBTYPE_PCM;
			}

		}

		/// <summary>
		/// WaveFormatExtensible for PCM or floating point can be awkward to work with
		/// This creates a regular WaveFormat structure representing the same audio format
		/// Returns the WaveFormat unchanged for non PCM or IEEE float
		/// </summary>
		/// <returns></returns>
		public WaveFormat ToStandardWaveFormat()
		{
			if (subFormat == AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT && bitsPerSample == 32)
				return CreateIeeeFloatWaveFormat(sampleRate, channels);
			if (subFormat == AudioMediaSubtypes.MEDIASUBTYPE_PCM)
				return new WaveFormat(sampleRate, bitsPerSample, channels);
			return this;
			//throw new InvalidOperationException("Not a recognised PCM or IEEE float format");
		}

		/// <summary>
		/// SubFormat (may be one of AudioMediaSubtypes)
		/// </summary>
		public Guid SubFormat { get { return subFormat; } }

		/// <summary>
		/// Serialize
		/// </summary>
		/// <param name="writer"></param>
		public override void Serialize(System.IO.BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(wValidBitsPerSample);
			writer.Write(dwChannelMask);
			byte[] guid = subFormat.ToByteArray();
			writer.Write(guid, 0, guid.Length);
		}

		/// <summary>
		/// String representation
		/// </summary>
		public override string ToString()
		{
			return String.Format("{0} wBitsPerSample:{1} dwChannelMask:{2} subFormat:{3} extraSize:{4}",
				base.ToString(),
				wValidBitsPerSample,
				dwChannelMask,
				subFormat,
				extraSize);
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class WaveFormatExtraData : WaveFormat
	{
		// try with 100 bytes for now, increase if necessary
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
		private byte[] extraData = new byte[100];

		/// <summary>
		/// Allows the extra data to be read
		/// </summary>
		public byte[] ExtraData => extraData;

		/// <summary>
		/// parameterless constructor for marshalling
		/// </summary>
		internal WaveFormatExtraData()
		{
		}

		/// <summary>
		/// Reads this structure from a BinaryReader
		/// </summary>
		public WaveFormatExtraData(BinaryReader reader)
			: base(reader)
		{
			ReadExtraData(reader);
		}

		internal void ReadExtraData(BinaryReader reader)
		{
			if (this.extraSize > 0)
			{
				reader.Read(extraData, 0, extraSize);
			}
		}

		/// <summary>
		/// Writes this structure to a BinaryWriter
		/// </summary>
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			if (extraSize > 0)
			{
				writer.Write(extraData, 0, extraSize);
			}
		}
	}

	public static class MmControl
	{
		public static MmResult Verify(this MmResult result, [CallerMemberName]string source = null) => result.Is(MmResult.NoError)
			? result
			: throw new MmException(result, source);
	}

	public class MmException : Exception
	{
		public MmException(MmResult result, string function)
			: base(ErrorMessage(result, function))
		{
			Result = result;
			Source = function;
		}

		private static string ErrorMessage(MmResult result, string function) =>
			string.Format("{0} calling {1}", result, function);


		public MmResult Result { get; }
		public new string Source { get; }
	}

	public static class MarshalHelpers
	{
		/// <summary>
		/// SizeOf a structure
		/// </summary>
		public static int SizeOf<T>()
		{
			return Marshal.SizeOf(typeof(T));
		}

		/// <summary>
		/// Offset of a field in a structure
		/// </summary>
		//public static IntPtr OffsetOf<T>(string fieldName)
		//{
		//    return Marshal.OffsetOf(typeof(T), fieldName);
		//}

		/// <summary>
		/// Pointer to Structure
		/// </summary>
		public static T PtrToStructure<T>(IntPtr pointer)
		{
			return (T)Marshal.PtrToStructure(pointer, typeof(T));
		}
	}

	public enum WaveFormatEncoding : ushort
	{
		Unknown = 0x0000,
		Pcm = 0x0001,
		Adpcm = 0x0002,
		IeeeFloat = 0x0003,
		ALaw = 0x0006,
		MuLaw = 0x0007,
		Gsm610 = 0x0031,
		Extensible = 0xFFFE,
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class WaveFormat
	{
		/// <summary>format type</summary>
		protected WaveFormatEncoding waveFormatTag;
		/// <summary>number of channels</summary>
		protected short channels;
		/// <summary>sample rate</summary>
		protected int sampleRate;
		/// <summary>for buffer estimation</summary>
		protected int averageBytesPerSecond;
		/// <summary>block size of data</summary>
		protected short blockAlign;
		/// <summary>number of bits per sample of mono data</summary>
		protected short bitsPerSample;
		/// <summary>number of following bytes</summary>
		protected short extraSize;

		/// <summary>
		/// Creates a new PCM 44.1Khz stereo 16 bit format
		/// </summary>
		public WaveFormat() : this(44100, 16, 2)
		{

		}

		/// <summary>
		/// Creates a new 16 bit wave format with the specified sample
		/// rate and channel count
		/// </summary>
		/// <param name="sampleRate">Sample Rate</param>
		/// <param name="channels">Number of channels</param>
		public WaveFormat(int sampleRate, int channels)
			: this(sampleRate, 16, channels)
		{
		}

		/// <summary>
		/// Gets the size of a wave buffer equivalent to the latency in milliseconds.
		/// </summary>
		/// <param name="milliseconds">The milliseconds.</param>
		/// <returns></returns>
		public int ConvertLatencyToByteSize(int milliseconds)
		{
			int bytes = (int)((AverageBytesPerSecond / 1000.0) * milliseconds);
			if ((bytes % BlockAlign) != 0)
			{
				// Return the upper BlockAligned
				bytes = bytes + BlockAlign - (bytes % BlockAlign);
			}
			return bytes;
		}

		/// <summary>
		/// Creates a WaveFormat with custom members
		/// </summary>
		/// <param name="tag">The encoding</param>
		/// <param name="sampleRate">Sample Rate</param>
		/// <param name="channels">Number of channels</param>
		/// <param name="averageBytesPerSecond">Average Bytes Per Second</param>
		/// <param name="blockAlign">Block Align</param>
		/// <param name="bitsPerSample">Bits Per Sample</param>
		/// <returns></returns>
		public static WaveFormat CreateCustomFormat(WaveFormatEncoding tag, int sampleRate, int channels, int averageBytesPerSecond, int blockAlign, int bitsPerSample) => new WaveFormat
		{
			waveFormatTag = tag,
			channels = (short)channels,
			sampleRate = sampleRate,
			averageBytesPerSecond = averageBytesPerSecond,
			blockAlign = (short)blockAlign,
			bitsPerSample = (short)bitsPerSample,
			extraSize = 0
		};

		/// <summary>
		/// Creates an A-law wave format
		/// </summary>
		/// <param name="sampleRate">Sample Rate</param>
		/// <param name="channels">Number of Channels</param>
		/// <returns>Wave Format</returns>
		public static WaveFormat CreateALawFormat(int sampleRate, int channels)
		{
			return CreateCustomFormat(WaveFormatEncoding.ALaw, sampleRate, channels, sampleRate * channels, channels, 8);
		}

		/// <summary>
		/// Creates a Mu-law wave format
		/// </summary>
		/// <param name="sampleRate">Sample Rate</param>
		/// <param name="channels">Number of Channels</param>
		/// <returns>Wave Format</returns>
		public static WaveFormat CreateMuLawFormat(int sampleRate, int channels)
		{
			return CreateCustomFormat(WaveFormatEncoding.MuLaw, sampleRate, channels, sampleRate * channels, channels, 8);
		}

		/// <summary>
		/// Creates a new PCM format with the specified sample rate, bit depth and channels
		/// </summary>
		public WaveFormat(int rate, int bits, int channels)
		{
			if (channels < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(channels), "Channels must be 1 or greater");
			}
			// minimum 16 bytes, sometimes 18 for PCM
			waveFormatTag = WaveFormatEncoding.Pcm;
			this.channels = (short)channels;
			sampleRate = rate;
			bitsPerSample = (short)bits;
			extraSize = 0;

			blockAlign = (short)(channels * (bits / 8));
			averageBytesPerSecond = sampleRate * blockAlign;
		}

		/// <summary>
		/// Creates a new 32 bit IEEE floating point wave format
		/// </summary>
		/// <param name="sampleRate">sample rate</param>
		/// <param name="channels">number of channels</param>
		public static WaveFormat CreateIeeeFloatWaveFormat(int sampleRate, int channels) => new WaveFormat
		{
			waveFormatTag = WaveFormatEncoding.IeeeFloat,
			channels = (short)channels,
			bitsPerSample = 32,
			sampleRate = sampleRate,
			blockAlign = (short)(4 * channels),
			averageBytesPerSecond = sampleRate * (4 * channels),
			extraSize = 0
		};

		/// <summary>
		/// Helper function to retrieve a WaveFormat structure from a pointer
		/// </summary>
		/// <param name="pointer">WaveFormat structure</param>
		/// <returns></returns>
		public static WaveFormat MarshalFromPtr(IntPtr pointer)
		{
			var waveFormat = MarshalHelpers.PtrToStructure<WaveFormat>(pointer);
			switch (waveFormat.Encoding)
			{
				case WaveFormatEncoding.Pcm:
					// can't rely on extra size even being there for PCM so blank it to avoid reading
					// corrupt data
					waveFormat.extraSize = 0;
					break;
				case WaveFormatEncoding.Extensible:
					waveFormat = MarshalHelpers.PtrToStructure<WaveFormatExtensible>(pointer);
					break;
				case WaveFormatEncoding.Adpcm:
					//waveFormat = MarshalHelpers.PtrToStructure<AdpcmWaveFormat>(pointer);
					break;
				case WaveFormatEncoding.Gsm610:
					//waveFormat = MarshalHelpers.PtrToStructure<Gsm610WaveFormat>(pointer);
					break;
				default:
					if (waveFormat.ExtraSize > 0)
					{
						waveFormat = MarshalHelpers.PtrToStructure<WaveFormatExtraData>(pointer);
					}
					break;
			}
			return waveFormat;
		}

		/// <summary>
		/// Helper function to marshal WaveFormat to an IntPtr
		/// </summary>
		/// <param name="format">WaveFormat</param>
		/// <returns>IntPtr to WaveFormat structure (needs to be freed by callee)</returns>
		public static IntPtr MarshalToPtr(WaveFormat format)
		{
			IntPtr formatPointer = default; //= Marshal.AllocHGlobal(formatSize);
			Marshal.StructureToPtr(format, formatPointer, false);
			return formatPointer;
		}

		/// <summary>
		/// Reads in a WaveFormat (with extra data) from a fmt chunk (chunk identifier and
		/// length should already have been read)
		/// </summary>
		/// <param name="br">Binary reader</param>
		/// <param name="formatChunkLength">Format chunk length</param>
		/// <returns>A WaveFormatExtraData</returns>
		public static WaveFormat FromFormatChunk(BinaryReader br, int formatChunkLength)
		{
			var waveFormat = new WaveFormatExtraData();
			waveFormat.ReadWaveFormat(br, formatChunkLength);
			waveFormat.ReadExtraData(br);
			return waveFormat;
		}

		private void ReadWaveFormat(BinaryReader br, int formatChunkLength)
		{
			if (formatChunkLength < 16)
				throw new InvalidDataException("Invalid WaveFormat Structure");
			waveFormatTag = (WaveFormatEncoding)br.ReadUInt16();
			channels = br.ReadInt16();
			sampleRate = br.ReadInt32();
			averageBytesPerSecond = br.ReadInt32();
			blockAlign = br.ReadInt16();
			bitsPerSample = br.ReadInt16();
			if (formatChunkLength > 16)
			{
				extraSize = br.ReadInt16();
				if (extraSize != formatChunkLength - 18)
				{
					Debug.WriteLine("Format chunk mismatch");
					extraSize = (short)(formatChunkLength - 18);
				}
			}
		}

		/// <summary>
		/// Reads a new WaveFormat object from a stream
		/// </summary>
		/// <param name="br">A binary reader that wraps the stream</param>
		public WaveFormat(BinaryReader br)
		{
			int formatChunkLength = br.ReadInt32();
			ReadWaveFormat(br, formatChunkLength);
		}

		/// <summary>
		/// Reports this WaveFormat as a string
		/// </summary>
		/// <returns>String describing the wave format</returns>
		public override string ToString()
		{
			switch (waveFormatTag)
			{
				case WaveFormatEncoding.Pcm:
				case WaveFormatEncoding.Extensible:
					// extensible just has some extra bits after the PCM header
					return $"{bitsPerSample} bit PCM: {sampleRate / 1000}kHz {channels} channels";
				default:
					return waveFormatTag.ToString();
			}
		}

		/// <summary>
		/// Compares with another WaveFormat object
		/// </summary>
		/// <param name="obj">Object to compare to</param>
		/// <returns>True if the objects are the same</returns>
		public override bool Equals(object obj)
		{
			if (obj is WaveFormat other)
			{
				return waveFormatTag == other.waveFormatTag &&
					channels == other.channels &&
					sampleRate == other.sampleRate &&
					averageBytesPerSecond == other.averageBytesPerSecond &&
					blockAlign == other.blockAlign &&
					bitsPerSample == other.bitsPerSample;
			}
			return false;
		}

		/// <summary>
		/// Provides a Hashcode for this WaveFormat
		/// </summary>
		/// <returns>A hashcode</returns>
		public override int GetHashCode()
		{
			return (int)waveFormatTag ^
				(int)channels ^
				sampleRate ^
				averageBytesPerSecond ^
				(int)blockAlign ^
				(int)bitsPerSample;
		}

		/// <summary>
		/// Returns the encoding type used
		/// </summary>
		public WaveFormatEncoding Encoding => waveFormatTag;

		/// <summary>
		/// Writes this WaveFormat object to a stream
		/// </summary>
		/// <param name="writer">the output stream</param>
		public virtual void Serialize(BinaryWriter writer)
		{
			writer.Write((int)(18 + extraSize)); // wave format length
			writer.Write((short)Encoding);
			writer.Write((short)Channels);
			writer.Write((int)SampleRate);
			writer.Write((int)AverageBytesPerSecond);
			writer.Write((short)BlockAlign);
			writer.Write((short)BitsPerSample);
			writer.Write((short)extraSize);
		}

		/// <summary>
		/// Returns the number of channels (1=mono,2=stereo etc)
		/// </summary>
		public int Channels => channels;

		/// <summary>
		/// Returns the sample rate (samples per second)
		/// </summary>
		public int SampleRate => sampleRate;

		/// <summary>
		/// Returns the average number of bytes used per second
		/// </summary>
		public int AverageBytesPerSecond => averageBytesPerSecond;

		/// <summary>
		/// Returns the block alignment
		/// </summary>
		public virtual int BlockAlign => blockAlign;

		/// <summary>
		/// Returns the number of bits per sample (usually 16 or 32, sometimes 24 or 8)
		/// Can be 0 for some codecs
		/// </summary>
		public int BitsPerSample => bitsPerSample;

		/// <summary>
		/// Returns the number of extra bytes used by this waveformat. Often 0,
		/// except for compressed formats which store extra data after the WAVEFORMATEX header
		/// </summary>
		public int ExtraSize => extraSize;
	}
}