using Microsoft.Extensions.Logging;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

/// <summary>
/// Author:    Phuc Hoang
/// Partner:   Chanphone Visathip
/// Date:      23-Mar-2024
/// Course:    CS 3500, University of Utah, School of Computing
/// Copyright: CS 3500 and Phuc Hoang - This work may not 
///            be copied for use in Academic Coursework.
///
/// I, Phuc Hoang, certify that I wrote this code from scratch and
/// did not copy it in part or whole from another source.  All 
/// references used in the completion of the assignments are cited 
/// in my README file.
///
/// File Contents
///
///    
/// </summary>
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

        public TcpClient _tcpClient;
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
            _cancellationTokenSource = new();
            logger.LogTrace("Networking constructor built");
           
        }
        /// <summary>
        ///   <para>
        ///     A Unique identifier for the entity on the "other end" of the wire.
        ///   </para>
        ///   <para>
        ///     The default ID is the tcp client's remote end point, but you can change it
        ///     if desired, to something like: "Jim"  (for a servers connection to the Jim client)
        ///   </para>
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        ///   True if there is an active connection.
        /// </summary>

        /// <summary>
        ///   <remark>
        ///     Only useful for server type programs.
        ///   </remark>
        ///   
        ///   <para>
        ///     Used by server type programs which have a port open listening
        ///     for clients to connect.
        ///   </para>
        ///   <para>
        ///     True if the connect loop is active.
        ///   </para>
        /// </summary>
        public bool IsConnected => _tcpClient?.Connected ?? false;

        /// <summary>
        ///   <remark>
        ///     Only useful for server type programs.
        ///   </remark>
        ///   
        ///   <para>
        ///     Used by server type programs which have a port open listening
        ///     for clients to connect.
        ///   </para>
        ///   <para>
        ///     True if the connect loop is active.
        ///   </para>
        /// </summary>
        public bool IsWaitingForClients { get; private set; }

        /// <summary>
        ///   <para>
        ///     When connected, return the address/port of the program we are talking to,
        ///     which is the tcpClient RemoteEndPoint.
        ///   </para>
        ///   <para>
        ///     If not connected then: "Disconnected". Note: if previously was connected, you should
        ///     return "Old Address/Port - Disconnected".
        ///   </para>
        ///   <para>
        ///     If waiting for clients (ISWaitingForClients is true) 
        ///     return "Waiting For Connections on Port: {Port}".  Note: probably shouldn't call this method
        ///     if you are a server waiting on clients.... use the LocalAddressPort method.
        ///   </para>
        /// </summary>
        public string RemoteAddressPort => IsConnected ? _tcpClient.Client.RemoteEndPoint.ToString() : "Disconnected";

        /// <summary>
        ///   <para>
        ///     When connected, return the address/port on this machine that we are talking on.
        ///     which is the tcpClient LocalEndPoint.
        ///   </para>
        ///   <para>
        ///     If not connected then: "Disconnected". Note: if previously was connected, you should
        ///     return "Old Address/Port - Disconnected".
        ///   </para>
        ///   <para>
        ///     If waiting for clients (ISWaitingForClients is true) 
        ///     return "Waiting For Connections on Port: {Port}"
        ///   </para>
        /// </summary>
        public string LocalAddressPort => IsConnected ? _tcpClient.Client.LocalEndPoint.ToString() : "Disconnected";

        /// <summary>
        ///   <para>
        ///     Open a connection to the given host/port.  Returns when the connection is established,
        ///     or when an exception is thrown.
        ///   </para>
        ///   <para>
        ///     Note: Servers will not call this method.  It is used by clients connecting to
        ///     a program that is waiting for connections.
        ///   </para>
        ///   <para>
        ///     If the connection happens to already be established, this is a NOP (i.e., nothing happens).
        ///   </para>
        ///   <para>
        ///     For the implementing class, the signature of this method should use async.
        ///   </para>
        ///   <remark>
        ///     This method will have to create and use the low level C# TcpClient class.
        ///   </remark>
        /// </summary>
        /// <param name="host">e.g., 127.0.0.1, or "localhost", or "thebes.cs.utah.edu"</param>
        /// <param name="port">e.g., 11000</param>
        /// <exception cref="Exception"> 
        ///     Any exception caused by the underlying TcpClient object should be handled (logged)
        ///     and then propagated (re-thrown).   For example, failure to connect will result in an exception
        ///     (i.e., when the server is down or unreachable).
        ///     
        ///     See TcpClient documentation for examples of exceptions.
        ///     https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient.-ctor?view=net-7.0#system-net-sockets-tcpclient-ctor
        /// </exception>
        public async Task ConnectAsync(string host, int port)
        {
            try
            {
                /// If the connection happens to already be established, this is a NOP (i.e., nothing happens).
                if (_tcpClient is null) 
                {
                    _tcpClient = new TcpClient();
                    await _tcpClient.ConnectAsync(host, port);
                    onConnect(this);
                    _logger.LogDebug("Connect Async succesful");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Connection error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        ///   <para>
        ///     Close the TcpClient connection between us and them.
        ///   </para>
        ///   <para>
        ///     Important: the reportDisconnect handler will _not_ be called (because if your code
        ///     is calling this method, you already know that the disconnect is supposed to happen).
        ///   </para>
        ///   <para>
        ///     Note: on the SERVER, this does not stop "waiting for connects" which should be stopped first with: StopWaitingForClients
        ///   </para>
        /// </summary>
        public void Disconnect()
        {
           _tcpClient?.Close();
           _cancellationTokenSource.Cancel();
           _logger.LogDebug("Disconnect");
        }

        /// <summary>
        ///   <para>
        ///     Precondition: Networking socket has already been connected.
        ///   </para>
        ///   <para>
        ///     Used when one side of the connection waits for a network messages 
        ///     from a the other (e.g., client -> server, or server -> client).
        ///     Usually repeated (see infinite).
        ///   </para>
        ///   <para>
        ///     Upon a complete message (based on terminating character, '\n') being received, the message
        ///     is "transmitted" to the _handleMessage function.  Upon successfully handling one message,
        ///     if multiple messages are "queued up", continue to send them (one after another)until no 
        ///     messages are left in the stored buffer.
        ///   </para>
        ///   <para>
        ///     Once all data/messages are processed, continue to wait for more data (and repeat).
        ///   </para>
        ///   <para>
        ///     If the TcpClient stream's ReadAsync is "interrupted" (by the connection being closed),
        ///     the stored handle disconnect delegate will be called and this function will end.  
        ///   </para>        
        ///   <para>
        ///     Note: This code will "await" network activity and thus the _handleMessage (and 
        ///     _handleDisconnect) methods are never guaranteed to be run on the same thread, nor are
        ///     they guaranteed to use the same thread for subsequent executions.
        ///   </para>
        /// </summary>
        /// 
        /// <param name="infinite">
        ///    if true, then continually await new messages. If false, stop after first complete message received.
        ///    Thus the "infinite" handling will never return (until the connection is severed).
        /// </param>
        public async Task HandleIncomingDataAsync(bool infinite)
        {
            try
            {
                if (IsConnected)
                {
                    StringBuilder saveMessage = new();
                    NetworkStream stream = _tcpClient.GetStream();

                    if (stream == null)
                    {
                        return;
                    }
                    using (stream)
                    {
                        while (infinite)
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                            _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            message.Replace("\n", "\\n");
                            saveMessage.Append(message);
                            onMessage(this, message);
                            if (!infinite)
                                break;
                        }
                    }
                }
               
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("Await message canceled");
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error handling incoming data: {ex.Message}");
                Disconnect();
                onDisconnect(this);
            }
        }

        /// <summary>
        ///   <para>
        ///     Send a message across the channel (i.e., the TCP Client Stream).  This method
        ///     uses WriteAsync and the await keyword.
        ///   </para>
        ///   <para>
        ///     Important: If the message contains the termination character (TC) (e.g., '\n') it is
        ///     considered part of a **single** message.  All instances of the TC will be replaced with the 
        ///     characters "\\n".
        ///   </para>
        ///   <para>
        ///     If an exception is raised upon writing a message to the client stream (e.g., trying to
        ///     send to a "disconnected" recipient) it must be caught, and then 
        ///     the _reportDisconnect method must be invoked letting the user of this object know 
        ///     that the connection is gone. No exception is thrown by this function.
        ///   </para>
        ///   <para>
        ///     If the connection has been closed already, the send will simply return without
        ///     doing anything.
        ///   </para>
        ///   <para>
        ///     Note: messages are encoded using UTF8 before being sent across the network.
        ///   </para>
        ///   <para>
        ///     For the implementing class, the signature of this method should use "async Task SendAsync(string text)".
        ///   </para>
        ///   <remark>
        ///     Will use the stored tcp object's stream's writeasync method.
        ///   </remark>
        /// </summary>
        /// <param name="text"> 
        ///   The entire message to send. Note: this string may contain the Termination Character '\n', 
        ///   but they will be replaced by "\\n".  Upon receipt, the "\\n" will be replaced with '\n'.
        ///   Regardless, it is a _single_ message from the Networking libraries point of view.
        /// </param>
        public async Task SendAsync(string text)
        {
            try
            {
                if (!IsConnected)
                {
                    _logger.LogError("Cannot send message, not connected.");
                    return;
                }
                
                NetworkStream stream = _tcpClient.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(text);
               
                await stream.WriteAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                Disconnect();
                onDisconnect(this);
            }
        }

        /// <summary>
        ///   <para>
        ///     This method is only used by Server applications.
        ///   </para>
        ///   <para>
        ///     Handle client connections;  wait for network connections using the low level
        ///     TcpListener object.  When a new connection is found:
        ///   </para>
        ///   <para> 
        ///     IMPORTANT: create a new thread to handle communications from the new client.  
        ///   </para>
        ///   <para>
        ///     This routine runs indefinitely until stopped (could accept many clients).
        ///     Important: The TcpListener should have a cancellationTokenSource attached to it in order
        ///     to allow for it to be shutdown.
        ///   </para>
        ///   <para>
        ///     Important: you will create a new Networking object for each client.  This
        ///     object should use the original call back methods instantiated in the servers Networking object. 
        ///     The new networking object will need to store the new tcp client object returned from the tcp listener.
        ///     Finally, the new networking object (on its new thread) should HandleIncomingDataAsync
        ///   </para>
        ///   <para>
        ///     Again: All connected clients will "share" the same onMessage and 
        ///     onDisconnect delegates, so those methods had better handle this Race Condition.  (IMPORTANT: 
        ///     the locking does _not_ occur in the networking code.)
        ///   </para>
        ///   <para>
        ///     For the implementing class, the signature of this method should use async.
        ///   </para>
        /// </summary>
        /// <param name="port"> Port to listen on </param>
        /// <param name="infinite"> If true, then each client gets a thread that read an infinite number of messages</param>
        public async Task WaitForClientsAsync(int port, bool infinite)
        {
            IsWaitingForClients = true;
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
            listener.Start();
            try { 
                _cancellationTokenSource = new CancellationTokenSource();

                while (IsWaitingForClients)
                {
                    Networking newClient = new Networking(_logger, onConnect, onDisconnect, onMessage);
                    newClient._tcpClient = await listener.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                    newClient.onConnect(newClient);

                    new Thread(async () => { await newClient.HandleIncomingDataAsync(infinite); }).Start();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error waiting for clients: {ex.Message}");
                listener.Stop();
            }
        }

        /// <summary>
        ///   <para>
        ///     Stop listening for connections.  This is achieved using the Cancellation Token Source that
        ///     was attached to the tcplistner back in the wait for clients method.
        ///   </para>
        ///   <para>
        ///     This code allows for graceful termination of the program, such as if a disconnect button
        ///     is pressed on a GUI.
        ///   </para>
        ///   <para>
        ///     This code should be a very simple call to the Cancel method of the appropriate cancellation token
        ///   </para>
        public void StopWaitingForClients()
        {
            IsWaitingForClients = false;
            _cancellationTokenSource?.Cancel();
            _logger.LogDebug("Stop waiting for clients");
        }
    }
}

