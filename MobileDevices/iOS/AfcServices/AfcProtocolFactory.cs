using System.IO;
using Microsoft.Extensions.Logging;
using MobileDevices.iOS.Services;

namespace MobileDevices.iOS.AfcServices
{
    public class AfcProtocolFactory : ServiceProtocolFactory
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AfcProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="AfcProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="AfcProtocol"/> class.
        /// </returns>
        public override ServiceProtocol Create(Stream stream, bool ownsStream, ILogger logger)
        {
            return new AfcProtocol(stream, ownsStream, logger);
        }
    }
}
