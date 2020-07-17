using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord.Utilities;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord.Modules
{
    public class LeaderboardModule : ModuleBase<SocketCommandContext>
    {
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly ILeaderboardResultService _leaderboardResultService;
        private readonly ILogger<LeaderboardModule> _logger;
        private readonly ILeaderboardParticipantService _participantService;
        private readonly IStravaService _stravaService;

        public LeaderboardModule(ILogger<LeaderboardModule> logger,
            ILeaderboardParticipantService participantService,
            IStravaService stravaService,
            ILeaderboardResultService leaderboardResultService,
            IEmbedBuilderService embedBuilderService)
        {
            _logger = logger;
            _participantService = participantService;
            _stravaService = stravaService;
            _leaderboardResultService = leaderboardResultService;
            _embedBuilderService = embedBuilderService;
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

                    var realRideCategoryResult = _leaderboardResultService.GetTopResultsForCategory(
                        groupedActivitiesByParticipant, Constants.LeaderboardRideType.RealRide,
                        x => x.Type == Constants.LeaderboardRideType.RealRide);
                    await ReplyAsync(embed: _embedBuilderService
                        .BuildLeaderboardEmbed(
                            realRideCategoryResult,
                            start,
                            DateTime.Now
                        )
                    );

                    var virtualRideCategoryResult = _leaderboardResultService.GetTopResultsForCategory(
                        groupedActivitiesByParticipant, Constants.LeaderboardRideType.VirtualRide,
                        x => x.Type == Constants.LeaderboardRideType.VirtualRide);
                    await ReplyAsync(embed: _embedBuilderService
                        .BuildLeaderboardEmbed(
                            virtualRideCategoryResult,
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
    }
}