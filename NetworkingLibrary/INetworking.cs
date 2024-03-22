namespace Communications;

/// <summary>
///   Author:  H. James de St. Germain
///   Date:    Spring 2023
///   Updated: Spring 2024
///   Version: 18-March-2024
///     
///   <para>
///     This interface specifies the required methods for an object to meet the Networking
///     Interface.
///   </para>
/// </summary>
public interface INetworking {

    /// <summary>
    ///   <para>
    ///     A Unique identifier for the entity on the "other end" of the wire.
    ///   </para>
    ///   <para>
    ///     The default ID is the tcp client's remote end point, but you can change it
    ///     if desired, to something like: "Jim"  (for a servers connection to the Jim client)
    ///   </para>
    /// </summary>
    string ID { get; set; }

    /// <summary>
    ///   True if there is an active connection.
    /// </summary>
    bool IsConnected { get; }

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
    bool IsWaitingForClients { get; }

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
    string RemoteAddressPort { get; }

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
    string LocalAddressPort { get; }

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
    Task HandleIncomingDataAsync(bool infinite = true);

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
    Task ConnectAsync(string host, int port);

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
    void Disconnect();

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
    Task SendAsync(string text);

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
    Task WaitForClientsAsync(int port, bool infinite);

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
    /// </summary>
    void StopWaitingForClients();

    /// <summary>
    ///   Stop listening for messages.  This is achieved using the Cancellation Token Source.
    ///   This allows for graceful termination of the program. This method should also be very simple
    ///   utilizing the cancellation token associated with the ReadAsync method used in the
    ///   HandleIncomingData method
    /// </summary>
    void StopWaitingForMessages();

}


/// <summary>
///   A method that will be called by the networking code when a complete message comes across the channel.
/// </summary>
/// <param name="channel">The Networking Object that received the message</param>
/// <param name="message">The message itself (of course without the terminating protocol character).</param>
public delegate void ReportMessageArrived(Networking channel, string message);

/// <summary>
///   <para>
///     A method that will be called by the networking object when the channel is disconnected.
///   </para>
///   <para>
///     Usage: an outside code base (e.g., a web browser) will be using a Networking object
///     to communicate (e.g., with a web server).  If the web server "goes down" (or isn't up
///     in the first place) the networking object will call this function so the outside program can take
///     the appropriate action (e.g., put up a "504" web page).
///   </para>
/// </summary>
/// <param name="channel"> The networking object. </param>
public delegate void ReportDisconnect(Networking channel);

/// <summary>
///  <para>
///    The Networking object will call this method when the program is connected to.
///  </para>
///  <para>
///    If the Networking object represents a client asking for a connection to a server
///    this method will be called when the connection is established (after connectasync is successful).
///  </para>
///  <para>
///    If the Networking object is being used by a SERVER waiting for client connects, then this
///    method will also be called (once for each client connect).
///  </para>
/// </summary>
/// <param name="channel">The Networking Object itself</param>
public delegate void ReportConnectionEstablished(Networking channel);