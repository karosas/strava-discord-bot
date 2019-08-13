using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Services.Storage;

namespace StravaDiscordBot.Services.Parser
{
    public class JoinLeaderboardCommand : ICommand
    {
        private readonly IRepository<LeaderboardParticipant> _repository;
        public JoinLeaderboardCommand(IRepository<LeaderboardParticipant> leaderboardRepository)
        {
            _repository = leaderboardRepository;
        }

        public bool CanExecute(SocketUserMessage message, int argPos)
        {
            return message.Content.Substring(argPos).Trim().ToLower().StartsWith("join");
        }

        public async Task Execute(SocketUserMessage message, int argPos)
        {
            var normalizedMessage = message.Content.Substring(argPos).Trim().ToLower();
            var arguments = normalizedMessage.Split(" ");
            if(arguments.Length != 2)
            {
                throw new InvalidCommandArgumentException($"Whoops, this seems wrong, the command should be in format of `join your_strava_id`");
            }

            if(await AlreadyExists(message.Channel.Id.ToString(), message.Author.Id.ToString()))
            {
                throw new InvalidCommandArgumentException($"Whoops, it seems like you're already participating in the leaderboard");
            }


            await message.Channel.SendMessageAsync($"I added you to the leaderboard, {message.Author.Mention} !");
            
        }

        private async Task<bool> AlreadyExists(string channelId, string userId)
        {
            var result = await _repository.GetById(channelId, userId);
            return result != null;
        }
    }
}
