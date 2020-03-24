using System.Collections.Generic;

namespace StravaDiscordBot.Models
{
    public class CategoryResult
    {
        public string Name { get; set; }
        public Dictionary<string, List<ParticipantResult>> ChallengeByChallengeResultDictionary { get; set; }
    }
}
