using Microsoft.Extensions.Logging;
using System;

namespace LoggerLibrary
{
    public class CustomFileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;

        public CustomFileLoggerProvider (string filePath)
        {
            _filePath = filePath;
        }
        //create and store a custom file logger object
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomFileLogger(_filePath);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
