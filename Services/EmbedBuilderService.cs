using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord
{
    public interface IEmbedBuilderService
    {
        Embed BuildLeaderboardEmbed(CategoryResult categoryResult, DateTime start, DateTime end);

        Embed BuildParticipantStatsForCategoryEmbed(LeaderboardParticipant participant,
            List<DetailedActivity> activities,
            string type, DateTime start, DateTime end);

        Embed BuildAthleteInfoEmbed(LeaderboardParticipant participant, AthleteDetailed athlete);
        List<Embed> BuildDetailedAthleteEmbeds(LeaderboardParticipant participant, AthleteDetailed athlete);
        Embed BuildSimpleEmbed(string title, string description);
    }

    public class EmbedBuilderService : IEmbedBuilderService
    {
        private readonly ILeaderboardResultService _leaderboardResultService;

        public EmbedBuilderService(ILeaderboardResultService leaderboardResultService)
        {
            _leaderboardResultService = leaderboardResultService;
        }

        public Embed BuildLeaderboardEmbed(CategoryResult categoryResult, DateTime start, DateTime end)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle(
                    $"'{categoryResult.Name}' leaderboard for '{start:yyyy MMMM dd} - {end:yyyy MMMM dd}'")
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
            var categoryResult = _leaderboardResultService.GetTopResultsForCategory(
                new Dictionary<LeaderboardParticipant, List<DetailedActivity>> {{participant, activities}},
                type,
                x => x.Type == type);

            var embedBuilder = new EmbedBuilder()
                .WithTitle(
                    $"'{type}' stats for '{start:yyyy MMMM dd} - {end:yyyy MMMM dd}'")
                .WithCurrentTimestamp()
                .WithColor(Color.Gold);

            if (!categoryResult.ChallengeByChallengeResultDictionary.Any())
            {
                embedBuilder.WithDescription("Something went wrong");
                return embedBuilder.Build();
            }

            foreach (var (categoryName, participantResults) in categoryResult.ChallengeByChallengeResultDictionary)
            foreach (var participantResult in participantResults)
                embedBuilder.AddField(categoryName,
                    OutputFormatters.ParticipantResultForChallenge(categoryName, participantResult.Value), true);

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
    }
}