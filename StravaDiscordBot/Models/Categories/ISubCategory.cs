using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO.Swagger.Model;

namespace StravaDiscordBot.Models.Categories
{
    public interface ISubCategory
    {
        /// <summary>
        ///     Display name of subcategory
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Calculates single participant's result for this specific leaderboard's subcategory
        /// </summary>
        /// <param name="participant">Participant of the leaderboard</param>
        /// <param name="activites">Participant's activities that are already pre-filtered for parent category and timespan of the leaderboard</param>
        /// <returns>ParticipantResult with participant and string result</returns>
        public ParticipantResult CalculateParticipantsResults(LeaderboardParticipant participant, List<SummaryActivity> activities);

        /// <summary>
        ///     Calculates final subcategory result
        /// </summary>
        /// <param name="participantResults">Results of all participants</param>
        /// <returns>SubcategoryResult with correctly ordered results</returns>
        public SubCategoryResult CalculateTotalResult(List<ParticipantResult> participantResults);
    }
}
