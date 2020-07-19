using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace StravaDiscordBot.Discord.Utilities
{
    public class RequireRoleAttribute : PreconditionAttribute
    {
        private readonly List<string> _roles;

        public RequireRoleAttribute(string[] roles)
        {
            _roles = roles.ToList();
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (context.User is SocketGuildUser gUser)
            {
                // If this command was executed by a user with the appropriate role, return a success
                if (gUser.Roles.Any(userRole => _roles.Any(requiredRoleName => requiredRoleName == userRole.Name)))
                    // Since no async work is done, the result has to be wrapped with `Task.FromResult` to avoid compiler errors
                    return Task.FromResult(PreconditionResult.FromSuccess());
                // Since it wasn't, fail
                return Task.FromResult(
                    PreconditionResult.FromError($"You must have one of these roles: {string.Join(',', _roles)}"));
            }

            return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}