using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.Lockdown;
using MobileDevices.iOS.PropertyLists;
using MobileDevices.iOS.Services;
using Nerdbank.Streams;
using System;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.PropertyLists
{
    /// <content>
    /// Tests the SSL-related methods in the <see cref="PropertyListProtocol"/> class.
    /// </content>
    public partial class PropertyListProtocolTests
    {
        /// <summary>
        /// <see cref="PropertyListProtocol.EnableSslAsync(PairingRecord, CancellationToken)"/> throws when passed <see langword="null"/>
        /// values.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task EnableSslAsync_NullValues_Throws_Async()
        {
            var protocol = new PropertyListProtocol(Stream.Null, ownsStream: false, NullLogger.Instance);
            await Assert.ThrowsAsync<ArgumentNullException>(() => protocol.EnableSslAsync(null, default)).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="PropertyListProtocol.DisableSslAsync(CancellationToken)"/> throws an exception when SSL is not enabled.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task DisableSslAsync_NotSsl_Throws_Async()
        {
            var protocol = new PropertyListProtocol(Stream.Null, ownsStream: false, NullLogger.Instance);
            await Assert.ThrowsAsync<InvalidOperationException>(() => protocol.DisableSslAsync(default)).ConfigureAwait(false);
        }

        /// <summary>
        /// Tests enabling SSL over a connection with an iOS device, and then disabling that connection, where
        /// <c>leaveInnerStreamOpen</c> is <see langword="true"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> which represents the asynchronous test.
        /// </returns>
        [Fact]
        public async Task EnableSslAsync_EnableAndDisable_Works_Async()
        {
            // Read the pairing record used to authenticate the device & client.
            var pairingRecord = PairingRecord.Read(File.ReadAllBytes("Lockdown/0123456789abcdef0123456789abcdef01234567.plist"));
            pairingRecord.DeviceCertificate = pairingRecord.RootCertificate;

            // Set up a client/server connection
            (var serverStream, var clientStream) = FullDuplexStream.CreatePair();

            // Set up the SSL stream which will act as the server, and have that server
            // authenticate the client.
            var sslServer = new SslStream(serverStream, leaveInnerStreamOpen: true);
            var authenticateSslClientTask = sslServer.AuthenticateAsServerAsync(
                new SslServerAuthenticationOptions()
                {
                    ServerCertificate = pairingRecord.RootCertificate.CopyWithPrivateKeyForSsl(pairingRecord.RootPrivateKey),
                    EncryptionPolicy = EncryptionPolicy.AllowNoEncryption,
                    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        return true;
                    },
                });

            // Have the client enable SSL on the connection.
            var protocol = new PropertyListProtocol(clientStream, ownsStream: false, NullLogger.Instance);
            Assert.False(protocol.SslEnabled);

            var enableSslTask = protocol.EnableSslAsync(pairingRecord, default);

            await Task.WhenAny(
                Task.WhenAll(authenticateSslClientTask, enableSslTask),
                Task.Delay(TimeSpan.FromSeconds(5))).ConfigureAwait(false);

            if (authenticateSslClientTask.IsFaulted)
            {
                await authenticateSslClientTask.ConfigureAwait(false);
            }

            if (enableSslTask.IsFaulted)
            {
                await enableSslTask.ConfigureAwait(false);
            }

            Assert.True(authenticateSslClientTask.IsCompletedSuccessfully);
            Assert.True(enableSslTask.IsCompletedSuccessfully);
            Assert.True(protocol.SslEnabled);

            // An attempt to enable SSL a second time will fail (SSL is already enabled)
            await Assert.ThrowsAsync<InvalidOperationException>(() => protocol.EnableSslAsync(pairingRecord, default)).ConfigureAwait(false);

            // Send/receive messages in both directions over the encrypted streams
            await PingTestAsync(protocol.Stream, sslServer).ConfigureAwait(false);
            await PingTestAsync(sslServer, protocol.Stream).ConfigureAwait(false);

            // Disable SSL
            var disableSslTask = protocol.DisableSslAsync(default);
            var stopSslServerTask = sslServer.ShutdownAsync();

            await Task.WhenAny(
                Task.WhenAll(
                    disableSslTask,
                    stopSslServerTask),
                Task.Delay(TimeSpan.FromSeconds(5))).ConfigureAwait(false);

            // Flush the any pending SSL packets on the SSL server side.
            // This is simlar to what's implemented in SslFactory.DisableSessionSslCore
            byte[] readBuffer = new byte[128];
            await serverStream.ReadAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false);

            Assert.False(protocol.SslEnabled);

            // Send/receive messages in both directions over the unencrypted streams
            await PingTestAsync(serverStream, clientStream).ConfigureAwait(false);
            await PingTestAsync(clientStream, serverStream).ConfigureAwait(false);

            await serverStream.DisposeAsync().ConfigureAwait(true);
            await clientStream.DisposeAsync().ConfigureAwait(true);
        }

        private async Task PingTestAsync(Stream sourceStream, Stream targetStream)
        {
            byte[] ping = Encoding.UTF8.GetBytes("ping");
            byte[] output = new byte[ping.Length];

            var writeTask = sourceStream.WriteAsync(ping, 0, ping.Length);
            var readTask = targetStream.ReadAsync(output, 0, output.Length);

            await Task.WhenAny(
                Task.WhenAll(
                    writeTask,
                    readTask),
                Task.Delay(TimeSpan.FromSeconds(5)));

            await writeTask.ConfigureAwait(false);
            int read = await readTask.ConfigureAwait(false);

            Assert.Equal(4, read);
            Assert.Equal(ping, output);
        }
    }
}
