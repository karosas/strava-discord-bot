using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace StravaDiscordBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logBuilder = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console();

            Console.WriteLine($"Env: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

            if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "PRODUCTION", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine($"Log path: {Environment.GetEnvironmentVariable("LOG_PATH")}");
                logBuilder.WriteTo.File(Environment.GetEnvironmentVariable("LOG_PATH"));
            }

            Log.Logger = logBuilder.CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
