using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StravaDiscordBot.Models;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Services
{
    public interface ILeaderboardService
    {
        Task<List<Leaderboard>> GetAllLeaderboards();
    }
    
    public class LeaderboardService : ILeaderboardService
    {
        private readonly BotDbContext _dbContext;

        public LeaderboardService(BotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<Leaderboard>> GetAllLeaderboards()
        {
            return _dbContext.Leaderboards.ToListAsync();
        }
    }
}