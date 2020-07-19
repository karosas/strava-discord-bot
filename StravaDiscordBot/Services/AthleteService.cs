using System;
using System.Threading.Tasks;
using IO.Swagger.Api;
using IO.Swagger.Model;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services
{
    public interface IAthleteService
    {
        Task<DetailedAthlete> Get(string stravaId, string accessTokenOverride = null);
    }

    public class AthleteService : BaseStravaService, IAthleteService
    {
        private readonly IAthletesApi _athletesApi;

        public AthleteService(IAthletesApi athletesApi, BotDbContext dbContext, ILogger<AthleteService> logger) : base(dbContext, logger)
        {
            _athletesApi = athletesApi;
        }

        public async Task<DetailedAthlete> Get(string stravaId, string accessTokenOverride = null)
        {
            try
            {
                Logger.LogInformation($"Fetching athlete for strava id {stravaId}");
                
                string accessToken = accessTokenOverride;
                if(accessToken == null)
                {
                    var credentials = await GetCredentials(stravaId);
                    accessToken = credentials.AccessToken;
                }

                _athletesApi.Configuration.AccessToken = accessToken;
                return await _athletesApi.GetLoggedInAthleteAsync();
            } 
            catch(Exception e)
            {
                Logger.LogError(e, "Failed to fetch athlete");
                _athletesApi.Configuration.AccessToken = string.Empty;
                throw;
            }
        }
    }
}
