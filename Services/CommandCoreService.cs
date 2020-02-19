using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Discord
{
    public interface ICommandCoreService
    {
        Task<string> GenerateJoinCommandContent(ulong serverId, ulong userId, string username);

        Task<List<Embed>> GenerateLeaderboardCommandContent(
            Dictionary<LeaderboardParticipant, List<DetailedActivity>> groupedActivities);

        Task<string> GenerateInitializeCommandContext(ulong serverId, ulong channelId);
        Task<List<Embed>> GenerateListLeaderboardParticipantsContent(ulong serverId);
        Task<List<Embed>> GenerateGetDetailedParticipantContent(ulong serverId, string discordId);
        Task<string> GenerateRemoveParticipantContent(string discordId, ulong serverId);
    }

    public class CommandCoreService : ICommandCoreService
    {
        private readonly ILogger<CommandCoreService> _logger;
        private readonly BotDbContext _context;
        private readonly IStravaService _stravaService;

        public CommandCoreService(ILogger<CommandCoreService> logger, BotDbContext context,
            IStravaService stravaService)
        {
            _logger = logger;
            _context = context;
            _stravaService = stravaService;
        }

        public async Task<string> GenerateInitializeCommandContext(ulong serverId, ulong channelId)
        {
            if (_context.Leaderboards.Any(x => x.ServerId == serverId.ToString()))
                return "Seems like a leaderboard is already setup on this server";

            var leaderboard = new Leaderboard {ServerId = serverId.ToString(), ChannelId = channelId.ToString()};
            _context.Leaderboards.Add(leaderboard);
            await _context.SaveChangesAsync();
            return "Initialized leaderboard for this server. Users can join by using `join` command.";
        }

        public async Task<string> GenerateJoinCommandContent(ulong serverId, ulong userId, string username)
        {
            return
                $"Hey, {username} ! Please go to this url to allow me check out your Strava activities: {_stravaService.GetOAuthUrl(serverId.ToString(), userId.ToString())}";
        }

        public async Task<List<Embed>> GenerateLeaderboardCommandContent(
            Dictionary<LeaderboardParticipant, List<DetailedActivity>> groupedActivities)
        {
            var start = DateTime.Now.AddDays(-7);

            return new List<Embed>
            {
                BuildLeaderboardEmbedMessage(groupedActivities, Constants.LeaderboardRideType.RealRide, start,
                    DateTime.Now),
                BuildLeaderboardEmbedMessage(groupedActivities, Constants.LeaderboardRideType.VirtualRide, start,
                    DateTime.Now)
            };
        }

        public async Task<List<Embed>> GenerateListLeaderboardParticipantsContent(ulong serverId)
        {
            var participants = _context.Participants.Where(x => x.ServerId == serverId.ToString());
            var result = new List<Embed>();
            var expiredAthletes = 0;
            foreach (var participant in participants)
            {
                AthleteDetailed updatedAthleteData = null;
                try
                {
                    updatedAthleteData = await _stravaService.GetAthlete(participant);
                }
                catch (StravaException e) when (e.Error == StravaException.StravaErrorType.RefreshFailed)
                {
                    _logger.LogError(e, "Failed to fetch athlete");
                    expiredAthletes++;
                    continue;
                }

                // For migration
                participant.StravaId = updatedAthleteData.Id.ToString(CultureInfo.InvariantCulture);
                _context.Update(participant);
                await _context.SaveChangesAsync();

                var embedBuilder = new EmbedBuilder()
                    .WithCurrentTimestamp();

                embedBuilder
                    .AddField(efb => efb.WithName("Discord User Id")
                        .WithValue(participant.DiscordUserId)
                        .WithIsInline(false));

                embedBuilder
                    .AddField(efb => efb.WithName("Strava First Name")
                        .WithValue(updatedAthleteData.Firstname ?? "Unknown")
                        .WithIsInline(false));

                embedBuilder
                    .AddField(efb => efb.WithName("Strava Athlete Id")
                        .WithValue(participant.StravaId)
                        .WithIsInline(false));

                embedBuilder
                    .AddField(efb => efb.WithName("Strava Body Weight")
                        .WithValue((updatedAthleteData.Weight ?? 0) == 0
                            ? "Not Specified"
                            : updatedAthleteData.Weight.ToString())
                        .WithIsInline(false));

                embedBuilder
                    .AddField(efb => efb.WithName("Strava FTP")
                        .WithValue(updatedAthleteData.Ftp?.ToString() ?? "Not Specified")
                        .WithIsInline(false));

                result.Add(embedBuilder.Build());
            }

            if (result.Count == 0)
            {
                result.Add(
                    new EmbedBuilder()
                        .WithTitle("No participants found")
                        .WithCurrentTimestamp()
                        .Build()
                );
            }

            if (expiredAthletes > 0)
            {
                result.Add(
                    new EmbedBuilder()
                        .WithTitle(
                            $"List include {expiredAthletes} expired participants who need to manually refresh access.")
                        .WithCurrentTimestamp()
                        .Build()
                );
            }

            return result;
        }

        public async Task<List<Embed>> GenerateGetDetailedParticipantContent(ulong serverId, string discordId)
        {
            var results = new List<Embed>();
            var participant =
                _context.Participants.FirstOrDefault(x =>
                    x.ServerId == serverId.ToString() && x.DiscordUserId == discordId);

            var embedBuilder = new EmbedBuilder()
                .WithCurrentTimestamp();
            if (participant == null)
            {
                results.Add( embedBuilder
                    .WithTitle("Participant Not Found")
                    .Build());
                return results;
            }

            embedBuilder.WithTitle($"Detailed Info - {discordId}");
            var updatedAthleteData = await _stravaService.GetAthlete(participant);
            var embedFieldsAdded = 0;
            foreach (var propertyInfo in updatedAthleteData.GetType().GetProperties())
            {
                if (string.IsNullOrEmpty(propertyInfo.Name)) continue;
                
                var value = propertyInfo.GetValue(updatedAthleteData)?.ToString() ?? "N/A";
                if (string.IsNullOrEmpty(value)) continue;
                
                embedBuilder
                    .AddField(efb => efb.WithName(propertyInfo.Name ?? "N/A")
                        .WithValue(value)
                        .WithIsInline(false));
                embedFieldsAdded++;
            }

            if (embedFieldsAdded >= 25)
            {
                results.Add(embedBuilder.Build());
                embedBuilder = new EmbedBuilder().WithCurrentTimestamp()
                    .WithTitle($"Detailed Info - {discordId} - CONTINUED");
            }

            if (embedBuilder.Fields.Count > 0)
            {
                results.Add(embedBuilder.Build());
            }

            return results;
        }

        public async Task<string> GenerateRemoveParticipantContent(string discordId, ulong serverId)
        {
            var participant = _context.Participants.SingleOrDefault(x =>
                x.DiscordUserId == discordId && x.ServerId == serverId.ToString());
            if (participant == null)
                return $"Participant with id {discordId} wasn't found.";

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();
            return $"Participant with id {discordId} was removed.";
        }

        private Embed BuildLeaderboardEmbedMessage(
            Dictionary<LeaderboardParticipant, List<DetailedActivity>> groupedActivitiesByParticipant, string type,
            DateTime start, DateTime end)
        {
            var categoryResult = GetTopResultsForCategory(groupedActivitiesByParticipant, x => x.Type == type);
            var embedBuilder = new EmbedBuilder()
                .WithTitle(
                    $"'{type}' leaderboard for '{start.ToString("yyyy MMMM dd")} - {end.ToString("yyyy MMMM dd")}'")
                .WithCurrentTimestamp()
                .WithColor(Color.Green);


            foreach (var challengeResult in categoryResult.ChallengeByChallengeResultDictionary)
            {
                embedBuilder.AddField(efb => efb.WithName("Category")
                    .WithValue($"{challengeResult.Key}")
                    .WithIsInline(false));
                var place = 1;
                foreach (var participantResult in challengeResult.Value)
                {
                    embedBuilder.AddField(efb => efb.WithValue(participantResult.Participant.GetDiscordMention())
                        .WithName(
                            $"{OutputFormatters.PlaceToEmote(place)} - {OutputFormatters.ParticipantResultForChallenge(challengeResult.Key, participantResult.Value)}")
                        .WithIsInline(true));
                    place++;
                    if (place > 3)
                        break;
                }
            }

            return embedBuilder.Build();
        }

        private CategoryResult GetTopResultsForCategory(
            Dictionary<LeaderboardParticipant, List<DetailedActivity>> participantActivitiesDict,
            Func<DetailedActivity, bool> activityFilter)
        {
            var distanceResult = new List<ParticipantResult>();
            var altitudeResult = new List<ParticipantResult>();
            var powerResult = new List<ParticipantResult>();
            var singleLongestRideResult = new List<ParticipantResult>();

            foreach (var participantActivityPair in participantActivitiesDict)
            {
                var matchingActivities = participantActivityPair.Value.Where(activityFilter);

                distanceResult.Add(new ParticipantResult(participantActivityPair.Key,
                    matchingActivities.Sum(x => (x.Distance ?? 0d) / 1000))); // meters to km 
                altitudeResult.Add(new ParticipantResult(participantActivityPair.Key,
                    matchingActivities.Sum(x => (x.TotalElevationGain ?? 0d))));
                powerResult.Add(new ParticipantResult(participantActivityPair.Key, matchingActivities
                    .Where(x => (x.ElapsedTime ?? 0d) > 20 * 60)
                    .Select(x => (x.WeightedAverageWatts ?? 0))
                    .DefaultIfEmpty()
                    .Max()));
                singleLongestRideResult.Add(new ParticipantResult(participantActivityPair.Key, matchingActivities
                    .Select(x => (x.Distance ?? 0d) / 1000)
                    .DefaultIfEmpty()
                    .Max()));
            }

            return new CategoryResult
            {
                ChallengeByChallengeResultDictionary = new Dictionary<string, List<ParticipantResult>>
                {
                    {Constants.ChallengeType.Distance, distanceResult.OrderByDescending(x => x.Value).ToList()},
                    {Constants.ChallengeType.Elevation, altitudeResult.OrderByDescending(x => x.Value).ToList()},
                    {Constants.ChallengeType.Power, powerResult.OrderByDescending(x => x.Value).ToList()},
                    {
                        Constants.ChallengeType.DistanceRide,
                        singleLongestRideResult.OrderByDescending(x => x.Value).ToList()
                    }
                }
            };
        }
    }
}