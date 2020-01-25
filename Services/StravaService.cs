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

namespace StravaDiscordBot.Services
{
    public interface IStravaService
    {
        Task<StravaCodeExchangeResult> ExchangeCodeAsync(string code);
        Task CreateLeaderboardParticipantAsync(string channelId, string discordUserId, StravaCodeExchangeResult stravaExchangeResult);
        Task<bool> ParticipantAlreadyExistsAsync(string channelId, string discordUserId);
        string GetOAuthUrl(string channelId, string discordUserId);
        Task<List<LeaderboardParticipant>> GetAllParticipantsForChannelAsync(string channelId);
        Task<Dictionary<LeaderboardParticipant, List<DetailedActivity>>> GetActivitiesSinceStartDate(string channelId, DateTime start);
        Task RefreshAccessTokenAsync(LeaderboardParticipant participant);
    }
    public class StravaService : IStravaService
    {
        private readonly AppOptions _options;
        private readonly ILogger<StravaService> _logger;
        private readonly BotDbContext _dbContext;

        public StravaService(AppOptions options, BotDbContext dbContext, ILogger<StravaService> logger)
        {
            _options = options;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task CreateLeaderboardParticipantAsync(string channelId, string discordUserId, StravaCodeExchangeResult stravaExchangeResult)
        {
            _logger.LogInformation($"Creating leaderboard participant. channel: {channelId} | discord user: {discordUserId}");
            var leaderboardParticipant = new LeaderboardParticipant(channelId, discordUserId, stravaExchangeResult.AccessToken, stravaExchangeResult.RefreshToken);
            await _dbContext.Participants.AddAsync(leaderboardParticipant);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<StravaCodeExchangeResult> ExchangeCodeAsync(string code)
        {
            using(var http = new HttpClient())
            {
                var stravaOptions = _options.Strava;
                var url = QueryHelpers.AddQueryString("https://www.strava.com/oauth/token",
                    new Dictionary<string, string>
                    {
                        { "client_id", stravaOptions.ClientId },
                        { "client_secret", stravaOptions.ClientSecret },
                        { "code", code },
                        { "grant_type", "authorization_code" }
                   });

                var result = await http.PostAsync(url, null).ConfigureAwait(false);
                if(!result.IsSuccessStatusCode)
                {
                    _logger.LogError($"Strava code exchange failed, Status: {result.StatusCode}");
                    throw new StravaException($"Exchange code failed with status code {result.StatusCode}");
                }

                var responseContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseContentSerialized = JObject.Parse(responseContent);
                return new StravaCodeExchangeResult(responseContentSerialized["access_token"].ToString(), responseContentSerialized["refresh_token"].ToString());
            }
        }

        public async Task<List<LeaderboardParticipant>> GetAllParticipantsForChannelAsync(string channelId)
        {
            _logger.LogInformation($"Fetching all participants within channel {channelId}");
            var participants = await _dbContext.Participants.ToListAsync().ConfigureAwait(false);
            return participants.Where(x => x.ChannelId == channelId).ToList();
        }

        public async Task<Dictionary<LeaderboardParticipant, List<DetailedActivity>>> GetActivitiesSinceStartDate(string channelId, DateTime start)
        {
            _logger.LogInformation($"Fetching all activities within channel {channelId}");
            var result = new Dictionary<LeaderboardParticipant, List<DetailedActivity>>();
            var participants = await GetAllParticipantsForChannelAsync(channelId).ConfigureAwait(false);
            _logger.LogInformation($"Found {participants?.Count} participants wtihing channels leaderboard");
            foreach (var participant in participants)
            {
                result.Add(participant, await GetActivitiesForParticipantSinceStartDate(participant, start).ConfigureAwait(false));
            }
            return result;
        }

        private async Task<List<DetailedActivity>> GetActivitiesForParticipantSinceStartDate(LeaderboardParticipant participant, DateTime start, bool isRetry = false)
        {
            _logger.LogInformation($"Fetching activities for {participant.GetDiscordMention()} from {start.ToShortDateString()}");
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", participant.StravaAccessToken);
                var url = QueryHelpers.AddQueryString("https://www.strava.com/api/v3/athlete/activities", new Dictionary<string, string>
                {
                    { "after", $"{(int)(start - new DateTime(1970, 1, 1)).TotalSeconds}" },
                    { "per_page", "100" }
                });

                var activityResponse = await http.GetAsync(url).ConfigureAwait(false);

                if (!activityResponse.IsSuccessStatusCode)
                {
                    if (activityResponse.StatusCode == HttpStatusCode.Unauthorized && !isRetry)
                    {
                        _logger.LogWarning("Received 401 from strava, attempting to refresh access token");
                        await RefreshAccessTokenAsync(participant).ConfigureAwait(false);
                        return await GetActivitiesForParticipantSinceStartDate(participant, start, true).ConfigureAwait(false);
                    }
                    _logger.LogError($"Failed to fetch activities");
                    throw new StravaException($"Failed to fetch activities, status code: {activityResponse.StatusCode}");
                }

                var activityResponseContent = await activityResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                _logger.LogInformation("Strava activity response");
                var activitiesDeserialized = JsonConvert.DeserializeObject<List<DetailedActivity>>(activityResponseContent);
                _logger.LogInformation($"Fetched {activitiesDeserialized.Count} activities");
                return activitiesDeserialized.ToList();
            }
        }

        public string GetOAuthUrl(string channelId, string discordUserId)
        {
            var stravaOptions = _options.Strava;
            var currentProxyUrl = _options.BaseUrl;
            return QueryHelpers.AddQueryString("http://www.strava.com/oauth/authorize",
                new Dictionary<string, string>
                {
                    { "client_id", stravaOptions.ClientId },
                    { "response_type", "code" },
                    { "redirect_uri", $"{currentProxyUrl}/strava/callback/{channelId}/{discordUserId}" },
                    { "approval_prompt", "force" },
                    { "scope", "read,activity:read" }
                });
        }

        public async Task<bool> ParticipantAlreadyExistsAsync(string channelId, string discordUserId)
        {
            try
            {
                var participants = await GetAllParticipantsForChannelAsync(channelId).ConfigureAwait(false);
                return participants.FirstOrDefault(x => x.DiscordUserId == discordUserId) != null;
            }
            catch(Exception e)
            {
                _logger.LogWarning(e, "Failed fetching participant");
                // Assumption for now, probably should be handled better in the future
                return true;
            }
        }

        public async Task RefreshAccessTokenAsync(LeaderboardParticipant participant)
        {
            _logger.LogInformation($"Refreshing access token for {participant.GetDiscordMention()}");
            using (var http = new HttpClient())
            {
                var stravaOptions = _options.Strava;
                var url = QueryHelpers.AddQueryString("https://www.strava.com/oauth/token",
                    new Dictionary<string, string>
                    {
                        { "client_id", stravaOptions.ClientId },
                        { "client_secret", stravaOptions.ClientSecret },
                        { "refresh_token", participant.StravaRefreshToken },
                        { "grant_type", "refresh_token" }
                   });

                var result = await http.PostAsync(url, null).ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to refresh access token for {participant.GetDiscordMention()}");
                    throw new StravaException($"Exchange code failed with status code {result.StatusCode}");
                }

                var responseContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseContentSerialized = JObject.Parse(responseContent);

                participant.UpdateWithNewTokens( new StravaCodeExchangeResult(responseContentSerialized["access_token"].ToString(), responseContentSerialized["refresh_token"].ToString()));
                _dbContext.Participants.Update(participant);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
