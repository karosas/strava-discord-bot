using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace StravaDiscordBot.Models
{
    public class LeaderboardParticipant : TableEntity
    {
        public string StravaId { get; set; }
        public LeaderboardParticipant() {}
        public LeaderboardParticipant(string channelId, string userId, string stravaId)
        {
            PartitionKey = channelId;
            RowKey = userId;
            StravaId = stravaId;
        }
    }
}
