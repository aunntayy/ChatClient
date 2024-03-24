using Communications;
using Microsoft.Extensions.Logging;

namespace NetworkingLibraryTest
{
    [TestClass]
    public class NetworkingTests
    {
        [TestMethod]
        public async Task ConnectAsync_Should_Return_Valid_IP_And_Port()
        {
            // Arrange
            var networking = new Networking(new Logger<Networking>(new LoggerFactory()), null, null, null); // Replace nulls with appropriate handlers if needed
            string host = "example.com"; // Replace with your server host
            int port = 1234; // Replace with your server port

            // Act
            await networking.ConnectAsync(host, port);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(networking.ServerIPAddress), "Server IP address should not be null or empty");
            Assert.IsTrue(networking.ServerPort > 0, "Server port should be greater than zero");
        }
    }
}