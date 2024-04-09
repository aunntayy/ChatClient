using Communications;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System;
using System.Net;

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
/// This file contains the implementation of the main page for the chat client application.
/// It includes event handlers for user interactions such as connecting to the server, sending messages, 
/// and retrieving user information. It also handles events related to server communication such as 
/// message reception, disconnection, and successful connection.    
/// </summary>
namespace ChatClient
{
    /// <summary>
    /// Represents the main page of the chat client application.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        private readonly Networking _client;
        private readonly ILogger _logger;
        private const int port = 11000;
        private string host { get; set; }
        private string message { get; set; }
        public string MachineName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging messages.</param>
        public MainPage(ILogger<MainPage> logger)
        {
          
            _logger = logger;
            _client = new Networking(logger, OnConnect, OnDisconnect, OnMessage);
            MachineName = Environment.MachineName;
            this.BindingContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for the "Retrieve User" button click.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void UserBttnClicked(object sender, EventArgs e)
        {
            _ = _client.SendAsync("Command Participants");
        }

        /// <summary>
        /// Event handler for the "Connect" button click.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private async void Connect(object sender, EventArgs e)
        {
            // Check if the username is empty
            if (string.IsNullOrWhiteSpace(userName.Text))
            {
                await DisplayAlert("Warning", "Please enter a username.", "OK");
                return;
            }

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


        /// <summary>
        /// Event handler for hitting enter on the message entry.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void Message(object sender, EventArgs e)
        {
            _logger.LogDebug("Message entry");
            message = textEntry.Text;
             _ = _client.SendAsync(message + "\n");
            if (_client.IsConnected == false) {
                ConnectBttn.Text = "Connect to server";
                ConnectBttn.IsEnabled = true;
                messageBoard.Text += "Server gone bruv" + Environment.NewLine;
            }
        }

        /// <summary>
        /// Event handler for receiving messages from the server.
        /// </summary>
        /// <param name="channel">The networking channel.</param>
        /// <param name="message">The received message.</param>
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
                    _logger.LogDebug("Participant list received");
                }
                else { messageBoard.Text += $"{message}"; }
                _logger.LogDebug("Message received");
            });
        }

        /// <summary>
        /// Event handler for handling disconnection from the server.
        /// </summary>
        /// <param name="channel">The networking channel.</param>
        private void OnDisconnect(Networking channel)
        {
            _logger.LogDebug($"Disconnecting {channel}");
            ConnectBttn.IsEnabled = true;
            _ = _client.HandleIncomingDataAsync(false);
      
                Dispatcher.Dispatch(() =>
                {
                    ConnectBttn.Text = "Connect to server";
                    ConnectBttn.IsEnabled = true;
                    messageBoard.Text += "Connected to server:)" + Environment.NewLine;
                });
            
        }

        /// <summary>
        /// Event handler for successful connection to the server.
        /// </summary>
        /// <param name="channel">The networking channel.</param>
        private void OnConnect(Networking channel)
        {
            _logger.LogDebug("Client on connect called");
            _ = _client.HandleIncomingDataAsync(true);
        }
    }
}
