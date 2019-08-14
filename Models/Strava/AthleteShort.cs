using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StravaDiscordBot.Models.Strava
{
    public class AthleteShort
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("resource_state")]
        public long ResourceState { get; set; }
    }
}
