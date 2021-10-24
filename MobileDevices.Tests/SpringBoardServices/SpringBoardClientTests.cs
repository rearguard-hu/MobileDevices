using Claunia.PropertyList;
using Microsoft.Extensions.Logging.Abstractions;
using MobileDevices.iOS.PropertyLists;
using MobileDevices.iOS.SpingBoardServices;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileDevices.Tests.SpringBoardServices
{
    /// <summary>
    /// Tests the <see cref="SpringBoardClient"/> class.
    /// </summary>
    public class SpringBoardClientTests
    {
        /// <summary>
        /// The <see cref="SpringBoardClient"/> cosntructor validates its arguments.
        /// </summary>
        [Fact]
        public void Constructor_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClient(null));
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClient(null, NullLogger.Instance));
            Assert.Throws<ArgumentNullException>(() => new SpringBoardClient(Stream.Null, null));
        }

        /// <summary>
        /// <see cref="SpringBoardClient.GetInterfaceOrientationAsync(CancellationToken)"/> works correctly.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public async Task GetInterfaceOrientationAsync_Works_Async()
        {
            var protocol = new Mock<PropertyListProtocol>();
            protocol
                .Setup(p => p.WriteMessageAsync(It.IsAny<NSDictionary>(), default))
                .Callback<NSDictionary, CancellationToken>((dict, ct) =>
                {
                    Assert.Collection(
                        dict,
                        entry =>
                        {
                            Assert.Equal("command", entry.Key);
                            Assert.Equal("getInterfaceOrientation", entry.Value.ToObject());
                        });
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var value = new NSDictionary();
            value.Add("interfaceOrientation", 4);

            protocol.Setup(p => p.ReadMessageAsync(default)).ReturnsAsync(value).Verifiable();

            var client = new SpringBoardClient(protocol.Object);
            var result = await client.GetInterfaceOrientationAsync(default).ConfigureAwait(false);

            Assert.Equal(SpringBoardServicesInterfaceOrientation.LandscapeLeft, result);

            protocol.Verify();
        }
    }
}
