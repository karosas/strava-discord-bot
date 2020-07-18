using System;
using System.Collections.Generic;
using System.Linq;
using IO.Swagger.Model;
using StravaDiscordBot.Helpers;

namespace StravaDiscordBot.Models.Categories
{
    public class RealRideCategory : ICategory
    {
        public string Name => Constants.CategoryTypes.RealRide;

        public List<ISubCategory> SubCategories => new List<ISubCategory>
        {
            new DistanceSubCategory(),
            new ElevationSubCategory(),
            new PowerSubCategory(),
            new SingleRideDistanceSubCategory()
        };

        public List<SummaryActivity> FilterActivities(List<SummaryActivity> activities)
        {
            if(activities?.Any() ?? false)
                return activities.Where(x => x.Type == ActivityType.Ride).ToList();

            return new List<SummaryActivity>();
        }
    }
}
