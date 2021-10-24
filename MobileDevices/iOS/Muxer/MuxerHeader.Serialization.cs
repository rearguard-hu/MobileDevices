using System;
using System.Buffers.Binary;
using System.Text;

namespace MobileDevices.iOS.Muxer
{
    /// <summary>
    /// Supports reading and writing <see cref="MuxerHeader"/> values.
    /// </summary>
    public partial struct MuxerHeader
    {
        /// <summary>
        /// Gets the size of the header in bytes.
        /// </summary>
        public const int BinarySize = 16;

        /// <summary>
        /// Reads a <see cref="MuxerHeader"/> value from a buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer from which to read the <see cref="MuxerHeader"/> value.
        /// </param>
        /// <returns>
        /// A <see cref="MuxerHeader"/> value.
        /// </returns>
        public static MuxerHeader Read(Span<byte> buffer)
        {
            return new MuxerHeader()
            {
                Length = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(0, 4)),
                Version = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(4, 4)),
                Message = (MuxerMessageType)BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(8, 4)),
                Tag = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(12, 4)),
            };
        }

        /// <summary>
        /// Writes this <see cref="MuxerHeader"/> value to a buffer.
        /// </summary>
        /// <param name="buffer">
        /// The buffer to which to write this value.
        /// </param>
        public void Write(Span<byte> buffer)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(0, 4), this.Length);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(4, 4), this.Version);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(8, 4), (uint)this.Message);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(12, 4), this.Tag);
        }
    }
}
