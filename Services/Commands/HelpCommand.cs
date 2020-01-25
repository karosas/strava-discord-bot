using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services.Commands
{
    public interface IHelpCommand : ICommand { }

    public class HelpCommand : CommandBase, IHelpCommand
    {
        private readonly IEnumerable<ICommand> _commands;
        private readonly ICommandCoreService _commandCoreService;

        public HelpCommand(AppOptions options, BotDbContext context, ILogger<HelpCommand> logger, IEnumerable<ICommand> commands, ICommandCoreService commandCoreService) : base(options, context, logger)
        {
            _commands = commands;
            _commandCoreService = commandCoreService;
        }

        public override string CommandName => "help";

        public override string Descriptions => "Show help including available commands";

        public override async Task Execute(SocketUserMessage message, int argPos)
        {
            await message
                .Channel
                .SendMessageAsync(_commandCoreService.GenerateHelpCommandContent(_commands.ToList()))
                .ConfigureAwait(false);
        }
    }
}
