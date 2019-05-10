using System.IO;
using System.IO.Compression;
using static System.IO.Compression.CompressionMode;

namespace Compressor
{
	public static class FileCompressor
	{
		public static string GetDefaultOutputFileName(string inputFileName, CompressionMode mode) => mode is Compress
			? inputFileName + ".compressed"
			: inputFileName.Replace(".compressed", string.Empty);

		public static void ConvertFile(this string inputFileName, CompressionMode mode, string outputFileName = null)
		{
			var inputFileBytes = File.ReadAllBytes(inputFileName);
			var outputFileBytes = ConvertBytes(inputFileBytes, mode);
			outputFileName = outputFileName ?? GetDefaultOutputFileName(inputFileName, mode);
			File.WriteAllBytes(outputFileName, outputFileBytes);
		}

		public static byte[] ConvertBytes(this byte[] inputBytes, CompressionMode mode)
		{
			using (var inputStream = new MemoryStream(inputBytes))
			using (var outputStream = new MemoryStream())
			{
				if (mode is CompressionMode.Compress)
					using (var convertStream = new GZipStream(outputStream, Compress))
						inputStream.CopyTo(convertStream);

				if (mode is CompressionMode.Decompress)
					using (var convertStream = new GZipStream(inputStream, Decompress))
						convertStream.CopyTo(outputStream);

				var outputBytes = outputStream.ToArray();
				return outputBytes;
			}
		}
	}
}
