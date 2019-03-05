using System.Collections.Generic;

namespace Pitch
{
	public static class Extensions
	{
		public static void Clear<T>(this T[] buffer, T value = default) => buffer.Clear(0, buffer.Length, value);
		public static void Clear<T>(this T[] buffer, int start, int length, T value = default)
		{
			if (EqualityComparer<T>.Default.Equals(value, default)) System.Array.Clear(buffer, start, length);
			else for (var i = start; i < length; i++) buffer[i] = value;
		}

		public static void Copy(this float[] fromBuffer, float[] toBuffer, int fromStart, int toStart, int length)
		{
			if (toBuffer is null || fromBuffer.Length is 0 || toBuffer.Length is 0)
				return;

			var fromBegIdx = fromStart;
			var fromEndIdx = fromStart + length;
			var toBegIdx = toStart;
			var toEndIdx = toStart + length;

			if (fromBegIdx < 0)
			{
				toBegIdx -= fromBegIdx;
				fromBegIdx = 0;
			}

			if (toBegIdx < 0)
			{
				fromBegIdx -= toBegIdx;
				toBegIdx = 0;
			}

			if (fromEndIdx >= fromBuffer.Length)
			{
				toEndIdx -= fromEndIdx - fromBuffer.Length + 1;
				fromEndIdx = fromBuffer.Length - 1;
			}

			if (toEndIdx >= toBuffer.Length)
			{
				fromEndIdx -= toEndIdx - toBuffer.Length + 1;
				toEndIdx = fromBuffer.Length - 1;
			}

			if (fromBegIdx < toBegIdx)
			{
				// Shift right, so start at the right
				for (int fromIdx = fromEndIdx, toIdx = toEndIdx; fromIdx >= fromBegIdx; fromIdx--, toIdx--)
					toBuffer[toIdx] = fromBuffer[fromIdx];
			}
			else
			{
				// Shift left, so start at the left
				for (int fromIdx = fromBegIdx, toIdx = toBegIdx; fromIdx <= fromEndIdx; fromIdx++, toIdx++)
					toBuffer[toIdx] = fromBuffer[fromIdx];
			}
		}
	}
}
