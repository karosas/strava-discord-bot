using System.Threading.Tasks;
using Discord.WebSocket;

namespace StravaDiscordBot.Services.Discord.Commands
{
    public abstract class CommandBase : ICommand
    {
        public abstract string CommandName { get; }
        public abstract string Descriptions { get; }

        public bool CanExecute(SocketUserMessage message, int argPos)
        {
            return message.Content.Substring(argPos).Trim().ToLower().StartsWith(CommandName);
        }

        public abstract Task Execute(SocketUserMessage message, int argPos);
    }
}
