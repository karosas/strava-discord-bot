using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO.Swagger.Api;
using IO.Swagger.Model;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Extensions;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services
{
    public interface IActivitiesService
    {
        Task<List<SummaryActivity>> GetForStravaUser(string stravaId, DateTime after);
    }

    public class ActivitiesService : BaseStravaService, IActivitiesService
    {
        private readonly IActivitiesApi _activitiesApi;

        public ActivitiesService(BotDbContext dbContext, ILogger<ActivitiesService> logger, IActivitiesApi activitiesApi) : base(dbContext, logger)
        {
            _activitiesApi = activitiesApi;
        }

        public async Task<List<SummaryActivity>> GetForStravaUser(string stravaId, DateTime after)
        {
            var credentials = await GetCredentials(stravaId);
            try
            {
                Logger.LogInformation($"Fetching activities for strava id {stravaId}");

                // This is going to end up in being a race condition, won't it...?
                _activitiesApi.Configuration.AccessToken = credentials.AccessToken;

                // It is a bit silly not to implement pagination properly, but no user should have more than 100 activities done in a week for leaderboard... hopefully..
                return await _activitiesApi.GetLoggedInAthleteActivitiesAsync(after: (int) after.GetEpochTimestamp(), perPage: 100);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to fetch activities for strava id {stravaId}");
                _activitiesApi.Configuration.AccessToken = string.Empty;
                throw;
            }
        }
    }
}
