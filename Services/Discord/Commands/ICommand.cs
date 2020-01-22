using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace StravaDiscordBot.Services.Discord.Commands
{
    public interface ICommand
    {
        bool CanExecute(SocketUserMessage message, int argPos);
        Task Execute(SocketUserMessage message, int argPos);
        string CommandName { get; }
        string Descriptions { get; }
    }
}
