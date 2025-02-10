using dotenv.net;
using Microsoft.Extensions.Configuration;

namespace StravaDiscordBot.Helpers
{
    public class DotEnvConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DotEnvConfigurationProvider();
        }
    }

    public class DotEnvConfigurationProvider : ConfigurationProvider
    {
        public override void Load()
        {
            DotEnv.Load();
            foreach (var (key, value) in DotEnv.Read())
            {
                Data.Add(key, value);
            }
        }
    }
}