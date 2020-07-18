using System.Collections.Generic;
using System.Linq;
using IO.Swagger.Model;
using StravaDiscordBot.Helpers;

namespace StravaDiscordBot.Models.Categories
{
    public class ElevationSubCategory : ISubCategory
    {
        public string Name => Constants.SubCategoryTypes.Elevation;

        public ParticipantResult CalculateParticipantsResults(LeaderboardParticipant participant, List<SummaryActivity> activities)
        {
            var result = activities?.Sum(x => x.TotalElevationGain ?? 0d) ?? 0;
            return new ParticipantResult(participant, result, string.Format("{0:0.#} m", result));
        }

        public SubCategoryResult CalculateTotalResult(List<ParticipantResult> participantResults)
        {
            return new SubCategoryResult
            {
                Name = Name,
                OrderedParticipantResults = participantResults?.OrderBy(x => x.Value).ToList()
            };
        }
    }
}
