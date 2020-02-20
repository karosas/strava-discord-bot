using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord.Utilities;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Discord;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord.Modules
{
    [RequireToBeWhitelistedServer]
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<PublicModule> _logger;
        private readonly CommandService _commandService;
        private readonly ICommandCoreService _commandCoreService;
        private readonly ILeaderboardParticipantService _participantService;
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly IStravaService _stravaService;

        public PublicModule(ILogger<PublicModule> logger,
            CommandService commandService,
            ICommandCoreService commandCoreService,
            ILeaderboardParticipantService participantService,
            IEmbedBuilderService embedBuilderService,
            IStravaService stravaService)
        {
            _logger = logger;
            _commandService = commandService;
            _commandCoreService = commandCoreService;
            _participantService = participantService;
            _embedBuilderService = embedBuilderService;
            _stravaService = stravaService;
        }

        [Command("help")]
        [Summary("Lists available commands")]
        public async Task Help()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var commands = _commandService.Commands.ToList();
                    var embedBuilder = new EmbedBuilder();

                    foreach (var command in commands)
                    {
                        var embedFieldText = command.Summary ?? "No description available\n";
                        embedBuilder.AddField(command.Name, embedFieldText);
                    }

                    await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Help failed");
                }
            }
        }

        [Command("join")]
        [Summary("Join leaderboard")]
        public async Task JoinLeaderboard()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var responseText = _commandCoreService.GenerateJoinCommandContent(Context.Guild.Id, Context.User.Id,
                        Context.User.Mention);
                    var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
                    await dmChannel.SendMessageAsync(responseText);
                }
                catch (InvalidCommandArgumentException e)
                {
                    await ReplyAsync(e.Message);
                }
            }
        }

        [Command("stats")]
        [Summary("Show your weekly stats")]
        public async Task ShowParticipantStats()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var participant =
                        await _participantService
                            .GetParticipantOrDefault(Context.Guild.Id.ToString(), Context.User.Id.ToString());
                    if (participant == null)
                        await ReplyAsync("It seems like you're not part of the leaderboard. Try joining it.");

                    var start = DateTime.Now.AddDays(-7);
                    var activities = await _stravaService.FetchActivitiesForParticipant(participant, start);

                    await ReplyAsync(embed: _embedBuilderService.BuildParticipantStatsForCategoryEmbed(participant,
                        activities, Constants.LeaderboardRideType.RealRide, start, DateTime.Now));

                    await ReplyAsync(embed: _embedBuilderService.BuildParticipantStatsForCategoryEmbed(participant,
                        activities, Constants.LeaderboardRideType.VirtualRide, start, DateTime.Now));
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "stats failed");
                }
            }
        }
    }
}
