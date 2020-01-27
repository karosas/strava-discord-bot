using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using StravaDiscordBot.Discord.Modules;

namespace StravaDiscordBot.Discord
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandlingService(DiscordSocketClient discordClient, CommandService commands, IServiceProvider services)
        {
            _discordClient = discordClient;
            _commands = commands;
            _services = services;
        }

        public async Task InstallCommandsAsync()
        {
            _discordClient.MessageReceived += HandleCommandAsync;

            await _commands.AddModuleAsync<PublicModule>(_services);
            await _commands.AddModuleAsync<AdminModule>(_services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null)
                return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!message.HasMentionPrefix(_discordClient.CurrentUser, ref argPos) || message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_discordClient, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _services);
        }
    }
}
