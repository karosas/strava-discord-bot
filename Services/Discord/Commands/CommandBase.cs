using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace StravaDiscordBot.Services.Discord.Commands
{
    public abstract class CommandBase : ICommand
    {
        public abstract string CommandName { get; }
        public abstract string Descriptions { get; }

        // Default logic to validate whether the message is executable by this command
        public bool CanExecute(SocketUserMessage message, int argPos)
        {
            return GetCleanCommandText(message, argPos).StartsWith(CommandName, StringComparison.InvariantCultureIgnoreCase);
        }

        internal string GetCleanCommandText(SocketUserMessage message, int argPos)
        {
            return message
                .Content
                .Substring(argPos)
                .Trim()
                .ToLower(CultureInfo.InvariantCulture);
        }

        public abstract Task Execute(SocketUserMessage message, int argPos);
    }
}
