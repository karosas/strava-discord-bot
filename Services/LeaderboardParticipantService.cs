using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Models;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services
{
    public interface ILeaderboardParticipantService
    {
        Task<LeaderboardParticipant> GetParticipantOrDefault(string serverId, string discordUserId);
        Task<List<LeaderboardParticipant>> GetAllParticipantsForServerAsync(string serverId);
    }

    public class LeaderboardParticipantService : ILeaderboardParticipantService
    {
        private readonly BotDbContext _dbContext;
        private readonly ILogger<LeaderboardParticipantService> _logger;

        public LeaderboardParticipantService(ILogger<LeaderboardParticipantService> logger, BotDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<List<LeaderboardParticipant>> GetAllParticipantsForServerAsync(string serverId)
        {
            _logger.LogInformation($"Fetching all participants within server {serverId}");
            var participants = await _dbContext
                .Participants
                .ToListAsync()
                .ConfigureAwait(false);

            return participants
                .Where(x => x.ServerId == serverId)
                .ToList();
        }

        public Task<LeaderboardParticipant> GetParticipantOrDefault(string serverId, string discordUserId)
        {
            return _dbContext
                .Participants
                .FirstOrDefaultAsync(x => x.ServerId == serverId && x.DiscordUserId == discordUserId);
        }
    }
}