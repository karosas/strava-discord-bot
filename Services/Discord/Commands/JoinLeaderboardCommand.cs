using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Services.Storage;

namespace StravaDiscordBot.Services.Discord.Commands
{
    public class JoinLeaderboardCommand : ICommand
    {
        private readonly IStravaService _stravaService;
        private readonly ILogger<JoinLeaderboardCommand> _logger;

        public JoinLeaderboardCommand(IStravaService stravaService, ILogger<JoinLeaderboardCommand> logger)
        {
            _stravaService = stravaService;
            _logger = logger;
        }

        public bool CanExecute(SocketUserMessage message, int argPos)
        {
            return message.Content.Substring(argPos).Trim().ToLower().StartsWith("join");
        }

        public async Task Execute(SocketUserMessage message, int argPos)
        {
            if (!CanExecute(message, argPos))
                throw new InvalidCommandArgumentException($"Whoops, this seems wrong, the command should be in format of `join`");

            _logger.LogInformation($"Executing 'Join' command. Full: {message.Content} | Author: {message.Author}");

            //if(await _stravaService.ParticipantAlreadyExistsAsync(message.Channel.Id.ToString(), message.Author.Id.ToString()))
            //    throw new InvalidCommandArgumentException($"Whoops, it seems like you're already participating in the leaderboard");

            var dmChannel = await message.Author.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"Hey, {message.Author.Mention} ! Please go to this url to allow me check out your Strava activities: {await _stravaService.GetOAuthUrl(message.Channel.Id.ToString(), message.Author.Id.ToString())}");
        }
    }
}
