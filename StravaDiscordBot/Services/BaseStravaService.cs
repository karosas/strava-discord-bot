using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Models;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services
{
    public abstract class BaseStravaService
    {
        protected readonly BotDbContext DbContext;
        protected readonly ILogger Logger;

        public BaseStravaService(BotDbContext dbContext, ILogger logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }

        protected async Task<StravaCredential> GetCredentials(string stravaId)
        {
            Logger.LogInformation($"Fetching credentials for strava user {stravaId}");
            return await DbContext.Credentials.FindAsync(stravaId);
        }
    }
}
