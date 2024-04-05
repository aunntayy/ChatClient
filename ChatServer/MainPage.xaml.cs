using Communications;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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
/// This file contains the implementation of a chat server application for the CS 3500 course project. 
/// It includes classes and methods related to managing client connections, handling messages, and server operations.
///    
/// </summary>
namespace ChatServer
{

    /// <summary>
    /// Represents the main page of the chat server application.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private readonly ILogger _logger;
        private readonly Networking _networking;
        private List<Networking> _clients;
        private readonly int _port = 11000;

        /// <summary>
        /// Gets or sets the machine name of the server.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the server.
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging messages.</param>
        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _clients = new List<Networking>();
            _logger = logger;
            _networking = new Networking(logger, OnConnection, OnDisconnect, OnMessageReceived);
            _ = _networking.WaitForClientsAsync(_port, true);
            MachineName = Environment.MachineName;
            IPAddress = GetIPAddress();
            this.BindingContext = this;
            _logger.LogInformation($"Main Page Constructor");
        }

        /// <summary>
        /// Retrieves the IP address of the server.
        /// </summary>
        /// <returns>The IP address of the server.</returns>
        private string GetIPAddress()
        {
            string ipAddress = string.Empty;
            try
            {
                string hostName = Dns.GetHostName();
                IPAddress[] addresses = Dns.GetHostAddresses(hostName);
                ipAddress = addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();

                if (ipAddress == null)
                {
                    ipAddress = "No IPv4 address found.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting IP address: {ex.Message}");
                ipAddress = "Error getting IP address.";
            }
            _logger.LogDebug("IP address obtained");
            return ipAddress;
        }

        /// <summary>
        /// Handles the connection of a client to the server.
        /// </summary>
        /// <param name="channel">The networking channel representing the client.</param>
        private void OnConnection(Networking channel)
        {
            lock (this._clients)
            {
                _clients.Add(channel);
            }
            _logger.LogDebug("Server OnConnection");
        }

        /// <summary>
        /// Handles disconnection of a client from the server.
        /// </summary>
        /// <param name="channel">The networking channel representing the disconnected client.</param>
        private void OnDisconnect(Networking channel)
        {
            if (_clients is not null)
            {
                lock (this._clients)
                {
                    _clients.Remove(channel);
                }

                Dispatcher.Dispatch(() =>
                {
                    messageBoard.Text += $"{channel.ID} has disconnected from the server" + Environment.NewLine;
                    _logger.LogInformation($"Disconnecting {channel.ID}");
                });
                participantUpdate();
                _logger.LogDebug("Server OnDisconnect");
            }
        }

        /// <summary>
        /// Handles the reception of a message from a client.
        /// </summary>
        /// <param name="channel">The networking channel representing the client.</param>
        /// <param name="message">The received message.</param>
        private void OnMessageReceived(Networking channel, string message) { 
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
                        _logger.LogInformation("Command name: " + $"{channel.ID} - {message}");
                        _logger.LogInformation("Participant List: " + $"{channel.ID} : {channel.RemoteAddressPort}\n");
                    });
                    participantUpdate();
                }
            }
            else if (message.StartsWith("Command Participants"))
            {
                string requestList = "Command Participants";
                List<Networking> tempList = new List<Networking>(_clients);
                //retrieve client
                foreach (var client in _clients)
                {
                    requestList += $",{client.ID} - {channel.RemoteAddressPort} {Environment.NewLine}";
                }

                _ = channel.SendAsync(requestList);
                _logger.LogDebug("Send participants list to client");
                _logger.LogInformation($"{channel.ID}" + " requested current participant list");
            }
            else
            {
                lock (this._clients)
                {
                    List<Networking> tempList = new List<Networking>(_clients);
                    foreach (var client in tempList)
                    {
                        _ = client.SendAsync($"{channel.ID} - {message}");
                        _logger.LogInformation("Message sent: " + $"{channel.ID} - {message}");
                    }

                    Dispatcher.Dispatch(() =>
                    {
                        messageBoard.Text += $"{channel.ID} - {message}";
                        _logger.LogInformation("Message update on server: " + $"{channel.ID} - {message}");
                    });
                }
            }
        }

        /// <summary>
        /// Shuts down the server.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Shutdown_Click(object sender, EventArgs e)
        {
            if (shutdownButton.Text == "Shutdown Server")
            {
                _logger.LogInformation("Shutdown button clicked");
                messageBoard.Text += "Server shut down" + Environment.NewLine;
                shutdownButton.Text = "Start Server";

                List<Networking> copy = new List<Networking>(_clients);
                foreach (Networking client in copy)
                {
                    client.Disconnect();
                }

                _clients.Clear();
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

        /// <summary>
        /// Updates the participant list on the UI.
        /// </summary>
        private void participantUpdate()
        {
            Dispatcher.Dispatch(() =>
            {
                participantList.Text = "";
            });

            foreach (var channel in _clients)
            {
                Dispatcher.Dispatch(() =>
                {
                    participantList.Text += $"{channel.ID} - {channel.RemoteAddressPort} {Environment.NewLine}" ;
                });
            }
            _logger.LogInformation("Participant list updated");
        }
    }
}
