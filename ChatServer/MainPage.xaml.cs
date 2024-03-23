using Microsoft.Extensions.Logging;

namespace ChatServer
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly ILogger _logger;
        public MainPage(ILogger<MainPage> logger)
        {
            _logger = logger;
            InitializeComponent();
            _logger.LogInformation($"Main Page Constructor");

        }

        private void Shutdown_Click(object sender, EventArgs e)
        {
           
        }
    }

}
