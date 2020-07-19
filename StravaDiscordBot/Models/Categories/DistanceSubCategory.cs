using System;
using System.Collections.Generic;
using System.Linq;
using IO.Swagger.Model;
using StravaDiscordBot.Helpers;

namespace StravaDiscordBot.Models.Categories
{
    public class DistanceSubCategory : ISubCategory
    {
        public string Name => Constants.SubCategoryTypes.Distance;

        public ParticipantResult CalculateParticipantsResults(LeaderboardParticipant participant, List<SummaryActivity> activites)
        {
            var result = activites?.Sum(x => (x.Distance ?? 0d) / 1000) ?? 0;

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
