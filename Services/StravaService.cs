using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Discord
{
    public interface IStravaService
    {
        string GetOAuthUrl(string serverId, string discordUserId);
        Task<bool> ParticipantDoesNotExist(string serverId, string discordUserId);
        Task<List<LeaderboardParticipant>> GetAllParticipantsForServerAsync(string serverId);
        Task<List<DetailedActivity>> FetchActivitiesForParticipant(LeaderboardParticipant participant, DateTime after);
        Task<StravaCredential> RefreshCredentialsForParticipant(LeaderboardParticipant participant);
        Task ExchangeCodeAndCreateOrRefreshParticipant(string serverId, string discordUserId, string code);
        Task<AthleteDetailed> GetAthlete(LeaderboardParticipant participant);
        Task<StravaCredential> GetCredentialsForParticipant(LeaderboardParticipant participant);
    }

    public class StravaService : IStravaService
    {
        private readonly AppOptions _options;
        private readonly ILogger<StravaService> _logger;
        private readonly BotDbContext _dbContext;
        private readonly IStravaApiClientService _stravaApiService;

        public StravaService(AppOptions options, BotDbContext dbContext, ILogger<StravaService> logger,
            IStravaApiClientService stravaApiService)
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

        public async Task<Dictionary<LeaderboardParticipant, List<DetailedActivity>>> GetActivitiesSinceStartDate(
            string serverId, DateTime after)
        {
            _logger.LogInformation($"Fetching all activities within server {serverId}");
            var result = new Dictionary<LeaderboardParticipant, List<DetailedActivity>>();
            var participants = await GetAllParticipantsForServerAsync(serverId).ConfigureAwait(false);
            _logger.LogInformation($"Found {participants?.Count} participants wtihing channels leaderboard");
            foreach (var participant in participants)
            {
                var credential = await GetCredentialsForParticipant(participant);
                var participantActivities = new List<DetailedActivity>();
                try
                {
                    participantActivities = await _stravaApiService
                        .FetchActivities(credential.AccessToken, after)
                        .ConfigureAwait(false);
                }
                catch (StravaException e) when (e.Error == StravaException.StravaErrorType.Unauthorized)
                {
                    _logger.LogInformation($"Refresh token expired, refreshing");
                    try
                    {
                        credential = await RefreshCredentialsForParticipant(participant)
                            .ConfigureAwait(false);

                        participantActivities = await _stravaApiService
                            .FetchActivities(credential.AccessToken, after)
                            .ConfigureAwait(false);
                    }
                    catch (StravaException ex) when (e.Error == StravaException.StravaErrorType.RefreshFailed)
                    {
                        _logger.LogError(ex, "Failed to refresh participant while fetching activies");
                        continue;
                    }
                }

                result.Add(participant, participantActivities);
            }

            return result;
        }

        public async Task<List<DetailedActivity>> FetchActivitiesForParticipant(LeaderboardParticipant participant,
            DateTime after)
        {
            StravaCredential credential = null;
            try
            {
                credential = await GetCredentialsForParticipant(participant);
                return await _stravaApiService.FetchActivities(credential.AccessToken, after)
                    .ConfigureAwait(false);
            }
            catch (StravaException e) when (e.Error == StravaException.StravaErrorType.Unauthorized)
            {
                _logger.LogInformation($"Refresh token expired, refreshing");
                try
                {
                    credential = await RefreshCredentialsForParticipant(participant)
                        .ConfigureAwait(false);

                    return await _stravaApiService
                        .FetchActivities(credential.AccessToken, after)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to refresh participant while fetching activies");
                    throw new StravaException(StravaException.StravaErrorType.RefreshFailed);
                }
            }
        }

        public string GetOAuthUrl(string serverId, string discordUserId)
        {
            return QueryHelpers.AddQueryString("http://www.strava.com/oauth/authorize",
                new Dictionary<string, string>
                {
                    {"client_id", _options.Strava.ClientId},
                    {"response_type", "code"},
                    {"redirect_uri", $"{_options.BaseUrl}/strava/callback/{serverId}/{discordUserId}"},
                    {"approval_prompt", "force"},
                    {"scope", "read,activity:read"}
                });
        }

        public async Task<bool> ParticipantDoesNotExist(string serverId, string discordUserId)
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

        public async Task<StravaCredential> RefreshCredentialsForParticipant(LeaderboardParticipant participant)
        {
            StravaOauthResponse authResponse = null;
            StravaCredential credential;
            try
            {
                credential = await GetCredentialsForParticipant(participant);
                authResponse = await _stravaApiService.RefreshAccessTokenAsync(credential.RefreshToken)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to refresh access token for {participant.DiscordUserId}");
                throw new StravaException(StravaException.StravaErrorType.RefreshFailed);
            }

            credential.UpdateWithNewTokens(authResponse);
            _dbContext.Credentials.Update(credential);
            await _dbContext.SaveChangesAsync();

            return credential;
        }

        public async Task ExchangeCodeAndCreateOrRefreshParticipant(string serverId, string discordUserId, string code)
        {
            var exchangeResult = await _stravaApiService.ExchangeCodeAsync(code).ConfigureAwait(false);
            var athlete = await _stravaApiService.GetAthlete(exchangeResult.AccessToken);

            if (_dbContext.Participants.FirstOrDefault(x =>
                x.ServerId == serverId && x.StravaId == athlete.Id.ToString(CultureInfo.InvariantCulture)) == null)
            {
                _dbContext.Participants.Add(new LeaderboardParticipant(serverId, discordUserId,
                    athlete.Id.ToString(CultureInfo.InvariantCulture)));
            }

            UpsertCredentialWithoutSaving(athlete.Id.ToString(CultureInfo.InvariantCulture), exchangeResult);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public Task<StravaCredential> GetCredentialsForParticipant(LeaderboardParticipant participant)
        {
            if (participant == null)
                throw new ArgumentNullException(nameof(participant));

            return _dbContext.Credentials.FirstOrDefaultAsync(x => x.StravaId == participant.StravaId);
        }

        private void UpsertCredentialWithoutSaving(string stravaId, StravaOauthResponse oauthResponse)
        {
            var credential = _dbContext.Credentials.FirstOrDefault(x => x.StravaId == stravaId);
            _logger.LogInformation($"Credentials found - {credential != null}");
            if (credential == null)
            {
                _logger.LogInformation("Credentials not found, adding");
                _dbContext.Credentials.Add(new StravaCredential(stravaId, oauthResponse.AccessToken,
                    oauthResponse.RefreshToken));
                return;
            }

            _logger.LogInformation("Credentials found, updating");
            _logger.LogDebug($"Access token changed - {credential.AccessToken == oauthResponse.AccessToken}");
            _logger.LogDebug($"Refresh token changed - {credential.RefreshToken == oauthResponse.RefreshToken}");

            credential.UpdateWithNewTokens(oauthResponse);
            _dbContext.Credentials.Update(credential);
        }

        public async Task<AthleteDetailed> GetAthlete(LeaderboardParticipant participant)
        {
            try
            {
                var credentials = await GetCredentialsForParticipant(participant);
                return await _stravaApiService.GetAthlete(credentials.AccessToken);
            }
            catch (StravaException e) when (e.Error == StravaException.StravaErrorType.Unauthorized)
            {
                var credentials = await RefreshCredentialsForParticipant(participant)
                    .ConfigureAwait(false);

                return await _stravaApiService.GetAthlete(credentials.AccessToken);
            }
        }
    }
}