using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Services.Discord.Commands;

namespace StravaDiscordBot.Services.Discord
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly List<ICommand> _commands;

        public CommandHandlingService(DiscordSocketClient discordClient, IEnumerable<ICommand> commands, IHelpCommand helpCommand)
        {
            _discordClient = discordClient;
            _commands = commands.ToList();
            _commands.Add(helpCommand);
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
    }
}
