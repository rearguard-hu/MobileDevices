using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Claunia.PropertyList;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.PropertyLists;

namespace MobileDevices.iOS.AfcServices
{
    public class AfcClient : IAsyncDisposable
    {
        /// <summary>
        /// Gets the name of the afc service running on the device.
        /// </summary>
        public const string ServiceName = "com.apple.afc";

        private readonly AfcProtocol protocol;


        private readonly MemoryPool<byte> memoryPool = MemoryPool<byte>.Shared;
        /// <summary>
        /// Initializes a new instance of the <see cref="AfcClient"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents a connection to the afc service running on the device.
        /// </param>
        /// <param name="logger">
        /// A logger which can be used when logging.
        /// </param>
        public AfcClient(Stream stream, ILogger<AfcClient> logger)
        {
            this.protocol = new AfcProtocol(stream, ownsStream: true, logger: logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AfcClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="PropertyListProtocol"/> which represents a connection to the afc service running on the device.
        /// </param>
        public AfcClient(AfcProtocol protocol)
        {
            this.protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }


        public bool IsDisposed { get; private set; }


        public async Task<string[]> ReadDirectoryAsync(string path, CancellationToken token)
        {
            var request = new AfcRequest
            {
                FilePath = path,
                AfcOperation = AfcOperations.ReadDir
            };


            if (!await protocol.WriteDataAsync(request, token)) return Array.Empty<string>();


            var rt = await protocol.ReceiveDataAsync(token);

            var list = Encoding.ASCII.GetString(rt.Span).Split("\0", StringSplitOptions.RemoveEmptyEntries);

            return list;
        }

        public async Task<NSDictionary> GetFileInfoAsync(string path, CancellationToken token)
        {
            var request = new AfcRequest
            {
                FilePath = path,
                AfcOperation = AfcOperations.GetFileInfo
            };

            if (!await protocol.WriteDataAsync(request, token)) return new NSDictionary();


            var result = await protocol.ReceiveDataAsync(token);

            return GetNsDictionary(result.ToArray());
        }

        public NSDictionary GetNsDictionary(byte[] data)
        {
            var list = Encoding.ASCII.GetString(data).Split("\0", StringSplitOptions.RemoveEmptyEntries);
            var dict = new NSDictionary();

            if (list.Length < 2)
                return null;

            for (var i = 0; i < list.Length; i += 2)
                dict.Add(list[i], list[i + 1]);

            return dict;
        }

        public async Task<ulong> FileOpenAsync(string fileName, AfcFileMode afcFileMode, CancellationToken token)
        {
            var request = new AfcRequest
            {
                FilePath = fileName,
                AfcFileMode = afcFileMode,
                AfcOperation = AfcOperations.FileRefOpen
            };

            if (!await protocol.WriteDataAsync(request, token)) return 0;


            var rt = await protocol.ReceiveDataAsync(token);
            var result = BinaryPrimitives.ReadUInt64LittleEndian(rt.Span);

            return result;
        }

        public async Task<ulong> FileCloseAsync(ulong handle, CancellationToken token)
        {
            var request = new AfcRequest
            {
                FileHandle = handle,
                AfcOperation = AfcOperations.FileRefClose
            };

            if (!await protocol.WriteDataAsync(request, token)) return 0;

            var rt = await protocol.ReceiveDataAsync(token);
            var result = BinaryPrimitives.ReadUInt64LittleEndian(rt.Span);

            return result;
        }

        public async Task<bool> FileWriteAsync(ulong handle, ReadOnlyMemory<byte> data, int length, CancellationToken token)
        {
            var offset = 0;
            ulong result = 1;

            var request = new AfcRequest
            {
                FileHandle = handle,
                AfcOperation = AfcOperations.FileRefWrite
            };

            while (length > 0 && !token.IsCancellationRequested)
            {
                var len = length;
                var tempSend = data.Slice(offset, len);

                request.FileData = tempSend;

                if (!await protocol.WriteDataAsync(request, token)) return false;


                var rt = await protocol.ReceiveDataAsync(token);
                result = BinaryPrimitives.ReadUInt64LittleEndian(rt.ToArray());

                length -= len;
                offset += len;
            }

            return result == 0;

        }

        public async Task<bool> MakeDirectoryAsync(string path, CancellationToken token)
        {
            var dataLength = path.Length;
            var packet = Encoding.UTF8.GetBytes(path);

            var request = new AfcRequest
            {
                FilePath = path,
                AfcOperation = AfcOperations.MakeDir
            };

            if (!await protocol.WriteDataAsync(request, token)) return false;

            var result = await protocol.ReceiveDataAsync(token);

            var rt = BinaryPrimitives.ReadUInt64LittleEndian(result.Span);

            return true;
        }




        public ValueTask DisposeAsync()
        {
            this.IsDisposed = true;

            return protocol?.DisposeAsync() ?? ValueTask.CompletedTask;
        }
    }
}
