using System;

namespace StravaDiscordBot.Exceptions
{
    public class StravaException : Exception
    {
        public enum StravaErrorType
        {
            Unknown,
            Unauthorized,
            RefreshFailed
        }

        public StravaException(StravaErrorType error, string message) : base(message)
        {
            Error = error;
        }

        public StravaException(StravaErrorType error)
        {
            Error = error;
        }

        public StravaException(StravaErrorType error, string message, Exception innerException) : base(message,
            innerException)
        {
            Error = error;
        }

        public StravaErrorType Error { get; }
    }
}