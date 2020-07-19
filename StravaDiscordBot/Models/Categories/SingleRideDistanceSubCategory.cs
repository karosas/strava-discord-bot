using System.Collections.Generic;
using System.Linq;
using IO.Swagger.Model;
using StravaDiscordBot.Helpers;

namespace StravaDiscordBot.Models.Categories
{
    public class SingleRideDistanceSubCategory : ISubCategory
    {
        public string Name => Constants.SubCategoryTypes.DistanceRide;

        public ParticipantResult CalculateParticipantsResults(LeaderboardParticipant participant, List<SummaryActivity> activities)
        {
            var result = 0d;
            if (activities?.Any() ?? false)
                result = activities.Select(x => (x.Distance ?? 0d) / 1000)
                       .DefaultIfEmpty()
                       .Max();

            return new ParticipantResult(participant, result, string.Format("{0:0.##} km", result));
        }

        public SubCategoryResult CalculateTotalResult(List<ParticipantResult> participantResults)
        {
            return new SubCategoryResult
            {
                Name = Name,
                OrderedParticipantResults = participantResults?.OrderByDescending(x => x.Value).ToList()
            };
        }
    }
}
