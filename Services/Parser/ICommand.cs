using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace StravaDiscordBot.Services.Parser
{
    public interface ICommand
    {
        bool CanExecute(SocketUserMessage message, int argPos);
        Task Execute(SocketUserMessage message, int argPos);
    }
}
