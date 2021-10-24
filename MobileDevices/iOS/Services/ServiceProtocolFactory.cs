using System.IO;
using Microsoft.Extensions.Logging;

namespace MobileDevices.iOS.Services
{
    public class ServiceProtocolFactory
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProtocol"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the connection to the muxer.
        /// </param>
        /// <param name="ownsStream">
        /// A value indicating whether this <see cref="ServiceProtocol"/> instance owns the <paramref name="stream"/> or not.
        /// </param>
        /// <param name="logger">
        /// A <see cref="ILogger"/> which can be used when logging.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="ServiceProtocol"/> class.
        /// </returns>
        public virtual ServiceProtocol Create(Stream stream, bool ownsStream, ILogger logger)
        {
            return new ServiceProtocol(stream, ownsStream, logger);
        }

    }
}
