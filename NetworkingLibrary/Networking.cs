using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Communications
{
    public class Networking
    {
        // Delegates
        public delegate void ReportMessageArrived(Networking channel, string message); //callback delegate for when messages arrive
        public delegate void ReportDisconnect(Networking channel); //callback delegate for when a client disconnects
        public delegate void ReportConnectionEstablished(Networking channel); //callback delegate for when a client connects to a listener
        ReportMessageArrived onMessage;
        ReportDisconnect onDisconnect;
        ReportConnectionEstablished onConnect;

        private TcpClient _tcpClient;
        private readonly ILogger _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public Networking(ILogger logger,
            ReportConnectionEstablished onConnect,
            ReportDisconnect onDisconnect,
            ReportMessageArrived onMessage)
        {
            this._logger = logger;
            this.onConnect = onConnect;
            this.onDisconnect = onDisconnect;
            this.onMessage = onMessage;
            logger.LogTrace("Networking constructor built");
        }

        public string ID { get; set; }

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public bool IsWaitingForClients { get; private set; }

        public string RemoteAddressPort => IsConnected ? _tcpClient.Client.RemoteEndPoint.ToString() : "Disconnected";

        public string LocalAddressPort => IsConnected ? _tcpClient.Client.LocalEndPoint.ToString() : "Disconnected";

        public async Task ConnectAsync(string host, int port)
        {
            try
            {
                /// If the connection happens to already be established, this is a NOP (i.e., nothing happens).
                if (_tcpClient is null) 
                {
                    _tcpClient = new TcpClient();
                    await _tcpClient.ConnectAsync(host, port);

                    ID = _tcpClient.Client.RemoteEndPoint.ToString();

                    onConnect?.Invoke(this);

                    _logger.LogDebug("ConnectAsunc succesful");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Connection error: {ex.Message}");
                throw;
            }
        }

        public void Disconnect()
        {
            _tcpClient?.Close();
            onDisconnect?.Invoke(this);
        }

        public async Task HandleIncomingDataAsync(bool infinite)
        {
            try
            {
                while (IsConnected)
                {
                    NetworkStream stream = _tcpClient.GetStream();
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
                _logger.LogError($"Error handling incoming data: {ex.Message}");
                onDisconnect?.Invoke(this);
            }
        }

        public async Task SendAsync(string text)
        {
            try
            {
                if (!IsConnected)
                {
                    _logger.LogError("Cannot send message, not connected.");
                    return;
                }

                byte[] buffer = Encoding.UTF8.GetBytes(text.Replace("\n", "\\n"));
                await _tcpClient.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
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
                _cancellationTokenSource = new CancellationTokenSource();

                while (IsWaitingForClients)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Networking newClient = new Networking(_logger, onConnect, onDisconnect, onMessage);
                    newClient._tcpClient = client;

                    Task.Run(() => newClient.HandleIncomingDataAsync(infinite), _cancellationTokenSource.Token);
                    onConnect?.Invoke(newClient);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error waiting for clients: {ex.Message}");
            }
        }

        public void StopWaitingForClients()
        {
            IsWaitingForClients = false;
            _cancellationTokenSource?.Cancel();
        }
    }
}

