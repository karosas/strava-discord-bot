using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.Commands
{
    public class JoinLeaderboardCommand : CommandBase
    {
        private readonly ILogger<JoinLeaderboardCommand> _logger;
        private readonly ICommandCoreService _commandCoreService;

        public JoinLeaderboardCommand(AppOptions options, BotDbContext context, ILogger<JoinLeaderboardCommand> logger, ICommandCoreService commandCoreService) : base(options, context, logger)
        {
            _logger = logger;
            _commandCoreService = commandCoreService;
        }

        public override string CommandName => "join";
        public override string Descriptions => "Join the leaderboard.";

        public override bool CanExecute(SocketUserMessage message, int argPos)
        {
            return GetCleanCommandText(message, argPos)
                .StartsWith(CommandName, StringComparison.InvariantCultureIgnoreCase)
                && IsWrittenInWhitelistedServer(message);
        }

        public override async Task Execute(SocketUserMessage message, int argPos)
        {
            if (!CanExecute(message, argPos))
                throw new InvalidCommandArgumentException($"Whoops, this seems wrong, the command should be in format of `{CommandName}`");

            _logger.LogInformation($"Executing 'Join' command. Full: {message.Content} | Author: {message.Author}");

            if (TryCastChannelToServerChannel(message, out var serverChannel))
            {
                var dmChannel = await message.Author.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                await dmChannel
                    .SendMessageAsync(await _commandCoreService.GenerateJoinCommandContent(serverChannel.Guild.Id, serverChannel.Id, message.Author.Id, message.Author.Mention))
                    .ConfigureAwait(false);
            }
            else
            {
                message.Channel.SendMessageAsync("This doesn't seem like a channel within a server. Try joining the leaderboard from channel inside a server where this bot is set up.");
            }
        }
    }
}
