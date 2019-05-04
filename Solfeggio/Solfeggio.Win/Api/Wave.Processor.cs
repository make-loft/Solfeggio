using Ace;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Solfeggio.Api.ProcessingState;

namespace Solfeggio.Api
{
	public interface IProcessor
	{
		void Wake();
		void Lull();
		void Free();

		event EventHandler<ProcessingEventArgs> DataAvailable;
	}

	public partial class Wave
	{
		public abstract class Processor<TDeviceInfo> : IDisposable, IExposable, IProcessor
		{
			private readonly IDataSource<short> dataSource;
			private readonly Callback callback;

			public Processor(ASession session, int bufferSize, IDataSource<short> source = default)
			{
				dataSource = source;
				this.session = session;
				BufferSize = bufferSize;
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

			public int NumberOfBuffers { get; set; }

			public int BufferSize { get; set; }

			protected void CreateBuffers()
			{
				var bufferSize = BufferSize;
				//var waveFormat = session.WaveFormat;
				//if (bufferSize % waveFormat.BlockAlign != 0)
				//{
				//	bufferSize -= bufferSize % waveFormat.BlockAlign;
				//}

				buffers = new Buffer[NumberOfBuffers];
				for (var n = 0; n < buffers.Length; n++)
				{
					var buffer = buffers[n] = new Buffer(session, bufferSize);
					buffer.MarkAsProcessed();
				}
			}

			private Buffer Fill(in Buffer buffer, IDataSource<short> source)
			{
				//lock (source)
				{
					var data = buffer.Data;
					source.Fill(data, 0, data.Length);
					return buffer;
				}
			}

			public event EventHandler<ProcessingEventArgs> DataAvailable;

			private void Callback(IntPtr waveHandle, Message message, IntPtr userData, Header header, IntPtr reserved)
			{
				if (State.IsNot(Processing)) return;

				if (message.Is(Message.WaveInData) || message.Is(Message.WaveOutDone))
				{
					var handle = (GCHandle)header.userData;
					var buffer = (Buffer)handle.Target;
					if (buffer.IsDone)
					{
						if (dataSource.Is()) Fill(in buffer, dataSource);
						DataAvailable?.Invoke(this, new ProcessingEventArgs(buffer.Data, buffer.BinsCount));
						buffer.MarkAsProcessed();
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