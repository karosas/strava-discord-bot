using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StravaDiscordBot.Discord.Utilities;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord.Modules
{
    [RequireRole(new[] {"Owner", "Bot Manager"})]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly ICommandCoreService _commandCoreService;
        private readonly IStravaService _stravaService;
        private readonly ILogger<AdminModule> _logger;
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly ILeaderboardParticipantService _participantService;

        public AdminModule(ICommandCoreService commandCoreService,
            ILogger<AdminModule> logger,
            IStravaService stravaService,
            IEmbedBuilderService embedBuilderService,
            ILeaderboardParticipantService participantService)
        {
            _commandCoreService = commandCoreService;
            _logger = logger;
            _stravaService = stravaService;
            _embedBuilderService = embedBuilderService;
            _participantService = participantService;
        }

        [Command("init")]
        [Summary("[ADMIN] Sets up channel command is written as destined leaderboard channel for the server")]
        public async Task InitializeLeaderboard()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    _logger.LogInformation("Executing init");
                    if (Context.Guild?.Id == null)
                    {
                        await ReplyAsync("Doesn't seem like this is written inside a server.");
                        return;
                    }

                    await ReplyAsync(
                        await _commandCoreService.GenerateInitializeCommandContext(Context.Guild.Id,
                            Context.Channel.Id));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "init failed");
                }
            }
        }

        [Command("leaderboard")]
        [Summary("[ADMIN] Manually triggers leaderboard in channel written")]
        [RequireToBeWhitelistedServer]
        public async Task ShowLeaderboard()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    _logger.LogInformation("Executing leaderboard command");
                    var start = DateTime.Now.AddDays(-7);
                    var groupedActivitiesByParticipant =
                        new Dictionary<LeaderboardParticipant, List<DetailedActivity>>();
                    var participants =
                        await _participantService.GetAllParticipantsForServerAsync(Context.Guild.Id.ToString());
                    foreach (var participant in participants)
                    {
                        try
                        {
                            groupedActivitiesByParticipant.Add(participant,
                                await _stravaService.FetchActivitiesForParticipant(participant,
                                    start));
                        }
                        catch (StravaException e) when (e.Error == StravaException.StravaErrorType.RefreshFailed)
                        {
                            await AskToRelogin(participant.DiscordUserId);
                        }
                    }

                    await ReplyAsync(embed: _embedBuilderService
                        .BuildLeaderboardEmbed(
                            groupedActivitiesByParticipant,
                            Constants.LeaderboardRideType.RealRide,
                            start,
                            DateTime.Now
                        )
                    );
                    
                    await ReplyAsync(embed: _embedBuilderService
                        .BuildLeaderboardEmbed(
                            groupedActivitiesByParticipant,
                            Constants.LeaderboardRideType.VirtualRide,
                            start,
                            DateTime.Now
                        )
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "leaderboard failed");
                }
            }
        }

        [Command("list")]
        [Summary("[ADMIN] Lists participants for server")]
        [RequireToBeWhitelistedServer]
        public async Task ListLeaderboardParticipants()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    _logger.LogInformation("Executing list");
                    var embeds = new List<Embed>();
                    var participants = await _participantService.GetAllParticipantsForServerAsync(Context.Guild.Id.ToString());
                    foreach (var participant in participants)
                    {
                        AthleteDetailed updatedAthleteData = null;
                        try
                        {
                            updatedAthleteData = await _stravaService.GetAthlete(participant);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, $"Failed to fetch athlete while executing list command");
                        }

                        embeds.Add(_embedBuilderService.BuildAthleteInfoEmbed(participant, updatedAthleteData));
                    }

                    if (!embeds.Any())
                    {
                        embeds.Add(
                            new EmbedBuilder()
                                .WithTitle("No participants found")
                                .WithCurrentTimestamp()
                                .Build()
                        );
                    }

                    foreach (var embed in embeds)
                    {
                        await ReplyAsync(embed: embed);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "list failed");
                }
            }
        }

        [Command("get")]
        [Summary("[ADMIN] Get detailed information of the participant")]
        [RequireToBeWhitelistedServer]
        public async Task GetDetailedParticipant(string discordId)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    _logger.LogInformation($"Executing get {discordId}");

                    var participant =
                        await _participantService.GetParticipantOrDefault(Context.Guild.Id.ToString(), discordId);
                    if (participant == null)
                    {
                        await ReplyAsync(embed: _embedBuilderService.BuildSimpleEmbed(
                            "Not Found",
                            "Couldn't find participant with this discord id")
                        );
                        return;
                    }

                    var athlete = await _stravaService.GetAthlete(participant);
                    foreach (var embed in _embedBuilderService.BuildDetailedAthleteEmbeds(participant, athlete))
                    {
                        await ReplyAsync(embed: embed);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "list failed");
                }
            }
        }

        [Command("remove")]
        [Summary("[ADMIN] Remove user from leaderboard by discord Id. Usage: `@mention remove 1234`")]
        [RequireToBeWhitelistedServer]
        public async Task RemoveParticipant(string discordId)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    await ReplyAsync(
                        await _commandCoreService.GenerateRemoveParticipantContent(discordId, Context.Guild.Id));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "list failed");
                }
            }
        }

        private async Task AskToRelogin(string discordId)
        {
            _logger.LogInformation($"Sending refresh notification to {discordId}");
            try
            {
                var user = Context.Client.GetUser(ulong.Parse(discordId));
                await user.SendMessageAsync(
                    $"Hey, I failed refreshing access to your Strava account. Please use `join` command again in the server of leaderboard.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to deliver relogin message");
            }
        }
    }
}