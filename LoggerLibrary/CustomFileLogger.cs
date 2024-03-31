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
        private readonly object _lock = new object();
        public CustomFileLogger(string categoryName)
        {
            _filePath = Environment.GetFolderPath(
                  Environment.SpecialFolder.MyDocuments)
                  + Path.DirectorySeparatorChar
                  + $"CS3500-{categoryName}.log";
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
            Task.Factory.StartNew(() => {
                lock (_lock) {
                    string logMessage = $"{DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt")} ({System.Threading.Thread.CurrentThread.ManagedThreadId}) - {logLevel} - {formatter(state, exception)}{Environment.NewLine}";
                    File.AppendAllText(_filePath, logMessage);
                }
            });
           
           
        }
    }


}

