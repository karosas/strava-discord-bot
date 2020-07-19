using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using IO.Swagger.Api;
using IO.Swagger.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using StravaDiscordBot.Discord;
using StravaDiscordBot.Discord.Modules;
using StravaDiscordBot.Services;
using StravaDiscordBot.Services.HostedService;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot
{
    public class Startup
    {
        private ILogger<Startup> _logger;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        private DiscordSocketClient DiscordClient { get; set; }
        private CommandHandlingService CommandHandlingService { get; set; }

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

            app.UseEndpoints(x => { x.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"); });

            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using (var serviceScope = serviceScopeFactory.CreateScope())
            {
                serviceScope
                    .ServiceProvider
                    .GetService<ILogger<Startup>>()
                    .LogInformation("Ensuring database is created for ");

                var dbContext = serviceScope.ServiceProvider.GetService<BotDbContext>();
                dbContext.Database.EnsureCreated();
            }

            StartDiscordBot(app, logger)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        private async Task StartDiscordBot(IApplicationBuilder app, ILogger<Startup> logger)
        {
            _logger = logger;
            var options = app.ApplicationServices.GetService<AppOptions>();
            DiscordClient = app.ApplicationServices.GetRequiredService<DiscordSocketClient>();
            DiscordClient.Log += LogAsync;
            app.ApplicationServices.GetRequiredService<CommandService>().Log += LogAsync;
            await DiscordClient.LoginAsync(TokenType.Bot, options.Discord.Token).ConfigureAwait(false);
            await DiscordClient.StartAsync().ConfigureAwait(false);

            CommandHandlingService = app.ApplicationServices.GetService<CommandHandlingService>();
            await CommandHandlingService.InstallCommandsAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            _logger.LogInformation(log.Exception, $"[{log.Severity}] {log.Message}");
            return Task.CompletedTask;
        }
    }
}