using Communications;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Net;
using System.Threading.Channels;
using System.Text;
using System.Collections.Immutable;
using System.Linq;

namespace ChatServer
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger _logger;
        private Networking _networking;
        private List<Networking> _clients;
        private int _port = 11000;
        public string MachineName { get; set; }
        public string IPAddress { get; set; }
        public MainPage(ILogger<MainPage> logger)
        {
            
            InitializeComponent();
            _clients = new List<Networking>();
            _logger = logger;
            // Initialize an instance of Networking
            _networking = new Networking( logger,
                                         OnConnection,
                                         OnDisconnect,
                                         OnMessageReceived);
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
            // Add to client list
            _clients.Add(channel);
            // Update message and participant list
            Dispatcher.Dispatch(() =>{ 
                participantList.Text += channel.ID;
                message.Text += channel.ID + "has connected to sever" + Environment.NewLine;
            });
            _logger.LogDebug("server OnConnection");
        }

        /// <summary>
        /// Called when the is a disconnect with clients
        /// </summary>
        /// <param name="channel"></param>
        private void OnDisconnect(Networking channel)
        {
            // Remove clients from the list
            _clients.Remove(channel);
            // Add a noti to message
            Dispatcher.Dispatch(() =>
            {
                message.Text += channel.ID + "has disconnected from sever" + Environment.NewLine;
            });
            _logger.LogDebug("server OnDisconnect");
        }

        private void OnMessageReceived(Networking channel, string message)
        {
            // Command name [name]
            if (message.StartsWith("Command Name"))
            {
                // Find the index of the opening bracket '['
                int startIndex = message.IndexOf('[');
                if (startIndex != -1)
                {
                    // Find the index of the closing bracket ']' starting from the index after the opening bracket
                    int endIndex = message.IndexOf(']', startIndex + 1);
                    if (endIndex != -1)
                    {
                        // Extract the substring between the brackets
                        string name = message.Substring(startIndex + 1, endIndex - startIndex - 1);
                        bool nameExists = false;

                        // Check if the name already exists
                        foreach (var client in _clients)
                        {
                            if (client.ID == name)
                            {
                                nameExists = true;
                                break;
                            }
                        }

                        if (!nameExists)
                        {
                            // Assign the extracted name to the channel's ID property
                            channel.ID = name;
                            // Update the list
                            participantUpdate();
                        }
                        else
                        {
                            channel.SendAsync("NAME REJECTED");
                        }

                        _logger.LogDebug("Send participants list to client");
                    }
                }
            }
            // Command Participants
            // send a list of participants back to the requesting client:
            if (message.StartsWith("Command Participants")) 
            {
                StringBuilder participantList = new StringBuilder();
                foreach (var client in _clients) 
                {
                    participantList.Append("[" + _clients + "]");
                }
                channel.SendAsync(participantList.ToString());
                _logger.LogDebug("Send participants list to client");
            }
        }

        // Shut down server
        private void Shutdown_Click(object sender, EventArgs e)
        {
            _logger.LogDebug("Shut down button clicked");
            Dispatcher.Dispatch(() =>
            {
                message.Text += "Server shut down" + Environment.NewLine;
            });
            _networking.Disconnect();
            // Clean the list
            participantUpdate();
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
                    // Add the name and the IP address
                    participantList.Text += channel.ID + channel._tcpClient.Client.RemoteEndPoint;
                }
                _logger.LogDebug("Participant list updated");
        }
    }

}
