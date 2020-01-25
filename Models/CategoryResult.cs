using System.Collections.Generic;

namespace StravaDiscordBot.Models
{
    public class CategoryResult
    {
        public Dictionary<string, List<ParticipantResult>> ChallengeByChallengeResultDictionary { get; set; }
    }
}
