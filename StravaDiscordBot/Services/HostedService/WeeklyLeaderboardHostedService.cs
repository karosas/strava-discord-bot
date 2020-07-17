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
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.HostedService
{
    public class WeeklyLeaderboardHostedService : CronHostedServiceBase
    {
        private const string JobCronExpression = "0 8 * * 1";
        private readonly BotDbContext _dbContext;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly ILeaderboardResultService _leaderboardResultService;

        private readonly ILogger<WeeklyLeaderboardHostedService> _logger;
        private readonly ILeaderboardParticipantService _participantService;
        private readonly IRoleService _roleService;
        private readonly IStravaService _stravaService;

        public WeeklyLeaderboardHostedService(ILogger<WeeklyLeaderboardHostedService> logger,
            DiscordSocketClient discordClient,
            IStravaService stravaService,
            IEmbedBuilderService embedBuilderService,
            ILeaderboardParticipantService participantService,
            ILeaderboardResultService leaderboardResultService, IRoleService roleService, BotDbContext dbContext)
            : base(JobCronExpression, TimeZoneInfo.Utc)
        {
            _logger = logger;
            _discordSocketClient = discordClient;
            _stravaService = stravaService;
            _embedBuilderService = embedBuilderService;
            _participantService = participantService;
            _leaderboardResultService = leaderboardResultService;
            _roleService = roleService;
            _dbContext = dbContext;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing leaderboard hosted service");
            var start = DateTime.Now.AddDays(-7);

            foreach (var leaderboard in _dbContext.Leaderboards.ToList())
            {
                await RemoveRolesFromAllInLeaderboard(leaderboard);
                var groupedActivitiesByParticipant = new Dictionary<LeaderboardParticipant, List<DetailedActivity>>();

                var participants = await _participantService.GetAllParticipantsForServerAsync(leaderboard.ServerId);
                foreach (var participant in participants)
                    try
                    {
                        groupedActivitiesByParticipant.Add(participant,
                            await _stravaService.FetchActivitiesForParticipant(participant, start));
                    }
                    catch (StravaException e) when (e.Error == StravaException.StravaErrorType.RefreshFailed)
                    {
                        await AskToRelogin(participant.DiscordUserId);
                    }

                var channel = _discordSocketClient.GetChannel(ulong.Parse(leaderboard.ChannelId)) as SocketTextChannel;

                var realRideCategoryResult = _leaderboardResultService.GetTopResultsForCategory(
                    groupedActivitiesByParticipant, Constants.LeaderboardRideType.RealRide,
                    x => x.Type == Constants.LeaderboardRideType.RealRide);

                await channel.SendMessageAsync(embed: _embedBuilderService
                    .BuildLeaderboardEmbed(realRideCategoryResult, start, DateTime.Now));

                var virtualRideCategoryResult = _leaderboardResultService.GetTopResultsForCategory(
                    groupedActivitiesByParticipant, Constants.LeaderboardRideType.VirtualRide,
                    x => x.Type == Constants.LeaderboardRideType.VirtualRide);

                await channel.SendMessageAsync(embed: _embedBuilderService
                    .BuildLeaderboardEmbed(
                        virtualRideCategoryResult,
                        start,
                        DateTime.Now
                    )
                );

                var allParticipantResults =
                    realRideCategoryResult
                        .ChallengeByChallengeResultDictionary
                        .SelectMany(x => x.Value)
                        .Union(virtualRideCategoryResult.ChallengeByChallengeResultDictionary
                            .SelectMany(x => x.Value))
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

        private async Task GrantWinnerRoles(Leaderboard leaderboard, List<ParticipantResult> leaderboardResults)
        {
            foreach (var participantResult in leaderboardResults)
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