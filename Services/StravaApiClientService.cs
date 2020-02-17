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
        Task<StravaOauthResponse> ExchangeCodeAsync(string code);
        Task<AthleteDetailed> GetAthlete(string accessToken);
        Task<List<DetailedActivity>> FetchActivities(string accessToken, DateTime after, int itemsPerPage = 100);
        Task<StravaOauthResponse> RefreshAccessTokenAsync(string refreshToken);
    }
    public class StravaApiClientService : IStravaApiClientService
    {
        private const string STRAVA_BASE_URL = "https://www.strava.com";

        private readonly ILogger<StravaApiClientService> _logger;
        private readonly StravaOptions _options;

        public StravaApiClientService(ILogger<StravaApiClientService> logger, AppOptions options)
        {
            _logger = logger;
            _options = options.Strava;
        }
        public async Task<StravaOauthResponse> ExchangeCodeAsync(string code)
        {
            return await PostAsync<StravaOauthResponse>(QueryHelpers.AddQueryString("https://www.strava.com/oauth/token",
                    new Dictionary<string, string>
                    {
                        { "client_id", _options.ClientId },
                        { "client_secret", _options.ClientSecret },
                        { "code", code },
                        { "grant_type", "authorization_code" }
                   }));
        }

        public async Task<List<DetailedActivity>> FetchActivities(string accessToken, DateTime after, int itemsPerPage = 100)
        {
            return await GetAsync<List<DetailedActivity>>($"/api/v3/athlete/activities?after={(int) (after - new DateTime(1970, 1, 1)).TotalSeconds}&per_page={itemsPerPage}", accessToken);
        }

        public async Task<AthleteDetailed> GetAthlete(string accessToken)
        {
            return await GetAsync<AthleteDetailed>($"/api/v3/athlete", accessToken);
        }

        public async Task<StravaOauthResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            return await PostAsync<StravaOauthResponse>(QueryHelpers.AddQueryString("https://www.strava.com/oauth/token",
                    new Dictionary<string, string>
                    {
                        { "client_id", _options.ClientId },
                        { "client_secret", _options.ClientSecret },
                        { "refresh_token", refreshToken },
                        { "grant_type", "refresh_token" }
                   }), null);
        }

        public async Task<T> GetAsync<T>(string urlSuffix, string accessToken)
        {
            using (var http = new HttpClient { BaseAddress = new Uri(STRAVA_BASE_URL) })
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await http.GetAsync(urlSuffix).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        throw new StravaException(StravaException.StravaErrorType.Unauthorized, $"Access token expired");

                    _logger.LogError($"Failed call to strava - {response.StatusCode}");
                    _logger.LogError(responseContent);
                    throw new StravaException(StravaException.StravaErrorType.Unknown, $"Failed to get from strava, status code: {response.StatusCode}");
                }

                return JsonConvert.DeserializeObject<T>(responseContent);
            }
        }

        public async Task<T> PostAsync<T>(string urlSuffix, string accessToken = null)
        {
            using (var http = new HttpClient { BaseAddress = new Uri(STRAVA_BASE_URL) })
            {
                if (accessToken != null)
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await http.PostAsync(urlSuffix, null).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        throw new StravaException(StravaException.StravaErrorType.Unauthorized, $"Access token expired");

                    _logger.LogError($"Failed call to strava - {response.StatusCode}");
                    _logger.LogError(responseContent);
                    throw new StravaException(StravaException.StravaErrorType.Unknown, $"Failed to post to strava, status code: {response.StatusCode}");
                }


                return JsonConvert.DeserializeObject<T>(responseContent);
            }
        }
    }
}
