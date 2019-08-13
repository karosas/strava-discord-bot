using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Services.Parser;

namespace StravaDiscordBot.Services.Discord
{
    public class CommandHandlingService
    {
        private readonly CommandService _commandService;
        private readonly DiscordSocketClient _discordClient;
        private readonly List<ICommand> _commands;

        public CommandHandlingService(CommandService commandService, DiscordSocketClient discordClient, IEnumerable<ICommand> commands)
        {
            _commandService = commandService;
            _discordClient = discordClient;
            _commands = commands.ToList();
            // Hook CommandExecuted to handle post-command-execution logic.
            _commandService.CommandExecuted += CommandExecutedAsync;
            // Hook MessageReceived so we can process each message to see
            // if it qualifies as a command.
            _discordClient.MessageReceived += MessageReceivedAsync;
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message))
                return;
            if (message.Source != MessageSource.User)
                return;

            int argpos = 0;
            if (!message.HasMentionPrefix(_discordClient.CurrentUser, ref argpos))
                return;

            bool alreadyRespondedWithError = false;
            foreach(var validCommand in _commands.Where(cmd => cmd.CanExecute(message, argpos)))
            {
                try
                {
                    await validCommand.Execute(message, argpos);
                }
                catch (InvalidCommandArgumentException e)
                {
                    // To avoid accidentally writing multiple error messages per one command
                    if(!alreadyRespondedWithError)
                    {
                        alreadyRespondedWithError = true;
                        await message.Channel.SendMessageAsync(e.Message);
                    }
                }
            }
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
                return;

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // the command failed, let's notify the user that something happened.
            await context.Channel.SendMessageAsync($"error: {result}");
        }
    }
}
