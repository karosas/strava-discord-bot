using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using StravaDiscordBot.Discord.Utilities;

namespace StravaDiscordBot.Discord.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        private readonly ICommandCoreService _commandCoreService;

        public AdminModule(CommandService commandService, ICommandCoreService commandCoreService)
        {
            _commandService = commandService;
            _commandCoreService = commandCoreService;
        }

        [Command("init")]
        [Summary("Sets up channel command is written as destined leaderboard channel for the server")]
        public async Task InitializeLeaderboard()
        {
            if (Context.Guild?.Id == null || Context.Guild?.Id == default)
            {
                await ReplyAsync("Doesn't seem like this is written inside a server.");
            }

            await ReplyAsync(await _commandCoreService.GenerateInitializeCommandContext(Context.Guild.Id, Context.Channel.Id));
        }

        [Command("leaderboard")]
        [Summary("Manually triggers leaderboard in channel written")]
        [RequireToBeWhitelistedServer]
        public async Task ShowLeaderboard()
        {
            await ReplyAsync("Leaderboard command triggered");
        }
    }
}
