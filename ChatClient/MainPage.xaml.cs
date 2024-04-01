
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
        private string message {  get; set; }   
        public MainPage(ILogger<MainPage> logger){
            
            _logger = logger;
            _client = new Networking(logger, OnConnect, OnDisconnect, OnMessage);
            InitializeComponent();
        }


        
        //connect button
        private void Connect(object sender, EventArgs e) {
            _logger.LogDebug("Connect button clicked");
            //  host = hostAddress.Text;
            //  _client.ID = userName.Text; 
            Dispatcher.Dispatch(() => {
                _client.ID = userName.Text;
                _ = _client.ConnectAsync("192.168.50.201", port);
                _ = _client.SendAsync("Command Name" + "[" + userName.Text + "]");
            
            });
        }

        //hit enter on message
        private async void Message(object sender, EventArgs e) {
            _logger.LogDebug("Message entry");
            message = textEntry.Text;
            messageBoard.Text += userName.Text + ": " + message + Environment.NewLine;
            await _client.SendAsync(message);

            if (_client.IsConnected == false) {
                messageBoard.Text += "Server gone bruv" + Environment.NewLine;
            }
        }

        private void OnMessage(Networking channel, string message)
        {
            if (!message.StartsWith("Command Name"))
            {


            }
            else
            {
                lock (this._client)
                {
                    Dispatcher.Dispatch(() =>
                    {
                            messageBoard.Text += $"{channel.ID} - {message}\n";
                            //messageBoard.Text += message + Environment.NewLine;
                    });

                }
            }
        }
       

        private void OnDisconnect(Networking channel)
        {
          _logger.LogDebug($"Disconnecting {channel}");
           ConnectBttn.IsEnabled = false;
            
        }

        private void OnConnect(Networking channel)
        {
            _logger.LogDebug("Client on connect called");

            if (userName.Text.Length > 0)
            {
                channel.ID = userName.Text;
            }
        }
    }

}
