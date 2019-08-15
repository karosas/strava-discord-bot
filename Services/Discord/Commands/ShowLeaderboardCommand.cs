using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;

namespace StravaDiscordBot.Services.Discord.Commands
{
    // TODO: Make more generic
    public class ShowLeaderboardCommand : ICommand
    {
        private readonly IStravaService _stravaService;
        public ShowLeaderboardCommand(IStravaService stravaService)
        {
            _stravaService = stravaService;
        }

        public bool CanExecute(SocketUserMessage message, int argPos)
        {
            return message.Content.Substring(argPos).Trim().ToLower().StartsWith("leaderboard");
        }

        public async Task Execute(SocketUserMessage message, int argPos)
        {
            if (!CanExecute(message, argPos))
                throw new InvalidCommandArgumentException($"Whoops, this seems wrong, the command should be in format of `leaderboard`");

            var groupedActivitiesByParticipant = await _stravaService.GetAllLeaderboardActivitiesForChannelIdAsync(message.Channel.Id.ToString());
            var leaderboardMessage = FormatActivitiesIntoLeaderboardMessage(groupedActivitiesByParticipant);

            await message.Channel.SendMessageAsync(leaderboardMessage);

        }
        private string FormatActivitiesIntoLeaderboardMessage(Dictionary<LeaderboardParticipant, List<DetailedActivity>> activities) 
        {
            var leaderboard = new Leaderboard(activities);
            var topDistances = leaderboard.GetTopDistances();

            var builder = new StringBuilder();
            builder.AppendLine($"Leaderboard for {DateTime.Now.ToString("yyyy MMMM dd")}\n\n");

            var index = 1;
            foreach(var distance in topDistances)
            {
                builder.AppendLine($"{index}. {distance.Key.DiscordMention} @ {distance.Value / 1000} km");
                index++;
            }
            return builder.ToString();
        }
    }
}
