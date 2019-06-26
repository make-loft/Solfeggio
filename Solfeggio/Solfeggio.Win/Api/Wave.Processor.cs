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
		float Level { get; set; }
		double Boost { get; set; }
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
		public static short[] Scale(this short[] data, double boost)
		{
			if (boost.Is(1d)) return data;

			for (var i = 0; i < data.Length; i++)
			{
				var normalizedValue = (double)data[i] / short.MaxValue;
				data[i] = (short)(short.MaxValue * Math.Pow(normalizedValue, 1d/boost));
			}

			return data;
		}

		public static double[] Stretch(this double[] data, double volume)
		{
			if (volume.Is(1d)) return data;

			for (var i = 0; i < data.Length; i++)
				data[i] *= volume;

			return data;
		}

		public abstract class Processor<TDeviceInfo> :IProcessor, IExposable, IDisposable
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

				Expose();

				if (source.IsNot()) return;

				source.DataAvailable += (o, e) =>
				{
					var buffer = buffers.Where(b => b.IsDone).FirstOrDefault();
					if (buffer.Is())
					{
						Array.Copy(e.Bins.Scale(Boost), buffer.Data, e.BinsCount);
						if (State.Is(Processing)) buffer.MarkForProcessing();
					}
				};
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

				buffers?.ForEach(b => b.Dispose());

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
							DataAvailable?.Invoke(this, new ProcessingEventArgs(this, buffer.Data.Scale(Boost), buffer.BinsCount));
							if (State.Is(Processing)) buffer.MarkForProcessing();
						}

						if (message.Is(Message.WaveOutDone))
						{
							_source.Tick();
						}
					}
				}
			}

			public float Level
			{
				get => _session.GetVolume();
				set => _session.SetVolume(value);
			}

			public double Boost { get; set; } = 1d;

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