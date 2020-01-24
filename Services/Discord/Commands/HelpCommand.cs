using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace StravaDiscordBot.Services.Discord.Commands
{
    public interface IHelpCommand : ICommand { }

    public class HelpCommand : CommandBase, IHelpCommand
    {
        private readonly IEnumerable<ICommand> _commands;

        public HelpCommand(IEnumerable<ICommand> commands)
        {
            _commands = commands;
        }

        public override string CommandName => "help";

        public override string Descriptions => "Show help including available commands";

        public override async Task Execute(SocketUserMessage message, int argPos)
        {
            await message.Channel.SendMessageAsync(BuildHelpCommandText());
        }

        private string BuildHelpCommandText()
        {
            var builder = new StringBuilder();
            builder.AppendLine("This is a Discord Strava Leaderboard Bot.");
            builder.AppendLine("Commands:");
            foreach(var command in _commands)
            {
                builder.AppendLine($"**{command.CommandName}** - {command.CommandName}");
            }
            return builder.ToString();
        }
    }
}
