using Communications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;

namespace NetworkingLibraryTest
{
    [TestClass]
    public class NetworkingTests
    {
        [TestMethod]
        public async Task TestServerWaitingForClients()
        {
            // Arrange
            Networking server = null;

            // Initialize the server
            server = new Networking(NullLogger.Instance, (c) => {; }, (c) => {; }, (c, m) => {; });

            // Act
            var waitForClientsTask = server.WaitForClientsAsync(11000, infinite: true);

            // Assert
            Assert.IsTrue(server.IsWaitingForClients); // Server should be waiting for clients
        }

        [TestMethod]
        public async Task ConnectClientToServer()
        {
            // Arrange
            Networking server = null;
            string receivedMessage = string.Empty;
            var serverReceiveMessageCalled = new ManualResetEventSlim(false);

            // Define the method to handle received messages on the server
            void ServerReceiveMessage(Networking client, string message)
            {
                Console.WriteLine("Server received message: " + message);
                receivedMessage = message;
                serverReceiveMessageCalled.Set();
                server.StopWaitingForClients();
            }

            // Define the method to send a message once the client connects
            async void ClientOnConnect(Networking server)
            {
                Console.WriteLine("Client sending message");
                await server.SendAsync("hello");
            }

            // Initialize and start the server
            server = new Networking(NullLogger.Instance, (c) => {; }, (c) => {; }, ServerReceiveMessage);
            var serverTask = server.WaitForClientsAsync(11000, infinite: true);

            // Initialize and connect the client
            Networking client = new Networking(NullLogger.Instance, ClientOnConnect, (s) => {; }, (a, b) => {; });
            var clientTask = client.ConnectAsync("localhost", 11000);

            // Wait for both server and client tasks to complete
            await Task.WhenAll(serverTask, clientTask);

            // Wait for the server to receive the message
            serverReceiveMessageCalled.Wait(TimeSpan.FromSeconds(5)); // Adjust timeout as needed

            // Assert
            Assert.AreEqual("hello", receivedMessage);
        }

        [TestMethod]
        public async Task ConnectClientToServerAndSendMessage()
        {
            // Arrange
            Networking server = null;
            string receivedMessage = null;
            var serverReceiveMessageCalled = new ManualResetEventSlim(false);

            // Define the method to handle received messages on the server
            void ServerReceiveMessage(Networking client, string message)
            {
                Console.WriteLine($"Server received message from {client.ID}: {message}");
                receivedMessage = message;
                serverReceiveMessageCalled.Set();
                server.StopWaitingForClients();
            }

            // Initialize and start the server
            server = new Networking(NullLogger.Instance, (c) => {; }, (c) => {; }, ServerReceiveMessage);
            var serverTask = server.WaitForClientsAsync(11000, infinite: true);

            // Initialize and connect the client
            void ConnectAndSendMessage(Networking server)
            {
                Console.WriteLine("Client sending message");
                server.SendAsync("hello from client").Wait(); // Synchronously wait for SendAsync completion
            }

            Networking client = new Networking(NullLogger.Instance, ConnectAndSendMessage, (s) => {; }, (a, b) => {; });
            var clientTask = client.ConnectAsync("localhost", 11000);

            // Wait for both server and client tasks to complete
            await Task.WhenAll(serverTask, clientTask);

            // Wait for the server to receive the message
            serverReceiveMessageCalled.Wait(TimeSpan.FromSeconds(5)); // Adjust timeout as needed

            // Assert
            Assert.AreEqual("hello from client", receivedMessage);
        }
    }
}