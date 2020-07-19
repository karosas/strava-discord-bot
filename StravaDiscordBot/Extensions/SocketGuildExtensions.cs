using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace StravaDiscordBot.Extensions
{
    public static class SocketGuildExtensions
    {
        public static bool TryGetRole(this SocketGuild server, string roleName, out SocketRole role)
        {
            if (server == null || server.Roles.All(x => x.Name != roleName))
            {
                role = null;
                return false;
            }

            role = server.Roles.First(x => x.Name == roleName);
            return true;
        }

        public static bool TryGetUser(this SocketGuild server, ulong userId, out SocketGuildUser user)
        {
            if (server == null)
            {
                user = null;
                return false;
            }

            user = server.GetUser(userId);
            return user != null;
        }

        public static bool TryGetUser(this SocketGuild server, string userId, out SocketGuildUser user)
        {
            if (ulong.TryParse(userId, out var longUserId))
                return server.TryGetUser(longUserId, out user);

            user = null;
            return false;
        }
    }
}
