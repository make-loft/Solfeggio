using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;

using Compressor;

using Solfeggio.Launcher.Properties;

namespace Solfeggio.Launcher;

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
		domain.AssemblyResolve += (sender, args) => domain.GetAssemblies()
			.FirstOrDefault(a => a.GetName().Name == new AssemblyName(args.Name).Name)
			;
		GetAppNestedRawAssemblies().Select(domain.Load).ToList();
	}

	static byte[] GetAppRawAssembly() => Assemblies.App_exe.ConvertBytes(CompressionMode.Decompress);
	static IEnumerable<byte[]> GetAppNestedRawAssemblies() => App.EnumerateNestedRawAssemblies();

	[STAThread] static void Main(string[] args) => App.Main(args);

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
		var appDirectory = Path.GetDirectoryName(appPath);
		Directory.GetFiles(appDirectory)
			.Where(f => f.EndsWith(".dll") || f.EndsWith(".pdb"))
			.Select(TryDelete)
			.ToList()
			;

		var suspectedPath = Path.Combine(appDirectory, "Solfeggio.exe");
		if (appPath != suspectedPath) TryDelete(suspectedPath);
	}
}
