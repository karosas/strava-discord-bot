using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Services.Storage;

namespace StravaDiscordBot.Services
{
    public interface IStravaService
    {
        Task<StravaCodeExchangeResult> ExchangeCodeAsync(string code);
        Task CreateLeaderboardParticipantAsync(string channelId, string discordUserId, StravaCodeExchangeResult stravaExchangeResult);
        Task<bool> ParticipantAlreadyExistsAsync(string channelId, string discordUserId);
        string GetOAuthUrl(string channelId, string discordUserId);
        Task<List<LeaderboardParticipant>> GetAllParticipantsForChannelAsync(string channelId);
        Task<Dictionary<LeaderboardParticipant, List<DetailedActivity>>> GetAllLeaderboardActivitiesForChannelIdAsync(string channelId);
        Task RefreshAccessTokenAsync(LeaderboardParticipant participant);
    }
    public class StravaService : IStravaService
    {
        private readonly AppOptions _options;
        private readonly IRepository<LeaderboardParticipant> _leaderboardRepository;

        public StravaService(AppOptions options, IRepository<LeaderboardParticipant> leaderboardRepository)
        {
            _options = options;
            _leaderboardRepository = leaderboardRepository;
        }

        public async Task CreateLeaderboardParticipantAsync(string channelId, string discordUserId, StravaCodeExchangeResult stravaExchangeResult)
        {
            var leaderboardParticipant = new LeaderboardParticipant(channelId, discordUserId, stravaExchangeResult.AccessToken, stravaExchangeResult.RefreshToken);
            await _leaderboardRepository.Save(leaderboardParticipant);
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

                var result = await http.PostAsync(url, null);
                if(!result.IsSuccessStatusCode)
                    throw new StravaException($"Exchange code failed with status code {result.StatusCode}");

                var responseContent = await result.Content.ReadAsStringAsync();
                var responseContentSerialized = JObject.Parse(responseContent);
                return new StravaCodeExchangeResult(responseContentSerialized["access_token"].ToString(), responseContentSerialized["refresh_token"].ToString());
            }
        }

        public Task<List<LeaderboardParticipant>> GetAllParticipantsForChannelAsync(string channelId)
        {
            return _leaderboardRepository.GetForPartition(channelId);
        }

        public async Task<Dictionary<LeaderboardParticipant, List<DetailedActivity>>> GetAllLeaderboardActivitiesForChannelIdAsync(string channelId)
        {
            var result = new Dictionary<LeaderboardParticipant, List<DetailedActivity>>();
            var participants = await _leaderboardRepository.GetForPartition(channelId);
            foreach (var participant in participants)
            {
                result.Add(participant, await Get7DaysActivitiesForParticipant(participant));
            }
            return result;
        }

        private async Task<List<DetailedActivity>> Get7DaysActivitiesForParticipant(LeaderboardParticipant participant, bool isRetry = false)
        {
            using(var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", participant.StravaAccessToken);
                var url = QueryHelpers.AddQueryString("https://www.strava.com/api/v3/athlete/activities", new Dictionary<string, string>
                {
                    { "after", GetActivityStartDate().ToString() },
                    { "per_page", "100" }
                });

                var activityResponse = await http.GetAsync(url);

                if(!activityResponse.IsSuccessStatusCode)
                {
                    if(activityResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized && !isRetry)
                    {
                        await RefreshAccessTokenAsync(participant);
                        return await Get7DaysActivitiesForParticipant(participant, true);
                    }
                    throw new StravaException($"Failed to fetch activities, status code: {activityResponse.StatusCode}");
                }

                var activityResponseContent = await activityResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DetailedActivity>>(activityResponseContent);
            }
        }

        public string GetOAuthUrl(string channelId, string discordUserId)
        {
            var stravaOptions = _options.Strava;
            return QueryHelpers.AddQueryString("http://www.strava.com/oauth/authorize",
                new Dictionary<string, string>
                {
                    { "client_id", stravaOptions.ClientId },
                    { "response_type", "code" },
                    { "redirect_uri", $"http://localhost:5000/strava/callback/{channelId}/{discordUserId}" },
                    { "approval_prompt", "force" },
                    { "scope", "read,activity:read" }
                });
        }

        public async Task<bool> ParticipantAlreadyExistsAsync(string channelId, string discordUserId)
        {
            try
            {
                var participant = await _leaderboardRepository.GetById(channelId, discordUserId);
                return participant != null;
            }
            catch
            {
                // Assumption for now, probably should be handled better in the future
                return true;
            }
        }

        public long GetActivityStartDate()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long) timeSpan.TotalSeconds - 7 * 24 * 60 * 60;
        }

        public async Task RefreshAccessTokenAsync(LeaderboardParticipant participant)
        {
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

                var result = await http.PostAsync(url, null);
                if (!result.IsSuccessStatusCode)
                    throw new StravaException($"Exchange code failed with status code {result.StatusCode}");

                var responseContent = await result.Content.ReadAsStringAsync();
                var responseContentSerialized = JObject.Parse(responseContent);

                participant.UpdateWithNewTokens( new StravaCodeExchangeResult(responseContentSerialized["access_token"].ToString(), responseContentSerialized["refresh_token"].ToString()));
                await _leaderboardRepository.Save(participant);
            }
        }


    }
}
