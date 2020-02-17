using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StravaDiscordBot.Exceptions
{
    public class StravaException : Exception
    {
        public StravaErrorType Error { get; }
        public StravaException(StravaErrorType error, string message) : base(message) 
        {
            Error = error;
        }

        public StravaException(StravaErrorType error)
        {
            Error = error;
        }

        public StravaException(StravaErrorType error, string message, Exception innerException) : base(message, innerException)
        {
            Error = error;
        }

        public enum StravaErrorType
        {
            Unknown,
            Unauthorized,
            RefreshFailed
        }
    }
}
