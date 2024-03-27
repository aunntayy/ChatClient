
using Communications;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System.Reflection.PortableExecutable;

namespace ChatClient
{
    public partial class MainPage : ContentPage
    {

        //Type in the name of the remote machine you wish to talk to(default to localhost)
        //Type in the name of the chatter(e.g. "Jim")
        //Type in a message(and have it sent to the server)
        //Have a way to show who is currently on the server(participants)
        //Have a list of the conversation.

        private Networking _client;
        private readonly ILogger _logger;
        private const int port = 11000;
        private string host { get; set; }

        public MainPage(ILogger<MainPage> logger){
            _logger = logger;
            InitializeComponent();
            _client = new Networking(logger, OnConnect, OnDisconnect, OnMessage);
            host = Environment.MachineName;

        }


        //connect button
        private void Connect(object sender, EventArgs e) {
            _logger.LogDebug("Connect button clicked");
            _ = _client.ConnectAsync(host, port);
        }

        private void OnMessage(Networking channel, string message)
        {
            throw new NotImplementedException();
        }

        private void OnDisconnect(Networking channel)
        {
            throw new NotImplementedException();
        }

        private void OnConnect(Networking channel)
        {
            _logger.LogDebug("Client on connect called");
            _ = _client.HandleIncomingDataAsync(true);
            if (userName.Text.Length > 0)
            {
                channel.ID = userName.Text;
                _ = _client.SendAsync("Command Name" + "[" + userName.Text + "]");
            }
        }
    }

}
