using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using StravaDiscordBot.Models.Strava;

namespace StravaDiscordBot.Models
{
    public class LeaderboardParticipant
    {
        [Key]
        public string DiscordUserId { get; set; }
        public string ChannelId { get; set; }
        public string StravaAccessToken { get; set; }
        public string StravaRefreshToken { get; set; }
        public string GetDiscordMention(bool silent = false) => silent ? $"`<@{DiscordUserId}>`" : $"<@{DiscordUserId}>";
        public LeaderboardParticipant() {}

        // Don't like amount of the constructor parameters, but good enough for now
        public LeaderboardParticipant(string channelId, string userId, string stravaAccessToken, string stravaRefreshToken)
        {
            ChannelId = channelId;
            DiscordUserId = userId;
            StravaAccessToken = stravaAccessToken;
            StravaRefreshToken = stravaRefreshToken;
        }

        public void UpdateWithNewTokens(StravaCodeExchangeResult result)
        {
            StravaAccessToken = result.AccessToken;
            StravaRefreshToken = result.RefreshToken;
        }
    }
}
