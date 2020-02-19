using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using StravaDiscordBot.Discord.Utilities;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;

namespace StravaDiscordBot.Discord.Modules
{
    [RequireRole(new[] { "Owner", "Bot Manager"})]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        private readonly ICommandCoreService _commandCoreService;
        private readonly IStravaService _stravaService;
        private readonly ILogger<AdminModule> _logger;

        public AdminModule(CommandService commandService, ICommandCoreService commandCoreService,
            ILogger<AdminModule> logger, IStravaService stravaService, DiscordSocketClient client)
        {
            _commandService = commandService;
            _commandCoreService = commandCoreService;
            _logger = logger;
            _stravaService = stravaService;
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
                    if (Context.Guild?.Id == null || Context.Guild?.Id == default)
                    {
                        await ReplyAsync("Doesn't seem like this is written inside a server.");
                    }

                    await ReplyAsync(
                        await _commandCoreService.GenerateInitializeCommandContext(Context.Guild.Id,
                            Context.Channel.Id));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "init failed");
                }
            }
        }

        [Command("leaderboard")]
        [Summary("[ADMIN] Manually triggers leaderboard in channel written")]
        [RequireToBeWhitelistedServer]
        public async Task ShowLeaderboard()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var groupedActivitiesByParticipant = new Dictionary<LeaderboardParticipant, List<DetailedActivity>>();
                    var participants = await _stravaService.GetAllParticipantsForServerAsync(Context.Guild.Id.ToString());
                    foreach (var participant in participants)
                    {
                        try
                        {
                            groupedActivitiesByParticipant.Add(participant, await _stravaService.FetchActivitiesForParticipant(participant, DateTime.Now.AddDays(-7)));
                        }
                        catch (StravaException e) when (e.Error == StravaException.StravaErrorType.RefreshFailed)
                        {
                            await AskToRelogin(participant.DiscordUserId);
                        }
                    }

                    var embeds = await _commandCoreService.GenerateLeaderboardCommandContent(groupedActivitiesByParticipant);
                    _logger.LogInformation("Executing leaderboard");
                    foreach (var embed in embeds)
                    {
                        await ReplyAsync(embed: embed);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "leaderboard failed");
                }
            }
        }

        [Command("list")]
        [Summary("[ADMIN] Lists participants for server")]
        [RequireToBeWhitelistedServer]
        public async Task ListLeaderboardParticipants()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    _logger.LogInformation("Executing list");
                    foreach (var embed in await _commandCoreService.GenerateListLeaderboardParticipantsContent(
                        Context.Guild.Id))
                    {
                        await ReplyAsync(embed: embed);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "list failed");
                }
            }
        }
        
        [Command("get")]
        [Summary("[ADMIN] Get detailed information of the participant")]
        [RequireToBeWhitelistedServer]
        public async Task GetDetailedParticipant(string discordId)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    _logger.LogInformation($"Executing get {discordId}");

                    var embed = await _commandCoreService.GenerateGetDetailedParticipantContent(Context.Guild.Id,
                        discordId);
                    await ReplyAsync(embed: embed);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "list failed");
                }
            }
        }

        [Command("remove")]
        [Summary("[ADMIN] Remove user from leaderboard by discord Id. Usage: `@mention remove 1234`")]
        [RequireToBeWhitelistedServer]
        public async Task RemoveParticipant(string discordId)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    await ReplyAsync(
                        await _commandCoreService.GenerateRemoveParticipantContent(discordId, Context.Guild.Id));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "list failed");
                }
            }
        }

        private async Task AskToRelogin(string discordId)
        {
            _logger.LogInformation($"Sending refresh notification to {discordId}");
            try
            {
                var user = Context.Client.GetUser(ulong.Parse(discordId));
                await user.SendMessageAsync(
                    $"Hey, I failed refreshing access to your Strava account. Please use `join` command again in the server of leaderboard.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to deliver relogin message");
            }
           
        }
    }
}