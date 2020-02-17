using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace StravaDiscordBot.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseSerilog(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .UseSerilog((builderContext, loggerConfig) =>
                {
                    loggerConfig.Enrich.FromLogContext();
                    loggerConfig.Enrich.WithProperty("Environment", builderContext.HostingEnvironment.EnvironmentName);
                    loggerConfig.Enrich.WithProperty("Type", "DiscordStravaBot");
                    loggerConfig.MinimumLevel.Debug();

                    var options = new AppOptions();
                    builderContext.Configuration.Bind(options);

                    if (options.Humio != null && !string.IsNullOrEmpty(options.Humio.Token) &&
                        !string.IsNullOrEmpty(options.Humio.Url))
                    {
                        loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(options.Humio.Url))
                        {
                            MinimumLogEventLevel = LogEventLevel.Debug,
                            ModifyConnectionSettings = connConfig =>
                                connConfig.BasicAuthentication(options.Humio.Token, ""),
                            Period = TimeSpan.FromMilliseconds(500)
                        });
                    }

                    if (builderContext.HostingEnvironment.IsDevelopment())
                    {
                        loggerConfig.WriteTo.Console();
                    }
                });
        }
    }
}