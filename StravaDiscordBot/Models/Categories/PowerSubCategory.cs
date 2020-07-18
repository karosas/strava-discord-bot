using System;
using System.Collections.Generic;
using System.Linq;
using IO.Swagger.Model;
using StravaDiscordBot.Helpers;

namespace StravaDiscordBot.Models.Categories
{
    public class PowerSubCategory : ISubCategory
    {
        public string Name => Constants.SubCategoryTypes.Power;

        public ParticipantResult CalculateParticipantsResults(LeaderboardParticipant participant, List<SummaryActivity> activities)
        {
            var result = 0d;
            if (activities?.Any() ?? false)
                result = activities?.Where(x => (x.ElapsedTime ?? 0d) > 20 * 60)
                        .Select(x => x.WeightedAverageWatts ?? 0)
                        .DefaultIfEmpty()
                        .Max() ?? 0;

            return new ParticipantResult(participant, result, string.Format("{0:0} W", result));
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
