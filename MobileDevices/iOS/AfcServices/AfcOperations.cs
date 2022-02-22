namespace MobileDevices.iOS.AfcServices
{
    public enum AfcOperations:ulong
    {
		Invalid = 0x00000000,      /* Invalid */
		Status = 0x00000001, /* Status */
		Data = 0x00000002,   /* Data */
		ReadDir = 0x00000003,   /* ReadDir */
		ReadFile = 0x00000004,  /* ReadFile */
		WriteFile = 0x00000005, /* WriteFile */
		WritePart = 0x00000006, /* WritePart */
		TruncateFile = 0x00000007,   /* TruncateFile */
		RemovePath = 0x00000008,    /* RemovePath */
		MakeDir = 0x00000009,   /* MakeDir */
		GetFileInfo = 0x0000000A,  /* GetFileInfo */
		GetDeviceInfo = 0x0000000B,    /* GetDeviceInfo */
		WriteFileAtomic = 0x0000000C,    /* WriteFileAtomic (tmp file+rename) */
		FileRefOpen = 0x0000000D,  /* FileRefOpen */
		FileRefOpenResult = 0x0000000E,  /* FileRefOpenResult  文件句柄*/
		FileRefRead = 0x0000000F,  /* FileRefRead */
		FileRefWrite = 0x00000010, /* FileRefWrite */
		FileRefSeek = 0x00000011,  /* FileRefSeek */
		FileRefTell = 0x00000012,  /* FileRefTell */
		FileRefTellResult = 0x00000013,  /* FileRefTellResult */
		FileRefClose = 0x00000014, /* FileRefClose */
		FileRefSetFileSize = 0x00000015,  /* FileRefSetFileSize (ftruncate) */
		GetConnectionInfo = 0x00000016,   /* GetConnectionInfo */
		SetConnectionOptions = 0x00000017,    /* SetConnectionOptions */
		RenamePath = 0x00000018,    /* RenamePath */
		SetFSBlockSize = 0x00000019,  /* SetFSBlockSize (0x800000) */
		SetSocketBlockSize = 0x0000001A,  /* SetSocketBlockSize (0x800000) */
		FileRefLock = 0x0000001B,  /* FileRefLock */
		MakeLink = 0x0000001C,  /* MakeLink */
		GetFileHash = 0x0000001D,  /* GetFileHash */
		SetModTime = 0x0000001E,  /* SetModTime */
		GetFileHashWithRange = 0x0000001F,    /* GetFileHashWithRange */
		/* iOS 6+ */
		FileRefSetImmutableHint = 0x00000020,    /* FileRefSetImmutableHint */
		GetSizeOfPathContents = 0x00000021,  /* GetSizeOfPathContents */
		RemovePathAndContents = 0x00000022,   /* RemovePathAndContents */
		DirectoryEnumeratorRefOpen = 0x00000023,   /* DirectoryEnumeratorRefOpen */
		DirectoryEnumeratorRefOpenResult = 0x00000024,    /* DirectoryEnumeratorRefOpenResult */
		DirectoryEnumeratorRefRead = 0x00000025,   /* DirectoryEnumeratorRefRead */
		DirectoryEnumeratorRefClose = 0x00000026,  /* DirectoryEnumeratorRefClose */
		/* iOS 7+ */
		FileRefReadWithOffset = 0x00000027,   /* FileRefReadWithOffset */
		FileRefWriteWithOffset = 0x00000028   /* FileRefWriteWithOffset */
	}

    /** Error Codes */
    public enum AfcError
    {
        AfcSuccess = 0,
        AfcUnknownError = 1,
        AfcOpHeaderInvalid = 2,
        AfcNoResources = 3,
        AfcReadError = 4,
        AfcWriteError = 5,
        AfcUnknownPacketType = 6,
        AfcInvalidArg = 7,
        AfcObjectNotFound = 8,
        AfcObjectIsDir = 9,
        AfcPermDenied = 10,
        AfcServiceNotConnected = 11,
        AfcOpTimeout = 12,
        AfcTooMuchData = 13,
        AfcEndOfData = 14,
        AfcOpNotSupported = 15,
        AfcObjectExists = 16,
        AfcObjectBusy = 17,
        AfcNoSpaceLeft = 18,
        AfcOpWouldBlock = 19,
        AfcIoError = 20,
        AfcOpInterrupted = 21,
        AfcOpInProgress = 22,
        AfcInternalError = 23,
        AfcMuxError = 30,
        AfcNoMem = 31,
        AfcNotEnoughData = 32,
        AfcDirNotEmpty = 33,
        AfcForceSignedType = -1
    }
}
