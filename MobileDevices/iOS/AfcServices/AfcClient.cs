using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Claunia.PropertyList;
using Microsoft.Extensions.Logging;

namespace MobileDevices.iOS.AfcServices
{
    public class AfcClient : IAsyncDisposable, IDisposable
    {
        /// <summary>
        /// Gets the name of the afc service running on the device.
        /// </summary>
        public const string ServiceName = "com.apple.afc";

        private readonly AfcProtocol _protocol;

        private readonly ILogger _logger;

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
            this._protocol = new AfcProtocol(stream, ownsStream: true, logger: logger);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AfcClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="AfcProtocol"/> which represents a connection to the afc service running on the device.
        /// </param>
        public AfcClient(AfcProtocol protocol)
        {
            this._protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AfcClient"/> class.
        /// </summary>
        /// <param name="protocol">
        /// A <see cref="AfcProtocol"/> which represents a connection to the afc service running on the device.
        /// </param>
        /// <param name="logger"></param>
        public AfcClient(AfcProtocol protocol, ILogger logger)
        {
            _logger = logger;
            this._protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        }


        public bool IsDisposed { get; private set; }


        public async Task<string[]> ReadDirectoryAsync(string path, CancellationToken token)
        {
            var request = new AfcRequest
            {
                FilePath = path,
                AfcOperation = AfcOperations.ReadDir
            };

            if (!await _protocol.WriteMessageAsync(request, token)) return Array.Empty<string>();

            var (owner, packetHeader) = await _protocol.ReceiveDataAsync(token);
            using (owner)
            {
                var memory = owner.Memory;
                var check = CheckOperationTypes(memory.Span, packetHeader);

                if (check != AfcError.AfcSuccess) return default;
                var list = Encoding.UTF8.GetString(memory.Span.Slice(0, owner.ValidLength)).Split("\0", StringSplitOptions.RemoveEmptyEntries);
                return list;

            }
        }

        private AfcError CheckOperationTypes(Span<byte> memory, AfcPacketHeard afcPacket)
        {
            if (afcPacket.Operation == AfcOperations.Status)
            {
                var result = (AfcError)BinaryPrimitives.ReadUInt64LittleEndian(memory);
                if (result != AfcError.AfcSuccess)
                {
                    _logger.LogError("got a status response, code {message}", result.ToString());
                }

                return result;
            }

            return AfcError.AfcSuccess;
        }

        public async Task<NSDictionary> GetFileInfoAsync(string path, CancellationToken token)
        {
            var request = new AfcRequest
            {
                FilePath = path,
                AfcOperation = AfcOperations.GetFileInfo
            };

            if (!await _protocol.WriteMessageAsync(request, token)) return new NSDictionary();

            var (owner, packetHeader) = await _protocol.ReceiveDataAsync(token);
            using (owner)
            {
                var dic = GetNsDictionary(owner.Memory.Span.Slice(0, owner.ValidLength));
                return dic;
            }
        }

        public NSDictionary GetNsDictionary(Span<byte> data)
        {
            var list = Encoding.UTF8.GetString(data).Split("\0", StringSplitOptions.RemoveEmptyEntries);
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

            if (!await _protocol.WriteMessageAsync(request, token)) return 0;
            var (owner, packetHeader) = await _protocol.ReceiveDataAsync(token);
            using (owner)
            {
                if (packetHeader.Operation != AfcOperations.FileRefOpenResult)
                {
                    _logger.LogError("Did not get a file handle response");
                }
                var result = BinaryPrimitives.ReadUInt64LittleEndian(owner.Memory.Span.Slice(0, owner.ValidLength));
                return result;
            }

        }

        public async Task<ulong> FileCloseAsync(ulong handle, CancellationToken token)
        {
            var request = new AfcRequest
            {
                FileHandle = handle,
                AfcOperation = AfcOperations.FileRefClose
            };

            if (!await _protocol.WriteMessageAsync(request, token)) return 0;

            var (owner, packetHeader) = await _protocol.ReceiveDataAsync(token);
            using (owner)
            {
                var result = BinaryPrimitives.ReadUInt64LittleEndian(owner.Memory.Span);
                return result;
            }
        }

        public async Task<bool> FileWriteAsync(ulong handle, ReadOnlyMemory<byte> data, int length, CancellationToken token)
        {
            ulong result = 1;

            var request = new AfcRequest
            {
                FileHandle = handle,
                AfcOperation = AfcOperations.FileRefWrite,
                FileData = data
            };

            if (!await _protocol.WriteMessageAsync(request, token)) return false;

            var (owner, packetHeader) = await _protocol.ReceiveDataAsync(token);
            using (owner)
            {

                result = BinaryPrimitives.ReadUInt64LittleEndian(owner.Memory.Span);
                return result == 0;
            }

        }

        public async Task<bool> MakeDirectoryAsync(string path, CancellationToken token)
        {
            ulong result = 1;
            var request = new AfcRequest
            {
                FilePath = path,
                AfcOperation = AfcOperations.MakeDir
            };

            if (!await _protocol.WriteMessageAsync(request, token)) return false;

            var (owner, packetHeader) = await _protocol.ReceiveDataAsync(token);
            using (owner)
            {

                result = BinaryPrimitives.ReadUInt64LittleEndian(owner.Memory.Span);
                return result == 0;
            }
        }




        public ValueTask DisposeAsync()
        {
            this.IsDisposed = true;

            return _protocol?.DisposeAsync() ?? ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
