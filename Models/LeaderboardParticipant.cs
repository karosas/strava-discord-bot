using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using StravaDiscordBot.Models.Strava;

namespace StravaDiscordBot.Models
{
    public class LeaderboardParticipant : TableEntity
    {
        public string StravaAccessToken { get; set; }
        public string StravaRefreshToken { get; set; }
        public string DiscordMention => $"<@{RowKey}>";
        public LeaderboardParticipant() {}

        // Don't like amount of the constructor parameters, but good enough for now
        public LeaderboardParticipant(string channelId, string userId, string stravaAccessToken, string stravaRefreshToken)
        {
            PartitionKey = channelId;
            RowKey = userId;
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
