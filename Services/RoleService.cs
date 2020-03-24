using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace StravaDiscordBot.Services
{
    public interface IRoleService
    {
        Task GrantUserRoleIfExists(string serverId, string userId, string roleName);
        Task RemoveUserRole(string serverId, string userId, string roleName);
    }
    public class RoleService : IRoleService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly ILogger<RoleService> _logger;

        public RoleService(DiscordSocketClient discordSocketClient, ILogger<RoleService> logger)
        {
            _discordSocketClient = discordSocketClient;
            _logger = logger;
        }

        public async Task GrantUserRoleIfExists(string serverId, string userId, string roleName)
        {
            _logger.LogInformation($"Attempting to grant user '{userId}' role '{roleName}' in server '{serverId}'");
            var server = _discordSocketClient.GetGuild(ulong.Parse(serverId));
            var role = server?.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role == null)
            {
                _logger.LogWarning($"Role '{roleName}' not found in server '{serverId}'");
                return;
            }
            var user = server.GetUser(ulong.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning($"User '{userId}' not found in server '{serverId}'");
                return;
            }

            if (user.Roles.Any(x => x.Name == roleName))
            {
                _logger.LogInformation($"User '{userId}' already has role '{roleName}'");
                return;
            }

            await user.AddRoleAsync(role);
        }

        public async Task RemoveUserRole(string serverId, string userId, string roleName)
        {
            _logger.LogInformation($"Attempting to grant user '{userId}' role '{roleName}' in server '{serverId}'");
            var server = _discordSocketClient.GetGuild(ulong.Parse(serverId));
            var role = server?.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role == null)
            {
                _logger.LogWarning($"Role '{roleName}' not found in server '{serverId}'");
                return;
            }
            var user = server.GetUser(ulong.Parse(userId));
            if (user == null)
            {
                _logger.LogWarning($"User '{userId}' not found in server '{serverId}'");
                return;
            }

            if (user.Roles.Any(x => x.Name == roleName))
            {
                await user.RemoveRoleAsync(role);
            }
        }
    }
}