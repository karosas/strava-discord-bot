using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Humio;

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
                    loggerConfig.Enrich.WithProperty("Environment", builderContext.HostingEnvironment.EnvironmentName);
                    loggerConfig.Enrich.WithProperty("Type", "DiscordStravaBot");
                    loggerConfig.MinimumLevel.Debug();

                    var options = new AppOptions();
                    builderContext.Configuration.Bind(options);

                    if (options.Humio != null && !string.IsNullOrEmpty(options.Humio.Token))
                        loggerConfig.WriteTo.HumioSink(new HumioSinkConfiguration
                        {
                            BatchSizeLimit = 50,
                            Period = TimeSpan.FromMilliseconds(1000),
                            IngestToken = options.Humio.Token
                        });

                    loggerConfig.WriteTo.Console();
                });
        }
    }
}