using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StravaDiscordBot
{
    public class AppOptions
    {
        public DiscordOptions Discord { get; set; }
        public StravaOptions Strava { get; set; }
        public string StorageConnectionString { get; set; }
    }

    public class DiscordOptions
    {
        public string Token { get; set; }
    }
    public class StravaOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
