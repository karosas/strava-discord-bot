using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    }

    public class LeaderboardService : ILeaderboardService
    {
        private readonly BotDbContext _dbContext;
        private readonly ILogger<LeaderboardService> _logger;

        public LeaderboardService(BotDbContext dbContext, ILogger<LeaderboardService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Create(Leaderboard leaderboard)
        {
            _dbContext.Leaderboards.Add(leaderboard);
            await _dbContext.SaveChangesAsync();
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
                .Select(x =>
                {
                    return x.Value.subCategory.CalculateTotalResult(x.Value.results);
                })
                .ToList()
            };
        }
    }
}