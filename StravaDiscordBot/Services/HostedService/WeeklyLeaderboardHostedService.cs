using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Models.Categories;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.HostedService
{
    public class WeeklyLeaderboardHostedService : CronHostedServiceBase
    {
        private const string JobCronExpression = "0 8 * * 1";
        private readonly BotDbContext _dbContext;
        private readonly DiscordSocketClient _discordSocketClient;

        private readonly ILogger<WeeklyLeaderboardHostedService> _logger;
        private readonly ILeaderboardService _leaderboardService;

        public WeeklyLeaderboardHostedService(ILogger<WeeklyLeaderboardHostedService> logger,
            DiscordSocketClient discordClient,
            BotDbContext dbContext,
            ILeaderboardService leaderboardService) : base(JobCronExpression, TimeZoneInfo.Utc)
        {
            _logger = logger;
            _discordSocketClient = discordClient;
            _dbContext = dbContext;
            _leaderboardService = leaderboardService;
        }


        //var channel = _discordSocketClient.GetChannel(ulong.Parse(leaderboard.ChannelId)) as SocketTextChannel;
        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing leaderboard hosted service");
            var start = DateTime.Now.AddDays(-7);

            foreach (var leaderboard in _dbContext.Leaderboards.ToList())
            {
                try
                {
                    await _leaderboardService.GenerateForServer(
                         _discordSocketClient.GetChannel(ulong.Parse(leaderboard.ChannelId)) as SocketTextChannel,
                         leaderboard.ServerId,
                         DateTime.Now.AddDays(-7),
                         true,
                         new RealRideCategory(),
                         new VirtualRideCategory()
                    );
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"Failed to generate automated leaderboard for {leaderboard.ServerId}");
                }

            }
        }
    }
}