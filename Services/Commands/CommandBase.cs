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
        internal AppOptions _options;
        internal BotDbContext _context;
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
        public virtual bool CanExecute(SocketUserMessage message, int argPos)
        {
            return GetCleanCommandText(message, argPos)
                .StartsWith(CommandName, StringComparison.InvariantCultureIgnoreCase)
                && IsWrittenByAdmin(message)
                && IsWrittenInWhitelistedServer(message);
        }

        internal bool IsWrittenByAdmin(SocketMessage message)
        {
            var result = _options.AdminDiscordIds.Contains(message.Author.Id);
            _logger.LogInformation($"Is message sent by admin - {result}");
            return result;
        }

        internal bool IsWrittenInWhitelistedServer(SocketUserMessage message)
        {
            if(TryCastChannelToServerChannel(message, out var serverChannel))
            {
                var result = _context.Leaderboards.Any(x => x.ServerId == serverChannel.Guild.Id.ToString());
                _logger.LogInformation($"Is message sent from whitelisted server - {result}");
                return result;
            }
            return false;
        }

        internal bool TryCastChannelToServerChannel(SocketUserMessage message, out SocketGuildChannel serverChannel)
        {
            if (message.Channel is SocketGuildChannel channel)
            {
                serverChannel = channel;
                return true;
            }

            serverChannel = null;
            return false;
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
