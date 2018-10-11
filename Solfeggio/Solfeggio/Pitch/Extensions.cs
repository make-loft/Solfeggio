using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pitch
{
	public static class Extensions
	{
		public static void Clear<T>(this T[] buffer) => Array.Clear(buffer, 0, buffer.Length);

		public static void Copy(this float[] fromBuffer, float[] toBuffer, int fromStart, int toStart, int length)
		{
			if (toBuffer == null || fromBuffer.Length == 0 || toBuffer.Length == 0)
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

		public static void Clear(this float[] buffer, int startIdx, int endIdx)
		{
			Array.Clear(buffer, startIdx, endIdx - startIdx + 1);
		}

		public static void Fill(this double[] buffer, double value)
		{
			for (int idx = 0; idx < buffer.Length; idx++)
				buffer[idx] = value;
		}
	}
}
