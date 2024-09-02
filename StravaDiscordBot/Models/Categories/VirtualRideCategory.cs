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
        public const string IndoorRideName = "Indoor Cycling";

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
                return activities.Where(ActivityIsVirtual).ToList();

            return new List<SummaryActivity>();
        }

        public static bool ActivityIsVirtual(SummaryActivity activity)
        {
            if(activity.Type == ActivityType.VirtualRide)
                return true;

            if(activity.Name == IndoorRideName)
                return true;

            return false;
        }
    }
}
