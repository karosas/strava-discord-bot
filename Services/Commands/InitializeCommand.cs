using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.Commands
{
    public class InitializeCommand : CommandBase
    {
        private readonly ILogger<InitializeCommand> _logger;
        private readonly ICommandCoreService _commandCoreService;
        public InitializeCommand(AppOptions options, BotDbContext context, ILogger<InitializeCommand> logger, ICommandCoreService commandCoreService) : base(options, context, logger)
        {
            _logger = logger;
            _commandCoreService = commandCoreService;
        }

        public override string CommandName => "init";
        public override string Description => "[Admin] Initialize leaderboard for this server in this channel.";

        public override bool CanExecute(SocketUserMessage message, int argPos)
        {
            return GetCleanCommandText(message, argPos)
               .StartsWith(CommandName, StringComparison.InvariantCultureIgnoreCase)
               && IsWrittenByAdmin(message)
               && !LeaderboardAlreadyExists(message);
        }

        private bool LeaderboardAlreadyExists(SocketUserMessage message)
        {
            if(TryCastChannelToServerChannel(message, out var serverChannel))
            {
                return _context.Leaderboards.Any(x => x.ServerId == serverChannel.Guild.Id.ToString());
            }

            return true;
        }

        public override async Task Execute(SocketUserMessage message, int argPos)
        {
            if (!CanExecute(message, argPos))
                throw new InvalidCommandArgumentException($"Whoops, this seems wrong, the command should be in format of `{CommandName}`");

            _logger.LogInformation($"Executing 'Initialize' command. Full: {message.Content} | Author: {message.Author}");

            if(TryCastChannelToServerChannel(message, out var serverChannel))
            {
                var leaderboard = new Leaderboard { ServerId = serverChannel.Guild.Id.ToString(), ChannelId = serverChannel.Id.ToString() };
                _context.Leaderboards.Add(leaderboard);
                await _context.SaveChangesAsync();
                message.Channel.SendMessageAsync("Initialized leaderboard for this server. Users can join by using `join` command.");
            } 
            else
            {
                await message.Channel.SendMessageAsync("Hmm, it seems that leaderboard is already set up on this server?");
            }
        }
    }
}
