using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StravaDiscordBot.Helpers
{
    public static class OutputFormatters
    {
        public static string PlaceToEmote(int place)
        {
            switch (place)
            {
                case 1:
                    return "🥇";
                case 2:
                    return "🥈";
                case 3:
                    return "🥉";
                default:
                    return place.ToString();
            }
        }

        public static string ParticipantResultForChallenge(string challenge, double value)
        {
            switch (challenge)
            {
                case Constants.ChallengeType.Distance:
                case Constants.ChallengeType.DistanceRide:
                    return $"{value:n1} km";
                case Constants.ChallengeType.Elevation:
                    return $"{value:n0} m";
                case Constants.ChallengeType.Power:
                    return $"{value:n0} W";
                default:
                    return $"{value:n1}";
            }
        }
    }
}
