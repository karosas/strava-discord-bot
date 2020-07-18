using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Models;
using StravaDiscordBot.Storage;
using StravaDiscordBot.Strava;

namespace StravaDiscordBot.Services
{
    public interface IStravaCredentialService
    {
        Task<StravaCredential> GetByStravaId(string stravaId);
        Task UpsertTokens(string stravaId, StravaOauthResponse stravaOauthResponse);
    }

    public class StravaCredentialService : BaseStravaService, IStravaCredentialService
    {
        public StravaCredentialService(BotDbContext dbContext, ILogger<StravaCredentialService> logger) : base(dbContext, logger)
        {
        }

        public Task<StravaCredential> GetByStravaId(string stravaId)
        {
            return GetCredentials(stravaId);
        }

        public async Task UpsertTokens(string stravaId, StravaOauthResponse stravaOauthResponse)
        {
            var existing = await GetCredentials(stravaId);
            if (existing == null)
            {
                DbContext.Credentials.Add(new StravaCredential(stravaId, stravaOauthResponse.AccessToken, stravaOauthResponse.RefreshToken));
            }
            else
            {
                existing.UpdateWithNewTokens(stravaOauthResponse);
                DbContext.Credentials.Update(existing);
            }
            await DbContext.SaveChangesAsync();
        }
    }
}
