using System;
using System.IO.Compression;
using System.Windows.Forms;

namespace Compressor
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				foreach(var fileName in args)
				{
					var mode = fileName.EndsWith(".compressed")
						? CompressionMode.Decompress
						: CompressionMode.Compress;

					fileName.ConvertFile(mode);
				}
			}
			catch (Exception exception)
			{
				Console.Beep();
				MessageBox.Show(args[0] + "\n" + exception.ToString());
				Console.WriteLine(exception.Message);
				Console.ReadKey();
			}
		}
	}
}
