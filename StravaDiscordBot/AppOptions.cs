// ReSharper disable ClassNeverInstantiated.Global

namespace StravaDiscordBot
{
    public class AppOptions
    {
        public string ConnectionString { get; set; } = "Data Source=leaderboard.db;";
        public DiscordOptions Discord { get; set; }
        public StravaOptions Strava { get; set; }
        public LokiOptions Loki { get; set; }
        public string BaseUrl { get; set; }
        public bool LogToConsole { get; set; }
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

    public class LokiOptions
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}