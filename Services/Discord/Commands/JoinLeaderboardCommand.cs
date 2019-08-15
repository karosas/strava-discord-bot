using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Services.Storage;

namespace StravaDiscordBot.Services.Discord.Commands
{
    public class JoinLeaderboardCommand : ICommand
    {
        private readonly IStravaService _stravaService;
        public JoinLeaderboardCommand(IStravaService stravaService)
        {
            _stravaService = stravaService;
        }

        public bool CanExecute(SocketUserMessage message, int argPos)
        {
            return message.Content.Substring(argPos).Trim().ToLower().StartsWith("join");
        }

        public async Task Execute(SocketUserMessage message, int argPos)
        {
            if (!CanExecute(message, argPos))
                throw new InvalidCommandArgumentException($"Whoops, this seems wrong, the command should be in format of `join`");

            if(await _stravaService.ParticipantAlreadyExistsAsync(message.Channel.Id.ToString(), message.Author.Id.ToString()))
                throw new InvalidCommandArgumentException($"Whoops, it seems like you're already participating in the leaderboard");

            var dmChannel = await message.Author.GetOrCreateDMChannelAsync();
            await dmChannel.SendMessageAsync($"Hey, {message.Author.Mention} ! Please go to this url to allow me check out your Strava activities: {_stravaService.GetOAuthUrl(message.Channel.Id.ToString(), message.Author.Id.ToString())}");
        }
    }
}
