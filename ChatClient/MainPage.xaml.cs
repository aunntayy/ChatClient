
using Communications;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Threading.Channels;

namespace ChatClient
{
    public partial class MainPage : ContentPage
    {

        //Type in the name of the remote machine you wish to talk to(default to localhost)
        //Type in the name of the chatter(e.g. "Jim")
        //Type in a message(and have it sent to the server)
        //Have a way to show who is currently on the server(participants)
        //Have a list of the conversation.

        private readonly Networking _client;
        private readonly ILogger _logger;

        private const int port = 11000;
        private string host { get; set; }
        private string message { get; set; }
        public MainPage(ILogger<MainPage> logger)
        {

            _logger = logger;
            _client = new Networking(logger, OnConnect, OnDisconnect, OnMessage);
            InitializeComponent();
        }


        //clicked retrieve user button
        private void UserBttnClicked(object sender, EventArgs e)
        {
            _ = _client.SendAsync("Command Participants");
        }


        //connect button
        private void Connect(object sender, EventArgs e)
        {
            _logger.LogDebug("Connect button clicked");
            host = hostAddress.Text;

            _client.ID = userName.Text;
            _ = _client.ConnectAsync(host, port);
            _ = _client.SendAsync("Command Name " + userName.Text + "\n");
            if (_client.IsConnected)
            {
                // Update the button text to "Connected"
                Dispatcher.Dispatch(() =>
                {
                    ConnectBttn.Text = "Connected";
                    ConnectBttn.IsEnabled = false;
                    messageBoard.Text += "Connected to server:)" + Environment.NewLine;
                });
            }
        }

        //hit enter on message
        private void Message(object sender, EventArgs e)
        {
            _logger.LogDebug("Message entry");
            message = textEntry.Text;
            _ = _client.SendAsync(message + "\n");
            if (_client.IsConnected == false)
            {
                messageBoard.Text += "Server gone bruv" + Environment.NewLine;
            }
        }

        private void OnMessage(Networking channel, string message)
        {
            Dispatcher.Dispatch(() =>
            {
                if (message.StartsWith("Command Participants"))
                {
                    participantList.Text = "";
                    string[] list = message.Split(',');
                    list[0] = "Current user";
                    foreach (string userID in list)
                    {
                        participantList.Text += userID + Environment.NewLine;
                    }
                    _logger.LogDebug("Participant list receive");
                }
                else { messageBoard.Text += $"{channel.ID} - {message}"; }
                _logger.LogDebug("Message receive");
            });
        }



        private void OnDisconnect(Networking channel)
        {
            _logger.LogDebug($"Disconnecting {channel}");
            ConnectBttn.IsEnabled = false;
        }

        private void OnConnect(Networking channel)
        {
            _logger.LogDebug("Client on connect called");
            _ = _client.HandleIncomingDataAsync(true);
        }
    }
}
