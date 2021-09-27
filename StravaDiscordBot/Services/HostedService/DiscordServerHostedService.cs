using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord;

namespace StravaDiscordBot.Services.HostedService
{
    public class DiscordServerHostedService : BackgroundService
    {
        private readonly ILogger<DiscordServerHostedService> _logger;
        private readonly AppOptions _options;
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _commandService;
        private readonly CommandHandlingService _commandHandlingService;
        
        public DiscordServerHostedService(ILogger<DiscordServerHostedService> logger, 
            AppOptions options,
            DiscordSocketClient discordClient, 
            CommandService commandService, 
            CommandHandlingService commandHandlingService)
        {
            _logger = logger;
            _options = options;
            _discordClient = discordClient;
            _commandService = commandService;
            _commandHandlingService = commandHandlingService;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _discordClient.Log += LogAsync;
            _commandService.Log += LogAsync;

            await _discordClient.LoginAsync(TokenType.Bot, _options.Discord.Token).ConfigureAwait(false);
            await _discordClient.StartAsync().ConfigureAwait(false);

            await _commandHandlingService.InstallCommandsAsync();
        }
        
        private Task LogAsync(LogMessage log)
        {
            _logger.LogInformation(log.Exception, $"[{log.Severity}] {log.Message}");
            return Task.CompletedTask;
        }
    }
}