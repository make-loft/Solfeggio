﻿using Ace;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Solfeggio.Api.ProcessingState;

namespace Solfeggio.Api
{
	public partial class Wave
	{
		public abstract class Processor<TDeviceInfo> : IDisposable, IExposable
		{
			private readonly IDataSource<short> dataSource;
			private readonly Callback callback;

			public Processor(ASession session, IDataSource<short> source = default)
			{
				dataSource = source;
				this.session = session;
				if (source.Is()) BufferSize = source.SampleSize;
				BufferMilliseconds = 100;
				NumberOfBuffers = 3;
				callback = Callback;
				session.As<Out.Session>()?.SetVolume(1f);

				Expose();
			}

			~Processor() => Dispose();

			public void Expose()
			{
				session.Open(callback);

				CreateBuffers();
				Debug.WriteLine($"{session.Handle}.Expose()");
			}

			public void Dispose()
			{
				Debug.WriteLine($"{session.Handle}.Dispose()");
				GC.SuppressFinalize(this);

				for (var n = 0; n < buffers.Length; n++)
				{
					buffers[n].Dispose();
				}

				session.Close();
			}

			public ProcessingState State { get; private set; }

			private readonly ASession session;
			protected Buffer[] buffers;

			public event EventHandler<ProcessingEventArgs> DataAvailable;


			public int BufferMilliseconds { get; set; }
			public int NumberOfBuffers { get; set; }

			public int BufferSize { get; set; }

			protected void CreateBuffers()
			{
				// Default to three buffers of 100ms each
				var waveFormat = session.WaveFormat;
				int bufferSize = BufferSize.Is(default) ? BufferMilliseconds * waveFormat.AverageBytesPerSecond / 1000 : BufferSize;
				if (bufferSize % waveFormat.BlockAlign != 0)
				{
					bufferSize -= bufferSize % waveFormat.BlockAlign;
				}

				buffers = new Buffer[NumberOfBuffers];
				for (var n = 0; n < buffers.Length; n++)
				{
					var buffer = buffers[n] = new Buffer(session, bufferSize);
					buffer.MarkAsProcessed();
				}
			}

			private void RaiseDataAvailable(Buffer buffer) =>
				DataAvailable?.Invoke(this, new ProcessingEventArgs(buffer.Data, buffer.BinsCount));

			private Buffer Fill(in Buffer buffer, IDataSource<short> source)
			{
				lock (source)
				{
					var data = buffer.Data;
					source.Fill(data, 0, data.Length);
					return buffer;
				}
			}

			private void Callback(IntPtr waveHandle, Message message, IntPtr userData, Header header, IntPtr reserved)
			{
				if (State.IsNot(Processing)) return;

				if (message.Is(Message.WaveInData) || message.Is(Message.WaveOutDone))
				{
					GCHandle hBuffer = (GCHandle)header.userData;
					var buffer = (Buffer)hBuffer.Target;
					if (buffer.IsDone)
					{
						if (dataSource.Is()) Fill(in buffer, dataSource);
						buffer.MarkAsProcessed();
						RaiseDataAvailable(buffer);
					}
				}
			}

			public void Wake()
			{
				if (State.Is(Processing)) return;
				State = Processing;

				session.Wake().Verify();
				Debug.WriteLine($"{session.Handle}.Wake()");
			}

			public void Lull()
			{
				if (State.Is(Suspending)) return;
				State = Suspending;

				session.Lull().Verify();
				Debug.WriteLine($"{session.Handle}.Lull()");
			}

			public void Free()
			{
				if (State.Is(Hibernation)) return;
				State = Hibernation;

				Lull();
				Console.Beep();
				session.Reset().Verify();
				Debug.WriteLine($"{session.Handle}.Reset()");
			}
		}
	}
}