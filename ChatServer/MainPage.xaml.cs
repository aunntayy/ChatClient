using Communications;
using Microsoft.Extensions.Logging;

namespace ChatServer
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger _logger;
        private Networking networking;
        public MainPage(ILogger<MainPage> logger)
        {
            _logger = logger;
            InitializeComponent();
            // Initialize an instance of Networking
            networking = new Networking(new Logger<Networking>(new LoggerFactory()),
                                         OnConnectionEstablished,
                                         OnDisconnect,
                                         OnMessageReceived);
            _logger.LogInformation($"Main Page Constructor");
            this.BindingContext = networking;

        }
        // Shut down server
        private void Shutdown_Click(object sender, EventArgs e)
        {
            networking.Disconnect();
        }

        // Event handlers for networking events
        private void OnConnectionEstablished(Networking client)
        {
            // Update UI if needed
        }

        private void OnDisconnect(Networking client)
        {
            // Update UI if needed
        }

        private void OnMessageReceived(Networking client, string message)
        {
            // Update UI if needed
        }
    }

}
