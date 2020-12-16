using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using IO.Swagger.Client;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Storage;
using StravaDiscordBot.Strava;

namespace StravaDiscordBot.Services
{
    public interface IStravaAuthenticationService
    {
        string GetOAuthUrl(string serverId, string discordUserId);
        (AsyncRetryPolicy policy, Context context) GetUnauthorizedPolicy(string stravaId);
        Task<StravaOauthResponse> ExchangeCodeAsync(string code);
        Task<StravaOauthResponse> RefreshAccessTokenAsync(string refreshToken);

    }

    public class StravaAuthenticationService : BaseStravaService, IStravaAuthenticationService
    {
        public const string StravaIdContextKey = "strava-id";
        private readonly ILogger<StravaAuthenticationService> _logger;
        private readonly AppOptions _options;
        private readonly DiscordSocketClient _socketClient;

        public StravaAuthenticationService(ILogger<StravaAuthenticationService> logger, AppOptions options, BotDbContext dbContext, DiscordSocketClient socketClient) : base(dbContext, logger)
        {
            _logger = logger;
            _options = options;
            _socketClient = socketClient;
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
                    {"scope", "read,activity:read,activity:read_all,profile:read_all,"}
                });
        }

        public async Task<StravaOauthResponse> ExchangeCodeAsync(string code)
        {
            Logger.LogInformation("Exchanging strava code");
            return await PostAsync<StravaOauthResponse>(QueryHelpers.AddQueryString(
                "https://www.strava.com/oauth/token",
                new Dictionary<string, string>
                {
                        {"client_id", _options.Strava.ClientId},
                        {"client_secret", _options.Strava.ClientSecret},
                        {"code", code},
                        {"grant_type", "authorization_code"}
                }
            ));
        }
        public async Task<StravaOauthResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            Logger.LogInformation("Refreshing access token");
            return await PostAsync<StravaOauthResponse>(QueryHelpers.AddQueryString(
               "https://www.strava.com/oauth/token",
               new Dictionary<string, string>
               {
                    {"client_id", _options.Strava.ClientId},
                    {"client_secret", _options.Strava.ClientSecret},
                    {"refresh_token", refreshToken},
                    {"grant_type", "refresh_token"}
               }));
        }

        public (AsyncRetryPolicy policy, Context context) GetUnauthorizedPolicy(string stravaId)
        {
            var pollyContext = new Context();
            pollyContext[StravaIdContextKey] = stravaId;

            return (Policy
                    .Handle<StravaException>()
                    .RetryAsync(1, OnUnauthorizedRetry),
                pollyContext);
        }

        private async Task OnUnauthorizedRetry(Exception e, int retryAttempt, Context context)
        {
            _logger.LogInformation("OnUnauthorizedRetry");
            // Try to refresh access token
            if (retryAttempt == 1 && context.ContainsKey(StravaIdContextKey) && context[StravaIdContextKey] is string stravaId)
            {
                _logger.LogInformation("First retry attempt, trying to refresh access token");
                var credentials = DbContext.Credentials.FirstOrDefault(x => x.StravaId == stravaId);
                if (credentials == null)
                {
                    Logger.LogError("Couldn't find credentials to refresh");
                    // Or throw?
                    return;
                }
                try
                {
                    var refreshResult = await RefreshAccessTokenAsync(credentials.RefreshToken);
                    credentials.UpdateWithNewTokens(refreshResult);
                    DbContext.Update(credentials);
                    await DbContext.SaveChangesAsync();
                    return;
                }
                catch (ApiException ex)
                {
                    _logger.LogWarning(ex, "Refreshing access token failed, DM'ing user to re-join leaderboard");

                    var participant = DbContext.Participants.FirstOrDefault(x => x.StravaId == stravaId);
                    if (participant != null && ulong.TryParse(participant.DiscordUserId, out var discordUserId))
                    {
                        var user = _socketClient.GetUser(discordUserId);
                        var channel = await user?.GetOrCreateDMChannelAsync();
                        await channel?.SendMessageAsync(
                        "Hey, I failed to refresh access to your Strava account. Please use `join` command again in the server of leaderboard.");
                    }
                    throw;
                }
            }

            Logger.LogWarning("Couldn't find stravaId inside Polly context");
        }

        private async Task<T> PostAsync<T>(string url)
        {
            using var http = new HttpClient();

            var response = await http.PostAsync(url, null).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Logger.LogInformation(
                $"Call to {GetUrlSuffixWithoutQuery(url)} - {response.StatusCode} Status code");

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(responseContent);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new ApiException((int) response.StatusCode, "Access token expired");

            Logger.LogError($"Failed call to strava - {response.StatusCode}");
            Logger.LogError(responseContent);
            throw new ApiException((int) response.StatusCode, "Unknown error");
        }

        private static string GetUrlSuffixWithoutQuery(string urlSuffix)
        {
            return urlSuffix.Contains("?") ? urlSuffix.Split("?")[0] : urlSuffix;
        }
    }
}
