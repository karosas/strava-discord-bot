using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StravaDiscordBot
{
    public class AppOptions
    {
        public string ConnectionString { get; set; }
        public DiscordOptions Discord { get; set; }
        public StravaOptions Strava { get; set; }
        public string BaseUrl { get; set; }
        public HumioOptions Humio { get; set; }
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

    public class HumioOptions
    {
        public string Token { get; set; }
    }
}
