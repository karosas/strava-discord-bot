using Newtonsoft.Json;

namespace StravaDiscordBot.Models.Strava
{
    public class AthleteShort
    {
        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("resource_state")] public long? ResourceState { get; set; }
    }
}