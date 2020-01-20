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
        public RemoteItOptions Remote { get; set; }
        public string StorageConnectionString { get; set; }
        public string BaseUrl { get; set; }
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

    public class RemoteItOptions
    {
        public string DeveloperKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DeviceServiceId { get; set; }
        public string DeviceName { get; set; }
    }
}
