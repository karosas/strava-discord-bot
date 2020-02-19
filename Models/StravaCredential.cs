using System.ComponentModel.DataAnnotations;
using StravaDiscordBot.Models.Strava;

namespace StravaDiscordBot.Models
{
    public class StravaCredential
    {
        public StravaCredential() { }

        public StravaCredential(string stravaId, string accessToken, string refreshToken)
        {
            StravaId = stravaId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
        [Key]
        public string StravaId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        
        public void UpdateWithNewTokens(StravaOauthResponse result)
        {
            AccessToken = result.AccessToken;
            RefreshToken = result.RefreshToken;
        }
    }
}