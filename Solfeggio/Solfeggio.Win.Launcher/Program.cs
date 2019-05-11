using Compressor;
using Solfeggio.Launcher.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading;

namespace Solfeggio.Launcher
{
	class Program
	{
		static Program()
		{
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			AppDomain.CurrentDomain.LoadAndResolve(GetAppRawAssembly());
			AppDomain.CurrentDomain.LoadAndResolve(GetAppNestedRawAssemblies());
		}

		static byte[] GetAppRawAssembly() => Assemblies.App_exe.ConvertBytes(CompressionMode.Decompress);
		static IEnumerable<byte[]> GetAppNestedRawAssemblies() => App.EnumerateNestedRawAssemblies();

		[STAThread]
		static void Main(string[] _) => App.Main();

		//static void ProcessConfiguration()
		//{
		//	var configuration = ConfigurationManager.OpenExeConfiguration(ExecutableFilePath);
		//	if (!configuration.HasFile)
		//		File.WriteAllText(configuration.FilePath, Properties.Resources.App_exe_config);
		//}
	}
}
