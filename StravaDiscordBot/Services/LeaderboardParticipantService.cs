using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Models;
using StravaDiscordBot.Storage;
using StravaDiscordBot.Strava;

namespace StravaDiscordBot.Services
{
    public interface ILeaderboardParticipantService
    {
        LeaderboardParticipant GetParticipantByStravaIdOrDefault(string serverId, string stravaId);
        LeaderboardParticipant GetParticipantOrDefault(string serverId, string discordUserId);
        List<LeaderboardParticipant> GetAllParticipantsForServerAsync(string serverId);
        Task Remove(LeaderboardParticipant participant, StravaCredential credentials = null);
        Task CreateWithCredentials(LeaderboardParticipant participant, StravaOauthResponse oauthResponse);
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

        public async Task CreateWithCredentials(LeaderboardParticipant participant, StravaOauthResponse oauthResponse)
        {
            _dbContext.Participants.Add(participant);

            var credentials = _dbContext.Credentials.FirstOrDefault(x => x.StravaId == participant.StravaId);
            if (credentials == null)
            {
                _dbContext.Credentials.Add(new StravaCredential(participant.StravaId, oauthResponse.AccessToken, oauthResponse.RefreshToken));
            }
            else
            {
                credentials.UpdateWithNewTokens(oauthResponse);
                _dbContext.Credentials.Update(credentials);
            }
            await _dbContext.SaveChangesAsync();
        }

        public List<LeaderboardParticipant> GetAllParticipantsForServerAsync(string serverId)
        {
            _logger.LogInformation($"Fetching all participants within server {serverId}");
            var participants = _dbContext
                .Participants
                .ToList();

            return participants
                .Where(x => x.ServerId == serverId)
                .ToList();
        }

        public LeaderboardParticipant GetParticipantOrDefault(string serverId, string discordUserId)
        {
            return _dbContext
                .Participants
                .FirstOrDefault(x => x.ServerId == serverId && x.DiscordUserId == discordUserId);
        }

        public LeaderboardParticipant GetParticipantByStravaIdOrDefault(string serverId, string stravaId)
        {
            return _dbContext
                .Participants
                .FirstOrDefault(x => x.ServerId == serverId && x.StravaId == stravaId);
        }

        public async Task Remove(LeaderboardParticipant participant, StravaCredential credentials = null)
        {
            _dbContext.Participants.Remove(participant);
            if (credentials != null)
                _dbContext.Credentials.Remove(credentials);
            await _dbContext.SaveChangesAsync();
        }
    }
}