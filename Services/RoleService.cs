using System;
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
        Task RemoveRoleFromAllParticipantsInServer(string serverId, string roleName);
    }
    public class RoleService : IRoleService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly ILogger<RoleService> _logger;
        private readonly ILeaderboardParticipantService _participantService;

        public RoleService(DiscordSocketClient discordSocketClient, ILogger<RoleService> logger, ILeaderboardParticipantService participantService)
        {
            _discordSocketClient = discordSocketClient;
            _logger = logger;
            _participantService = participantService;
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

            try
            {
                await user.AddRoleAsync(role);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to grant role to {user?.Id}");
            }
        }

        public async Task RemoveUserRole(string serverId, string userId, string roleName)
        {
            _logger.LogInformation($"Attempting to remove user '{userId}' role '{roleName}' in server '{serverId}'");
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

        public async Task RemoveRoleFromAllParticipantsInServer(string serverId, string roleName)
        {
            var server = _discordSocketClient.GetGuild(ulong.Parse(serverId));
            var role = server?.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role == null)
            {
                _logger.LogWarning($"Role '{roleName}' not found in server '{serverId}'");
                return;
            }

            var participants = await _participantService.GetAllParticipantsForServerAsync(serverId);
            foreach (var participant in participants)
            {
                _logger.LogInformation($"Removing role from {participant?.DiscordUserId}");
                try
                {
                    var user = server.GetUser(ulong.Parse(participant?.DiscordUserId));
                    await user.RemoveRoleAsync(role);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to remove role from {participant?.DiscordUserId} in server {serverId}");
                }
            }
        }
    }
}