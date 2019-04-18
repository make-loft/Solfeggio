using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Solfeggio.Api
{
	public class WaveOut : IWaveOut, IWavePosition

	{
		private IntPtr hWaveOut;
		private WaveOutBuffer[] buffers;
		private IWaveProvider waveStream;
		private volatile PlaybackState playbackState;
		private readonly WaveInterop.WaveCallback callback;
		private readonly object waveOutLock;

		public static WaveOutCapabilities GetCapabilities(int devNumber)
		{
			var caps = new WaveOutCapabilities();
			var structSize = Marshal.SizeOf(caps);
			MmException.Try(WaveInterop.waveOutGetDevCaps((IntPtr)devNumber, out caps, structSize), "waveOutGetDevCaps");
			return caps;
		}

		public static int DeviceCount => WaveInterop.waveOutGetNumDevs();
		public int DeviceNumber { get; set; } = -1;

		public WaveOut()
		{
			playbackState = PlaybackState.Stopped;
			callback = Callback;
			waveOutLock = new object();
			Volume = 1f;
		}

		internal MmResult WaveOutOpen(out IntPtr waveOutHandle, int deviceNumber, WaveFormat waveFormat, WaveInterop.WaveCallback callback) =>
			WaveInterop.waveOutOpen(out waveOutHandle, (IntPtr)deviceNumber, waveFormat, callback, IntPtr.Zero,
				WaveInterop.WaveInOutOpenFlags.CallbackFunction);

		public int BufferSize { get; set; }

		public void Init(IWaveProvider waveProvider, int bufferSize)
		{
			waveStream = waveProvider;
			MmResult result;

			lock (waveOutLock)
			{
				result = WaveOutOpen(out hWaveOut, DeviceNumber, waveStream.WaveFormat, callback);
				MmException.Try(result, "waveOutOpen");
			}

			buffers = new WaveOutBuffer[]
			{
				new WaveOutBuffer(hWaveOut, bufferSize, waveStream, waveOutLock),
				new WaveOutBuffer(hWaveOut, bufferSize, waveStream, waveOutLock),
			};
		}

		public void Play()
		{
			if (playbackState == PlaybackState.Stopped)
			{
				playbackState = PlaybackState.Playing;
				foreach (var buffer in buffers)
					buffer.OnDone();
			}

			else if (playbackState == PlaybackState.Paused)
			{
				Resume();
				playbackState = PlaybackState.Playing;
			}
		}

		public void Pause()
		{
			if (playbackState == PlaybackState.Playing)
			{
				MmResult result;

				playbackState = PlaybackState.Paused; // set this here, to avoid a deadlock with some drivers

				lock (waveOutLock)
				{
					result = WaveInterop.waveOutPause(hWaveOut);
				}

				if (result != MmResult.NoError)
				{
					throw new MmException(result, "waveOutPause");
				}
			}
		}

		public void Resume()
		{
			if (playbackState == PlaybackState.Paused)
			{
				MmResult result;

				lock (waveOutLock)
				{
					result = WaveInterop.waveOutRestart(hWaveOut);
				}

				if (result != MmResult.NoError)
				{
					throw new MmException(result, "waveOutRestart");
				}

				playbackState = PlaybackState.Playing;
			}
		}

		public void Stop()
		{
			if (playbackState != PlaybackState.Stopped)
			{
				// in the call to waveOutReset with function callbacks
				// some drivers will block here until OnDone is called
				// for every buffer

				playbackState = PlaybackState.Stopped; // set this here to avoid a problem with some drivers whereby 
				MmResult result;

				lock (waveOutLock)
				{
					result = WaveInterop.waveOutReset(hWaveOut);
				}

				if (result != MmResult.NoError)
				{
					throw new MmException(result, "waveOutReset");
				}

				// with function callbacks, waveOutReset will call OnDone,
				// and so PlaybackStopped must not be raised from the handler
				// we know playback has definitely stopped now, so raise callback
				//if (callbackInfo.Strategy == WaveCallbackStrategy.FunctionCallback)
				{
					//RaisePlaybackStoppedEvent(null);
				}
			}
		}

		public long GetPosition()
		{
			lock (waveOutLock)
			{
				MmTime mmTime = new MmTime();
				mmTime.wType = MmTime.TIME_BYTES; // request results in bytes, TODO: perhaps make this a little more flexible and support the other types?
				MmException.Try(WaveInterop.waveOutGetPosition(hWaveOut, out mmTime, Marshal.SizeOf(mmTime)), "waveOutGetPosition");

				if (mmTime.wType != MmTime.TIME_BYTES)
					throw new Exception(string.Format("waveOutGetPosition: wType -> Expected {0}, Received {1}", MmTime.TIME_BYTES, mmTime.wType));

				return mmTime.cb;
			}
		}

		public WaveFormat OutputWaveFormat => waveStream.WaveFormat;
		public PlaybackState PlaybackState => playbackState;

		public float Volume
		{
			get => GetWaveOutVolume(hWaveOut, waveOutLock);
			set => SetWaveOutVolume(value, hWaveOut, waveOutLock);
		}

		internal static float GetWaveOutVolume(IntPtr hWaveOut, object lockObject)
		{
			int stereoVolume;
			MmResult result;

			lock (lockObject)
			{
				result = WaveInterop.waveOutGetVolume(hWaveOut, out stereoVolume);
			}

			MmException.Try(result, "waveOutGetVolume");

			return (stereoVolume & 0xFFFF) / (float)0xFFFF;
		}

		internal static void SetWaveOutVolume(float value, IntPtr hWaveOut, object lockObject)
		{
			if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");
			if (value > 1) throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0.0 and 1.0");

			float left = value;
			float right = value;

			int stereoVolume = (int)(left * 0xFFFF) + ((int)(right * 0xFFFF) << 16);

			MmResult result;

			lock (lockObject)
			{
				result = WaveInterop.waveOutSetVolume(hWaveOut, stereoVolume);
			}

			MmException.Try(result, "waveOutSetVolume");
		}

		#region Dispose Pattern

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			Stop();

			if (disposing)
			{
				foreach(var buffer in buffers) buffer.Dispose();
			}

			lock (waveOutLock)
			{
				WaveInterop.waveOutClose(hWaveOut);
			}
		}

		~WaveOut()
		{
			Debug.Assert(false, "WaveOut device was not closed");
			Dispose(false);
		}

		#endregion

		// made non-static so that playing can be stopped here
		private void Callback(IntPtr hWaveOut, WaveInterop.WaveMessage uMsg, IntPtr dwInstance, WaveHeader wavhdr, IntPtr dwReserved)
		{
			if (uMsg == WaveInterop.WaveMessage.WaveOutDone)
			{
				GCHandle hBuffer = (GCHandle)wavhdr.userData;
				WaveOutBuffer buffer = (WaveOutBuffer)hBuffer.Target;
				buffer.OnDone();
			}
		}
	}
}
