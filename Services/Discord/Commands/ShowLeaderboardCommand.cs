using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.Discord.Commands
{
    // TODO: Make more generic
    public partial class ShowLeaderboardCommand : CommandBase
    {
        private readonly IStravaService _stravaService;
        private readonly ILogger<ShowLeaderboardCommand> _logger;

        public ShowLeaderboardCommand(AppOptions options, BotDbContext context, IStravaService stravaService, ILogger<ShowLeaderboardCommand> logger) : base(options, context)
        {
            _stravaService = stravaService;
            _logger = logger;
        }

        public override string CommandName => "leaderboard";
        public override string Descriptions => "Print current leaderboard";

        private bool _silent = false;
        private const string VIRTUAL_RIDE_NAME = "VirtualRide";
        private const string REAL_RIDE_NAME = "Ride";

        public override async Task Execute(SocketUserMessage message, int argPos)
        {
            if (!CanExecute(message, argPos))
                throw new InvalidCommandArgumentException($"Whoops, this seems wrong, the command should be in format of `{CommandName}`");

            _silent = GetCleanCommandText(message, argPos).Contains("silent");

            _logger.LogInformation($"Executing 'leaderboard' command. Full: {message.Content} | Author: {message.Author}");
            var start = DateTime.Now.AddDays(-7);
            var groupedActivitiesByParticipant = await _stravaService.GetActivitiesSinceStartDate(message.Channel.Id.ToString(), start).ConfigureAwait(false);
            var leaderboardHeadline = $"Leaderboard from  {start.ToString("yyyy MMMM dd")} to {DateTime.Now.ToString("yyyy MMMM dd")}\n";
            var leaderboardMessage = FormatActivitiesIntoLeaderboardMessage(groupedActivitiesByParticipant);

            await message.Channel.SendMessageAsync($"{leaderboardHeadline}\n{leaderboardMessage}").ConfigureAwait(false);
        }
        private string FormatActivitiesIntoLeaderboardMessage(Dictionary<LeaderboardParticipant, List<DetailedActivity>> activities) 
        {
            var builder = new StringBuilder();

            builder.AppendLine(FormatActivitiesForType(activities, REAL_RIDE_NAME));
            builder.AppendLine();
            builder.AppendLine(FormatActivitiesForType(activities, VIRTUAL_RIDE_NAME));

            return builder.ToString();
        }

        private string FormatActivitiesForType(Dictionary<LeaderboardParticipant, List<DetailedActivity>> participantActivitiesDict, string type)
        {
            _logger.LogInformation($"Generating leaderboard for '{type}' category");
            var categoryResult = GetTopResultsForCategory(participantActivitiesDict, x => x.Type == type);
            var builder = new StringBuilder();

            builder.AppendLine($"**'{type}' category results**:");
            builder.AppendLine();

            builder.AppendLine("*Distance*\n");
            var index = 0;
            foreach(var distanceResult in categoryResult.Distance.Take(3)) {
                builder.AppendLine(GetParticipantPlaceString(index + 1, distanceResult.Participant.GetDiscordMention(_silent), distanceResult.Value / 1000, "km"));
            }
            builder.AppendLine();

            builder.AppendLine("*Altitude*\n");
            index = 0;
            foreach (var altitudeResult in categoryResult.Altitude.Take(3))
            {
                builder.AppendLine(GetParticipantPlaceString(index + 1, altitudeResult.Participant.GetDiscordMention(_silent), altitudeResult.Value, "m"));
            }
            builder.AppendLine();

            builder.AppendLine("*Highest weighted power ride* (only rides longer than 20 minutes are considered)\n");
            index = 0;
            foreach (var powerResult in categoryResult.Power.Take(3))
            {
                builder.AppendLine(GetParticipantPlaceString(index + 1, powerResult.Participant.GetDiscordMention(_silent), powerResult.Value, "W"));
            }

            return builder.ToString();
        }

        private static string GetParticipantPlaceString(int index, string participant, double value, string unit)
        {
            return $"{index}. {participant} @ {(value):n1} {unit}";
        }

        private CategoryResult GetTopResultsForCategory(Dictionary<LeaderboardParticipant, List<DetailedActivity>> participantActivitiesDict, Func<DetailedActivity, bool> activityFilter)
        {
            _logger.LogInformation($"Calculating distances for {participantActivitiesDict.Keys.Count} participants.");
            var distanceResult = new List<ParticipantResult>();
            var altitudeResult = new List<ParticipantResult>();
            var powerResult = new List<ParticipantResult>();

            foreach (var participantActivityPair in participantActivitiesDict)
            {
                var matchingActivities = participantActivityPair.Value.Where(activityFilter);
                _logger.LogInformation($"Activities before filter: {participantActivityPair.Value.Count} , after: {matchingActivities.Count()}");
                distanceResult.Add(new ParticipantResult(participantActivityPair.Key, matchingActivities.Sum(x => x.Distance)));
                altitudeResult.Add(new ParticipantResult(participantActivityPair.Key, matchingActivities.Sum(x => x.TotalElevationGain)));
                _logger.LogInformation($"Elapsed times {string.Join(',', participantActivityPair.Value.Select(x => x.ElapsedTime))}");
                _logger.LogInformation($"Powers {string.Join(',', participantActivityPair.Value.Select(x => x.WeightedAverageWatts))}");

                powerResult.Add(new ParticipantResult(participantActivityPair.Key, matchingActivities
                                                                                        .Where(x => x.ElapsedTime > 20 * 60)
                                                                                        .Select(x => x.WeightedAverageWatts)
                                                                                        .DefaultIfEmpty()
                                                                                        .Max()));
            }

            _logger.LogInformation($"Total distance results {distanceResult.Count}");
            _logger.LogInformation($"Total altitude results {altitudeResult.Count}");
            _logger.LogInformation($"Total power results {powerResult.Count}");

            return new CategoryResult
            {
                Distance = distanceResult.OrderByDescending(x => x.Value).ToList(),
                Altitude = altitudeResult.OrderByDescending(x => x.Value).ToList(),
                Power = powerResult.OrderByDescending(x => x.Value).ToList()
            };
        }
    }
}
