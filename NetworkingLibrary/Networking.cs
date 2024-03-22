using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communications
{
    public class Networking
    {
        private TcpClient tcpClient;
        private readonly ILogger logger;
        private readonly ReportConnectionEstablished onConnect;
        private readonly ReportDisconnect onDisconnect;
        private readonly ReportMessageArrived onMessage;
        private CancellationTokenSource cancellationTokenSource;

        public Networking(ILogger logger,
            ReportConnectionEstablished onConnect,
            ReportDisconnect onDisconnect,
            ReportMessageArrived onMessage)
        {
            this.logger = logger;
            this.onConnect = onConnect;
            this.onDisconnect = onDisconnect;
            this.onMessage = onMessage;
        }

        public string ID { get; set; }

        public bool IsConnected => tcpClient?.Connected ?? false;

        public bool IsWaitingForClients { get; private set; }

        public string RemoteAddressPort => IsConnected ? tcpClient.Client.RemoteEndPoint.ToString() : "Disconnected";

        public string LocalAddressPort => IsConnected ? tcpClient.Client.LocalEndPoint.ToString() : "Disconnected";

        public async Task ConnectAsync(string host, int port)
        {
            try
            {
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(host, port);
                onConnect?.Invoke(this);
            }
            catch (Exception ex)
            {
                logger.LogError($"Connection error: {ex.Message}");
                throw;
            }
        }

        public void Disconnect()
        {
            tcpClient?.Close();
            onDisconnect?.Invoke(this);
        }

        public async Task HandleIncomingDataAsync(bool infinite)
        {
            try
            {
                while (IsConnected)
                {
                    NetworkStream stream = tcpClient.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    onMessage?.Invoke(this, message);

                    if (!infinite)
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error handling incoming data: {ex.Message}");
                onDisconnect?.Invoke(this);
            }
        }

        public async Task SendAsync(string text)
        {
            try
            {
                if (!IsConnected)
                {
                    logger.LogError("Cannot send message, not connected.");
                    return;
                }

                byte[] buffer = Encoding.UTF8.GetBytes(text.Replace("\n", "\\n"));
                await tcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error sending message: {ex.Message}");
                onDisconnect?.Invoke(this);
            }
        }

        public async Task WaitForClientsAsync(int port, bool infinite)
        {
            try
            {
                IsWaitingForClients = true;
                TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
                listener.Start();
                cancellationTokenSource = new CancellationTokenSource();

                while (IsWaitingForClients)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Networking newClient = new Networking(logger, onConnect, onDisconnect, onMessage);
                    newClient.tcpClient = client;

                    Task.Run(() => newClient.HandleIncomingDataAsync(infinite), cancellationTokenSource.Token);
                    onConnect?.Invoke(newClient);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error waiting for clients: {ex.Message}");
            }
        }

        public void StopWaitingForClients()
        {
            IsWaitingForClients = false;
            cancellationTokenSource?.Cancel();
        }
    }
}

