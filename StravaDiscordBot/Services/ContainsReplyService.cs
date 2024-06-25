using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BetterHostedServices;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StravaDiscordBot.Services
{
    public class ContainsReplyService : CriticalBackgroundService
    {
        private readonly AppOptions _appOptions;
        private readonly ILogger<ContainsReplyService> _logger;
        private readonly DiscordSocketClient _discordSocketClient;

        public ContainsReplyService(
            IApplicationEnder applicationEnder,
            AppOptions appOptions,
            ILogger<ContainsReplyService> logger,
            DiscordSocketClient discordSocketClient
        )
            : base(applicationEnder)
        {
            _appOptions = appOptions;
            _logger = logger;
            _discordSocketClient = discordSocketClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _discordSocketClient.MessageReceived += MessageReceivedHandler;
        }

        private async Task MessageReceivedHandler(SocketMessage message)
        {
            if (message.Author.IsBot)
            {
                return;
            }

            foreach (var containsReply in _appOptions.ContainsReplies)
            {
                string messageContent = message.Content.ToLower();
                string partSearchingFor = containsReply.Contains.ToLower();

                if (!messageContent.Contains(partSearchingFor))
                {
                    continue;
                }

                await message.Channel.SendMessageAsync(containsReply.Response);
            }
        }
    }
}
