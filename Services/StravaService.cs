using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Discord
{
    public interface IStravaService
    {
        string GetOAuthUrl(string serverId, string discordUserId);
        Task<bool> DoesParticipantAlreadyExistsAsync(string serverId, string discordUserId);
        Task<List<LeaderboardParticipant>> GetAllParticipantsForServerAsync(string serverId);
        Task<Dictionary<LeaderboardParticipant, List<DetailedActivity>>> GetActivitiesSinceStartDate(string serverId, DateTime start);
        Task<LeaderboardParticipant> RefreshAccessTokenAsync(LeaderboardParticipant participant);
        Task ExchangeCodeAndCreateParticipant(string serverId, string discordUserId, string code);
    }

    public class StravaService : IStravaService
    {
        private readonly AppOptions _options;
        private readonly ILogger<StravaService> _logger;
        private readonly BotDbContext _dbContext;
        private readonly IStravaApiClientService _stravaApiService;

        public StravaService(AppOptions options, BotDbContext dbContext, ILogger<StravaService> logger, IStravaApiClientService stravaApiService)
        {
            _options = options;
            _logger = logger;
            _dbContext = dbContext;
            _stravaApiService = stravaApiService;
        }

        public async Task<List<LeaderboardParticipant>> GetAllParticipantsForServerAsync(string serverId)
        {
            _logger.LogInformation($"Fetching all participants within server {serverId}");
            var participants = await _dbContext.Participants.ToListAsync().ConfigureAwait(false);
            return participants.Where(x => x.ServerId == serverId).ToList();
        }

        public async Task<Dictionary<LeaderboardParticipant, List<DetailedActivity>>> GetActivitiesSinceStartDate(string serverId, DateTime after)
        {
            _logger.LogInformation($"Fetching all activities within server {serverId}");
            var result = new Dictionary<LeaderboardParticipant, List<DetailedActivity>>();
            var participants = await GetAllParticipantsForServerAsync(serverId).ConfigureAwait(false);
            _logger.LogInformation($"Found {participants?.Count} participants wtihing channels leaderboard");
            foreach (var participant in participants)
            {
                var participantActivities = new List<DetailedActivity>();
                try
                {
                    participantActivities = await _stravaApiService.FetchActivities(participant.StravaAccessToken, participant.StravaRefreshToken, after)
                        .ConfigureAwait(false);
                }
                catch (StravaException e) when (e.Error == StravaException.StravaErrorType.Unauthorized)
                {
                    await RefreshAccessTokenAsync(participant)
                        .ConfigureAwait(false);

                    participantActivities = await _stravaApiService.FetchActivities(participant.StravaAccessToken, participant.StravaRefreshToken, after)
                       .ConfigureAwait(false);
                }
                result.Add(participant, participantActivities);
            }
            return result;
        }

        public string GetOAuthUrl(string serverId, string discordUserId)
        {
            return QueryHelpers.AddQueryString("http://www.strava.com/oauth/authorize",
                new Dictionary<string, string>
                {
                    { "client_id", _options.Strava.ClientId },
                    { "response_type", "code" },
                    { "redirect_uri", $"{_options.BaseUrl}/strava/callback/{serverId}/{discordUserId}" },
                    { "approval_prompt", "force" },
                    { "scope", "read,activity:read" }
                });
        }

        public async Task<bool> DoesParticipantAlreadyExistsAsync(string serverId, string discordUserId)
        {
            try
            {
                var participants = await GetAllParticipantsForServerAsync(serverId).ConfigureAwait(false);
                return participants.FirstOrDefault(x => x.DiscordUserId == discordUserId) != null;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed fetching participant");
                // Assumption for now, probably should be handled better in the future
                return true;
            }
        }

        public async Task<LeaderboardParticipant> RefreshAccessTokenAsync(LeaderboardParticipant participant)
        {
            (string updatedAccessToken, string updatedRefreshToken) = await _stravaApiService.RefreshAccessTokenAsync(participant.StravaRefreshToken).ConfigureAwait(false);

            participant.StravaAccessToken = updatedAccessToken;
            participant.StravaRefreshToken = updatedRefreshToken;
            _dbContext.Participants.Update(participant);

            return participant;
        }

        public async Task ExchangeCodeAndCreateParticipant(string serverId, string discordUserId, string code)
        {
            var exchangeResult = await _stravaApiService.ExchangeCodeAsync(code).ConfigureAwait(false);
            var leaderboardParticipant = new LeaderboardParticipant(serverId, discordUserId, exchangeResult.AccessToken, exchangeResult.RefreshToken);
            await _dbContext.Participants.AddAsync(leaderboardParticipant);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
