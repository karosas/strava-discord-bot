using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models.Strava;

namespace StravaDiscordBot.Discord
{
    public interface IStravaApiClientService
    {
        Task<StravaCodeExchangeResult> ExchangeCodeAsync(string code);
        Task<List<DetailedActivity>> FetchActivities(string accessToken, string refreshToken, DateTime after, int itemsPerPage = 100);
        Task<(string accessToken, string refreshToken)> RefreshAccessTokenAsync(string refreshToken);
    }
    public class StravaApiClientService : IStravaApiClientService
    {
        private readonly ILogger<StravaApiClientService> _logger;
        private readonly StravaOptions _options;
        private readonly string _botBaseUrl;

        public StravaApiClientService(ILogger<StravaApiClientService> logger, AppOptions options)
        {
            _logger = logger;
            _options = options.Strava;
            _botBaseUrl = options.BaseUrl;
        }
        public async Task<StravaCodeExchangeResult> ExchangeCodeAsync(string code)
        {
            using (var http = new HttpClient())
            {
                var url = QueryHelpers.AddQueryString("https://www.strava.com/oauth/token",
                    new Dictionary<string, string>
                    {
                        { "client_id", _options.ClientId },
                        { "client_secret", _options.ClientSecret },
                        { "code", code },
                        { "grant_type", "authorization_code" }
                   });

                var result = await http.PostAsync(url, null).ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogError($"Strava code exchange failed, Status: {result.StatusCode}");
                    throw new StravaException(StravaException.StravaErrorType.Unknown, $"Exchange code failed with status code {result.StatusCode}");
                }

                var responseContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseContentSerialized = JObject.Parse(responseContent);
                return new StravaCodeExchangeResult(responseContentSerialized["access_token"].ToString(), responseContentSerialized["refresh_token"].ToString());
            }
        }

        public async Task<List<DetailedActivity>> FetchActivities(string accessToken, string refreshToken, DateTime after, int itemsPerPage = 100)
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var url = QueryHelpers.AddQueryString("https://www.strava.com/api/v3/athlete/activities", new Dictionary<string, string>
                {
                    { "after", $"{(int)(after - new DateTime(1970, 1, 1)).TotalSeconds}" },
                    { "per_page", "100" }
                });

                var activityResponse = await http.GetAsync(url).ConfigureAwait(false);

                if (!activityResponse.IsSuccessStatusCode)
                {
                    if (activityResponse.StatusCode == HttpStatusCode.Unauthorized)
                        throw new StravaException(StravaException.StravaErrorType.Unauthorized, $"Access token expired");
                    
                    _logger.LogError($"Failed to fetch activities");
                    throw new StravaException(StravaException.StravaErrorType.Unknown, $"Failed to fetch activities, status code: {activityResponse.StatusCode}");
                }

                var activityResponseContent = await activityResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                var activitiesDeserialized = JsonConvert.DeserializeObject<List<DetailedActivity>>(activityResponseContent);
                _logger.LogInformation($"Fetched {activitiesDeserialized.Count} activities");
                return activitiesDeserialized.ToList();
            }
        }

        public async Task<(string accessToken, string refreshToken)> RefreshAccessTokenAsync(string refreshToken)
        {
            using (var http = new HttpClient())
            {
                var url = QueryHelpers.AddQueryString("https://www.strava.com/oauth/token",
                    new Dictionary<string, string>
                    {
                        { "client_id", _options.ClientId },
                        { "client_secret", _options.ClientSecret },
                        { "refresh_token", refreshToken },
                        { "grant_type", "refresh_token" }
                   });

                var result = await http.PostAsync(url, null).ConfigureAwait(false);
                if (!result.IsSuccessStatusCode)
                    throw new StravaException(StravaException.StravaErrorType.Unknown, $"Exchange code failed with status code {result.StatusCode}");

                var responseContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseContentSerialized = JObject.Parse(responseContent);

                return (responseContentSerialized["access_token"].ToString(), responseContentSerialized["refresh_token"].ToString());
            }
        }
    }
}
