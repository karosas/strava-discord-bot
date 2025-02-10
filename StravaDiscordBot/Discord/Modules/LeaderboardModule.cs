using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord.Modules.NamedArgs;
using StravaDiscordBot.Discord.Utilities;
using StravaDiscordBot.Models.Categories;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord.Modules
{
    public class LeaderboardModule : ModuleBase<SocketCommandContext>
    {
        private readonly IStravaAuthenticationService _stravaAuthenticationService;
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardModule> _logger;

        public LeaderboardModule(ILogger<LeaderboardModule> logger,
            IStravaAuthenticationService stravaAuthenticationService,
            ILeaderboardService leaderboardService)
        {
            _logger = logger;
            _stravaAuthenticationService = stravaAuthenticationService;
            _leaderboardService = leaderboardService;
        }

        [Command("join")]
        [Summary("Join leaderboard")]
        public async Task JoinLeaderboard()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var text = $"Hey, {Context.User.Mention} ! Please go to the following url to authorize me to view your Strava activities: {_stravaAuthenticationService.GetOAuthUrl(Context.Guild.Id.ToString(), Context.User.Id.ToString())}";

                    var dmChannel = await Context.User.CreateDMChannelAsync();
                    await dmChannel.SendMessageAsync(text);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Join failed for {Context.User.Id}");
                }
            }
        }

        [Command("leaderboard")]
        [Summary("[ADMIN] Manually triggers leaderboard in channel written")]
        [RequireToBeWhitelistedServer]
        [Utilities.RequireRole(new[] { "Owner", "Bot Manager" })]
        public async Task ShowLeaderboard(LeaderboardNamedArgs leaderboardArguments)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    await _leaderboardService.GenerateForServer(
                        Context.Channel,
                        Context.Guild.Id.ToString(),
                        DateTime.Now.AddDays(-7),
                        leaderboardArguments.WithRoles,
                        new RealRideCategory(),
                        new VirtualRideCategory()
                    );
                }
                catch(Exception e)
                {
                    _logger.LogError(e, $"Failed to generate manual leaderboard for { Context.Guild.Id.ToString()}");
                }

            }
        }
    }
}