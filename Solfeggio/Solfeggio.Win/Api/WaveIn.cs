using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Solfeggio.Api
{
    // http://msdn.microsoft.com/en-us/library/dd757347(v=VS.85).aspx
    [StructLayout(LayoutKind.Explicit)]
    struct MmTime
    {
        public const int TIME_MS = 0x0001;
        public const int TIME_SAMPLES = 0x0002;
        public const int TIME_BYTES = 0x0004;

        [FieldOffset(0)]
        public UInt32 wType;
        [FieldOffset(4)]
        public UInt32 ms;
        [FieldOffset(4)]
        public UInt32 sample;
        [FieldOffset(4)]
        public UInt32 cb;
        [FieldOffset(4)]
        public UInt32 ticks;
        [FieldOffset(4)]
        public Byte smpteHour;
        [FieldOffset(5)]
        public Byte smpteMin;
        [FieldOffset(6)]
        public Byte smpteSec;
        [FieldOffset(7)]
        public Byte smpteFrame;
        [FieldOffset(8)]
        public Byte smpteFps;
        [FieldOffset(9)]
        public Byte smpteDummy;
        [FieldOffset(10)]
        public Byte smptePad0;
        [FieldOffset(11)]
        public Byte smptePad1;
        [FieldOffset(4)]
        public UInt32 midiSongPtrPos;
    }
    
    public enum SupportedWaveFormat
    {
        /// <summary>
        /// 11.025 kHz, Mono,   8-bit
        /// </summary>
        WAVE_FORMAT_1M08 = 0x00000001,
        /// <summary>
        /// 11.025 kHz, Stereo, 8-bit
        /// </summary>
        WAVE_FORMAT_1S08 = 0x00000002,
        /// <summary>
        /// 11.025 kHz, Mono,   16-bit
        /// </summary>
        WAVE_FORMAT_1M16 = 0x00000004,
        /// <summary>
        /// 11.025 kHz, Stereo, 16-bit
        /// </summary>
        WAVE_FORMAT_1S16 = 0x00000008,
        /// <summary>
        /// 22.05  kHz, Mono,   8-bit
        /// </summary>
        WAVE_FORMAT_2M08 = 0x00000010,
        /// <summary>
        /// 22.05  kHz, Stereo, 8-bit 
        /// </summary>
        WAVE_FORMAT_2S08 = 0x00000020,
        /// <summary>
        /// 22.05  kHz, Mono,   16-bit
        /// </summary>
        WAVE_FORMAT_2M16 = 0x00000040,
        /// <summary>
        /// 22.05  kHz, Stereo, 16-bit
        /// </summary>
        WAVE_FORMAT_2S16 = 0x00000080,
        /// <summary>
        /// 44.1   kHz, Mono,   8-bit 
        /// </summary>
        WAVE_FORMAT_4M08 = 0x00000100,
        /// <summary>
        /// 44.1   kHz, Stereo, 8-bit 
        /// </summary>
        WAVE_FORMAT_4S08 = 0x00000200,
        /// <summary>
        /// 44.1   kHz, Mono,   16-bit
        /// </summary>
        WAVE_FORMAT_4M16 = 0x00000400,
        /// <summary>
        ///  44.1   kHz, Stereo, 16-bit
        /// </summary>
        WAVE_FORMAT_4S16 = 0x00000800,

        /// <summary>
        /// 44.1   kHz, Mono,   8-bit 
        /// </summary>
        WAVE_FORMAT_44M08 = 0x00000100,
        /// <summary>
        /// 44.1   kHz, Stereo, 8-bit 
        /// </summary>
        WAVE_FORMAT_44S08 = 0x00000200,
        /// <summary>
        /// 44.1   kHz, Mono,   16-bit
        /// </summary>
        WAVE_FORMAT_44M16 = 0x00000400,
        /// <summary>
        /// 44.1   kHz, Stereo, 16-bit
        /// </summary>
        WAVE_FORMAT_44S16 = 0x00000800,
        /// <summary>
        /// 48     kHz, Mono,   8-bit 
        /// </summary>
        WAVE_FORMAT_48M08 = 0x00001000,
        /// <summary>
        ///  48     kHz, Stereo, 8-bit
        /// </summary>
        WAVE_FORMAT_48S08 = 0x00002000,
        /// <summary>
        /// 48     kHz, Mono,   16-bit
        /// </summary>
        WAVE_FORMAT_48M16 = 0x00004000,
        /// <summary>
        /// 48     kHz, Stereo, 16-bit
        /// </summary>
        WAVE_FORMAT_48S16 = 0x00008000,
        /// <summary>
        /// 96     kHz, Mono,   8-bit 
        /// </summary>
        WAVE_FORMAT_96M08 = 0x00010000,
        /// <summary>
        /// 96     kHz, Stereo, 8-bit
        /// </summary>
        WAVE_FORMAT_96S08 = 0x00020000,
        /// <summary>
        /// 96     kHz, Mono,   16-bit
        /// </summary>
        WAVE_FORMAT_96M16 = 0x00040000,
        /// <summary>
        /// 96     kHz, Stereo, 16-bit
        /// </summary>
        WAVE_FORMAT_96S16 = 0x00080000,

    }
    
    [StructLayout(LayoutKind.Sequential)]
    class WaveHeader
    {
        /// <summary>pointer to locked data buffer (lpData)</summary>
        public IntPtr dataBuffer;
        /// <summary>length of data buffer (dwBufferLength)</summary>
        public int bufferLength;
        /// <summary>used for input only (dwBytesRecorded)</summary>
        public int bytesRecorded;
        /// <summary>for client's use (dwUser)</summary>
        public IntPtr userData;
        /// <summary>assorted flags (dwFlags)</summary>
        public WaveHeaderFlags flags;
        /// <summary>loop control counter (dwLoops)</summary>
        public int loops;
        /// <summary>PWaveHdr, reserved for driver (lpNext)</summary>
        public IntPtr next;
        /// <summary>reserved for driver</summary>
        public IntPtr reserved;
    }
    
    [Flags]
    public enum WaveHeaderFlags
    {
        /// <summary>
        /// WHDR_BEGINLOOP
        /// This buffer is the first buffer in a loop.  This flag is used only with output buffers.
        /// </summary>
        BeginLoop = 0x00000004,
        /// <summary>
        /// WHDR_DONE
        /// Set by the device driver to indicate that it is finished with the buffer and is returning it to the application.
        /// </summary>
        Done = 0x00000001,
        /// <summary>
        /// WHDR_ENDLOOP
        /// This buffer is the last buffer in a loop.  This flag is used only with output buffers.
        /// </summary>
        EndLoop = 0x00000008,
        /// <summary>
        /// WHDR_INQUEUE
        /// Set by Windows to indicate that the buffer is queued for playback.
        /// </summary>
        InQueue = 0x00000010,
        /// <summary>
        /// WHDR_PREPARED
        /// Set by Windows to indicate that the buffer has been prepared with the waveInPrepareHeader or waveOutPrepareHeader function.
        /// </summary>
        Prepared = 0x00000002
    }
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WaveInCapabilities
    {
        private short manufacturerId;
        private short productId;
        private int driverVersion;
        private SupportedWaveFormat supportedFormats;
        private short channels;
        private short reserved;

        // extra WAVEINCAPS2 members

        private const int MaxProductNameLength = 32;

        public int Channels => channels;

        [field: MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxProductNameLength)] public string ProductName { get; }

        public Guid NameGuid { get; }
        public Guid ProductGuid { get; }
        public Guid ManufacturerGuid { get; }

        public bool SupportsWaveFormat(SupportedWaveFormat waveFormat)
        {
            return (supportedFormats & waveFormat) == waveFormat;
        }

    }
    
    public class WaveIn : IWaveIn
    {
        private IntPtr waveInHandle;
        private volatile bool recording;
        private WaveInBuffer[] buffers;
        private readonly WaveInterop.WaveCallback callback;
        private int lastReturnedBufferIndex;
        /// <summary>
        /// Indicates recorded data is available 
        /// </summary>
        public event EventHandler<WaveInEventArgs> DataAvailable;

        /// <summary>
        /// Indicates that all recorded data has now been received.
        /// </summary>
        public event EventHandler<StoppedEventArgs> RecordingStopped;

        public WaveIn()
        {
            DeviceNumber = 0;
            WaveFormat = new WaveFormat(16000, 16, 1);
            BufferMilliseconds = 100;
            NumberOfBuffers = 3;
            callback = Callback;
        }
        
        internal MmResult WaveOutOpen(out IntPtr waveOutHandle, int deviceNumber, WaveFormat waveFormat, WaveInterop.WaveCallback callback)
        {
            var result = WaveInterop.waveOutOpen(out waveOutHandle, (IntPtr) deviceNumber, waveFormat, callback,
                IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackFunction);
            return result;
        }

        internal MmResult WaveInOpen(out IntPtr waveInHandle, int deviceNumber, WaveFormat waveFormat,
            WaveInterop.WaveCallback callback)
        {
            var result = WaveInterop.waveInOpen(out waveInHandle, (IntPtr) deviceNumber, waveFormat, callback, IntPtr.Zero,
                WaveInterop.WaveInOutOpenFlags.CallbackFunction);
            return result;
        }

        /// <summary>
        /// Returns the number of Wave In devices available in the system
        /// </summary>
        public static int DeviceCount => WaveInterop.waveInGetNumDevs();

        /// <summary>
        /// Retrieves the capabilities of a waveIn device
        /// </summary>
        /// <param name="devNumber">Device to test</param>
        /// <returns>The WaveIn device capabilities</returns>
        public static WaveInCapabilities GetCapabilities(int devNumber)
        {
            var caps = new WaveInCapabilities();
            int structSize = Marshal.SizeOf(caps);
            MmException.Try(WaveInterop.waveInGetDevCaps((IntPtr)devNumber, out caps, structSize), "waveInGetDevCaps");
            return caps;
        }

        /// <summary>
        /// Milliseconds for the buffer. Recommended value is 100ms
        /// </summary>
        public int BufferMilliseconds { get; set; }
        public int NumberOfBuffers { get; set; }
        public int DeviceNumber { get; set; }

        private void CreateBuffers()
        {
            // Default to three buffers of 100ms each
            int bufferSize = BufferMilliseconds * WaveFormat.AverageBytesPerSecond / 1000;
            if (bufferSize % WaveFormat.BlockAlign != 0)
            {
                bufferSize -= bufferSize % WaveFormat.BlockAlign;
            }

            buffers = new WaveInBuffer[NumberOfBuffers];
            for (int n = 0; n < buffers.Length; n++)
            {
                buffers[n] = new WaveInBuffer(waveInHandle, bufferSize);
            }
        }

        /// <summary>
        /// Called when we get a new buffer of recorded data
        /// </summary>
        private void Callback(IntPtr waveInHandle, WaveInterop.WaveMessage message, IntPtr userData, WaveHeader waveHeader, IntPtr reserved)
        {
            if (message == WaveInterop.WaveMessage.WaveInData)
            {
                if (recording)
                {
                    var hBuffer = (GCHandle)waveHeader.userData;
                    var buffer = (WaveInBuffer)hBuffer.Target;
                    if (buffer == null) return;

                    lastReturnedBufferIndex = Array.IndexOf(buffers, buffer);
                    RaiseDataAvailable(buffer);
                    try
                    {
                        buffer.Reuse();
                    }
                    catch (Exception e)
                    {
                        recording = false;
                    }
                }

            }
        }

        private void RaiseDataAvailable(WaveInBuffer buffer)
        {
            DataAvailable?.Invoke(this, new WaveInEventArgs(buffer.Data, buffer.BytesRecorded));
        }


        private void OpenWaveInDevice()
        {
            CloseWaveInDevice();
            WaveInOpen(out waveInHandle, DeviceNumber, WaveFormat, callback);
            CreateBuffers();
        }

        /// <summary>
        /// Start recording
        /// </summary>
        public void StartRecording()
        {
            if (recording)
            {
                throw new InvalidOperationException("Already recording");
            }
            OpenWaveInDevice();
            EnqueueBuffers();
            MmException.Try(WaveInterop.waveInStart(waveInHandle), "waveInStart");
            recording = true;
        }

        private void EnqueueBuffers()
        {
            foreach (var buffer in buffers)
            {
                if (!buffer.InQueue)
                {
                    buffer.Reuse();
                }
            }
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        public void StopRecording()
        {
            if (recording)
            {
                recording = false;
                MmException.Try(WaveInterop.waveInStop(waveInHandle), "waveInStop");
                // report the last buffers, sometimes more than one, so taking care to report them in the right order
                for (int n = 0; n < buffers.Length; n++)
                {
                    int index = (n + lastReturnedBufferIndex + 1) % buffers.Length;
                    var buffer = buffers[index];
                    if (buffer.Done)
                    {
                        RaiseDataAvailable(buffer);
                    }
                }
            }
            //MmException.Try(WaveInterop.waveInReset(waveInHandle), "waveInReset");      
            // Don't actually close yet so we get the last buffer
        }

        /// <summary>
        /// Gets the current position in bytes from the wave input device.
        /// it calls directly into waveInGetPosition)
        /// </summary>
        /// <returns>Position in bytes</returns>
        public long GetPosition()
        {
            MmTime mmTime = new MmTime();
            mmTime.wType = MmTime.TIME_BYTES; // request results in bytes, TODO: perhaps make this a little more flexible and support the other types?
            MmException.Try(WaveInterop.waveInGetPosition(waveInHandle, out mmTime, Marshal.SizeOf(mmTime)), "waveInGetPosition");

            if (mmTime.wType != MmTime.TIME_BYTES)
                throw new Exception(string.Format("waveInGetPosition: wType -> Expected {0}, Received {1}", MmTime.TIME_BYTES, mmTime.wType));

            return mmTime.cb;
        }

        public WaveFormat WaveFormat { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (recording) StopRecording();
            CloseWaveInDevice();
        }

        private void CloseWaveInDevice()
        {
            if (waveInHandle == IntPtr.Zero) return;
            // Some drivers need the reset to properly release buffers
            WaveInterop.waveInReset(waveInHandle);
            if (buffers != null)
            {
                for (int n = 0; n < buffers.Length; n++)
                {
                    buffers[n].Dispose();
                }
                buffers = null;
            }
            WaveInterop.waveInClose(waveInHandle);
            waveInHandle = IntPtr.Zero;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}