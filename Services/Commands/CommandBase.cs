using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.Commands
{
    public abstract class CommandBase : ICommand
    {
        private AppOptions _options;
        private BotDbContext _context;
        private ILogger _logger;
        public CommandBase(AppOptions options, BotDbContext context, ILogger logger)
        {
            _options = options;
            _context = context;
            _logger = logger;
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
            var result = _options.AdminDiscordIds.Contains(message.Author.Id);
            _logger.LogInformation($"Is message sent by admin - {result}");
            return result;
        }

        internal bool IsWrittenInWhitelistedChannel(SocketUserMessage message)
        {
            var result = _context.Participants.Any(x => x.ChannelId == message.Channel.Id.ToString());
            _logger.LogInformation($"Is message sent from whitelisted channel - {result}");
            return result;
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
