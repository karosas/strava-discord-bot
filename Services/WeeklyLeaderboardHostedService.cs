using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Services;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Discord
{
    public class WeeklyLeaderboardHostedService : CronHostedServiceBase
    {
        private const string JOB_CRON_EXPRESSION = "0 8 * * 1";

        private readonly ILogger<WeeklyLeaderboardHostedService> _logger;
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly IStravaService _stravaService;
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILeaderboardParticipantService _participantService;

        public WeeklyLeaderboardHostedService(ILogger<WeeklyLeaderboardHostedService> logger,
            DiscordSocketClient discordClient,
            IStravaService stravaService,
            IEmbedBuilderService embedBuilderService,
            ILeaderboardService leaderboardService,
            ILeaderboardParticipantService participantService) 
            : base(JOB_CRON_EXPRESSION, TimeZoneInfo.Utc)
        {
            _logger = logger;
            _discordSocketClient = discordClient;
            _stravaService = stravaService;
            _embedBuilderService = embedBuilderService;
            _leaderboardService = leaderboardService;
            _participantService = participantService;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing leaderboard hosted service");
            foreach(var leaderboard in await _leaderboardService.GetAllLeaderboards())
            {
                var start = DateTime.Now.AddDays(-7);
                var groupedActivitiesByParticipant = new Dictionary<LeaderboardParticipant, List<DetailedActivity>>();
                var participants = await _participantService.GetAllParticipantsForServerAsync(leaderboard.ServerId);
                foreach (var participant in participants)
                {
                    try
                    {
                        groupedActivitiesByParticipant.Add(participant, await _stravaService.FetchActivitiesForParticipant(participant, start));
                    }
                    catch (StravaException e) when (e.Error == StravaException.StravaErrorType.RefreshFailed)
                    {
                        await AskToRelogin(participant.DiscordUserId);
                    }
                }
                var channel = _discordSocketClient.GetChannel(ulong.Parse(leaderboard.ChannelId)) as SocketTextChannel;

                await channel.SendMessageAsync(embed: _embedBuilderService
                    .BuildLeaderboardEmbed(
                        groupedActivitiesByParticipant,
                        Constants.LeaderboardRideType.RealRide,
                        start,
                        DateTime.Now
                    )
                );
                    
                await channel.SendMessageAsync(embed: _embedBuilderService
                    .BuildLeaderboardEmbed(
                        groupedActivitiesByParticipant,
                        Constants.LeaderboardRideType.VirtualRide,
                        start,
                        DateTime.Now
                    )
                );
            }
        }
        
        private async Task AskToRelogin(string discordId)
        {
            _logger.LogInformation("Sending refresh notification");
            var channel = await _discordSocketClient.GetDMChannelAsync(ulong.Parse(discordId));
            await channel.SendMessageAsync(
                $"Hey, I failed refreshing access to your Strava account. Please use `join` command again in the server of leaderboard.");
        }
    }
}
