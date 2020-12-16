using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using IO.Swagger.Model;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord
{
    public interface IEmbedBuilderService
    {
        Embed BuildLeaderboardEmbed(CategoryResult categoryResult, DateTime start, DateTime end);
        Embed BuildParticipantStatsForCategoryEmbed(CategoryResult categoryResult, string title);
        Embed BuildAthleteInfoEmbed(LeaderboardParticipant participant, DetailedAthlete athlete);
        IEnumerable<Embed> BuildDetailedAthleteEmbeds(LeaderboardParticipant participant, DetailedAthlete athlete);
        Embed BuildSimpleEmbed(string title, string description);
    }

    public class EmbedBuilderService : IEmbedBuilderService
    {
        private readonly ILogger<EmbedBuilderService> _logger;
        public static readonly string[] StravaPropertyBlacklist = {
            "Lastname",
            "City",
            "UpdatedAt",
            "Clubs",
            "Bikes",
            "Shoes"
        };

        public EmbedBuilderService(ILogger<EmbedBuilderService> logger)
        {
            _logger = logger;
        }

        public Embed BuildLeaderboardEmbed(CategoryResult categoryResult, DateTime start, DateTime end)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle(
                    $"'{categoryResult.Name}' leaderboard for '{start:yyyy MMMM dd} - {end:yyyy MMMM dd}'")
                .WithCurrentTimestamp()
                .WithColor(Color.Green);

            foreach (var subCategoryResult in categoryResult.SubCategoryResults)
            {
                embedBuilder.AddField(efb => efb.WithName("Category")
                .WithValue(subCategoryResult.Name)
                .WithIsInline(false));


                var place = 1;
                foreach (var participantResult in subCategoryResult.OrderedParticipantResults)
                {
                    embedBuilder.AddField(efb => efb.WithValue(participantResult.Participant.GetDiscordMention())
                        .WithName(
                            $"{OutputFormatters.PlaceToEmote(place)} - {participantResult.DisplayValue}")
                        .WithIsInline(true));
                    place++;
                    if (place > 3)
                        break;
                }
            }

            return embedBuilder.Build();
        }

        public Embed BuildParticipantStatsForCategoryEmbed(CategoryResult categoryResult, string title)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithCurrentTimestamp()
                .WithColor(Color.Gold);

            if (!categoryResult.SubCategoryResults.Any())
            {
                embedBuilder.WithDescription("Something went wrong");
                return embedBuilder.Build();
            }

            foreach (var subCategory in categoryResult.SubCategoryResults)
                foreach (var participantResult in subCategory.OrderedParticipantResults)
                    embedBuilder.AddField(subCategory.Name, participantResult.DisplayValue, true);

            return embedBuilder.Build();
        }

        public IEnumerable<Embed> BuildDetailedAthleteEmbeds(LeaderboardParticipant participant,
            DetailedAthlete athlete)
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
                if (string.IsNullOrEmpty(propertyInfo.Name) || StravaPropertyBlacklist.Contains(propertyInfo.Name))
                    continue;

                var value = propertyInfo.GetValue(athlete)?.ToString();
                if (string.IsNullOrEmpty(value))
                    continue;

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

        public Embed BuildAthleteInfoEmbed(LeaderboardParticipant participant, DetailedAthlete athlete)
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