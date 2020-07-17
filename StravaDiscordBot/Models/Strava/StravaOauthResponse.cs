using Newtonsoft.Json;

namespace StravaDiscordBot.Models.Strava
{
    public class StravaOauthResponse
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; }

        [JsonProperty("refresh_token")] public string RefreshToken { get; set; }
    }
}