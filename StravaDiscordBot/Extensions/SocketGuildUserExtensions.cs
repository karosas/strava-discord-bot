using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace StravaDiscordBot.Extensions
{
    public static class SocketGuildUserExtensions
    {
        public static async Task<bool> TryRemoveRoleAsync(this SocketGuildUser user, IRole role)
        {
            if (user == null)
                return false;

            if (role == null)
                return false;

            if (user.Roles.All(x => x.Name != role.Name))
                return false;

            try
            {
                await user.RemoveRoleAsync(role);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
