using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord.Utilities;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Categories;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord.Modules
{
    public class LeaderboardModule : ModuleBase<SocketCommandContext>
    {
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly IStravaAuthenticationService _stravaAuthenticationService;
        private readonly IActivitiesService _activitiesService;
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILeaderboardService _leaderboardResultService;
        private readonly ILogger<LeaderboardModule> _logger;
        private readonly ILeaderboardParticipantService _participantService;

        public LeaderboardModule(ILogger<LeaderboardModule> logger,
            ILeaderboardParticipantService participantService,
            ILeaderboardService leaderboardResultService,
            IEmbedBuilderService embedBuilderService,
            IStravaAuthenticationService stravaAuthenticationService,
            IActivitiesService activitiesService,
            ILeaderboardService leaderboardService)
        {
            _logger = logger;
            _participantService = participantService;
            _leaderboardResultService = leaderboardResultService;
            _embedBuilderService = embedBuilderService;
            _stravaAuthenticationService = stravaAuthenticationService;
            _activitiesService = activitiesService;
            _leaderboardService = leaderboardService;
        }

        [Command("join")]
        [Summary("Join leaderboard")]
        public async Task JoinLeaderboard()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var text = $"Hey, {Context.User.Mention} ! Please go to the following url to authorize me to view your Strava activities: {_stravaAuthenticationService.GetOAuthUrl(Context.Guild.Id.ToString(), Context.User.Id.ToString())}";

                    var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                    await dmChannel.SendMessageAsync(text);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Join failed for {Context.User.Id}");
                }
            }
        }


        [Command("leaderboard")]
        [Summary("[ADMIN] Manually triggers leaderboard in channel written")]
        [RequireToBeWhitelistedServer]
        [RequireRole(new[] { "Owner", "Bot Manager" })]
        public async Task ShowLeaderboard()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    _logger.LogInformation("Executing leaderboard command");
                    var start = DateTime.Now.AddDays(-7);
                    var participantsWithActivities = new List<ParticipantWithActivities>();
                    var participants = _participantService.GetAllParticipantsForServerAsync(Context.Guild.Id.ToString());

                    foreach (var participant in participants)
                    {
                        var (policy, context) = _stravaAuthenticationService.GetUnauthorizedPolicy(participant.StravaId);

                        var activities = await policy.ExecuteAsync(x => _activitiesService.GetForStravaUser(participant.StravaId, start), context);
                        participantsWithActivities.Add(new ParticipantWithActivities
                        {
                            Participant = participant,
                            Activities = activities
                        });
                    }

                    var realRideResult = _leaderboardService.GetTopResultsForCategory(participantsWithActivities, new RealRideCategory());
                    await ReplyAsync(embed: _embedBuilderService
                        .BuildLeaderboardEmbed(
                            realRideResult,
                            start,
                            DateTime.Now
                        )
                    );

                    var virtualRideResult = _leaderboardService.GetTopResultsForCategory(participantsWithActivities, new VirtualRideCategory());
                    await ReplyAsync(embed: _embedBuilderService
                       .BuildLeaderboardEmbed(
                           virtualRideResult,
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