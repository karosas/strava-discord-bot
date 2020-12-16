namespace StravaDiscordBot.Models
{
    public class LeaderboardParticipant
    {
        public LeaderboardParticipant()
        {
        }

        public LeaderboardParticipant(string serverId, string userId, string stravaId)
        {
            DiscordUserId = userId;
            ServerId = serverId;
            StravaId = stravaId;
        }

        public string DiscordUserId { get; set; }
        public string ServerId { get; set; }
        public string StravaId { get; set; }

        public string GetDiscordMention(bool silent = false)
        {
            return silent ? $"`<@{DiscordUserId}>`" : $"<@!${DiscordUserId}>";
        }
    }
}