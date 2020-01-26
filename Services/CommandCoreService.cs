using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Services.Commands;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services
{
    public interface ICommandCoreService
    {
        string GenerateHelpCommandContent(List<ICommand> commands);
        Task<string> GenerateJoinCommandContent(ulong serverId, ulong channelId, ulong userId, string username);
        Task<List<Embed>> GenerateLeaderboardCommandContent(ulong serverId);
    }

    public class CommandCoreService : ICommandCoreService
    {
        private readonly ILogger<CommandCoreService> _logger;
        private readonly BotDbContext _context;
        private readonly IStravaService _stravaService;

        public CommandCoreService(ILogger<CommandCoreService> logger, BotDbContext context, IStravaService stravaService)
        {
            _logger = logger;
            _context = context;
            _stravaService = stravaService;
        }

        public string GenerateHelpCommandContent(List<ICommand> commands)
        {
            var builder = new StringBuilder();
            builder.AppendLine("This is a Discord Strava Leaderboard Bot.");
            builder.AppendLine("Commands:");
            foreach (var command in commands)
            {
                builder.AppendLine($"**{command.CommandName}** - {command.CommandName}");
            }
            return builder.ToString();
        }

        public async Task<string> GenerateJoinCommandContent(ulong serverId, ulong channelId, ulong userId, string username)
        {
            if (await _stravaService.DoesParticipantAlreadyExistsAsync(serverId.ToString(), userId.ToString()).ConfigureAwait(false))
                throw new InvalidCommandArgumentException("Whoops, it seems like you're already participating in the leaderboard");

            return $"Hey, {username} ! Please go to this url to allow me check out your Strava activities: {_stravaService.GetOAuthUrl(serverId.ToString(), channelId.ToString(), userId.ToString())}"   ;
        }

        public async Task<List<Embed>> GenerateLeaderboardCommandContent(ulong serverId)
        {
            var start = DateTime.Now.AddDays(-7);
            var groupedActivitiesByParticipant = await _stravaService.GetActivitiesSinceStartDate(serverId.ToString(), start).ConfigureAwait(false);

            return new List<Embed>
            {
                BuildLeaderboardEmbedMessage(groupedActivitiesByParticipant, Constants.LeaderboardRideType.RealRide, start, DateTime.Now),
                BuildLeaderboardEmbedMessage(groupedActivitiesByParticipant, Constants.LeaderboardRideType.VirtualRide, start, DateTime.Now)
            };
        }

        private Embed BuildLeaderboardEmbedMessage(Dictionary<LeaderboardParticipant, List<DetailedActivity>> groupedActivitiesByParticipant, string type, DateTime start, DateTime end)
        {
            var categoryResult = GetTopResultsForCategory(groupedActivitiesByParticipant, x => x.Type == type);
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"'{type}' leaderboard for '{ start.ToString("yyyy MMMM dd")} - { end.ToString("yyyy MMMM dd")}'")
                .WithCurrentTimestamp()
                .WithColor(Color.Green);


            foreach (var challengeResult in categoryResult.ChallengeByChallengeResultDictionary)
            {
                embedBuilder.AddField(efb => efb.WithName("Category")
                                                .WithValue($"{challengeResult.Key}")
                                                .WithIsInline(false));
                var place = 1;
                foreach (var participantResult in challengeResult.Value.Take(3))
                {
                    embedBuilder.AddField(efb => efb.WithValue(participantResult.Participant.GetDiscordMention())
                                                    .WithName($"{OutputFormatters.PlaceToEmote(place)} - {OutputFormatters.ParticipantResultForChallenge(challengeResult.Key, participantResult.Value)}")
                                                    .WithIsInline(true));
                    place++;
                }
            }
            return embedBuilder.Build();
        }

        private CategoryResult GetTopResultsForCategory(Dictionary<LeaderboardParticipant, List<DetailedActivity>> participantActivitiesDict, Func<DetailedActivity, bool> activityFilter)
        {
            var distanceResult = new List<ParticipantResult>();
            var altitudeResult = new List<ParticipantResult>();
            var powerResult = new List<ParticipantResult>();

            foreach (var participantActivityPair in participantActivitiesDict)
            {
                var matchingActivities = participantActivityPair.Value.Where(activityFilter);
                distanceResult.Add(new ParticipantResult(participantActivityPair.Key, matchingActivities.Sum(x => x.Distance / 1000))); // meters to km 
                altitudeResult.Add(new ParticipantResult(participantActivityPair.Key, matchingActivities.Sum(x => x.TotalElevationGain)));
                powerResult.Add(new ParticipantResult(participantActivityPair.Key, matchingActivities
                                                                                        .Where(x => x.ElapsedTime > 20 * 60)
                                                                                        .Select(x => x.WeightedAverageWatts)
                                                                                        .DefaultIfEmpty()
                                                                                        .Max()));
            }

            return new CategoryResult
            {
                ChallengeByChallengeResultDictionary = new Dictionary<string, List<ParticipantResult>>
                {
                    { Constants.ChallengeType.Distance,  distanceResult.OrderByDescending(x => x.Value).ToList() },
                    { Constants.ChallengeType.Elevation, altitudeResult.OrderByDescending(x => x.Value).ToList() },
                    { Constants.ChallengeType.Power, powerResult.OrderByDescending(x => x.Value).ToList() }
                }
            };
        }
    }
}
