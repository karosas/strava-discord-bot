using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using StravaDiscordBot.Exceptions;

namespace StravaDiscordBot.Services.Parser
{
    public class JoinLeaderboardCommand : ICommand
    {
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

            await message.Channel.SendMessageAsync($"I added you to the leaderboard, {message.Author.Mention} !");
        }
    }
}
