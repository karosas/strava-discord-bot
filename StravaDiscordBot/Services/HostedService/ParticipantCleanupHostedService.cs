using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.HostedService
{
    public class ParticipantCleanupHostedService : CronHostedServiceBase
    {
        private const string JobCronExpression = "0 7 * * *";

        private readonly ILogger<ParticipantCleanupHostedService> _logger;
        private readonly BotDbContext _dbContext;
        private readonly ILeaderboardService _leaderboardService;

        public ParticipantCleanupHostedService(ILogger<ParticipantCleanupHostedService> logger,
            ILeaderboardService leaderboardService, BotDbContext dbContext, AppOptions options) : base(JobCronExpression, TimeZoneInfo.Utc)
        {
            _logger = logger;
            _leaderboardService = leaderboardService;
            _dbContext = dbContext;
        }

        protected override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting participant cleanup");

            foreach (var leaderboard in _dbContext.Leaderboards)
            {
                await _leaderboardService.PruneUsers(leaderboard.ServerId, false);
            }

            _logger.LogInformation("Cleanup done");
        }
    }
}