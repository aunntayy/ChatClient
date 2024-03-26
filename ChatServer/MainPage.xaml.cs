using Communications;
using Microsoft.Extensions.Logging;

namespace ChatServer
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger _logger;
        private Networking _networking;
        private List<Networking> _clients;
        private int _port = 11000;
        public MainPage(ILogger<MainPage> logger)
        {
            
            InitializeComponent();
            _logger = logger;
            // Initialize an instance of Networking
            _networking = new Networking( logger,
                                         OnConnection,
                                         OnDisconnect,
                                         OnMessageReceived);
            _networking.WaitForClientsAsync(_port, true);
            _logger.LogInformation($"Main Page Constructor");
            this.BindingContext = _networking;

        }
        // Shut down server
        private void Shutdown_Click(object sender, EventArgs e)
        {
            _networking.Disconnect();
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
                message.Text += channel.ID + "has connected to sever";
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
            // Add a noti to message
            //
        }

        private void OnMessageReceived(Networking channel, string message)
        {
            // Update UI if needed
        }
    }

}
