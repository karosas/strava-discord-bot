using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;
using StravaDiscordBot.Exceptions;
using StravaDiscordBot.Helpers;
using StravaDiscordBot.Models;
using StravaDiscordBot.Models.Strava;
using StravaDiscordBot.Services;
using StravaDiscordBot.Storage;

namespace StravaDiscordBot.Discord
{
    public interface ICommandCoreService
    {
        string GenerateJoinCommandContent(ulong serverId, ulong userId, string username);
        Task<string> GenerateInitializeCommandContext(ulong serverId, ulong channelId);
        Task<string> GenerateRemoveParticipantContent(string discordId, ulong serverId);
    }

    public class CommandCoreService : ICommandCoreService
    {
        private readonly ILogger<CommandCoreService> _logger;
        private readonly BotDbContext _context;
        private readonly IStravaService _stravaService;

        public CommandCoreService(ILogger<CommandCoreService> logger, BotDbContext context,
            IStravaService stravaService)
        {
            _logger = logger;
            _context = context;
            _stravaService = stravaService;
        }

        public async Task<string> GenerateInitializeCommandContext(ulong serverId, ulong channelId)
        {
            if (_context.Leaderboards.Any(x => x.ServerId == serverId.ToString()))
                return "Seems like a leaderboard is already setup on this server";

            var leaderboard = new Leaderboard {ServerId = serverId.ToString(), ChannelId = channelId.ToString()};
            _context.Leaderboards.Add(leaderboard);
            await _context.SaveChangesAsync();
            return "Initialized leaderboard for this server. Users can join by using the `join` command.";
        }

        public string GenerateJoinCommandContent(ulong serverId, ulong userId, string username)
        {
            return
                $"Hey, {username} ! Please go to the following url to authorize me to view your Strava activities: {_stravaService.GetOAuthUrl(serverId.ToString(), userId.ToString())}";
        }

        public async Task<string> GenerateRemoveParticipantContent(string discordId, ulong serverId)
        {
            var participant = _context.Participants.SingleOrDefault(x =>
                x.DiscordUserId == discordId && x.ServerId == serverId.ToString());

            if (participant == null)
                return $"Participant with id {discordId} wasn't found.";

            var credentials = _context.Credentials.FirstOrDefault(x => x.StravaId == participant.StravaId);

            _context.Participants.Remove(participant);
            if (credentials != null)
                _context.Credentials.Remove(credentials);
            await _context.SaveChangesAsync();
            return $"Participant with id {discordId} was removed.";
        }
    }
}
