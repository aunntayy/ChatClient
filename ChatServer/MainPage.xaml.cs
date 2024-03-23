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

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
            _logger.LogDebug( $"Count is now { count }" );

        }
    }

}
