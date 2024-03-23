using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggerLibrary
{
    public class CustomFileLoggerProvider : ILoggerProvider
    {
        //create and store a custom file logger object
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomFileLogger(categoryName);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
