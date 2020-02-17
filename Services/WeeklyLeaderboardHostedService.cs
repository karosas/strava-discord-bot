using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Discord
{
    public class WeeklyLeaderboardHostedService : CronHostedServiceBase
    {
        private const string JOB_CRON_EXPRESSION = "0 8 * * 1";

        private ILogger<WeeklyLeaderboardHostedService> _logger;
        private ICommandCoreService _commandCoreService;
        private DiscordSocketClient _discordSocketClient;
        private IStravaService _stravaService;
        private BotDbContext _context;

        public WeeklyLeaderboardHostedService(ILogger<WeeklyLeaderboardHostedService> logger, DiscordSocketClient discordClient, ICommandCoreService commandCoreService, BotDbContext context, IStravaService stravaService) : base(JOB_CRON_EXPRESSION, TimeZoneInfo.Utc)
        {
            _logger = logger;
            _commandCoreService = commandCoreService;
            _discordSocketClient = discordClient;
            _context = context;
            _stravaService = stravaService;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing leaderboard hosted service");
            foreach(var leaderboard in _context.Leaderboards)
            {
                var groupedActivitiesByParticipant = new Dictionary<LeaderboardParticipant, List<DetailedActivity>>();
                var participants = await _stravaService.GetAllParticipantsForServerAsync(leaderboard.ServerId);
                foreach (var participant in participants)
                {
                    try
                    {
                        groupedActivitiesByParticipant.Add(participant, await _stravaService.FetchActivitiesForParticipant(participant, DateTime.Now.AddDays(-7)));
                    }
                    catch (StravaException e) when (e.Error == StravaException.StravaErrorType.RefreshFailed)
                    {
                        await AskToRelogin(participant.DiscordUserId);
                    }
                }

                var embeds = await _commandCoreService.GenerateLeaderboardCommandContent(groupedActivitiesByParticipant);
                
                var channel = _discordSocketClient.GetChannel(ulong.Parse(leaderboard.ChannelId)) as SocketTextChannel;
                foreach(var embed in embeds)
                {
                    await channel.SendMessageAsync(embed: embed);
                }
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
