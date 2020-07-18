using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO.Swagger.Model;
using StravaDiscordBot.Helpers;

namespace StravaDiscordBot.Models.Categories
{
    public class VirtualRideCategory : ICategory
    {
        public string Name => Constants.CategoryTypes.VirtualRide;

        public List<ISubCategory> SubCategories => new List<ISubCategory>
        {
            new DistanceSubCategory(),
            new ElevationSubCategory(),
            new PowerSubCategory(),
            new SingleRideDistanceSubCategory()
        };

        public List<SummaryActivity> FilterActivities(List<SummaryActivity> activities)
        {
            if (activities?.Any() ?? false)
                return activities.Where(x => x.Type == ActivityType.VirtualRide).ToList();

            return new List<SummaryActivity>();
        }
    }
}
