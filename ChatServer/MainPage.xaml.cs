using Communications;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;
using System.Threading.Channels;
using System.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;

namespace ChatServer
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger _logger;
        private Networking _networking;
        private List<Networking> _clients;
        private int _port = 11000;

        private TcpListener network_listener;
        public string MachineName { get; set; }
        public string IPAddress { get; set; }

        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _clients = new List<Networking>();
            _logger = logger;
            // Initialize an instance of Networking
            _networking = new Networking(logger,OnConnection,OnDisconnect,OnMessageReceived);
            _ = _networking.WaitForClientsAsync(_port, true);
            MachineName = Environment.MachineName;
            IPAddress = GetIPAddress();
            this.BindingContext = this;
            _logger.LogInformation($"Main Page Constructor");
        }
        private string GetIPAddress()
        {
            string ipAddress = string.Empty;
            try
            {
                // Get the host name of the local machine
                string hostName = Dns.GetHostName();

                // Get the IP addresses associated with the host name
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);

                // Find the IPv4 address
                ipAddress = addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();

                if (ipAddress == null)
                {
                    // If no IPv4 address found, return an error message
                    ipAddress = "No IPv4 address found.";
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions and log the error
                Console.WriteLine($"Error getting IP address: {ex.Message}");
                ipAddress = "Error getting IP address.";
            }
            _logger.LogDebug("IP address gotted");
            return ipAddress;
        }
        // Event handlers for networking events

        /// <summary>
        /// If the Networking object is being used by a SERVER waiting for client connects, then this
        /// method will also be called (once for each client connect). 
        /// </summary>
        /// <param name="channel"></param>
        private void OnConnection(Networking channel)
        {
            lock(this._clients) 
            {
                 _clients.Add(channel);
            }            
            _logger.LogDebug("server OnConnection");
        }

        /// <summary>
        /// Called when the is a disconnect with clients
        /// </summary>
        /// <param name="channel"></param>
        private void OnDisconnect(Networking channel)
        {
            if (_clients is not null)
            {
                // Remove clients from the list
                lock (this._clients) {
                    _clients.Remove(channel);
                }
               
                // Add a noti to message
                Dispatcher.Dispatch(() => {
                    messageBoard.Text += channel.ID + " has disconnected from server" + Environment.NewLine;
                    _logger.LogInformation($"Disconnecting {channel.ID}");
                });
                // Update participant list
                participantUpdate();
                _logger.LogDebug("server OnDisconnect");
            }
        }

        
        private  void OnMessageReceived(Networking channel, string message)
        {
            // Command name [name]
            if (message.StartsWith("Command Name"))
            {
                string[] parts = message.Split(new[] { "Command Name" }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string name = parts[1].Trim();
                    foreach (var client in _clients)
                    {
                        if (name.Equals(client.ID))
                        {
                            _ = channel.SendAsync("NAME REJECTED" + Environment.NewLine);
                            _logger.LogInformation($"{name} - rejected");
                            break;
                        }
                    }
                    channel.ID = name;
                    Dispatcher.Dispatch(() => {
                        //use remote address port to change name to what ever the name is
                        messageBoard.Text += $"{channel.ID} - {message}";
                        participantList.Text += $"{channel.ID} : {channel.RemoteAddressPort}\n";
                        _logger.LogInformation("Command name: "+$"{channel.ID} - {message}");
                        _logger.LogInformation("Participant List: "+$"{channel.ID} : {channel.RemoteAddressPort}\n");
                    });
                }
            }
            else if (message.StartsWith("Command Participants"))
            {
                string requestList = "Command Participants";
                List<Networking> tempList = new List<Networking>(_clients);
                //retrieve client
                foreach (var client in _clients)
                {
                    requestList += $",{client.ID}";
                }

                _ = channel.SendAsync(requestList);        
                _logger.LogDebug("Send participants list to client");
                _logger.LogInformation($"{channel.ID}" + " requested current participant list");
            }
            else
            {
                lock (this._clients)
                {                  
                    // Create a temporary list to store clients
                    List<Networking> tempList = new List<Networking>(_clients);
                    // Send messages to clients
                    foreach (var client in tempList)
                    {
                         _ = client.SendAsync(message);
                         _logger.LogInformation("Message sent: " + $"{channel.ID}-{message}");
                    }

                    // Update messageBoard UI element after sending messages to all clients
                    Dispatcher.Dispatch(() => {
                        messageBoard.Text += $"{channel.ID} - {message}";
                        _logger.LogInformation("Message update on server: " + $"{channel.ID} - {message}");
                    });
                }
            }
            // Command Participants
            // send a list of participants back to the requesting client:
            

        }

        // Shut down server
        private void Shutdown_Click(object sender, EventArgs e)
        {
            if (shutdownButton.Text == "Shutdown Server")
            {
                _logger.LogInformation("Shut down button clicked");
                List<Networking> copy = new List<Networking>(_clients);
                Dispatcher.Dispatch(() =>
                {
                messageBoard.Text += "Server shut down" + Environment.NewLine;
                shutdownButton.Text = "Start Server";
                    foreach (Networking client in copy)
                    {
                        messageBoard.Text += client.ID + " has disconnected from server" + Environment.NewLine;
                    }
                });
                foreach (Networking client in copy)
                {
                    client.Disconnect();
                }
                // Clear the list 
                _clients.Clear();
                // Stop waiting for the server
                _networking.StopWaitingForClients();
            }
            else
            {
                _logger.LogInformation("Start server button clicked");
                Dispatcher.Dispatch(() =>
                {
                    messageBoard.Text += "Server started" + Environment.NewLine;
                });
                shutdownButton.Text = "Shutdown Server";
                _ = _networking.WaitForClientsAsync(_port, true);
            }

        }

        // Helper method to update participant list
        private void participantUpdate()
        {
            // Clear the list
            Dispatcher.Dispatch(() =>
            {
                participantList.Text = "";
            });
            //Update the list
            foreach (var channel in _clients)
            {
                Dispatcher.Dispatch(() => {
                    // Add the name and the IP address
                    participantList.Text += channel.ID;
                });
            }
            _logger.LogInformation("Participant list updated");
        }
    }

}