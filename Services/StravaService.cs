using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
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

        public string GetOAuthUrl(string channelId, string discordUserId)
        {
            //http://www.strava.com/oauth/authorize?client_id=[REPLACE_WITH_YOUR_CLIENT_ID]&response_type=code&redirect_uri=http://localhost/exchange_token&approval_prompt=force&scope=read
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
    }
}
