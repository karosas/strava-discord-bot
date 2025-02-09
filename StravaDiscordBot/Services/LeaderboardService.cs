using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Discord;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Categories;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services
{
    public interface ILeaderboardService
    {
        CategoryResult GetTopResultsForCategory(List<ParticipantWithActivities> participantsWithActivities, ICategory category);
        Task<Leaderboard> GetForServer(string serverId);
        Task Create(Leaderboard leaderboard);
        Task GenerateForServer(IMessageChannel replyChannel, string serverId, DateTime start, bool grantWinnerRole, params ICategory[] categories);
        Task<int> PruneUsers(string serverId, bool dryRun);
    }

    public class LeaderboardService : ILeaderboardService
    {
        private readonly BotDbContext _dbContext;
        private readonly ILogger<LeaderboardService> _logger;
        private readonly IRoleService _roleService;
        private readonly ILeaderboardParticipantService _participantService;
        private readonly IStravaAuthenticationService _stravaAuthenticationService;
        private readonly IActivitiesService _activitiesService;
        private readonly IEmbedBuilderService _embedBuilderService;
        private readonly DiscordSocketClient _discordSocketClient;

        public LeaderboardService(
            BotDbContext dbContext,
            ILogger<LeaderboardService> logger,
            IRoleService roleService,
            ILeaderboardParticipantService participantService,
            IStravaAuthenticationService stravaAuthenticationService,
            IActivitiesService activitiesService,
            IEmbedBuilderService embedBuilderService, 
            DiscordSocketClient discordSocketClient)
        {
            _dbContext = dbContext;
            _logger = logger;
            _roleService = roleService;
            _participantService = participantService;
            _stravaAuthenticationService = stravaAuthenticationService;
            _activitiesService = activitiesService;
            _embedBuilderService = embedBuilderService;
            _discordSocketClient = discordSocketClient;
        }

        public async Task Create(Leaderboard leaderboard)
        {
            _dbContext.Leaderboards.Add(leaderboard);
            await _dbContext.SaveChangesAsync();
        }

        public async Task GenerateForServer(IMessageChannel replyChannel, string serverId, DateTime start, bool grantWinnerRole, params ICategory[] categories)
        {
            _logger.LogInformation("Executing leaderboard command");
            if (grantWinnerRole)
                await _roleService.RemoveRoleFromAllInServer(serverId, Constants.LeaderboardWinnerRoleName);

            var participantsWithActivities = new List<ParticipantWithActivities>();
            var participants = _participantService.GetAllParticipantsForServerAsync(serverId);

            foreach (var participant in participants)
            {
                var (policy, context) = _stravaAuthenticationService.GetUnauthorizedPolicy(participant.StravaId);
                try
                {
                    var activities =
                        await policy.ExecuteAsync(x => _activitiesService.GetForStravaUser(participant.StravaId, start),
                            context);
                    participantsWithActivities.Add(new ParticipantWithActivities
                    {
                        Participant = participant,
                        Activities = activities
                    });
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e,
                        $"Failed to fetch activities for {participant.DiscordUserId}, excluding participant from leaderboard");
                }
            }

            var categoryResults = new List<CategoryResult>();
            foreach (var category in categories)
            {
                var categoryResult = GetTopResultsForCategory(participantsWithActivities, category);
                await replyChannel.SendMessageAsync(embed: _embedBuilderService
                    .BuildLeaderboardEmbed(
                        categoryResult,
                        start,
                        DateTime.Now
                    )
                );
                categoryResults.Add(categoryResult);
            }

            if (grantWinnerRole)
            {
                var winners = new List<ParticipantResult>();
                foreach (var subCategoryResult in categoryResults.SelectMany(categoryResult => categoryResult.SubCategoryResults))
                {
                    winners.AddRange(subCategoryResult.OrderedParticipantResults.Take(3));
                }

                await GrantWinnerRoles(serverId, winners);
            }
        }

        public async Task<int> PruneUsers(string serverId, bool dryRun)
        {
            var leaderboard = await _dbContext.Leaderboards.FirstOrDefaultAsync(x => x.ServerId == serverId);
            if (leaderboard == null)
            {
                _logger.LogError($"Leaderboard '{serverId}' not found");
                return 0;
            }
            
            var guild = _discordSocketClient.GetGuild(ulong.Parse(leaderboard.ServerId));
            var nestedGuildUsers = await guild.GetUsersAsync().ToListAsync();
            var guildUsers = nestedGuildUsers.SelectMany(x => x).ToList();
            
            var usersRemoved = 0;
            _logger.LogInformation($"Cleaning up server {leaderboard.ServerId}");
            foreach (var participant in _dbContext.Participants.AsQueryable().Where(x => x.ServerId == leaderboard.ServerId))
            {
                // If we found discord user in this server with matching id -> continue
                if (guildUsers.Any(guildUser => guildUser.Id.ToString() == participant.DiscordUserId)) 
                    continue;
                
                _logger.LogInformation($"Removing user '{participant.DiscordUserId}' ({participant.GetDiscordMention()}' from '{leaderboard.ServerId}'");
                if(!dryRun)
                    _dbContext.Participants.Remove(participant);
                usersRemoved++;
            }

            if (usersRemoved > 0)
            {
                await _dbContext.SaveChangesAsync();
            }

            return usersRemoved;
        }

        public async Task<Leaderboard> GetForServer(string serverId)
        {
            _logger.LogInformation($"Attempting to find leaderboard for server {serverId}");
            return await _dbContext.Leaderboards.FindAsync(serverId);
        }

        public CategoryResult GetTopResultsForCategory(List<ParticipantWithActivities> participantsWithActivities, ICategory category)
        {
            var subcategoryParticipantResults = new Dictionary<string, (ISubCategory subCategory, List<ParticipantResult> results)>();

            foreach (var participantWithActivities in participantsWithActivities)
            {
                var participant = participantWithActivities.Participant;
                var matchingActivities = category.FilterActivities(participantWithActivities.Activities);

                foreach (var subCategory in category.SubCategories)
                {
                    if (!subcategoryParticipantResults.ContainsKey(subCategory.Name))
                        subcategoryParticipantResults.Add(subCategory.Name, (subCategory, new List<ParticipantResult>()));

                    subcategoryParticipantResults[subCategory.Name].results.Add(subCategory.CalculateParticipantsResults(participant, matchingActivities));
                }
            }

            return new CategoryResult
            {
                Name = category.Name,
                SubCategoryResults = subcategoryParticipantResults
                    .Select(x => x.Value.subCategory.CalculateTotalResult(x.Value.results))
                    .ToList()
            };
        }

        private async Task GrantWinnerRoles(string serverId, List<ParticipantResult> leaderboardResults)
        {
            foreach (var participantResult in leaderboardResults)
            {
                try
                {
                    await _roleService.GrantUserRole(serverId,
                        participantResult.Participant.DiscordUserId, Constants.LeaderboardWinnerRoleName);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        $"Failed to grant leaderboard role for '{participantResult.Participant.DiscordUserId}'");
                }
            }
        }
    }
}