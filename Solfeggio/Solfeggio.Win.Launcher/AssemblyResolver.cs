using System;
using System.Collections.Generic;
using System.Reflection;

namespace Solfeggio
{
	internal static class AssemblyResolver
	{
		public static void LoadAndResolve(this AppDomain domain, byte[] rawAssembly) =>
			Assembly.Load(rawAssembly).Resolve(domain);

		public static void LoadAndResolve(this AppDomain domain, IEnumerable<byte[]> rawAssemblies)
		{
			foreach (var rawAssembly in rawAssemblies) domain.LoadAndResolve(rawAssembly);
		}		

		public static Assembly Resolve(this Assembly assembly, AppDomain domain)
		{
			Assembly onResolve(object sender, ResolveEventArgs args)
			{
				domain.AssemblyResolve -= onResolve;
				return assembly.FullName == args.Name ? assembly : default;
			}

			try
			{
				domain.AssemblyResolve += onResolve;
				domain.CreateInstance(assembly.FullName, "AnyType");
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
			}

			return assembly;
		}
	}
}