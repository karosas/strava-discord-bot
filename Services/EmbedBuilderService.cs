using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using StravaDiscordBot.Extensions;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;

namespace StravaDiscordBot.Discord
{
    public interface IEmbedBuilderService
    {
        Embed BuildLeaderboardEmbed(
            Dictionary<LeaderboardParticipant, List<DetailedActivity>> groupedActivitiesByParticipant, string type,
            DateTime start, DateTime end);

        Embed BuildParticipantStatsForCategoryEmbed(LeaderboardParticipant participant, List<DetailedActivity> activities,
            string type, DateTime start, DateTime end);

        Embed BuildAthleteInfoEmbed(LeaderboardParticipant participant, AthleteDetailed athlete);
        List<Embed> BuildDetailedAthleteEmbeds(LeaderboardParticipant participant, AthleteDetailed athlete);
        Embed BuildSimpleEmbed(string title, string description);
    }

    public class EmbedBuilderService : IEmbedBuilderService
    {
        public Embed BuildLeaderboardEmbed(
            Dictionary<LeaderboardParticipant, List<DetailedActivity>> groupedActivitiesByParticipant, string type,
            DateTime start, DateTime end)
        {
            var categoryResult = GetTopResultsForCategory(groupedActivitiesByParticipant, x => x.Type == type);
            var embedBuilder = new EmbedBuilder()
                .WithTitle(
                    $"'{type}' leaderboard for '{start:yyyy MMMM dd} - {end:yyyy MMMM dd}'")
                .WithCurrentTimestamp()
                .WithColor(Color.Green);


            foreach (var (participant, participantResults) in categoryResult.ChallengeByChallengeResultDictionary)
            {
                embedBuilder.AddField(efb => efb.WithName("Category")
                    .WithValue($"{participant}")
                    .WithIsInline(false));
                var place = 1;
                foreach (var participantResult in participantResults)
                {
                    embedBuilder.AddField(efb => efb.WithValue(participantResult.Participant.GetDiscordMention())
                        .WithName(
                            $"{OutputFormatters.PlaceToEmote(place)} - {OutputFormatters.ParticipantResultForChallenge(participant, participantResult.Value)}")
                        .WithIsInline(true));
                    place++;
                    if (place > 3)
                        break;
                }
            }

            return embedBuilder.Build();
        }

        public Embed BuildParticipantStatsForCategoryEmbed(LeaderboardParticipant participant,
            List<DetailedActivity> activities, string type, DateTime start,
            DateTime end)
        {
            var categoryResult = GetTopResultsForCategory(
                new Dictionary<LeaderboardParticipant, List<DetailedActivity>> {{participant, activities}},
                x => x.Type == type);

            var embedBuilder = new EmbedBuilder()
                .WithTitle(
                    $"'{type}' stats for '{start:yyyy MMMM dd} - {end:yyyy MMMM dd}'")
                .WithCurrentTimestamp()
                .WithColor(Color.Gold);

            var participantResults = categoryResult.ChallengeByChallengeResultDictionary.FirstOrDefault();
            if (participantResults.Value == null)
            {
                embedBuilder.WithDescription("Something went wrong");
                return embedBuilder.Build();
            }

            embedBuilder.AddField("Category", participantResults.Key, true);
            foreach (var participantResult in participantResults.Value)
            {
                embedBuilder.AddField(efb => efb.WithValue(participantResult.Participant.GetDiscordMention())
                    .WithName(
                        $"{OutputFormatters.ParticipantResultForChallenge(participantResults.Key, participantResult.Value)}")
                    .WithIsInline(true));
            }

            return embedBuilder.Build();
        }

        public List<Embed> BuildDetailedAthleteEmbeds(LeaderboardParticipant participant,
            AthleteDetailed athlete)
        {
            var results = new List<Embed>();

            var embedBuilder = new EmbedBuilder()
                .WithCurrentTimestamp();

            if (participant == null)
            {
                results.Add(embedBuilder
                    .WithTitle("Participant Not Found")
                    .Build());
                return results;
            }

            embedBuilder.WithTitle($"Detailed Info - {participant.DiscordUserId}");
            var embedFieldsAdded = 0;
            foreach (var propertyInfo in athlete.GetType().GetProperties())
            {
                if (string.IsNullOrEmpty(propertyInfo.Name)) continue;

                var value = propertyInfo.GetValue(athlete)?.ToString();
                if (string.IsNullOrEmpty(value)) continue;

                embedBuilder
                    .AddField(efb => efb.WithName(propertyInfo.Name ?? "N/A")
                        .WithValue(value)
                        .WithIsInline(true));
                embedFieldsAdded++;
            }

            if (embedFieldsAdded >= 25)
            {
                results.Add(embedBuilder.Build());
                embedBuilder = new EmbedBuilder().WithCurrentTimestamp()
                    .WithTitle($"Detailed Info - {participant.DiscordUserId} - CONTINUED");
            }

            if (embedBuilder.Fields.Count > 0)
                results.Add(embedBuilder.Build());

            return results;
        }

        public Embed BuildSimpleEmbed(string title, string description)
        {
            return new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithTitle(title)
                .WithDescription(description)
                .Build();
        }

        public Embed BuildAthleteInfoEmbed(LeaderboardParticipant participant, AthleteDetailed athlete)
        {
            var embedBuilder = new EmbedBuilder()
                .WithCurrentTimestamp()
                .AddField("Discord User Id", participant.DiscordUserId, true)
                .AddField("Strava First Name", athlete?.Firstname ?? "Unknown", true)
                .AddField("Strava Athlete Id", participant.StravaId, true)
                .AddField("Strava Body Weight",
                    (athlete.Weight ?? 0) == 0
                        ? "Not Specified"
                        : athlete.Weight.ToString(), true)
                .AddField("Strava FTP", athlete?.Ftp?.ToString() ?? "Not Specified", true);

            return embedBuilder.Build();
        }

        private static CategoryResult GetTopResultsForCategory(
            Dictionary<LeaderboardParticipant, List<DetailedActivity>> participantActivitiesDict,
            Func<DetailedActivity, bool> activityFilter)
        {
            var distanceResult = new List<ParticipantResult>();
            var altitudeResult = new List<ParticipantResult>();
            var powerResult = new List<ParticipantResult>();
            var singleLongestRideResult = new List<ParticipantResult>();

            foreach (var (participant, participantActivities) in participantActivitiesDict)
            {
                var matchingActivities = participantActivities
                    .Where(activityFilter)
                    .ToList();

                distanceResult
                    .Add(new ParticipantResult(participant,
                        matchingActivities.Sum(x => (x.Distance ?? 0d) / 1000))); // meters to km 

                altitudeResult
                    .Add(new ParticipantResult(participant, matchingActivities
                        .Sum(x => (x.TotalElevationGain ?? 0d))));

                powerResult
                    .Add(new ParticipantResult(participant, matchingActivities
                        .Where(x => (x.ElapsedTime ?? 0d) > 20 * 60)
                        .Select(x => (x.WeightedAverageWatts ?? 0))
                        .DefaultIfEmpty()
                        .Max()));

                singleLongestRideResult
                    .Add(new ParticipantResult(participant, matchingActivities
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