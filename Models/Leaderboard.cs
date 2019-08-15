using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StravaDiscordBot.Models.Strava;

namespace StravaDiscordBot.Models
{
    public class Leaderboard
    {
        private Dictionary<LeaderboardParticipant, List<DetailedActivity>> _participantActivityDictionary;
        public Leaderboard(Dictionary<LeaderboardParticipant, List<DetailedActivity>> participantActivityDictionary)
        {
            _participantActivityDictionary = participantActivityDictionary;
        }

        public Dictionary<LeaderboardParticipant, double> GetTopDistances()
        {
            return _participantActivityDictionary
                .Select((keyValue) => new KeyValuePair<LeaderboardParticipant, double>(keyValue.Key, keyValue.Value.Sum(x => x.Distance)))
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
