using Microsoft.Extensions.Logging;
using System;

namespace LoggerLibrary
{
    public class FileLogger : ILogger {
        private readonly string _FileName;

        public FileLogger(string categoryName) {
           _FileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + 
                Path.DirectorySeparatorChar + $"CS3500-{categoryName}.log";
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel) {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            File.AppendAllText(_FileName,$"{DateTime.Now} : {logLevel}" + formatter(state,exception)+ $"{Environment.NewLine}");
        }
    }

    public class FileLoggerProvider : ILoggerProvider {
        public ILogger CreateLogger(string categoryName) {
            return new FileLogger(categoryName);
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
