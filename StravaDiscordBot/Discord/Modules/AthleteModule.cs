using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using IO.Swagger.Client;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord.Modules.NamedArgs;
using StravaDiscordBot.Discord.Utilities;
using StravaDiscordBot.Models;
using StravaDiscordBot.Services;

namespace StravaDiscordBot.Discord.Modules
{
    public class AthleteModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<AthleteModule> _logger;
        private readonly ILeaderboardParticipantService _participantService;
        private readonly IStravaAuthenticationService _stravaAuthenticationService;
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly IAthleteService _athleteService;
        private readonly IStravaCredentialService _stravaCredentialService;

        public AthleteModule(ILogger<AthleteModule> logger,
            ILeaderboardParticipantService participantService,
            IStravaAuthenticationService stravaAuthenticationService,
            IEmbedBuilderService embedBuilderService,
            IAthleteService athleteService,
            IStravaCredentialService stravaCredentialService)
        {
            _logger = logger;
            _participantService = participantService;
            _stravaAuthenticationService = stravaAuthenticationService;
            _embedBuilderService = embedBuilderService;
            _athleteService = athleteService;
            _stravaCredentialService = stravaCredentialService;
        }

        [Command("list")]
        [Summary("[ADMIN] Lists participants for server")]
        [RequireToBeWhitelistedServer]
        [RequireRole(new[] { "Owner", "Bot Manager" })]
        public async Task ListLeaderboardParticipants()
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    _logger.LogInformation("Executing list");
                    var embeds = new List<Embed>();
                    var participants = _participantService.GetAllParticipantsForServerAsync(Context.Guild.Id.ToString());
                    foreach (var participant in participants)
                    {
                        var (policy, context) = _stravaAuthenticationService.GetUnauthorizedPolicy(participant.StravaId);
                        try
                        {
                            var athlete = await policy.ExecuteAsync(x => _athleteService.Get(participant.StravaId), context);
                            embeds.Add(_embedBuilderService.BuildAthleteInfoEmbed(participant, athlete));
                        }
                        catch (ApiException e)
                        {
                            _logger.LogWarning(e, $"Failed to fetch athlete info for {participant.DiscordUserId}");
                        }
                    }

                    if (!embeds.Any())
                        embeds.Add(
                            new EmbedBuilder()
                                .WithTitle("No participants found")
                                .WithCurrentTimestamp()
                                .Build()
                        );

                    foreach (var embed in embeds)
                        await ReplyAsync(embed: embed);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "list failed");
                }
            }
        }

        [Command("profile")]
        [Summary("Get your profile")]
        [RequireToBeWhitelistedServer]
        [RequireRole(new[] { "Owner", "Bot Manager" })]
        public async Task GetDetailedParticipant()
        {
            var id = Context.User.Id.ToString();
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    foreach (var embed in await BuildProfileEmbedsForStravaOrDiscordId(id))
                        await ReplyAsync(embed: embed);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "get failed");
                    await ReplyAsync(embed: _embedBuilderService.BuildSimpleEmbed(
                        "Not Found",
                        "Couldn't find participant with given parameters")
                    );
                }
            }
        }
        
        [Command("profile")]
        [Summary("[ADMIN] Get detailed information of the participant by discord user ID. Usage: `@mention get 1234`")]
        [RequireToBeWhitelistedServer]
        [RequireRole(new[] { "Owner", "Bot Manager" })]
        public async Task GetDetailedParticipant(string id)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(id))
                        throw new ArgumentNullException(nameof(id));
                    
                    foreach (var embed in await BuildProfileEmbedsForStravaOrDiscordId(id))
                        await ReplyAsync(embed: embed);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "get failed");
                    await ReplyAsync(embed: _embedBuilderService.BuildSimpleEmbed(
                        "Not Found",
                        "Couldn't find participant with given parameters")
                    );
                }
            }
        }

        [Command("remove")]
        [Summary("[ADMIN] Remove user from leaderboard by discord user ID. Usage: `@mention remove 1234`")]
        [RequireToBeWhitelistedServer]
        [RequireRole(new[] { "Owner", "Bot Manager" })]
        public async Task RemoveParticipant(string discordId)
        {
            using (Context.Channel.EnterTypingState())
            {
                try
                {
                    var participant = _participantService.GetParticipantOrDefault(Context.Guild.Id.ToString(), discordId);
                    if (participant == null)
                    {
                        await ReplyAsync($"Participant with id {discordId} wasn't found.");
                        return;
                    }

                    var credentials = await _stravaCredentialService.GetByStravaId(participant.StravaId);
                    await _participantService.Remove(participant, credentials);

                    await ReplyAsync($"Participant with id {discordId} was removed.");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "list failed");
                }
            }
        }

        private async Task<IEnumerable<Embed>> BuildProfileEmbedsForStravaOrDiscordId(string id)
        {
            LeaderboardParticipant participant = null;

                    
            participant = _participantService.GetParticipantOrDefault(Context.Guild.Id.ToString(), id) ??
                          _participantService.GetParticipantByStravaIdOrDefault(Context.Guild.Id.ToString(), id);

            if (participant == null)
                throw new ArgumentException("Couldn't find participant");

            var (policy, context) = _stravaAuthenticationService.GetUnauthorizedPolicy(participant.StravaId);
            var athlete = await policy.ExecuteAsync(x => _athleteService.Get(participant.StravaId), context);
            return _embedBuilderService.BuildDetailedAthleteEmbeds(participant, athlete);
        }
    }
}
