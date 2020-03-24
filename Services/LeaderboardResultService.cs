using System;
using System.Collections.Generic;
using System.Linq;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;

namespace StravaDiscordBot.Services
{
    public interface ILeaderboardResultService
    {
        CategoryResult GetTopResultsForCategory(
            Dictionary<LeaderboardParticipant, List<DetailedActivity>> participantActivitiesDict,
            string categoryName,
            Func<DetailedActivity, bool> activityFilter);
    }
    
    public class LeaderboardResultService : ILeaderboardResultService
    {
        public CategoryResult GetTopResultsForCategory(
            Dictionary<LeaderboardParticipant, List<DetailedActivity>> participantActivitiesDict,
            string categoryName,
            Func<DetailedActivity, bool> activityFilter)
        {
            var distanceResult = new List<ParticipantResult>();
            var altitudeResult = new List<ParticipantResult>();
            var powerResult = new List<ParticipantResult>();
            var singleLongestRideResult = new List<ParticipantResult>();

            foreach (var (participant, participantActivities) in participantActivitiesDict)
            {
                var matchingActivities = participantActivities
                    .Where(activityFilter)
                    .ToList();

                distanceResult
                    .Add(new ParticipantResult(participant,
                        matchingActivities.Sum(x => (x.Distance ?? 0d) / 1000))); // meters to km 

                altitudeResult
                    .Add(new ParticipantResult(participant, matchingActivities
                        .Sum(x => (x.TotalElevationGain ?? 0d))));

                powerResult
                    .Add(new ParticipantResult(participant, matchingActivities
                        .Where(x => (x.ElapsedTime ?? 0d) > 20 * 60)
                        .Select(x => (x.WeightedAverageWatts ?? 0))
                        .DefaultIfEmpty()
                        .Max()));

                singleLongestRideResult
                    .Add(new ParticipantResult(participant, matchingActivities
                        .Select(x => (x.Distance ?? 0d) / 1000)
                        .DefaultIfEmpty()
                        .Max()));
            }

            return new CategoryResult
            {
                Name = categoryName,
                ChallengeByChallengeResultDictionary = new Dictionary<string, List<ParticipantResult>>
                {
                    {Constants.ChallengeType.Distance, distanceResult.OrderByDescending(x => x.Value).ToList()},
                    {Constants.ChallengeType.Elevation, altitudeResult.OrderByDescending(x => x.Value).ToList()},
                    {Constants.ChallengeType.Power, powerResult.OrderByDescending(x => x.Value).ToList()},
                    {
                        Constants.ChallengeType.DistanceRide,
                        singleLongestRideResult.OrderByDescending(x => x.Value).ToList()
                    }
                }
            };
        }
    }
}