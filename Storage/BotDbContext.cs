using Microsoft.EntityFrameworkCore;
using StravaDiscordBot.Models;

namespace StravaDiscordBot.Storage
{
    public class BotDbContext : DbContext
    {
        private readonly AppOptions _options;

        public BotDbContext(AppOptions options)
        {
            _options = options;
        }

        public DbSet<LeaderboardParticipant> Participants { get; set; }
        public DbSet<Leaderboard> Leaderboards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(_options.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LeaderboardParticipant>()
                .HasKey(c => new {c.DiscordUserId, c.ServerId});
        }
    }
}