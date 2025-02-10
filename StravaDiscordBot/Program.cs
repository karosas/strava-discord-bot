using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using StravaDiscordBot.Extensions;
using StravaDiscordBot.Helpers;

namespace StravaDiscordBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.Add(new DotEnvConfigurationSource());
                })
                .UseStartup<Startup>();
        }
    }
}