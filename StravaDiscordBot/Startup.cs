using System;
using System.Linq;
using System.Net.Http;
using Discord.Commands;
using Discord.WebSocket;
using IO.Swagger.Api;
using IO.Swagger.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using StravaDiscordBot.Discord;
using StravaDiscordBot.Discord.Modules;
using StravaDiscordBot.Models;
using StravaDiscordBot.Services;
using StravaDiscordBot.Services.HostedService;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var appOptions = new AppOptions();
            Configuration.Bind(appOptions);
            services.AddSingleton(appOptions);
            services.AddLogging(builder => builder.AddSerilog(dispose: true));

            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandlingService>();
            services.AddSingleton<HttpClient>();

            // Discord modules

            services.AddSingleton<PublicModule>();
            services.AddSingleton<AdminModule>();
            services.AddSingleton<AthleteModule>();
            services.AddSingleton<LeaderboardModule>();

            // Discord wrapper services

            services.AddSingleton<IRoleService, RoleService>();
            services.AddSingleton<IEmbedBuilderService, EmbedBuilderService>();

            // Storage

            services.AddDbContext<BotDbContext>(ServiceLifetime.Singleton);

            // Storage wrapper services

            services.AddSingleton<ILeaderboardParticipantService, LeaderboardParticipantService>();
            services.AddSingleton<ILeaderboardService, LeaderboardService>();
            services.AddSingleton<IStravaCredentialService, StravaCredentialService>();

            // Strava API CLient

            services.AddSingleton(typeof(Configuration), new Configuration());
            services.AddSingleton<IActivitiesApi, ActivitiesApi>();
            services.AddSingleton<IAthletesApi, AthletesApi>();

            // Strava wrapper services

            services.AddSingleton<IStravaAuthenticationService, StravaAuthenticationService>();
            services.AddSingleton<IActivitiesService, ActivitiesService>();
            services.AddSingleton<IAthleteService, AthleteService>();

            // Hosted services

            services.AddHostedService<WeeklyLeaderboardHostedService>();
            //services.AddHostedService<ParticipantCleanupHostedService>();
            services.AddHostedService<DiscordServerHostedService>();
            services.AddHostedService<ContainsReplyService>();

            // API

            services.AddControllers();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseEndpoints(x =>
            {
                x.MapGet("/strava/callback/{serverId}/{discordUserId}", async context =>
                {
                    if (!context.Request.Query.TryGetValue("scope", out var scope) ||
                        !context.Request.Query.TryGetValue("code", out var code))
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Invalid Query");
                        return;
                    }

                    var serverId = context.Request.RouteValues["serverId"].ToString();
                    var discordUserId = context.Request.RouteValues["discordUserId"].ToString();

                    var requestLogger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
                    var stravaAuthenticationService =
                        context.RequestServices.GetRequiredService<IStravaAuthenticationService>();
                    var athleteService = context.RequestServices.GetRequiredService<IAthleteService>();
                    var leaderboardParticipantService =
                        context.RequestServices.GetRequiredService<ILeaderboardParticipantService>();
                    var credentialService = context.RequestServices.GetRequiredService<IStravaCredentialService>();

                    if (scope.FirstOrDefault() == null || !scope.First()
                        .Contains("activity:read", StringComparison.InvariantCultureIgnoreCase))
                    {
                        requestLogger.LogInformation($"Insufficient scopes for {discordUserId}");
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync(
                            "Failed to authorize user, read activities permission is needed");
                        return;
                    }

                    try
                    {
                        var exchangeResult = await stravaAuthenticationService.ExchangeCodeAsync(code);
                        var athlete = await athleteService.Get(null, exchangeResult.AccessToken);
                        var participant =
                            leaderboardParticipantService.GetParticipantOrDefault(serverId, discordUserId);

                        if (participant == null)
                            await leaderboardParticipantService.CreateWithCredentials(
                                new LeaderboardParticipant(serverId, discordUserId, athlete.Id.ToString()),
                                exchangeResult);

                        await credentialService.UpsertTokens(athlete.Id.ToString(), exchangeResult);
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsync("You are now part of the leaderboard");
                    }
                    catch (ApiException e)
                    {
                        requestLogger.LogError(e, "Failed to authorize with strava");
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync(
                            $"Failed to authorize with Strava, error message: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        requestLogger.LogError(e, "Failed to create user with unknown error");
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync(e.Message);
                    }
                });
            });

            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using var serviceScope = serviceScopeFactory.CreateScope();
            serviceScope
                .ServiceProvider
                .GetService<ILogger<Startup>>()
                .LogInformation("Ensuring database is created for ");

            var dbContext = serviceScope.ServiceProvider.GetService<BotDbContext>();
            dbContext?.Database.EnsureCreated();
        }
    }
}