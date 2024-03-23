using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggerLibrary
{
    public class CustomFileLogger : ILogger
    {
        private readonly string _filePath;
        public CustomFileLogger(string filePath)
        {
            _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), filePath);
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            File.AppendAllText(_filePath, $"{DateTime.Now} {System.Threading.Thread.CurrentThread.ManagedThreadId} {logLevel}" + formatter(state, exception) + $"{Environment.NewLine}");
        }
    }
}

