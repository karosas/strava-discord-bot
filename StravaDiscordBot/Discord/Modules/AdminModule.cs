using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord.Utilities;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Categories;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord.Modules
{
    [RequireRole(new[] {"Owner", "Bot Manager"})]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<AdminModule> _logger;
        private readonly ILeaderboardParticipantService _participantService;
        private readonly IRoleService _roleService;
        private readonly IStravaCredentialService _stravaCredentialService;
        private readonly IAthleteService _athleteService;
        private readonly IActivitiesService _activitiesService;
        private readonly IStravaAuthenticationService _stravaAuthenticationService;

        public AdminModule(
            ILogger<AdminModule> logger,
            IEmbedBuilderService embedBuilderService,
            ILeaderboardParticipantService participantService,
            ILeaderboardService leaderboardResultService,
            IRoleService roleService,
            IStravaCredentialService stravaCredentialService,
            IAthleteService athleteService,
            IActivitiesService activitiesService,
            IStravaAuthenticationService stravaAuthenticationService)
        {
            _logger = logger;
            _embedBuilderService = embedBuilderService;
            _participantService = participantService;
            _leaderboardService = leaderboardResultService;
            _roleService = roleService;
            _stravaCredentialService = stravaCredentialService;
            _athleteService = athleteService;
            _activitiesService = activitiesService;
            this._stravaAuthenticationService = stravaAuthenticationService;
        }

        [Command("init")]
        [Summary("[ADMIN] Sets up channel command is written as destined leaderboard channel for the server")]
        public async Task InitializeLeaderboard()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    _logger.LogInformation("Executing init");
                    if (Context.Guild?.Id == null)
                    {
                        await ReplyAsync("Doesn't seem like this is written inside a server.");
                        return;
                    }

                    var leaderboard = await _leaderboardService.GetForServer(Context.Guild.Id.ToString());
                    if (leaderboard != null)
                    {
                        await ReplyAsync("Seems like a leaderboard is already setup on this server");
                        return;
                    }

                    leaderboard = new Leaderboard { 
                        ServerId = Context.Guild.Id.ToString(), 
                        ChannelId = Context.Channel.Id.ToString() 
                    };
                    await _leaderboardService.Create(leaderboard);
                    await ReplyAsync( "Initialized leaderboard for this server. Users can join by using the `join` command.");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "init failed");
                }
            }
        }

        [Command("grant-winner-role")]
        [Summary(
            "[ADMIN] Grant leaderboard winner role to discord user ID (for testing purposes). Usage: `@mention grant-winner-role 1234`")]
        [RequireToBeWhitelistedServer]
        public async Task GrantWinnerRole(string discordId)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    await _roleService.GrantUserRole(
                        Context.Guild.Id.ToString(), 
                        discordId,
                        Constants.LeaderboardWinnerRoleName
                    );
                    await ReplyAsync("Success");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to grant role for user {discordId}");
                    await ReplyAsync($"Failed - {e.Message}");
                }
            }
        }

        [Command("remove-winner-role")]
        [Summary(
            "[ADMIN] Remove leaderboard winner role from discord user ID (for testing purposes). Usage: `@mention remove-winner-role 1234`")]
        [RequireToBeWhitelistedServer]
        public async Task RemoveWinnerRole(string discordId)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    await _roleService.RemoveUserRole(Context.Guild.Id.ToString(), discordId,
                        Constants.LeaderboardWinnerRoleName);
                    await ReplyAsync("Success");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to remove role for user {discordId}");
                    await ReplyAsync($"Failed - {e.Message}");
                }
            }
        }
        
        [Command("prune")]
        [Summary(
            "[ADMIN] Remove leaderboard participants who're no longer part of the server Usage: `@mention rprune`")]
        [RequireToBeWhitelistedServer]
        public async Task PruneUsers()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var usersRemoved = await _leaderboardService.PruneUsers(Context.Guild.Id.ToString());
                    await ReplyAsync($"Removed {usersRemoved} users");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to prune users");
                    await ReplyAsync($"Failed - {e.Message}");
                }
            }
        }
    }
}
