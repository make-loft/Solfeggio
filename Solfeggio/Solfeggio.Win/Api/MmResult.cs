using System;
using System.Runtime.CompilerServices;

namespace Solfeggio.Api
{
	public static class MmControl
	{
		public static MmResult Verify(this MmResult result, [CallerMemberName]string source = null) =>
			result == MmResult.NoError ? result : throw new Exception($"{result} calling {source}");
	}

	public enum MmResult
	{
		NoError = 0,
		UnspecifiedError = 1,
		BadDeviceId = 2,
		NotEnabled = 3,
		AlreadyAllocated = 4,
		InvalidHandle = 5,
		NoDriver = 6,
		MemoryAllocationError = 7,
		NotSupported = 8,
		BadErrorNumber = 9,
		InvalidFlag = 10,
		InvalidParameter = 11,
		HandleBusy = 12,
		InvalidAlias = 13,
		BadRegistryDatabase = 14,
		RegistryKeyNotFound = 15,
		RegistryReadError = 16,
		RegistryWriteError = 17,
		RegistryDeleteError = 18,
		RegistryValueNotFound = 19,
		NoDriverCallback = 20,
		MoreData = 21,

		WaveBadFormat = 32,
		WaveStillPlaying = 33,
		WaveHeaderUnprepared = 34,
		WaveSync = 35,

		// ACM error codes, found in msacm.h
		AcmNotPossible = 512,
		AcmBusy = 513,
		AcmHeaderUnprepared = 514,
		AcmCancelled = 515,

		// Mixer error codes, found in mmresult.h
		MixerInvalidLine = 1024,
		MixerInvalidControl = 1025,
		MixerInvalidValue = 1026,
	}
}