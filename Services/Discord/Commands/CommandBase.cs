using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.Discord.Commands
{
    public abstract class CommandBase : ICommand
    {
        private AppOptions _options;
        private BotDbContext _context;
        public CommandBase(AppOptions options, BotDbContext context)
        {
            _options = options;
            _context = context;
        }

        public abstract string CommandName { get; }
        public abstract string Descriptions { get; }

        // Default logic to validate whether the message is executable by this command
        public bool CanExecute(SocketUserMessage message, int argPos)
        {
            return GetCleanCommandText(message, argPos)
                .StartsWith(CommandName, StringComparison.InvariantCultureIgnoreCase)
                && (IsWrittenByAdmin(message) || IsWrittenInWhitelistedChannel(message));
        }

        internal bool IsWrittenByAdmin(SocketMessage message)
        {
            return _options.AdminDiscordIds.Contains(message.Author.Id);
        }

        internal bool IsWrittenInWhitelistedChannel(SocketUserMessage message)
        {
            return _context.Participants.Any(x => x.ChannelId == message.Channel.Id.ToString());
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
