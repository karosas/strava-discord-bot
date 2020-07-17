using System.ComponentModel.DataAnnotations;

namespace StravaDiscordBot.Models
{
    public class Leaderboard
    {
        [Key] public string ServerId { get; set; }

        public string ChannelId { get; set; }
    }
}