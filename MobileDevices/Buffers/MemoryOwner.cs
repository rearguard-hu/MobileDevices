using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileDevices.Buffers
{
    public class MemoryOwner : IDisposable, IMemoryOwner<byte>
    {
        private readonly IMemoryOwner<byte> _memoryOwner;

        public int ValidLength { get; }

        public Memory<byte> Memory => _memoryOwner.Memory;

        public MemoryOwner(IMemoryOwner<byte> memoryOwner, int validLength)
        {
            _memoryOwner = memoryOwner;
            ValidLength = validLength;
        }

        public void Dispose()
        {
            _memoryOwner.Dispose();
        }
    }
}
