using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using StravaDiscordBot.Discord.Utilities;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Discord;

namespace StravaDiscordBot.Discord.Modules
{
    [RequireToBeWhitelistedServer]
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commandService;
        private readonly ICommandCoreService _commandCoreService;

        public PublicModule(CommandService commandService, ICommandCoreService commandCoreService)
        {
            _commandService = commandService;
            _commandCoreService = commandCoreService;
        }

        [Command("help")]
        [Summary("Lists available commands")]
        public async Task Help()
        {
            var commands = _commandService.Commands.ToList();
            var embedBuilder = new EmbedBuilder();

            foreach (var command in commands)
            {
                string embedFieldText = command.Summary ?? "No description available\n";
                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }

        [Command("join")]
        [Summary("Join leaderboard")]
        public async Task JoinLeaderboard()
        {
            try
            {
                var responseText = await _commandCoreService.GenerateJoinCommandContent(Context.Guild.Id, Context.User.Id, Context.User.Mention);
                var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                await dmChannel.SendMessageAsync(responseText);
            } 
            catch(InvalidCommandArgumentException e)
            {
                await ReplyAsync(e.Message);
            }
        }
    }
}
