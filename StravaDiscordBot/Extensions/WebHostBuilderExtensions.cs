using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Discord;
using Serilog.Sinks.Grafana.Loki;
using LokiCredentials = Serilog.Sinks.Grafana.Loki.LokiCredentials;

namespace StravaDiscordBot.Extensions
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseSerilog(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder
                .UseSerilog((builderContext, loggerConfig) =>
                {
                    loggerConfig.Enrich.FromLogContext();
                    loggerConfig.Enrich.WithExceptionDetails();
                    loggerConfig.Enrich.WithProperty("Environment", builderContext.HostingEnvironment.EnvironmentName);
                    loggerConfig.Enrich.WithProperty("Type", "DiscordStravaBot");
                    loggerConfig.MinimumLevel.Debug();

                    var options = new AppOptions();
                    builderContext.Configuration.Bind(options);

                    if (options.Loki != null)
                    {
                        loggerConfig.WriteTo.GrafanaLoki(options.Loki.Url,
                            new[] {new LokiLabel {Key = "Project", Value = "strava-discord-bot"}},
                            credentials: new LokiCredentials
                            {
                                Login = options.Loki.User,
                                Password = options.Loki.Password
                            });
                    }

                    if (options.Discord?.LogWebhooks != null)
                    {
                        foreach (var webhook in options.Discord.LogWebhooks)
                        {
                            loggerConfig.WriteTo.Discord(webhook.Id, webhook.Token);
                        }
                    }

                    if (options.LogToConsole)
                        loggerConfig.WriteTo.Console();
                });
        }
    }
}