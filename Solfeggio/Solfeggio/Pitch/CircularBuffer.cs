using System;

namespace Pitch
{
	public class CircularBuffer<T> : IDisposable
	{
		int m_bufSize;
		int m_begBufOffset;
		int m_availBuf;
		T[] m_buffer;

		public CircularBuffer()
		{
		}

		public CircularBuffer(int bufCount) => SetSize(bufCount);
		public void Dispose() => SetSize(0);


		public void Reset()
		{
			m_begBufOffset = 0;
			m_availBuf = 0;
			StartPosition = 0;
		}

		public void SetSize(int newSize)
		{
			Reset();

			if (m_bufSize == newSize)
				return;

			m_buffer = null;

			m_bufSize = newSize;

			if (m_bufSize > 0)
				m_buffer = new T[m_bufSize];
		}

		public void Clear() => Array.Clear(m_buffer, 0, m_buffer.Length);

		public long StartPosition { get; set; }

		public long EndPosition => StartPosition + m_availBuf;

		public int Available
		{
			get => m_availBuf;
			set => m_availBuf = Math.Min(value, m_bufSize);
		}

		public int WriteBuffer(T[] m_pInBuffer, int count)
		{
			count = Math.Min(count, m_bufSize);

			var startPos = m_availBuf != m_bufSize ? m_availBuf : m_begBufOffset;
			var pass1Count = Math.Min(count, m_bufSize - startPos);
			var pass2Count = count - pass1Count;

			PitchDsp.CopyBuffer(m_pInBuffer, 0, m_buffer, startPos, pass1Count);

			if (pass2Count > 0)
				PitchDsp.CopyBuffer(m_pInBuffer, pass1Count, m_buffer, 0, pass2Count);

			if (pass2Count == 0)
			{
				// did not wrap around
				if (m_availBuf != m_bufSize)
					m_availBuf += count;   // have never wrapped around
				else
				{
					m_begBufOffset += count;
					StartPosition += count;
				}
			}
			else
			{
				// wrapped around
				if (m_availBuf != m_bufSize)
					StartPosition += pass2Count;  // first time wrap-around
				else
					StartPosition += count;

				m_begBufOffset = pass2Count;
				m_availBuf = m_bufSize;
			}

			return count;
		}

		public bool ReadBuffer(T[] outBuffer, long startRead, int readCount)
		{
			var endRead = (int)(startRead + readCount);
			var endAvail = (int)(StartPosition + m_availBuf);

			if (startRead < StartPosition || endRead > endAvail)
				return false;

			var startReadPos = (int)(((startRead - StartPosition) + m_begBufOffset) % m_bufSize);
			var block1Samples = Math.Min(readCount, m_bufSize - startReadPos);
			var block2Samples = readCount - block1Samples;

			PitchDsp.CopyBuffer(m_buffer, startReadPos, outBuffer, 0, block1Samples);

			if (block2Samples > 0)
				PitchDsp.CopyBuffer(m_buffer, 0, outBuffer, block1Samples, block2Samples);

			return true;
		}
	}
}
