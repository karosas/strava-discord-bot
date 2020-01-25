using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services
{
    public class WeeklyLeaderboardHostedService : CronHostedServiceBase
    {
        private const string JOB_CRON_EXPRESSION = "0 8 * * 1";

        private ILogger<WeeklyLeaderboardHostedService> _logger;
        private ICommandCoreService _commandCoreService;
        private DiscordSocketClient _discordSocketClient;
        private BotDbContext _context;

        public WeeklyLeaderboardHostedService(ILogger<WeeklyLeaderboardHostedService> logger, DiscordSocketClient discordClient, ICommandCoreService commandCoreService, BotDbContext context) : base(JOB_CRON_EXPRESSION, TimeZoneInfo.Utc)
        {
            _logger = logger;
            _commandCoreService = commandCoreService;
            _discordSocketClient = discordClient;
            _context = context;
        }

        public override async Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Executing leaderboard hosted service");
            foreach(var channelId in GetAllLeaderboardChannels())
            {
                var embeds = await _commandCoreService.GenerateLeaderboardCommandContent(channelId).ConfigureAwait(false);
                var channel = _discordSocketClient.GetChannel(channelId) as SocketTextChannel;
                foreach(var embed in embeds)
                {
                    await channel.SendMessageAsync(embed: embed);
                }
            }
        }

        private List<ulong> GetAllLeaderboardChannels()
        {
            return _context.Participants.Select(x => ulong.Parse(x.ChannelId)).Distinct().ToList();
        }
    }
}
