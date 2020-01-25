using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Helpers;
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

        public ShowLeaderboardCommand(AppOptions options, BotDbContext context, ILogger<ShowLeaderboardCommand> logger, IStravaService stravaService) : base(options, context, logger)
        {
            _stravaService = stravaService;
            _logger = logger;
        }

        public override string CommandName => "leaderboard";
        public override string Descriptions => "Print current leaderboard";

        public override async Task Execute(SocketUserMessage message, int argPos)
        {
            if (!CanExecute(message, argPos))
                throw new InvalidCommandArgumentException($"Whoops, this seems wrong, the command should be in format of `{CommandName}`");

            _logger.LogInformation($"Executing 'leaderboard' command. Full: {message.Content} | Author: {message.Author}");

            var start = DateTime.Now.AddDays(-7);
            var groupedActivitiesByParticipant = await _stravaService.GetActivitiesSinceStartDate(message.Channel.Id.ToString(), start).ConfigureAwait(false);

            await message.Channel.SendMessageAsync(embed: BuildLeaderboardEmbedMessage(groupedActivitiesByParticipant, Constants.LeaderboardRideType.RealRide, start, DateTime.Now)).ConfigureAwait(false);
            await message.Channel.SendMessageAsync(embed: BuildLeaderboardEmbedMessage(groupedActivitiesByParticipant, Constants.LeaderboardRideType.VirtualRide, start, DateTime.Now)).ConfigureAwait(false);
        }

        private Embed BuildLeaderboardEmbedMessage(Dictionary<LeaderboardParticipant, List<DetailedActivity>> groupedActivitiesByParticipant, string type, DateTime start, DateTime end)
        {
            var categoryResult = GetTopResultsForCategory(groupedActivitiesByParticipant, x => x.Type == type);
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"'{type}' leaderboard for '{ start.ToString("yyyy MMMM dd")} - { end.ToString("yyyy MMMM dd")}'")
                .WithCurrentTimestamp()
                .WithColor(Color.Green);
            

            foreach(var challengeResult in categoryResult.ChallengeByChallengeResultDictionary)
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
                distanceResult.Add(new ParticipantResult(participantActivityPair.Key, matchingActivities.Sum(x => x.Distance)));
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
