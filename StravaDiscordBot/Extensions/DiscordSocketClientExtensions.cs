using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace StravaDiscordBot.Extensions
{
    public static class DiscordSocketClientExtensions
    {
        public static SocketGuild GetGuild(this DiscordSocketClient client, string serverId)
        {
            if (ulong.TryParse(serverId, out var longServerId))
                return client.GetGuild(longServerId);

            return null;
        }
    }
}
