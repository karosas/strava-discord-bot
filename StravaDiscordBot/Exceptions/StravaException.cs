using System;

namespace StravaDiscordBot.Exceptions
{
    public class StravaException : Exception
    {
        public StravaException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}