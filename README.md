<summary>
Author:     Phuc Hoang
Partner:    Chanphone Visathip
Start Date: 23-Mar-2024
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  PhucHoang123
Repo:       https://github.com/uofu-cs3500-spring24/assignment-seven-logging-and-networking-v_tekkkkk2
Commit Date:4-April-2024 11:25pm
Solution:   Networking_and_logging
Copyright:  CS 3500 and Phuc Hoang and Chanphone Visathip - This work may not be copied for use in Academic Coursework.
</summary>

# Overview of the Chat Client functionality
	It is a Chat Client program that let user connect to same IP address and port number to chat with others user.

# Consulted Peers:
	We talked with Trenton, Mia, Ted and Shadrach.
# Comments to  Networking_and_logging:
	There are some minor bugs that we haven't get done by the due date.
	The shut down button is working for professor chat client. But it won't work for our chat client. It throw this exception 
	"2024-04-04 10:29:39 PM (12) - Error - Error handling incoming data: Unable to read data from the transport connection: 
	The I/O operation has been aborted because of either a thread exit or an application request.."
	When we clicked the shut down server the chat client will freeze. On the April 3rd, it was working fine until it stopped today. 

# Assignment Specific Topics
	-none-
# Time Expenditures:
	Assignment Seven: Predicted Hour:48    Actual Hour:54

# Partnerships
	Phuc Hoang: 5 
	Chanphone Visathip: 5
	Pair programming hour: 40
	Debug hour: 19
	We was struggling on trying to get ChatClient recieve message from ChatServer. Most of debug hour was on that and the rest
	are for small debugging. The shut down is working on professor chat client.
# Testing
	For testing, we only write basic functions tests for networking class. Most of the tests were experiments running Clients and Servers together to see how they behave and correct them.

# Good software practice (GSP)	
	1.Well-named, commented and short methods : 
		We use lots of small comment on some methods. That tell the reader know what that actual do.
	2.Separation of concerns:
		We think the Networking class was not a really good software practice. Because it does 2 jobs for both ChatClient and ChatServer, 
		we think it would be better if the professor create 2 class 1 for ChatClient and 1 for ChatServer.
# References
	1.Piazza post
	2.ChatGPT - https://chat.openai.com/
	3.TcpClient - https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcpclient?view=net-8.0
	4.Logger C# - https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line
	5.Error - https://stackoverflow.com/questions/7228703/error-the-i-o-operation-has-been-aborted-because-of-either-a-thread-exit-or-an
	