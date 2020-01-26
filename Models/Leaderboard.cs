using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StravaDiscordBot.Models
{
    public class Leaderboard
    {
        [Key]
        public string ServerId { get; set; }
        public string ChannelId { get; set; }
    }
}
