using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Extensions;

namespace StravaDiscordBot.Services
{
    public interface IRoleService
    {
        Task GrantUserRole(string serverId, string userId, string roleName);
        Task RemoveUserRole(string serverId, string userId, string roleName);
        Task RemoveRoleFromAllInServer(string serverId, string roleName);
    }

    public class RoleService : IRoleService
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly ILogger<RoleService> _logger;
        private readonly ILeaderboardParticipantService _participantService;

        public RoleService(DiscordSocketClient discordSocketClient, ILogger<RoleService> logger,
            ILeaderboardParticipantService participantService)
        {
            _discordSocketClient = discordSocketClient;
            _logger = logger;
            _participantService = participantService;
        }

        public async Task GrantUserRole(string serverId, string userId, string roleName)
        {
            _logger.LogInformation($"Attempting to grant user '{userId}' role '{roleName}' in server '{serverId}'");
            var server = _discordSocketClient.GetGuild(serverId);
            if (server.TryGetRole(roleName, out var role) && server.TryGetUser(userId, out var user))
            {
                try
                {
                    await user.AddRoleAsync(role);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to grant role to {user?.Id}");
                }
            }
            else
                _logger.LogError($"Failed to find {roleName} role OR to find {userId} user");
        }

        public async Task RemoveUserRole(string serverId, string userId, string roleName)
        {
            _logger.LogInformation($"Attempting to remove user '{userId}' role '{roleName}' in server '{serverId}'");

            var server = _discordSocketClient.GetGuild(serverId);
            if (server.TryGetRole(roleName, out var role) && server.TryGetUser(userId, out var user))
                await user.TryRemoveRoleAsync(role);
            else
                _logger.LogError($"Failed to find {roleName} role OR to find {userId} user");
        }

        public async Task RemoveRoleFromAllInServer(string serverId, string roleName)
        {
            var server = _discordSocketClient.GetGuild(serverId);
            if (!server.TryGetRole(roleName, out var role))
            {
                _logger.LogWarning($"Role '{roleName}' not found in server '{serverId}'");
                return;
            }

            var participants = _participantService.GetAllParticipantsForServerAsync(serverId);
            foreach (var participant in participants)
            {
                _logger.LogInformation($"Removing role from {participant?.DiscordUserId}");
                if (server.TryGetUser(participant?.DiscordUserId, out var user))
                    await user.TryRemoveRoleAsync(role);
                else
                    _logger.LogError($"Failed ot find {participant?.DiscordUserId} user");
            }
        }
    }
}