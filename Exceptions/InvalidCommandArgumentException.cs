using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StravaDiscordBot.Exceptions
{
    public class InvalidCommandArgumentException : Exception
    {
        public InvalidCommandArgumentException(string message) : base(message)
        {
        }

        public InvalidCommandArgumentException()
        {
        }

        public InvalidCommandArgumentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
