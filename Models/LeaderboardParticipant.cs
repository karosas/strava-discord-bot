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
        public string ServerId { get; set; }
        public string StravaAccessToken { get; set; }
        public string StravaRefreshToken { get; set; }
        public string GetDiscordMention(bool silent = false) => silent ? $"`<@{DiscordUserId}>`" : $"<@{DiscordUserId}>";

        public LeaderboardParticipant() {}
        public LeaderboardParticipant(string serverId, string userId, string stravaAccessToken, string stravaRefreshToken)
        {
            DiscordUserId = userId;
            StravaAccessToken = stravaAccessToken;
            StravaRefreshToken = stravaRefreshToken;
            ServerId = serverId;
        }

        public void UpdateWithNewTokens(StravaCodeExchangeResult result)
        {
            StravaAccessToken = result.AccessToken;
            StravaRefreshToken = result.RefreshToken;
        }
    }
}
