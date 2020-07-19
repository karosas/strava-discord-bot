using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO.Swagger.Model;

namespace StravaDiscordBot.Models.Categories
{
    public interface ICategory
    {
        /// <summary>
        ///     Display name for the category
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Filter out unwanted activities (e.g. by activity type)
        /// </summary>
        /// <param name="activities">Unfiltered activities for leaderboard period</param>
        /// <returns>Filtered activity list that are sanitized for this category</returns>
        List<SummaryActivity> FilterActivities(List<SummaryActivity> activities);

        /// <summary>
        ///     List of subcategories for this category
        /// </summary>
        List<ISubCategory> SubCategories { get; }
    }
}
