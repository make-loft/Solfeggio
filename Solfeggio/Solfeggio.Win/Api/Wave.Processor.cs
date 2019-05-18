using Ace;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static Solfeggio.Api.ProcessingState;

namespace Solfeggio.Api
{
	public interface IProcessor : IDataSource
	{
		void Wake();
		void Lull();
		void Free();
		void Tick();
	}

	public interface IDataSource
	{
		short[] Next();

		event EventHandler<ProcessingEventArgs> DataAvailable;
	}

	public partial class Wave
	{
		public abstract class Processor<TDeviceInfo> : IProcessor, IExposable, IDisposable
		{
			private readonly IProcessor _source;
			private readonly Callback _callback;

			public Processor(ASession session, int bufferSize, int buffersCount = 4, IProcessor source = default)
			{
				NumberOfBuffers = buffersCount;
				_source = source;
				_session = session;
				BufferSize = bufferSize;
				_callback = ProcessingCallback;
				session.As<Out.Session>()?.SetVolume(1f);

				if (source.Is())
				{
					source.DataAvailable += (o, e) =>
					{
						var buffer = buffers.Where(b => b.IsDone).FirstOrDefault();
						if (buffer.Is())
						{
							Array.Copy(e.Bins, buffer.Data, e.BinsCount);
							if (State.Is(Processing)) buffer.MarkForProcessing();
						}
					};
				}

				Expose();
			}

			~Processor() => Dispose();

			public short[] Next() => default;

			public void Tick() { }

			public void Expose()
			{
				_session.Open(_callback);

				CreateBuffers();
				Debug.WriteLine($"{_session.Handle}.Expose()");
			}

			public void Dispose()
			{
				Debug.WriteLine($"{_session.Handle}.Dispose()");
				GC.SuppressFinalize(this);

				for (var n = 0; n < buffers.Length; n++)
				{
					buffers[n].Dispose();
				}

				_session.Close();
			}

			public ProcessingState State { get; private set; }

			private readonly ASession _session;
			protected Buffer[] buffers;

			public int NumberOfBuffers { get; set; }

			public int BufferSize { get; set; }

			protected void CreateBuffers()
			{
				var bufferSize = BufferSize;
				//var waveFormat = session.WaveFormat;
				//if (bufferSize % waveFormat.BlockAlign != 0)
				//	bufferSize -= bufferSize % waveFormat.BlockAlign;

				buffers = new Buffer[NumberOfBuffers];
				for (var n = 0; n < buffers.Length; n++)
				{
					var buffer = buffers[n] = new Buffer(_session, bufferSize);
					buffer.MarkForProcessing();
				}
			}

			public event EventHandler<ProcessingEventArgs> DataAvailable;

			private void ProcessingCallback(IntPtr waveHandle, Message message, IntPtr userData, Header header, IntPtr reserved)
			{
				if (State.IsNot(Processing)) return;

				if (message.Is(Message.WaveInData) || message.Is(Message.WaveOutDone))
				{
					var handle = (GCHandle)header.userData;
					var buffer = (Buffer)handle.Target;
					if (buffer.IsDone)
					{
						if (message.Is(Message.WaveInData))
						{
							DataAvailable?.Invoke(this, new ProcessingEventArgs(buffer.Data, buffer.BinsCount));
							if (State.Is(Processing)) buffer.MarkForProcessing();
						}

						if (message.Is(Message.WaveOutDone))
						{
							_source.Tick();
						}
					}
				}
			}

			public void Wake()
			{
				if (State.Is(Processing)) return;
				State = Processing;

				_session.Wake().Verify();
				Debug.WriteLine($"{_session.Handle}.Wake()");
			}

			public void Lull()
			{
				if (State.Is(Suspending)) return;
				State = Suspending;

				_session.Lull().Verify();
				Debug.WriteLine($"{_session.Handle}.Lull()");
			}

			public void Free()
			{
				if (State.Is(Hibernation)) return;
				State = Hibernation;

				Lull();
				Console.Beep();
				_session.Reset().Verify();
				Debug.WriteLine($"{_session.Handle}.Reset()");
			}
		}
	}
}