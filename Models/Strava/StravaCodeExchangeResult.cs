using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StravaDiscordBot.Models.Strava
{
    public class StravaCodeExchangeResult
    {
        public StravaCodeExchangeResult(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; }
        public string RefreshToken { get; }
    }
}
