using Android.Opengl;
using Microsoft.Maui.Controls;
using static Android.Graphics.ColorSpace;

namespace ChatClient
{
    public partial class MainPage : ContentPage
    {

        //Type in the name of the remote machine you wish to talk to(default to localhost)
        //Type in the name of the chatter(e.g. "Jim")
        //Type in a message(and have it sent to the server)
        //Have a way to show who is currently on the server(participants)
        //Have a list of the conversation.
        //Show the status (Connected/Disconnected/Error/etc.)
        private Networking _networking;



        public MainPage(){
            InitializeComponent();
            
        }

       
    }

}
