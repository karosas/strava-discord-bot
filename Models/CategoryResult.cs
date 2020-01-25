using System.Collections.Generic;

namespace StravaDiscordBot.Models
{
    public class CategoryResult
    {
        public List<ParticipantResult> Distance { get; set; }
        public List<ParticipantResult> Altitude { get; set; }
        public List<ParticipantResult> Power { get; set; }
    }
}
