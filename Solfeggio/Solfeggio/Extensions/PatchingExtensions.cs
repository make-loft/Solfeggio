using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Solfeggio.Extensions
{
	public static class PatchingExtensions
	{
		public static void Replace(this MethodInfo oldMethod, MethodInfo newMethod)
		{
			try
			{
				var typeTable = new int[128];
				var address = newMethod.DeclaringType.TypeHandle.Value;
				Marshal.Copy(address, typeTable, 0, typeTable.Length);

				var pointer = newMethod.MethodHandle.GetFunctionPointer().ToInt32();

				//var indexOfPointer = typeTable.IndexesOf(pointer).FirstOrDefault();

				var oldMethodAddress = GetMethodAddress(oldMethod);
				var newMethodAddress = GetMethodAddress(newMethod);

				//var offsetBytes = 2;

				var oldInts = new int[64];
				var newInts = new int[64];

				Marshal.Copy(oldMethodAddress, oldInts, 0, oldInts.Length);
				Marshal.Copy(newMethodAddress, newInts, 0, newInts.Length);

				//var oldValue = Marshal.ReadIntPtr(oldMethodAddress + offsetBytes);
				//var newValue = Marshal.ReadIntPtr(newMethodAddress + offsetBytes);

				var oldPointer = oldMethod.MethodHandle.GetFunctionPointer().ToInt32();
				var newPointer = newMethod.MethodHandle.GetFunctionPointer().ToInt32();

				Marshal.Copy(newInts, 0, oldMethodAddress, 1);

				var oldPointer_ = oldMethod.MethodHandle.GetFunctionPointer().ToInt32();

				if (oldPointer == oldPointer_)
					Console.WriteLine("No replace");
			}
			catch (Exception exception)
			{
				exception = exception;
			}
		}

		private static IntPtr GetMethodAddress(MethodInfo mi)
		{
			const ushort SLOT_NUMBER_MASK = 0xffff; // 2 bytes mask
			const int MT_OFFSET_32BIT = 0x28;       // 40 bytes offset
			const int MT_OFFSET_64BIT = 0x40;       // 64 bytes offset

			IntPtr address;

			// JIT compilation of the method
			RuntimeHelpers.PrepareMethod(mi.MethodHandle);

			IntPtr md = mi.MethodHandle.Value;             // MethodDescriptor address
			IntPtr mt = mi.DeclaringType.TypeHandle.Value; // MethodTable address

			if (mi.IsVirtual)
			{
				// The fixed-size portion of the MethodTable structure depends on the process type:
				// For 32-bit process (IntPtr.Size == 4), the fixed-size portion is 40 (0x28) bytes
				// For 64-bit process (IntPtr.Size == 8), the fixed-size portion is 64 (0x40) bytes
				int offset = IntPtr.Size == 4 ? MT_OFFSET_32BIT : MT_OFFSET_64BIT;

				// First method slot = MethodTable address + fixed-size offset
				// This is the address of the first method of any type (i.e. ToString)
				IntPtr ms = Marshal.ReadIntPtr(mt + offset);

				// Get the slot number of the virtual method entry from the MethodDesc data structure
				long shift = Marshal.ReadInt64(md) >> 32;
				int slot = (int)(shift & SLOT_NUMBER_MASK);

				// Get the virtual method address relative to the first method slot
				address = ms + (slot * IntPtr.Size);
			}
			else
			{
				// Bypass default MethodDescriptor padding (8 bytes) 
				// Reach the CodeOrIL field which contains the address of the JIT-compiled code
				address = md + 8;
			}

			return address;
		}

		public static void ReplaceJmp(MethodInfo source, MethodInfo target)
		{
			RuntimeHelpers.PrepareMethod(source.MethodHandle);
			RuntimeHelpers.PrepareMethod(target.MethodHandle);

			var sourceAddress = source.MethodHandle.GetFunctionPointer();
			var targetAddress = target.MethodHandle.GetFunctionPointer();

			int offset = (int)((long)targetAddress - (long)sourceAddress - 4 - 1); // four bytes for relative address and one byte for opcode

			byte[] instruction =
			{
				0xE9, // Long jump relative instruction
				(byte)((offset >> 00) & 0xFF),
				(byte)((offset >> 08) & 0xFF),
				(byte)((offset >> 16) & 0xFF),
				(byte)((offset >> 24) & 0xFF)
			};

			Marshal.Copy(instruction, 0, sourceAddress, instruction.Length);
		}
	}
}
