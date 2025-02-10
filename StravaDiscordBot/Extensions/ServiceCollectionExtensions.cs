using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Discord;
using Serilog.Sinks.Grafana.Loki;
using LokiCredentials = Serilog.Sinks.Grafana.Loki.LokiCredentials;

namespace StravaDiscordBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSerilogX(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddSerilog((builderContext, loggerConfig) =>
                {
                    loggerConfig.Enrich.FromLogContext();
                    loggerConfig.Enrich.WithExceptionDetails();
                    loggerConfig.MinimumLevel.Debug();

                    var options = new AppOptions();
                    configuration.Bind(options);

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

                    if (options.LogToConsole)
                        loggerConfig.WriteTo.Console();
                });
        }
    }
}