﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord.Utilities;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Categories;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord.Modules
{
    [RequireToBeWhitelistedServer]
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly ILogger<PublicModule> _logger;
        private readonly ILeaderboardParticipantService _participantService;
        private readonly IStravaAuthenticationService _stravaAuthenticationService;
        private readonly IActivitiesService _activityService;
        private readonly ILeaderboardService _leaderboardService;

        public PublicModule(ILogger<PublicModule> logger,
            CommandService commandService,
            ILeaderboardParticipantService participantService,
            IEmbedBuilderService embedBuilderService,
            IStravaAuthenticationService stravaAuthenticationService,
            IActivitiesService activityService,
            ILeaderboardService leaderboardService)
        {
            _logger = logger;
            _commandService = commandService;
            _participantService = participantService;
            _embedBuilderService = embedBuilderService;
            _stravaAuthenticationService = stravaAuthenticationService;
            _activityService = activityService;
            _leaderboardService = leaderboardService;
        }

        [Command("help")]
        [Summary("Lists available commands")]
        public async Task Help()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var commands = _commandService.Commands.ToList();
                    var embedBuilder = new EmbedBuilder();

                    foreach (var command in commands)
                    {
                        var embedFieldText = command.Summary ?? "No description available\n";
                        embedBuilder.AddField(command.Name, embedFieldText);
                    }

                    await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Help failed");
                }
            }
        }

        [Command("stats")]
        [Summary("Show your weekly stats")]
        public async Task ShowParticipantStats()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var participant = _participantService.GetParticipantOrDefault(Context.Guild.Id.ToString(), Context.User.Id.ToString());
                    if (participant == null)
                    {
                        await ReplyAsync("It seems like you're not part of the leaderboard. Try joining it.");
                        return;
                    }

                    var start = DateTime.Now.AddDays(-7);

                    var (policy, context) = _stravaAuthenticationService.GetUnauthorizedPolicy(participant.StravaId);
                    var activities = await policy.ExecuteAsync(x => _activityService.GetForStravaUser(participant.StravaId, start), context);

                    var participantWithActivities = new ParticipantWithActivities { Participant = participant, Activities = activities };

                    var realRideCategoryResult = _leaderboardService.GetTopResultsForCategory(
                        new List<ParticipantWithActivities> { participantWithActivities },
                        new RealRideCategory());

                    await ReplyAsync(embed: _embedBuilderService.BuildParticipantStatsForCategoryEmbed(realRideCategoryResult,
                        $"'{new RealRideCategory().Name}' leaderboard for '{start:yyyy MMMM dd} - {DateTime.Now:yyyy MMMM dd}'"));

                    var virtualRideCategoryResult = _leaderboardService.GetTopResultsForCategory(
                       new List<ParticipantWithActivities> { participantWithActivities },
                       new VirtualRideCategory());

                    await ReplyAsync(embed: _embedBuilderService.BuildParticipantStatsForCategoryEmbed(virtualRideCategoryResult,
                        $"'{new VirtualRideCategory().Name}' leaderboard for '{start:yyyy MMMM dd} - {DateTime.Now:yyyy MMMM dd}'"));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "stats failed");
                }
            }
        }
    }
}