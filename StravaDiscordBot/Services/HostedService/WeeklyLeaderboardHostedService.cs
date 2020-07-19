using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Extensions;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Categories;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.HostedService
{
    public class WeeklyLeaderboardHostedService : CronHostedServiceBase
    {
        private const string JobCronExpression = "0 8 * * 1";
        private readonly BotDbContext _dbContext;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly ILeaderboardService _leaderboardResultService;

        private readonly ILogger<WeeklyLeaderboardHostedService> _logger;
        private readonly ILeaderboardParticipantService _participantService;
        private readonly IRoleService _roleService;
        private readonly IStravaAuthenticationService _stravaAuthenticationService;
        private readonly IActivitiesService _activitiesService;
        private readonly ILeaderboardService _leaderboardService;

        public WeeklyLeaderboardHostedService(ILogger<WeeklyLeaderboardHostedService> logger,
            DiscordSocketClient discordClient,
            IEmbedBuilderService embedBuilderService,
            ILeaderboardParticipantService participantService,
            ILeaderboardService leaderboardResultService,
            IRoleService roleService,
            BotDbContext dbContext,
            IStravaAuthenticationService stravaAuthenticationService,
            IActivitiesService activitiesService,
            ILeaderboardService leaderboardService) : base(JobCronExpression, TimeZoneInfo.Utc)
        {
            _logger = logger;
            _discordSocketClient = discordClient;
            _embedBuilderService = embedBuilderService;
            _participantService = participantService;
            _leaderboardResultService = leaderboardResultService;
            _roleService = roleService;
            _dbContext = dbContext;
            _stravaAuthenticationService = stravaAuthenticationService;
            _activitiesService = activitiesService;
            _leaderboardService = leaderboardService;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing leaderboard hosted service");
            var start = DateTime.Now.AddDays(-7);

            foreach (var leaderboard in _dbContext.Leaderboards.ToList())
            {
                await RemoveRolesFromAllInLeaderboard(leaderboard);
                var participantsWithActivities = new List<ParticipantWithActivities>();
                var participants = _participantService.GetAllParticipantsForServerAsync(leaderboard.ServerId);

                foreach (var participant in participants)
                {
                    try
                    {
                        var (policy, context) = _stravaAuthenticationService.GetUnauthorizedPolicy(participant.StravaId);

                        var activities = await policy.ExecuteAsync(x => _activitiesService.GetForStravaUser(participant.StravaId, start), context);
                        participantsWithActivities.Add(new ParticipantWithActivities
                        {
                            Participant = participant,
                            Activities = activities
                        });
                    }
                    catch(Exception e)
                    {
                        _logger.LogError(e, $"Failed to fetch activities for participant {participant.DiscordUserId}");
                    }
                }

                var channel = _discordSocketClient.GetChannel(ulong.Parse(leaderboard.ChannelId)) as SocketTextChannel;

                var realRideResult = _leaderboardService.GetTopResultsForCategory(participantsWithActivities, new RealRideCategory());
                await channel.SendMessageAsync(embed: _embedBuilderService
                    .BuildLeaderboardEmbed(
                        realRideResult,
                        start,
                        DateTime.Now
                    )
                );

                var virtualRideResult = _leaderboardService.GetTopResultsForCategory(participantsWithActivities, new VirtualRideCategory());
                await channel.SendMessageAsync(embed: _embedBuilderService
                   .BuildLeaderboardEmbed(
                       virtualRideResult,
                       start,
                       DateTime.Now
                   )
                );

                var allParticipantResults =
                   realRideResult.SubCategoryResults.SelectMany(x => x.OrderedParticipantResults).Union(
                       virtualRideResult.SubCategoryResults.SelectMany(x => x.OrderedParticipantResults))
                   .ToList();

                await GrantWinnerRoles(leaderboard, allParticipantResults);
            }
        }

        private async Task AskToRelogin(string discordId)
        {
            _logger.LogInformation("Sending refresh notification");
            var channel = await _discordSocketClient.GetDMChannelAsync(ulong.Parse(discordId));
            await channel.SendMessageAsync(
                "Hey, I failed refreshing access to your Strava account. Please use `join` command again in the server of leaderboard.");
        }

        private async Task RemoveRolesFromAllInLeaderboard(Leaderboard leaderboard)
        {
            try
            {
                _logger.LogInformation("Removing role from all users");
                await _roleService.RemoveRoleFromAllInServer(leaderboard.ServerId,
                    Constants.LeaderboardWinnerRoleName);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    $"Failed to remove leaderboard role from all users in '{leaderboard.ServerId}'");
            }
        }

        //TODO: Refactor to be reused in here and leaderboard module
        private async Task GrantWinnerRoles(Leaderboard leaderboard, List<ParticipantResult> leaderboardResults)
        {
            foreach (var participantResult in leaderboardResults)
            {
                try
                {
                    await _roleService.GrantUserRole(leaderboard.ServerId,
                        participantResult.Participant.DiscordUserId, Constants.LeaderboardWinnerRoleName);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        $"Failed to grant leaderboard role for '{participantResult.Participant.DiscordUserId}'");
                }
            }
        }
    }
}