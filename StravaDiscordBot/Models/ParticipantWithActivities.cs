using System.Collections.Generic;
using IO.Swagger.Model;

namespace StravaDiscordBot.Models
{
    public class ParticipantWithActivities
    {
        public LeaderboardParticipant Participant { get; set; }
        public List<SummaryActivity> Activities { get; set; }
    }
}
