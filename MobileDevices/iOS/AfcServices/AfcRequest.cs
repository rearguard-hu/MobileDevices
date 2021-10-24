using System;

namespace MobileDevices.iOS.AfcServices
{
    public class AfcRequest
    {
        public AfcFileMode? AfcFileMode { get; set; }

        public string FilePath { get; set; }

        public AfcOperations AfcOperation { get; set; }

        public ulong? FileHandle { get; set; }

        public ReadOnlyMemory<byte> FileData { get; set; } = ReadOnlyMemory<byte>.Empty;

    }
}