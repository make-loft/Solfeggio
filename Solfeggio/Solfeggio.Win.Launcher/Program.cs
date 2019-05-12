using Compressor;
using Solfeggio.Launcher.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Solfeggio.Launcher
{
	class Program
	{
		static Program()
		{
			var currentProcess = Process.GetCurrentProcess();
			CleanAssemblyFiles(currentProcess.MainModule.FileName);

			try
			{
				currentProcess.PriorityClass = ProcessPriorityClass.High;
				Thread.CurrentThread.Priority = ThreadPriority.Highest;
			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception);
			}

			var domain = AppDomain.CurrentDomain;
			var appAssembly = domain.Load(GetAppRawAssembly());
			domain.AssemblyResolve += (o, e) =>
				domain.GetAssemblies().FirstOrDefault(a => a.FullName == e.Name);

			GetAppNestedRawAssemblies().ToList().ForEach(b => domain.Load(b));
		}

		static byte[] GetAppRawAssembly() => Assemblies.App_exe.ConvertBytes(CompressionMode.Decompress);
		static IEnumerable<byte[]> GetAppNestedRawAssemblies() => App.EnumerateNestedRawAssemblies();

		[STAThread] static void Main(string[] _) => App.Main();

		static bool TryDelete(string path)
		{
			try
			{
				if (File.Exists(path))
					File.Delete(path);
				return true;
			}
			catch
			{
				return false;
			}
		}

		static void CleanAssemblyFiles(string appPath)
		{
			var files = new[]
			{
				"Ace.dll", "Ace.pdb",
				"Rainbow.dll", "Rainbow.pdb",
				"Xamarin.Synonyms.dll", "Xamarin.Synonyms.pdb",
				"Yandex.Metrica.NET.dll",
				"Solfeggio.pdb",
			};

			var appDirectory = Path.GetDirectoryName(appPath);
			var suspectedPath = Path.Combine(appDirectory, "Solfeggio.exe");
			foreach (var file in files) TryDelete(Path.Combine(appDirectory, file));
			if (appPath != suspectedPath) TryDelete(suspectedPath);
		}
	}
}
